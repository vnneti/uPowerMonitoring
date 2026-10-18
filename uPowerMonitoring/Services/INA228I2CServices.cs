using System;
using System.Threading;
using System.Threading.Tasks;
using FTDI; // Offizieller FTDI D2XX Namespace
using uPowerMonitoring.Models;

namespace uPowerMonitoring.Services;

public interface IIna228Service
{
    Task InitializeAsync(string serial, double shunt, byte addr);
    Task<UInt16> GetVendorID();
    Task<UInt16> GetDeviceID();    
    Task<Ina228Measurement> ReadMeasuredDataAsync();
    Task<bool> SetADCConfig(UInt16 reg);
    Task<bool> SetConfig(UInt16 reg);
    Task<double> ReadTemperature();
    void SetConversionFactor(int range);
    void SetShuntResistor(double shunt);
}

public class Ina228I2cService : IIna228Service
{
    const byte INA228_REG_MANUFACTURER_ID = 0x3E;
        const byte INA228_REG_DEVICE_ID = 0x3F;

        const byte INA228_REG_CONFIG = 0x00;
        public const UInt16 INA228_REG_CFG_RST = (1 << 15);
        public const UInt16 INA228_REG_CFG_RSTACC = (1 << 14);
        public static UInt16 INA228_REG_CFG_CONVDLY(byte value) { return (UInt16)((value & 0xFF) << 6); }
        public const UInt16 INA228_REG_CFG_TEMPCOMP = (1 << 5);
        public static UInt16 INA228_REG_CFG_ADCRANGE(byte value) {return (UInt16)((value & 0x01)<<4);}
        public const UInt16 INA228_REG_CFG_ADCRANGE_POS = 4;
        const byte INA228_REG_ADC_CONFIG = 0x01;
        const UInt16 INA228_REG_ACFG_MODE_MASK = (0x0F << 12);
        public static UInt16 INA228_REG_ACFG_MODE(byte value) { return (UInt16)((value & 0x0F) << 12); }
        public const byte INA228_ACFG_MODE_SHUTDOWN = 0;
        public const byte INA228_ACFG_MODE_VBUS_SS = 1;
        public const byte INA228_ACFG_MODE_VSHUNT_SS = 2;
        public const byte INA228_ACFG_MODE_VSHUNT_BUS_SS = 3;
        public const byte INA228_ACFG_MODE_VSHUNT_BUS_TEMP_SS = 7;
        public const byte INA228_ACFG_MODE_VBUS_CON = 9;
        public const byte INA228_ACFG_MODE_VSHUNT_CON = 0x0A;
        public const byte INA228_ACFG_MODE_VSHUNT_BUS_CON = 0x0B;
        public const byte INA228_ACFG_MODE_VSHUNT_BUS_TEMP_CON = 0xF;
        public const UInt16 INA228_REG_ACFG_VBUSCT_MASK = (0x07 << 9);
        public static UInt16 INA228_REG_ACFG_VBUSCT(byte value) { return (UInt16)((value & 0x07) << 9); }
        public const UInt16 INA228_REG_ACFG_VSHCT_MASK = (0x07 << 6);
        public static UInt16 INA228_REG_ACFG_VSHCT(byte value) { return (UInt16)((value & 0x07) << 6); }
        public const UInt16 INA228_REG_ACFG_VTCT_MASK = (0x07 << 3);
        public static UInt16 INA228_REG_ACFG_VTCT(byte value) { return (UInt16)((value & 0x07) << 3); }
        public const UInt16 INA228_REG_ACFG_AVG_MASK = (0x07 << 0);
        public static UInt16 INA228_REG_ACFG_AVG(byte value) { return (UInt16)((value & 0x07) << 0); }




        const byte INA228_REG_SHUNT_CAL = 0x02;
        const byte INA228_REG_SHUNT_TEMPCO = 0x03;
        const byte INA228_REG_VSHUNT = 0x04;
        const byte INA228_REG_VBUS = 0x05;
        const byte INA228_REG_DIETEMP = 0x06;
        const byte INA228_REG_CURRENT = 0x07;
        const byte INA228_REG_POWER = 0x08;
        const byte INA228_REG_ENERGY = 0x09;
        const byte INA228_REG_CHARGE = 0x0A;
        const byte INA228_REG_DIAG_ALERT = 0x0B;
        const byte INA228_REG_SOVL = 0x0C;

        public const double INA228_CURRENT0_LSB = 1.5625E-6;
        public const double INA228_CURRENT1_LSB = 0.390625E-6;
        public const double INA228_VBUS_LSB = 195.3125E-6;
        public const double INA228_VSHUNT0_LSB = 312.5E-9;
        public const double INA228_VSHUNT1_LSB = 78.125E-9;
    private FTMPSSEDevice _FTMpsse;
    private readonly SemaphoreSlim _LockObject = new SemaphoreSlim(1,1);
    private string _FT232HSerialNumber ="";
    private byte _Ina228I2cAddress = 0x40; // I2C Adresse des INA228 (abhängig von A0/A1)
    
    private double _ShuntResistor = 1.0;
    private double _VShuntLSB = INA228_VSHUNT0_LSB;

    private UInt32 _VBus = 0;
    public Ina228I2cService()
    {        
        _FTMpsse = new FTMPSSEDevice();
    }

    public void SetShuntResistor(double shunt)
    {
        _ShuntResistor = shunt;
    }
    public void SetConversionFactor(int range)
    {
        _VShuntLSB = INA228_VSHUNT0_LSB;
        if(range > 0)
            _VShuntLSB = INA228_VSHUNT1_LSB;
    }
    public async Task InitializeAsync(string serial, double shunt, byte addr)
    {
        await _LockObject.WaitAsync();
        try
        {
            _FT232HSerialNumber = serial;
            _ShuntResistor = shunt;
            _Ina228I2cAddress = addr;
            var dev = await _FTMpsse.SearchFT232HDeviceAsync(_FT232HSerialNumber);
            if (dev == null)
            {
                throw new Exception("No FT232H found");
            }       
            if(!(await _FTMpsse.OpenAndInitializeAsync(dev.LocId)))
            {
                throw new Exception("Fehler bei der Initialisierung des MPSSE-I2C-Modus.");
            }     
        }
        finally
        {
            _LockObject.Release();
        }
    }

    public async Task<UInt16> GetVendorID()
    {
        await _LockObject.WaitAsync();
        try
        {
            if (!await SendRegisterPointer(_Ina228I2cAddress, INA228_REG_MANUFACTURER_ID))
                return 0;
            return await Read16BitRegister(_Ina228I2cAddress);
        }
        finally
        {
            _LockObject.Release();
        }
    }

    public async Task<UInt16> GetDeviceID()
    {
        await _LockObject.WaitAsync();
        try
        {
            if (!await SendRegisterPointer(_Ina228I2cAddress, INA228_REG_DEVICE_ID))
                return 0;
            return await Read16BitRegister(_Ina228I2cAddress);
        }
        finally
        {
            _LockObject.Release();
        }
    }

    public async Task<Ina228Measurement> ReadMeasuredDataAsync()
    {
        await _LockObject.WaitAsync();
        try
        {
            byte[] data = await ReadVShuntAsync(_Ina228I2cAddress);
            Int32 Vshunt = ((data[0] << 24) | (data[1] << 16) | (data[2] << 8)) >> 12;
            await Task.Delay(1);
            data = await ReadVBusAsync(_Ina228I2cAddress);
            UInt32 Vbus = (UInt32)((data[0] << 16) | (data[1] << 8) | data[2]) >> 4;
            if(Vbus < 0xFFFFF)
                _VBus = Vbus;
            double current = (Vshunt*_VShuntLSB)/_ShuntResistor;
            double volt = (_VBus * INA228_VBUS_LSB);
            return new Ina228Measurement(            
                Timestamp: DateTime.Now,
                VoltageV: volt,
                CurrentA:  current,
                PowerW: volt*current,
                Temperature: null         
            );
        }
        finally
        {
            _LockObject.Release();
        }
            
    }

    public async Task<bool> SetADCConfig(UInt16 reg)
    {
        await _LockObject.WaitAsync();
        try
        {                
            byte[] send = new byte[3]{INA228_REG_ADC_CONFIG, (byte)(reg >>8), (byte)(reg & 0xFF)};
            if(!await _FTMpsse.FT_I2C_SetLineIdleAsync())
                return false;
            if(!await _FTMpsse.FT_I2C_SetStartAsync())
                return false;
            if(!await _FTMpsse.FT_I2C_WriteAddressAsync(_Ina228I2cAddress, false))
            {
                await _FTMpsse.FT_I2C_SetStopAsync();
                return false;
            }
            for(int i = 0; i < 3; i++)
            {
                if(!await _FTMpsse.FT_I2C_WriteByteAsync(send[i]))
                {
                    await _FTMpsse.FT_I2C_SetStopAsync();
                    return false;
                }
            }
        
            if(!await _FTMpsse.FT_I2C_SetStopAsync())
                return false;
            return true;                    
        }
        finally
        {
            _LockObject.Release();
        }

    }

    public async Task<bool> SetConfig(UInt16 reg)
    {
        await _LockObject.WaitAsync();
        try
        {
            byte[] send = new byte[3]{INA228_REG_CONFIG, (byte)(reg >>8), (byte)(reg & 0xFF)};
            if(!await _FTMpsse.FT_I2C_SetLineIdleAsync())
                return false;
            if(!await _FTMpsse.FT_I2C_SetStartAsync())
                return false;
            if(!await _FTMpsse.FT_I2C_WriteAddressAsync(_Ina228I2cAddress, false))
            {
                await _FTMpsse.FT_I2C_SetStopAsync();
                return false;
            }
            for(int i = 0; i < 3; i++)
            {
                if(!await _FTMpsse.FT_I2C_WriteByteAsync(send[i]))
                {
                    await _FTMpsse.FT_I2C_SetStopAsync();
                    return false;
                }
            }
        
            if(!await _FTMpsse.FT_I2C_SetStopAsync())
                return false;
            return true;            
        }
        finally
        {
            _LockObject.Release();
        }                
    } 

    public async Task<double> ReadTemperature()
    {
        await _LockObject.WaitAsync();
        try
        {
            await SendRegisterPointer(_Ina228I2cAddress, INA228_REG_DIETEMP);
            Int32 Temp = (await Read16BitRegister(_Ina228I2cAddress) << 16)>> 16;
            return (Temp * 0.0078125);
        }
        finally
        {
            _LockObject.Release();
        }      
    }

    private async Task<byte[]> ReadVShuntAsync(byte addr)
    {
         byte[] res = new byte[3];
        Array.Fill(res, (byte)0xFF);
        if (!await SendRegisterPointer(addr, INA228_REG_VSHUNT))
            return res;
        return await ReadRegisters(addr, 3);
    }

     private async Task<byte[]> ReadVBusAsync(byte addr)
    {
         byte[] res = new byte[3];
        Array.Fill(res, (byte)0xFF);
        if (!await SendRegisterPointer(addr, INA228_REG_VBUS))
            return res;
        return await ReadRegisters(addr, 3);
    }

    private async Task<byte[]> ReadAllDataAsync(byte addr)
    {
        byte[] res = new byte[6];
        Array.Fill(res, (byte)0xFF);
        if (!await SendRegisterPointer(addr, INA228_REG_VSHUNT))
            return res;
        return await ReadRegisters(addr, 6);
    }

    private async Task<bool> SendRegisterPointer(byte addr, byte reg)
    {
        if(!await _FTMpsse.FT_I2C_SetLineIdleAsync())
            return false;
        if(!await _FTMpsse.FT_I2C_SetStartAsync())
            return false;
        if(!await _FTMpsse.FT_I2C_WriteAddressAsync(addr, false))
        {
            await _FTMpsse.FT_I2C_SetStopAsync();
            return false;
        }
        if(!await _FTMpsse.FT_I2C_WriteByteAsync(reg))
        {
            await _FTMpsse.FT_I2C_SetStopAsync();
            return false;
        }
        if(!await _FTMpsse.FT_I2C_SetStopAsync())
            return false;
        return true;
    }

    private async Task<byte> ReadRegister(byte addr)
    {
        if(!await _FTMpsse.FT_I2C_SetLineIdleAsync())
            return 0;
        if(!await _FTMpsse.FT_I2C_SetStartAsync())
            return 0;
        if(!await _FTMpsse.FT_I2C_WriteAddressAsync(addr, true))
        {
            await _FTMpsse.FT_I2C_SetStopAsync();
            return 0;
        }
        byte reg = await _FTMpsse.FT_I2C_ReadByteAsync(true);
        await _FTMpsse.FT_I2C_SetStopAsync();
        return reg;
    }

    private async Task<UInt16> Read16BitRegister(byte addr)
    {
        byte[] regbytes = new byte[2];
        if(!await _FTMpsse.FT_I2C_SetLineIdleAsync())
            return 0;
        if(!await _FTMpsse.FT_I2C_SetStartAsync())
            return 0;
        if(!await _FTMpsse.FT_I2C_WriteAddressAsync(addr, true))
        {
            await _FTMpsse.FT_I2C_SetStopAsync();
            return 0;
        }
        regbytes[0] = await _FTMpsse.FT_I2C_ReadByteAsync(false);
        regbytes[1] = await _FTMpsse.FT_I2C_ReadByteAsync(true);
        await _FTMpsse.FT_I2C_SetStopAsync();
        return (UInt16)(regbytes[0] << 8 | regbytes[1]);
    }

    private async Task<UInt32> Read32BitRegister(byte addr)
    {
        byte[] regbytes = new byte[3];
        if(!await _FTMpsse.FT_I2C_SetLineIdleAsync())
            return 0xFFFFFFFF;
        if(!await _FTMpsse.FT_I2C_SetStartAsync())
            return 0xFFFFFFFF;
        if(!await _FTMpsse.FT_I2C_WriteAddressAsync(addr, true))
        {
            await _FTMpsse.FT_I2C_SetStopAsync();
            return 0xFFFFFFFF;
        }
        regbytes[0] = await _FTMpsse.FT_I2C_ReadByteAsync(false);
        regbytes[1] = await _FTMpsse.FT_I2C_ReadByteAsync(false);
        regbytes[2] = await _FTMpsse.FT_I2C_ReadByteAsync(true);
        await _FTMpsse.FT_I2C_SetStopAsync();
        return (UInt32)(regbytes[0] << 16 | regbytes[1] << 8 | regbytes[2]);
    }

    private async Task<byte[]> ReadRegisters(byte addr, int length)
    {
        byte[] regbytes = new byte[length];
        Array.Fill(regbytes, (byte)0xFF);
        if(!await _FTMpsse.FT_I2C_SetLineIdleAsync())
            return regbytes;
        if(!await _FTMpsse.FT_I2C_SetStartAsync())
            return regbytes;
        if(!await _FTMpsse.FT_I2C_WriteAddressAsync(addr, true))
        {
            await _FTMpsse.FT_I2C_SetStopAsync();
            return regbytes;
        }
        for(int i = 0; i < length - 1; i++)
        {
          regbytes[i] = await _FTMpsse.FT_I2C_ReadByteAsync(false);   
        }
        regbytes[length - 1] = await _FTMpsse.FT_I2C_ReadByteAsync(true);   
        await _FTMpsse.FT_I2C_SetStopAsync();
        return regbytes;
    }

    public async Task ConfigureSensorAsync(Ina228Config config)
    {
        await Task.Run(() =>
        {
            // TODO: Sende I2C Write Commands an die INA228 Konfigurationsregister (0x00, 0x01 etc.)
            // Beispiel: Schreibvorgang [I2C-Adresse Write] -> [Register 0x00] -> [Config High Byte] -> [Config Low Byte]
        });
    }
}