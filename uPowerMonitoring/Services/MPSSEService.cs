using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FTDI;
namespace uPowerMonitoring.Services;


    public class FTMPSSEDevice
    {
    const byte I2C_Dir_SDAin_SCLin = 0x00;
    const byte I2C_Dir_SDAin_SCLout = 0x01;
    const byte I2C_Dir_SDAout_SCLout = 0x03;
    const byte I2C_Dir_SDAout_SCLin = 0x02;
    const byte I2C_Data_SDAhi_SCLhi = 0x03;
    const byte I2C_Data_SDAlo_SCLhi = 0x01;
    const byte I2C_Data_SDAlo_SCLlo = 0x00;
    const byte I2C_Data_SDAhi_SCLlo = 0x02;
    // MPSSE clocking commands
    const byte MSB_FALLING_EDGE_CLOCK_BYTE_IN = 0x24;
    const byte MSB_RISING_EDGE_CLOCK_BYTE_IN = 0x20;
    const byte MSB_FALLING_EDGE_CLOCK_BYTE_OUT = 0x11;
    const byte MSB_DOWN_EDGE_CLOCK_BIT_IN = 0x26;
    const byte MSB_UP_EDGE_CLOCK_BYTE_IN = 0x20;
    const byte MSB_UP_EDGE_CLOCK_BYTE_OUT = 0x10;
    const byte MSB_RISING_EDGE_CLOCK_BIT_IN = 0x22;
    const byte MSB_FALLING_EDGE_CLOCK_BIT_OUT = 0x13;

    private FTDI.FTDIDevice _FTDevice = new FTDIDevice();

    public async Task<FTDI.FT_DEVICE_INFO_NODE?> SearchFT232HDeviceAsync(string serial)
    {
        return await Task.Run(() =>
        {
            uint devCount = 0;
            var status = _FTDevice.GetNumberOfDevices(ref devCount);
            AppLogger.AppLogger.Info("Number of devices = " + devCount, "FTDI Devices");

            if (devCount == 0) return null;

            FTDI.FT_DEVICE_INFO_NODE[] listDevices = new FTDI.FT_DEVICE_INFO_NODE[devCount];
            _FTDevice.GetDeviceList(listDevices);

            if (devCount == 1 && string.IsNullOrEmpty(serial))
                return listDevices[0];

            foreach (var device in listDevices)
            {
                if (device.SerialNumber == serial)
                {
                    return device;
                }
            }

            return listDevices[0]; // Fallback auf das erste Gerät
        });
    }

    public async Task<bool> OpenAndInitializeAsync(uint locId)
    {
        return await Task.Run(() =>
        {
            var status = _FTDevice.OpenByLocation(locId);
            if (status != FTDI.FT_STATUS.FT_OK) return false;

            status |= _FTDevice.SetCharacters(0, false, 0, false);
            status |= _FTDevice.SetTimeouts(5000, 5000);
            status |= _FTDevice.SetLatency(16);
            status |= _FTDevice.SetFlowControl(FTDI.FT_FLOW_CONTROL.FT_FLOW_NONE, 0x00, 0x00);
            status |= _FTDevice.SetBitMode(0x00, 0x00);
            status |= _FTDevice.SetBitMode(0x00, 0x02); // MPSSE mode

            if (status != FTDI.FT_STATUS.FT_OK) return false;

            FlushBuffer();

            // Synchronisation Check 0xAA / 0xAB
            if (!SendAndReceiveCheck(0xAA, 0xFA, 0xAA)) return false;
            if (!SendAndReceiveCheck(0xAB, 0xFA, 0xAB)) return false;

            // MPSSE I2C Konfiguration (100 kHz Clock Divisor = 199)
            byte[] mpsseBuffer = new byte[]
            {
                0x8A, 0x97, 0x8C, 0x9E, 0x07, 0x00, 0x85,
                0x86, 199, 0x00 // Clock divisor Low/High
            };

            if (!Send_Data(mpsseBuffer)) return false;
            return FT_I2C_SetLineIdle();
        });
    }

    private bool SendAndReceiveCheck(byte cmd, byte expected1, byte expected2)
    {
        if (!Send_Data(new byte[] { cmd })) return false;
        if (!Receive_Data(2, out byte[] revBuf)) return false;
        return revBuf[0] == expected1 && revBuf[1] == expected2;
    }

    private bool Send_Data(byte[] data)
    {
        uint numBytesSent = 0;
        return _FTDevice.Write(data, data.Length, ref numBytesSent) == FTDI.FT_STATUS.FT_OK && numBytesSent == data.Length;
    }

    private bool Send_Data(byte[] data, int length)
    {
    uint NumBytesSent = 0;
    uint NumBytesToSend = (uint)length;
    // Send data. This will return once all sent or if times out
    FTDI.FT_STATUS ftStatus = _FTDevice.Write(data, NumBytesToSend, ref NumBytesSent);

    // Ensure that call completed OK and that all bytes sent as requested
    if ((NumBytesSent != NumBytesToSend) || (ftStatus != FTDI.FT_STATUS.FT_OK))
        return false;   // error   calling function can check NumBytesSent to see how many got sent
    else
        return true;   // success
    }

    private bool Receive_Data(uint bytesToRead, out byte[] data)
    {
    data = new byte[bytesToRead];
    uint bytesReceived = 0;
    var status = _FTDevice.Read(data, bytesToRead, ref bytesReceived);
    return status == FTDI.FT_STATUS.FT_OK && bytesReceived == bytesToRead;
    }

    private bool FlushBuffer()
    {
    uint bytesAvailable = 0;
    if (_FTDevice.GetRxBytesAvailable(ref bytesAvailable) != FTDI.FT_STATUS.FT_OK) return false;
    if (bytesAvailable > 0)
    {
        byte[] inputBuffer = new byte[bytesAvailable];
        uint nbRead = 0;
        _FTDevice.Read(inputBuffer, bytesAvailable, ref nbRead);
    }
    return true;
    }

    private bool FT_I2C_SetLineIdle()
    {         
    byte[] buffer = { 0x80, 0xFF, 0xFB };
    uint sent = 0;
    return _FTDevice.Write(buffer, buffer.Length, ref sent) == FTDI.FT_STATUS.FT_OK;

    }

    public async Task<bool> FT_I2C_SetLineIdleAsync()
    {         
    return await Task.Run(() =>
    {
        return FT_I2C_SetLineIdle();
    });

    }

    public async Task<bool> FT_I2C_SetStartAsync()
    {
    return await Task.Run(() =>
    {
        byte Count = 0;
        byte ADbusVal = 0;
        byte ADbusDir = 0;

        byte[] MPSSEbuffer = new byte[39];
        int NumBytesToSend = 0;
        byte GPIO_Low_Dat = 0;
        byte GPIO_Low_Dir = 0;

        // SDA lo, SCL high
        ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLhi | (GPIO_Low_Dat & 0xF8));
        ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));

        for (Count = 0; Count < 4; Count++) // Repeat commands to ensure minimum period of start setup time ie 600ns
        {
            MPSSEbuffer[NumBytesToSend++] = 0x80;       // ADbus GPIO command
            MPSSEbuffer[NumBytesToSend++] = 0xFD;       // Set data value
            MPSSEbuffer[NumBytesToSend++] = 0xFB;       // Set direction
        }

        // SDA lo, SCL lo
        ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));

        for (Count = 0; Count < 1; Count++) 
        {
            MPSSEbuffer[NumBytesToSend++] = 0x80;       // ADbus GPIO command
            MPSSEbuffer[NumBytesToSend++] = 0xFC;       // Set data value
            MPSSEbuffer[NumBytesToSend++] = 0xFB;       // Set direction
        }

        return Send_Data(MPSSEbuffer, NumBytesToSend);
    });
    }


    public async Task<bool> FT_I2C_SetStopAsync()
    {
        return await Task.Run(() =>
        {
            byte Count = 0;
            byte ADbusVal = 0;
            byte ADbusDir = 0;
            byte GPIO_Low_Dat = 0;
            byte GPIO_Low_Dir = 0;
            byte[] MPSSEbuffer = new byte[100];
            int NumBytesToSend = 0;
            // SDA low, SCL low
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));    // on FT232H lines always output

            for (Count = 0; Count < 4; Count++)
            {
                MPSSEbuffer[NumBytesToSend++] = 0x80;	    // ADbus GPIO command
                MPSSEbuffer[NumBytesToSend++] = 0xFC;   // Set data value
                MPSSEbuffer[NumBytesToSend++] = 0xFB;	// Set direction
            }

            // SDA low, SCL high
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLhi | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));    // on FT232H lines always output

            for (Count = 0; Count < 4; Count++)
            {
                MPSSEbuffer[NumBytesToSend++] = 0x80;	    // ADbus GPIO command
                MPSSEbuffer[NumBytesToSend++] = 0xFD;   // Set data value
                MPSSEbuffer[NumBytesToSend++] = 0xFB;	// Set direction
            }

            // SDA high, SCL high
            ADbusVal = (byte)(0x00 | I2C_Data_SDAhi_SCLhi | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));        // on FT232H lines always output

            for (Count = 0; Count < 4; Count++)
            {
                MPSSEbuffer[NumBytesToSend++] = 0x80;	    // ADbus GPIO command
                MPSSEbuffer[NumBytesToSend++] = 0xFF;   // Set data value
                MPSSEbuffer[NumBytesToSend++] = 0xFB;	// Set direction
            }
            if (Send_Data(MPSSEbuffer, NumBytesToSend) == false)
                return false;
            return true;
        });
        
    }

    public async Task<bool> FT_I2C_WriteAddressAsync(byte addr, bool rw)
    {
        return await Task.Run(() =>
        {
            byte[] MPSSEbuffer = new byte[100];           
            int NumBytesToSend = 0;
            byte address = (byte)(addr << 1);
            if(rw)
                address |= 0x01;
            MPSSEbuffer[NumBytesToSend++] = 0x11;        // clock data byte out
            MPSSEbuffer[NumBytesToSend++] = 0x00;                                   // 
            MPSSEbuffer[NumBytesToSend++] = 0x00;                                   //  Data length of 0x0000 means 1 byte data to clock in
            MPSSEbuffer[NumBytesToSend++] = address;           //  Byte to send         
            MPSSEbuffer[NumBytesToSend++] = 0x80;                                   // Command - set low byte
            MPSSEbuffer[NumBytesToSend++] = 0xFE;                                // Set the values
            MPSSEbuffer[NumBytesToSend++] = 0xFB;                               // Set the directions
            MPSSEbuffer[NumBytesToSend++] = 0x22;           // clock data bits in
            MPSSEbuffer[NumBytesToSend++] = 0x00;                                   //                  //  ' Send answer back immediate command

            // Set the directions
            if (Send_Data(MPSSEbuffer, NumBytesToSend) == false)
                return false;

            byte[] recv = new byte[1];
            // read back byte containing ack
            if (Receive_Data(1, out recv) == false)
                return false;
            // if ack bit is 0 then sensor acked the transfer, otherwise it nak'd the transfer
            return (recv[0] & 0x01) == 0;
            
        });        
    }

    public async Task<bool> FT_I2C_WriteByteAsync(byte data)
    {
        return await Task.Run(() =>
        {
            byte[] MPSSEbuffer = new byte[100];           
            int NumBytesToSend = 0;

            MPSSEbuffer[NumBytesToSend++] = 0x11;        // clock data byte out
            MPSSEbuffer[NumBytesToSend++] = 0x00;                                   // 
            MPSSEbuffer[NumBytesToSend++] = 0x00;                                   //  Data length of 0x0000 means 1 byte data to clock in
            MPSSEbuffer[NumBytesToSend++] = data;           //  Byte to send         
            MPSSEbuffer[NumBytesToSend++] = 0x80;                                   // Command - set low byte
            MPSSEbuffer[NumBytesToSend++] = 0xFE;                                // Set the values
            MPSSEbuffer[NumBytesToSend++] = 0xFB;                               // Set the directions
            MPSSEbuffer[NumBytesToSend++] = 0x22;           // clock data bits in
            MPSSEbuffer[NumBytesToSend++] = 0x00;                                   //                  //  ' Send answer back immediate command

            // Set the directions
            if (Send_Data(MPSSEbuffer, NumBytesToSend) == false)
                return false;

            byte[] recv = new byte[1];
            // read back byte containing ack
            if (Receive_Data(1, out recv) == false)
                return false;
            // if ack bit is 0 then sensor acked the transfer, otherwise it nak'd the transfer
            return (recv[0] & 0x01) == 0;
            
        });        
    }
    public async Task<byte> FT_I2C_ReadByteAsync(bool nak)
    {
        return await Task.Run<byte>(() =>
        {
            int NumBytesToSend = 0;
            byte[] MPSSEbuffer = new byte[100];    
            byte ack_byte = nak ? (byte)0xFF : (byte)0x00;
            MPSSEbuffer[NumBytesToSend++] = 0x20;      // Clock data byte in
            MPSSEbuffer[NumBytesToSend++] = 0x00;
            MPSSEbuffer[NumBytesToSend++] = 0x00;                               // Data length of 0x0000 means 1 byte data to clock in

            MPSSEbuffer[NumBytesToSend++] = 0x13;    // Clock data bit out
            MPSSEbuffer[NumBytesToSend++] = 0x00;                              // Length of 0 means 1 bit
            MPSSEbuffer[NumBytesToSend++] = ack_byte;                              // Sending 0 here as ACK
            // Put I2C line back to idle (during transfer) state... Clock line low, Data line high
            MPSSEbuffer[NumBytesToSend++] = 0x80;                               //       ' Command - set low byte
            MPSSEbuffer[NumBytesToSend++] = 0xFE;                            //      ' Set the values
            MPSSEbuffer[NumBytesToSend++] = 0xFB;                             //     ' Set the directions
                                                                                // This command then tells the MPSSE to send any results gathered back immediately
            MPSSEbuffer[NumBytesToSend++] = 0x87; // Send answer back immediate command
            if (!Send_Data(MPSSEbuffer, NumBytesToSend))
                return 0xFF;
            byte[] recv = new byte[1];
            // read back byte containing ack
            if (!Receive_Data(1, out recv))
                return 0xFF;
            return recv[0];
            
        });        
    }
}

