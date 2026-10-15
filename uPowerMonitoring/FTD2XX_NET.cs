/*
** FTD2XX_NET.cs
**
** Copyright © 2009-2012 Future Technology Devices International Limited
**
** C# Source file for .NET wrapper of the Windows FTD2XX.dll API calls.
** Main module
**
** Author: FTDI
** Project: CDM Windows Driver Package
** Module: FTD2XX_NET Managed Wrapper
** Requires: 
** Comments:
**
** History:
**  1.0.0	-	Initial version
**  1.0.12	-	Included support for the FT232H device.
**  1.0.14	-	Included Support for the X-Series of devices.
**  1.X.X.X -	fixed 232H-TxRx, XSERIES-TxRx, add 4233H
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

namespace FTDI
{
    // Constants for FT_STATUS
    /// <summary>
    /// Status values for FTDI devices.
    /// </summary>
    public enum FT_STATUS
    {
        /// <summary>
        /// Status OK
        /// </summary>
        FT_OK = 0,
        /// <summary>
        /// The device handle is invalid
        /// </summary>
        FT_INVALID_HANDLE,
        /// <summary>
        /// Device not found
        /// </summary>
        FT_DEVICE_NOT_FOUND,
        /// <summary>
        /// Device is not open
        /// </summary>
        FT_DEVICE_NOT_OPENED,
        /// <summary>
        /// IO error
        /// </summary>
        FT_IO_ERROR,  //4
        /// <summary>
        /// Insufficient resources
        /// </summary>
        FT_INSUFFICIENT_RESOURCES,
        /// <summary>
        /// A parameter was invalid
        /// </summary>
        FT_INVALID_PARAMETER,
        /// <summary>
        /// The requested baud rate is invalid
        /// </summary>
        FT_INVALID_BAUD_RATE,
        /// <summary>
        /// Device not opened for erase
        /// </summary>
        FT_DEVICE_NOT_OPENED_FOR_ERASE,
        /// <summary>
        /// Device not poened for write
        /// </summary>
        FT_DEVICE_NOT_OPENED_FOR_WRITE,
        /// <summary>
        /// Failed to write to device
        /// </summary>
        FT_FAILED_TO_WRITE_DEVICE, //10
        /// <summary>
        /// Failed to read the device EEPROM
        /// </summary>
        FT_EEPROM_READ_FAILED,
        /// <summary>
        /// Failed to write the device EEPROM
        /// </summary>
        FT_EEPROM_WRITE_FAILED,
        /// <summary>
        /// Failed to erase the device EEPROM
        /// </summary>
        FT_EEPROM_ERASE_FAILED,
        /// <summary>
        /// An EEPROM is not fitted to the device
        /// </summary>
        FT_EEPROM_NOT_PRESENT,
        /// <summary>
        /// Device EEPROM is blank
        /// </summary>
        FT_EEPROM_NOT_PROGRAMMED, //15
        /// <summary>
        /// Invalid arguments
        /// </summary>
        FT_INVALID_ARGS,
        /// <summary>
        /// An other error has occurred
        /// </summary>
		FT_NOT_SUPPORTED,
        FT_OTHER_ERROR,
		FT_DEVICE_LIST_NOT_READY,
	};


	public enum D2XXPINS : byte
	{
		TxD =  (1 << 0),
		RxD =  (1 << 1),
		nRTS = (1 << 2),
		nCTS = (1 << 3),
		nDTR = (1 << 4),
		nDSR = (1 << 5),
		nDCD = (1 << 6),
		nRI =  (1 << 7),
	};


	// Constants for other error states internal to this class library
	/// <summary>
	/// Error states not supported by FTD2XX DLL.
	/// </summary>
	public enum FT_ERROR
	{
		FT_NO_ERROR = 0,
		FT_INCORRECT_DEVICE,
		FT_INVALID_BITMODE,
		FT_BUFFER_SIZE
	};

		// Flags for FT_OpenEx

	public enum FT_OPENFLAGS : uint
	{
		FT_OPEN_BY_SERIAL_NUMBER = 0x00000001,
		FT_OPEN_BY_DESCRIPTION = 0x00000002,
		FT_OPEN_BY_LOCATION = 0x00000004,
	};

		// Word Lengths
		/// <summary>
		/// Permitted data bits for FTDI devices
		/// </summary>
	public class FT_DATA_BITS
		{
			/// <summary>
			/// 8 data bits
			/// </summary>
			public const byte FT_BITS_8 = 0x08;
			/// <summary>
			/// 7 data bits
			/// </summary>
			public const byte FT_BITS_7 = 0x07;
		}

		// Stop Bits
		/// <summary>
		/// Permitted stop bits for FTDI devices
		/// </summary>
	public class FT_STOP_BITS
	{
		/// <summary>
		/// 1 stop bit
		/// </summary>
		public const byte FT_STOP_BITS_1 = 0x00;
		/// <summary>
		/// 2 stop bits
		/// </summary>
		public const byte FT_STOP_BITS_2 = 0x02;
	}

	// Parity
	/// <summary>
	/// Permitted parity values for FTDI devices
	/// </summary>
	public class FT_PARITY
	{
		/// <summary>
		/// No parity
		/// </summary>
		public const byte FT_PARITY_NONE = 0x00;
		/// <summary>
		/// Odd parity
		/// </summary>
		public const byte FT_PARITY_ODD = 0x01;
		/// <summary>
		/// Even parity
		/// </summary>
		public const byte FT_PARITY_EVEN = 0x02;
		/// <summary>
		/// Mark parity
		/// </summary>
		public const byte FT_PARITY_MARK = 0x03;
		/// <summary>
		/// Space parity
		/// </summary>
		public const byte FT_PARITY_SPACE = 0x04;
	}

	// Flow Control
	/// <summary>
	/// Permitted flow control values for FTDI devices
	/// </summary>
	public class FT_FLOW_CONTROL
	{
		/// <summary>
		/// No flow control
		/// </summary>
		public const UInt16 FT_FLOW_NONE = 0x0000;
		/// <summary>
		/// RTS/CTS flow control
		/// </summary>
		public const UInt16 FT_FLOW_RTS_CTS = 0x0100;
		/// <summary>
		/// DTR/DSR flow control
		/// </summary>
		public const UInt16 FT_FLOW_DTR_DSR = 0x0200;
		/// <summary>
		/// Xon/Xoff flow control
		/// </summary>
		public const UInt16 FT_FLOW_XON_XOFF = 0x0400;
	}

	// Purge Rx and Tx buffers
	/// <summary>
	/// Purge buffer constant definitions
	/// </summary>
	public class FT_PURGE
	{
		/// <summary>
		/// Purge Rx buffer
		/// </summary>
		public const byte FT_PURGE_RX = 0x01;
		/// <summary>
		/// Purge Tx buffer
		/// </summary>
		public const byte FT_PURGE_TX = 0x02;
	}

	// Modem Status bits
	/// <summary>
	/// Modem status bit definitions
	/// </summary>
	public class FT_MODEM_STATUS
	{
		/// <summary>
		/// Clear To Send (CTS) modem status
		/// </summary>
		public const byte FT_CTS = 0x10;
		/// <summary>
		/// Data Set Ready (DSR) modem status
		/// </summary>
		public const byte FT_DSR = 0x20;
		/// <summary>
		/// Ring Indicator (RI) modem status
		/// </summary>
		public const byte FT_RI = 0x40;
		/// <summary>
		/// Data Carrier Detect (DCD) modem status
		/// </summary>
		public const byte FT_DCD = 0x80;
	}

	// Line Status bits
	/// <summary>
	/// Line status bit definitions
	/// </summary>
	public class FT_LINE_STATUS
	{
		/// <summary>
		/// Overrun Error (OE) line status
		/// </summary>
		public const byte FT_OE = 0x02;
		/// <summary>
		/// Parity Error (PE) line status
		/// </summary>
		public const byte FT_PE = 0x04;
		/// <summary>
		/// Framing Error (FE) line status
		/// </summary>
		public const byte FT_FE = 0x08;
		/// <summary>
		/// Break Interrupt (BI) line status
		/// </summary>
		public const byte FT_BI = 0x10;
	}

	// Events
	/// <summary>
	/// FTDI device event types that can be monitored
	/// </summary>
	public class FT_EVENTS
	{
		/// <summary>
		/// Event on receive character
		/// </summary>
		public const UInt32 FT_EVENT_RXCHAR = 0x00000001;
		/// <summary>
		/// Event on modem status change
		/// </summary>
		public const UInt32 FT_EVENT_MODEM_STATUS = 0x00000002;
		/// <summary>
		/// Event on line status change
		/// </summary>
		public const UInt32 FT_EVENT_LINE_STATUS = 0x00000004;
	}

	// Bit modes
	/// <summary>
	/// Permitted bit mode values for FTDI devices.  For use with SetBitMode
	/// </summary>
	public class FT_BIT_MODES
	{
		/// <summary>
		/// Reset bit mode
		/// </summary>
		public const byte FT_BIT_MODE_RESET = 0x00;
		/// <summary>
		/// Asynchronous bit-bang mode
		/// </summary>
		public const byte FT_BIT_MODE_ASYNC_BITBANG = 0x01;
		/// <summary>
		/// MPSSE bit mode - only available on FT2232, FT2232H, FT4232H and FT232H
		/// </summary>
		public const byte FT_BIT_MODE_MPSSE = 0x02;
		/// <summary>
		/// Synchronous bit-bang mode
		/// </summary>
		public const byte FT_BIT_MODE_SYNC_BITBANG = 0x04;
		/// <summary>
		/// MCU host bus emulation mode - only available on FT2232, FT2232H, FT4232H and FT232H
		/// </summary>
		public const byte FT_BIT_MODE_MCU_HOST = 0x08;
		/// <summary>
		/// Fast opto-isolated serial mode - only available on FT2232, FT2232H, FT4232H and FT232H
		/// </summary>
		public const byte FT_BIT_MODE_FAST_SERIAL = 0x10;
		/// <summary>
		/// CBUS bit-bang mode - only available on FT232R and FT232H
		/// </summary>
		public const byte FT_BIT_MODE_CBUS_BITBANG = 0x20;
		/// <summary>
		/// Single channel synchronous 245 FIFO mode - only available on FT2232H channel A and FT232H
		/// </summary>
		public const byte FT_BIT_MODE_SYNC_FIFO = 0x40;
	}

	// FT232R CBUS Options
	/// <summary>
	/// Available functions for the FT232R CBUS pins.  Controlled by FT232R EEPROM settings
	/// </summary>
	public class FT_232R_CBUS_OPTIONS
	{
		/// <summary>
		/// FT232R CBUS EEPROM options - Tx Data Enable
		/// </summary>
		public const byte FT_CBUS_TXDEN = 0x00;
		/// <summary>
		/// FT232R CBUS EEPROM options - Power On
		/// </summary>
		public const byte FT_CBUS_PWRON = 0x01;
		/// <summary>
		/// FT232R CBUS EEPROM options - Rx LED
		/// </summary>
		public const byte FT_CBUS_RXLED = 0x02;
		/// <summary>
		/// FT232R CBUS EEPROM options - Tx LED
		/// </summary>
		public const byte FT_CBUS_TXLED = 0x03;
		/// <summary>
		/// FT232R CBUS EEPROM options - Tx and Rx LED
		/// </summary>
		public const byte FT_CBUS_TXRXLED = 0x04;
		/// <summary>
		/// FT232R CBUS EEPROM options - Sleep
		/// </summary>
		public const byte FT_CBUS_SLEEP = 0x05;
		/// <summary>
		/// FT232R CBUS EEPROM options - 48MHz clock
		/// </summary>
		public const byte FT_CBUS_CLK48 = 0x06;
		/// <summary>
		/// FT232R CBUS EEPROM options - 24MHz clock
		/// </summary>
		public const byte FT_CBUS_CLK24 = 0x07;
		/// <summary>
		/// FT232R CBUS EEPROM options - 12MHz clock
		/// </summary>
		public const byte FT_CBUS_CLK12 = 0x08;
		/// <summary>
		/// FT232R CBUS EEPROM options - 6MHz clock
		/// </summary>
		public const byte FT_CBUS_CLK6 = 0x09;
		/// <summary>
		/// FT232R CBUS EEPROM options - IO mode
		/// </summary>
		public const byte FT_CBUS_IOMODE = 0x0A;
		/// <summary>
		/// FT232R CBUS EEPROM options - Bit-bang write strobe
		/// </summary>
		public const byte FT_CBUS_BITBANG_WR = 0x0B;
		/// <summary>
		/// FT232R CBUS EEPROM options - Bit-bang read strobe
		/// </summary>
		public const byte FT_CBUS_BITBANG_RD = 0x0C;
	}

	// FT232H CBUS Options
	/// <summary>
	/// Available functions for the FT232H CBUS pins.  Controlled by FT232H EEPROM settings
	/// </summary>
	public class FT_232H_CBUS_OPTIONS
	{
		/// <summary>
		/// FT232H CBUS EEPROM options - Tristate
		/// </summary>
		public const byte FT_CBUS_TRISTATE = 0x00;
		/// <summary>
		/// FT232H CBUS EEPROM options - Rx LED
		/// </summary>
		public const byte FT_CBUS_TXLED = 0x01;
		/// <summary>
		/// FT232H CBUS EEPROM options - Tx LED
		/// </summary>
		public const byte FT_CBUS_RXLED = 0x02;
		/// <summary>
		/// FT232H CBUS EEPROM options - Tx and Rx LED
		/// </summary>
		public const byte FT_CBUS_TXRXLED = 0x03;
		/// <summary>
		/// FT232H CBUS EEPROM options - Power Enable
		/// </summary>
		public const byte FT_CBUS_PWREN = 0x04;
		/// <summary>
		/// FT232H CBUS EEPROM options - Sleep
		/// </summary>
		public const byte FT_CBUS_SLEEP = 0x05;
		/// <summary>
		/// FT232H CBUS EEPROM options - Drive pin to logic 0
		/// </summary>
		public const byte FT_CBUS_DRIVE_0 = 0x06;
		/// <summary>
		/// FT232H CBUS EEPROM options - Drive pin to logic 1
		/// </summary>
		public const byte FT_CBUS_DRIVE_1 = 0x07;
		/// <summary>
		/// FT232H CBUS EEPROM options - IO Mode
		/// </summary>
		public const byte FT_CBUS_IOMODE = 0x08;
		/// <summary>
		/// FT232H CBUS EEPROM options - Tx Data Enable
		/// </summary>
		public const byte FT_CBUS_TXDEN = 0x09;
		/// <summary>
		/// FT232H CBUS EEPROM options - 30MHz clock
		/// </summary>
		public const byte FT_CBUS_CLK30 = 0x0A;
		/// <summary>
		/// FT232H CBUS EEPROM options - 15MHz clock
		/// </summary>
		public const byte FT_CBUS_CLK15 = 0x0B;/// <summary>
											   /// FT232H CBUS EEPROM options - 7.5MHz clock
											   /// </summary>
		public const byte FT_CBUS_CLK7_5 = 0x0C;
	}

	/// <summary>
	/// Available functions for the X-Series CBUS pins.  Controlled by X-Series EEPROM settings
	/// </summary>
	public class FT_XSERIES_CBUS_OPTIONS
	{
		/// <summary>
		/// FT X-Series CBUS EEPROM options - Tristate
		/// </summary>
		public const byte FT_CBUS_TRISTATE = 0x00;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - RxLED#
		/// </summary>
		public const byte FT_CBUS_TXLED = 0x01;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - TxLED#
		/// </summary>
		public const byte FT_CBUS_RXLED = 0x02;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - TxRxLED
		/// </summary>
		public const byte FT_CBUS_TXRXLED = 0x03;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - PwrEn#
		/// </summary>
		public const byte FT_CBUS_PWREN = 0x04;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - Sleep#
		/// </summary>
		public const byte FT_CBUS_SLEEP = 0x05;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - Drive_0
		/// </summary>
		public const byte FT_CBUS_Drive_0 = 0x06;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - Drive_1
		/// </summary>
		public const byte FT_CBUS_Drive_1 = 0x07;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - GPIO
		/// </summary>
		public const byte FT_CBUS_IOMODE = 0x08;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - TxdEn
		/// </summary>
		public const byte FT_CBUS_TXDEN = 0x09;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - Clk24MHz
		/// </summary>
		public const byte FT_CBUS_CLK24MHz = 0x0A;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - Clk12MHz
		/// </summary>
		public const byte FT_CBUS_CLK12MHz = 0x0B;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - Clk6MHz
		/// </summary>
		public const byte FT_CBUS_CLK6MHz = 0x0C;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - BCD_Charger
		/// </summary>
		public const byte FT_CBUS_BCD_Charger = 0x0D;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - BCD_Charger#
		/// </summary>
		public const byte FT_CBUS_BCD_Charger_N = 0x0E;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - I2C_TXE#
		/// </summary>
		public const byte FT_CBUS_I2C_TXE = 0x0F;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - I2C_RXF#
		/// </summary>
		public const byte FT_CBUS_I2C_RXF = 0x10;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - VBUS_Sense
		/// </summary>
		public const byte FT_CBUS_VBUS_Sense = 0x11;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - BitBang_WR#
		/// </summary>
		public const byte FT_CBUS_BitBang_WR = 0x12;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - BitBang_RD#
		/// </summary>
		public const byte FT_CBUS_BitBang_RD = 0x13;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - Time_Stampe
		/// </summary>
		public const byte FT_CBUS_Time_Stamp = 0x14;
		/// <summary>
		/// FT X-Series CBUS EEPROM options - Keep_Awake#
		/// </summary>
		public const byte FT_CBUS_Keep_Awake = 0x15;
	}

	// Flag values for FT_GetDeviceInfoDetail and FT_GetDeviceInfo
	/// <summary>
	/// Flags that provide information on the FTDI device state
	/// </summary>
	public class FT_FLAGS
	{
		/// <summary>
		/// Indicates that the device is open
		/// </summary>
		public const UInt32 FT_FLAGS_OPENED = 0x00000001;
		/// <summary>
		/// Indicates that the device is enumerated as a hi-speed USB device
		/// </summary>
		public const UInt32 FT_FLAGS_HISPEED = 0x00000002;
	}

	// Valid drive current values for FT2232H, FT4232H and FT232H devices
	/// <summary>
	/// Valid values for drive current options on FT2232H, FT4232H and FT232H devices.
	/// </summary>
	public class FT_DRIVE_CURRENT
	{
		/// <summary>
		/// 4mA drive current
		/// </summary>
		public const byte FT_DRIVE_CURRENT_4MA = 4;
		/// <summary>
		/// 8mA drive current
		/// </summary>
		public const byte FT_DRIVE_CURRENT_8MA = 8;
		/// <summary>
		/// 12mA drive current
		/// </summary>
		public const byte FT_DRIVE_CURRENT_12MA = 12;
		/// <summary>
		/// 16mA drive current
		/// </summary>
		public const byte FT_DRIVE_CURRENT_16MA = 16;
	}

	// Device type identifiers for FT_GetDeviceInfoDetail and FT_GetDeviceInfo
	/// <summary>
	/// List of FTDI device types
	// </summary>FT_DEVICE
	public enum FT_DEVICETYPE
	{
		FT_DEVICE_BM = 0,
		FT_DEVICE_AM,
		FT_DEVICE_100AX,
		FT_DEVICE_UNKNOWN,
		FT_DEVICE_2232C,
		FT_DEVICE_232R,
		FT_DEVICE_2232H,
		FT_DEVICE_4232H,
		FT_DEVICE_232H,
		FT_DEVICE_X_SERIES,
		FT_DEVICE_4222H_0,
		FT_DEVICE_4222H_1_2,
		FT_DEVICE_4222H_3,
		FT_DEVICE_4222_PROG,
		FT_DEVICE_900,
		FT_DEVICE_930,
		FT_DEVICE_UMFTPD3A,         //16
		FT_DEVICE_2233H,
		FT_DEVICE_4233HP,           //18
		FT_DEVICE_2232HP,
		FT_DEVICE_4232HP,
		FT_DEVICE_233HP,
		FT_DEVICE_232HP,
		FT_DEVICE_2232HA,
		FT_DEVICE_4232HA,
		FT_DEVICE_232RV2,
	};

	/// <summary>
	/// Type that holds device information for GetDeviceInformation method.
	/// Used with FT_GetDeviceInfo and FT_GetDeviceInfoDetail in FTD2XX.DLL
	/// </summary>
	public class FT_DEVICE_INFO_NODE
	{
		/// <summary>
		/// Indicates device state.  Can be any combination of the following: FT_FLAGS_OPENED, FT_FLAGS_HISPEED
		/// </summary>
		public UInt32 Flags;
		/// <summary>
		/// Indicates the device type.  Can be one of the following: FT_DEVICE_232R, FT_DEVICE_2232C, FT_DEVICE_BM, FT_DEVICE_AM, FT_DEVICE_100AX or FT_DEVICE_UNKNOWN
		/// </summary>
		public FT_DEVICETYPE Type;
		/// <summary>
		/// The Vendor ID and Product ID of the device
		/// </summary>
		public UInt32 ID;
		/// <summary>
		/// The physical location identifier of the device
		/// </summary>
		public UInt32 LocId;
		/// <summary>
		/// The device serial number
		/// </summary>
		public string SerialNumber;
		/// <summary>
		/// The device description
		/// </summary>
		public string Description;
		/// <summary>
		/// The device handle.  This value is not used externally and is provided for information only.
		/// If the device is not open, this value is 0.
		/// </summary>
		public IntPtr ftHandle;
	}

	// Internal structure for reading and writing EEPROM contents
	// NOTE:  NEED Pack=1 for byte alignment!  Without this, data is garbage
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public class FT_PROGRAM_DATA
	{
		public UInt32 Signature1;
		public UInt32 Signature2;
		public UInt32 Version;
		public UInt16 VendorID;
		public UInt16 ProductID;

		public IntPtr Manufacturer;
		public IntPtr ManufacturerID;
		public IntPtr Description;
		public IntPtr SerialNumber;

		public UInt16 MaxPower;
		public UInt16 PnP;
		public UInt16 SelfPowered;
		public UInt16 RemoteWakeup;
		// FT232B extensions
		public byte Rev4;
		public byte IsoIn;
		public byte IsoOut;
		public byte PullDownEnable;
		public byte SerNumEnable;
		public byte USBVersionEnable;
		public UInt16 USBVersion;
		// FT2232D extensions
		public byte Rev5;
		public byte IsoInA;
		public byte IsoInB;
		public byte IsoOutA;
		public byte IsoOutB;
		public byte PullDownEnable5;
		public byte SerNumEnable5;
		public byte USBVersionEnable5;
		public UInt16 USBVersion5;
		public byte AIsHighCurrent;
		public byte BIsHighCurrent;
		public byte IFAIsFifo;
		public byte IFAIsFifoTar;
		public byte IFAIsFastSer;
		public byte AIsVCP;
		public byte IFBIsFifo;
		public byte IFBIsFifoTar;
		public byte IFBIsFastSer;
		public byte BIsVCP;
		// FT232R extensions
		public byte UseExtOsc;
		public byte HighDriveIOs;
		public byte EndpointSize;
		public byte PullDownEnableR;
		public byte SerNumEnableR;
		public byte InvertTXD;          // non-zero if invert TXD
		public byte InvertRXD;          // non-zero if invert RXD
		public byte InvertRTS;          // non-zero if invert RTS
		public byte InvertCTS;          // non-zero if invert CTS
		public byte InvertDTR;          // non-zero if invert DTR
		public byte InvertDSR;          // non-zero if invert DSR
		public byte InvertDCD;          // non-zero if invert DCD
		public byte InvertRI;           // non-zero if invert RI
		public byte Cbus0;              // Cbus Mux control - Ignored for FT245R
		public byte Cbus1;              // Cbus Mux control - Ignored for FT245R
		public byte Cbus2;              // Cbus Mux control - Ignored for FT245R
		public byte Cbus3;              // Cbus Mux control - Ignored for FT245R
		public byte Cbus4;              // Cbus Mux control - Ignored for FT245R
		public byte RIsD2XX;            // Default to loading VCP
										// FT2232H extensions
		public byte PullDownEnable7;
		public byte SerNumEnable7;
		public byte ALSlowSlew;         // non-zero if AL pins have slow slew
		public byte ALSchmittInput;     // non-zero if AL pins are Schmitt input
		public byte ALDriveCurrent;     // valid values are 4mA, 8mA, 12mA, 16mA
		public byte AHSlowSlew;         // non-zero if AH pins have slow slew
		public byte AHSchmittInput;     // non-zero if AH pins are Schmitt input
		public byte AHDriveCurrent;     // valid values are 4mA, 8mA, 12mA, 16mA
		public byte BLSlowSlew;         // non-zero if BL pins have slow slew
		public byte BLSchmittInput;     // non-zero if BL pins are Schmitt input
		public byte BLDriveCurrent;     // valid values are 4mA, 8mA, 12mA, 16mA
		public byte BHSlowSlew;         // non-zero if BH pins have slow slew
		public byte BHSchmittInput;     // non-zero if BH pins are Schmitt input
		public byte BHDriveCurrent;     // valid values are 4mA, 8mA, 12mA, 16mA
		public byte IFAIsFifo7;         // non-zero if interface is 245 FIFO
		public byte IFAIsFifoTar7;      // non-zero if interface is 245 FIFO CPU target
		public byte IFAIsFastSer7;      // non-zero if interface is Fast serial
		public byte AIsVCP7;            // non-zero if interface is to use VCP drivers
		public byte IFBIsFifo7;         // non-zero if interface is 245 FIFO
		public byte IFBIsFifoTar7;      // non-zero if interface is 245 FIFO CPU target
		public byte IFBIsFastSer7;      // non-zero if interface is Fast serial
		public byte BIsVCP7;            // non-zero if interface is to use VCP drivers
		public byte PowerSaveEnable;    // non-zero if using BCBUS7 to save power for self-powered designs
										// FT4232H extensions
		public byte PullDownEnable8;
		public byte SerNumEnable8;
		public byte ASlowSlew;          // non-zero if AL pins have slow slew
		public byte ASchmittInput;      // non-zero if AL pins are Schmitt input
		public byte ADriveCurrent;      // valid values are 4mA, 8mA, 12mA, 16mA
		public byte BSlowSlew;          // non-zero if AH pins have slow slew
		public byte BSchmittInput;      // non-zero if AH pins are Schmitt input
		public byte BDriveCurrent;      // valid values are 4mA, 8mA, 12mA, 16mA
		public byte CSlowSlew;          // non-zero if BL pins have slow slew
		public byte CSchmittInput;      // non-zero if BL pins are Schmitt input
		public byte CDriveCurrent;      // valid values are 4mA, 8mA, 12mA, 16mA
		public byte DSlowSlew;          // non-zero if BH pins have slow slew
		public byte DSchmittInput;      // non-zero if BH pins are Schmitt input
		public byte DDriveCurrent;      // valid values are 4mA, 8mA, 12mA, 16mA
		public byte ARIIsTXDEN;
		public byte BRIIsTXDEN;
		public byte CRIIsTXDEN;
		public byte DRIIsTXDEN;
		public byte AIsVCP8;            // non-zero if interface is to use VCP drivers
		public byte BIsVCP8;            // non-zero if interface is to use VCP drivers
		public byte CIsVCP8;            // non-zero if interface is to use VCP drivers
		public byte DIsVCP8;            // non-zero if interface is to use VCP drivers
										// FT232H extensions
		public byte PullDownEnableH;    // non-zero if pull down enabled
		public byte SerNumEnableH;      // non-zero if serial number to be used
		public byte ACSlowSlewH;        // non-zero if AC pins have slow slew
		public byte ACSchmittInputH;    // non-zero if AC pins are Schmitt input
		public byte ACDriveCurrentH;    // valid values are 4mA, 8mA, 12mA, 16mA
		public byte ADSlowSlewH;        // non-zero if AD pins have slow slew
		public byte ADSchmittInputH;    // non-zero if AD pins are Schmitt input
		public byte ADDriveCurrentH;    // valid values are 4mA, 8mA, 12mA, 16mA
		public byte Cbus0H;             // Cbus Mux control
		public byte Cbus1H;             // Cbus Mux control
		public byte Cbus2H;             // Cbus Mux control
		public byte Cbus3H;             // Cbus Mux control
		public byte Cbus4H;             // Cbus Mux control
		public byte Cbus5H;             // Cbus Mux control
		public byte Cbus6H;             // Cbus Mux control
		public byte Cbus7H;             // Cbus Mux control
		public byte Cbus8H;             // Cbus Mux control
		public byte Cbus9H;             // Cbus Mux control
		public byte IsFifoH;            // non-zero if interface is 245 FIFO
		public byte IsFifoTarH;         // non-zero if interface is 245 FIFO CPU target
		public byte IsFastSerH;         // non-zero if interface is Fast serial
		public byte IsFT1248H;          // non-zero if interface is FT1248
		public byte FT1248CpolH;        // FT1248 clock polarity
		public byte FT1248LsbH;         // FT1248 data is LSB (1) or MSB (0)
		public byte FT1248FlowControlH; // FT1248 flow control enable
		public byte IsVCPH;             // non-zero if interface is to use VCP drivers
		public byte PowerSaveEnableH;   // non-zero if using ACBUS7 to save power for self-powered designs
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	struct FT_EEPROM_HEADER
	{
		public UInt32 deviceType;       // FTxxxx device type to be programmed
										// Device descriptor options
		public UInt16 VendorId;             // 0x0403
		public UInt16 ProductId;                // 0x6001
		public byte SerNumEnable;           // non-zero if serial number to be used
											// Config descriptor options
		public UInt16 MaxPower;             // 0 < MaxPower <= 500
		public byte SelfPowered;            // 0 = bus powered, 1 = self powered
		public byte RemoteWakeup;           // 0 = not capable, 1 = capable
											// Hardware options
		public byte PullDownEnable;     // non-zero if pull down in suspend enabled
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	struct FT_XSERIES_DATA
	{
		public FT_EEPROM_HEADER common;

		public byte ACSlowSlew;         // non-zero if AC bus pins have slow slew
		public byte ACSchmittInput;     // non-zero if AC bus pins are Schmitt input
		public byte ACDriveCurrent;     // valid values are 4mA, 8mA, 12mA, 16mA
		public byte ADSlowSlew;         // non-zero if AD bus pins have slow slew
		public byte ADSchmittInput;     // non-zero if AD bus pins are Schmitt input
		public byte ADDriveCurrent;     // valid values are 4mA, 8mA, 12mA, 16mA
										// CBUS options
		public byte Cbus0;              // Cbus Mux control
		public byte Cbus1;              // Cbus Mux control
		public byte Cbus2;              // Cbus Mux control
		public byte Cbus3;              // Cbus Mux control
		public byte Cbus4;              // Cbus Mux control
		public byte Cbus5;              // Cbus Mux control
		public byte Cbus6;              // Cbus Mux control
										// UART signal options
		public byte InvertTXD;          // non-zero if invert TXD
		public byte InvertRXD;          // non-zero if invert RXD
		public byte InvertRTS;          // non-zero if invert RTS
		public byte InvertCTS;          // non-zero if invert CTS
		public byte InvertDTR;          // non-zero if invert DTR
		public byte InvertDSR;          // non-zero if invert DSR
		public byte InvertDCD;          // non-zero if invert DCD
		public byte InvertRI;               // non-zero if invert RI
											// Battery Charge Detect options
		public byte BCDEnable;          // Enable Battery Charger Detection
		public byte BCDForceCbusPWREN;  // asserts the power enable signal on CBUS when charging port detected
		public byte BCDDisableSleep;        // forces the device never to go into sleep mode
											// I2C options
		public UInt16 I2CSlaveAddress;      // I2C slave device address
		public UInt32 I2CDeviceId;          // I2C device ID
		public byte I2CDisableSchmitt;  // Disable I2C Schmitt trigger
										// FT1248 options
		public byte FT1248Cpol;         // FT1248 clock polarity - clock idle high (1) or clock idle low (0)
		public byte FT1248Lsb;          // FT1248 data is LSB (1) or MSB (0)
		public byte FT1248FlowControl;  // FT1248 flow control enable
										// Hardware options
		public byte RS485EchoSuppress;  // 
		public byte PowerSaveEnable;        // 
											// Driver option
		public byte DriverType;         // 
	}
		
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	struct STRUCT_EEPROM_FT2232H
	{
		// Common header
		public FT_EEPROM_HEADER common;	// common elements for all device EEPROMs
		// Drive options
		public Byte ALSlowSlew;			// non-zero if AL pins have slow slew
		public Byte ALSchmittInput;		// non-zero if AL pins are Schmitt input
		public Byte ALDriveCurrent;		// valid values are 4mA, 8mA, 12mA, 16mA
		public Byte AHSlowSlew;			// non-zero if AH pins have slow slew
		public Byte AHSchmittInput;		// non-zero if AH pins are Schmitt input
		public Byte AHDriveCurrent;		// valid values are 4mA, 8mA, 12mA, 16mA
		public Byte BLSlowSlew;			// non-zero if BL pins have slow slew
		public Byte BLSchmittInput;		// non-zero if BL pins are Schmitt input
		public Byte BLDriveCurrent;		// valid values are 4mA, 8mA, 12mA, 16mA
		public Byte BHSlowSlew;			// non-zero if BH pins have slow slew
		public Byte BHSchmittInput;		// non-zero if BH pins are Schmitt input
		public Byte BHDriveCurrent;		// valid values are 4mA, 8mA, 12mA, 16mA
		// Hardware options
		public Byte AIsFifo;				// non-zero if interface is 245 FIFO
		public Byte AIsFifoTar;			// non-zero if interface is 245 FIFO CPU target
		public Byte AIsFastSer;			// non-zero if interface is Fast serial
		public Byte BIsFifo;				// non-zero if interface is 245 FIFO
		public Byte BIsFifoTar;			// non-zero if interface is 245 FIFO CPU target
		public Byte BIsFastSer;			// non-zero if interface is Fast serial
		public Byte PowerSaveEnable;		// non-zero if using BCBUS7 to save power for self-powered designs
		// Driver option
		public Byte ADriverType;			// non-zero if interface is to use VCP drivers
		public Byte BDriverType;			// non-zero if interface is to use VCP drivers
	}


	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	struct STRUCT_EEPROM_FT4232H
	{
		// Common header
		public FT_EEPROM_HEADER common;    // common elements for all device EEPROMs
									// Drive options
		public Byte ASlowSlew;            // non-zero if A pins have slow slew
		public Byte ASchmittInput;        // non-zero if A pins are Schmitt input
		public Byte ADriveCurrent;        // valid values are 4mA, 8mA, 12mA, 16mA
		public Byte BSlowSlew;            // non-zero if B pins have slow slew
		public Byte BSchmittInput;        // non-zero if B pins are Schmitt input
		public Byte BDriveCurrent;        // valid values are 4mA, 8mA, 12mA, 16mA
		public Byte CSlowSlew;            // non-zero if C pins have slow slew
		public Byte CSchmittInput;        // non-zero if C pins are Schmitt input
		public Byte CDriveCurrent;        // valid values are 4mA, 8mA, 12mA, 16mA
		public Byte DSlowSlew;            // non-zero if D pins have slow slew
		public Byte DSchmittInput;        // non-zero if D pins are Schmitt input
		public Byte DDriveCurrent;        // valid values are 4mA, 8mA, 12mA, 16mA
										  // Hardware options
		public Byte ARIIsTXDEN;           // non-zero if port A uses RI as RS485 TXDEN
		public Byte BRIIsTXDEN;           // non-zero if port B uses RI as RS485 TXDEN
		public Byte CRIIsTXDEN;           // non-zero if port C uses RI as RS485 TXDEN
		public Byte DRIIsTXDEN;           // non-zero if port D uses RI as RS485 TXDEN
										  // Driver option
		public Byte ADriverType;          // 
		public Byte BDriverType;          // 
		public Byte CDriverType;          // 
		public Byte DDriverType;          // 
	}

	struct STRUCT_EEPROM_FT4233H
    {
		public STRUCT_EEPROM_FT4232H ft4232h;
		public FT_EEPROM_PD pd;
	}
	
	struct STRUCT_EEPROM_FT2232HP
	{
		public STRUCT_EEPROM_FT2232H ft2232h;
		public FT_EEPROM_PD pd;
	}
	
	struct STRUCT_EEPROM_FT2233HP
	{
		public STRUCT_EEPROM_FT2232H ft2232h;
		public FT_EEPROM_PD pd;
	}
	
	struct STRUCT_EEPROM_FT4232HP
    {
		public STRUCT_EEPROM_FT4232H ft4232h;
		public FT_EEPROM_PD pd;
	}

	struct STRUCT_EEPROM_FT232HP
    {
		// Common header
		public FT_EEPROM_HEADER common;    // common elements for all device EEPROMs
									// Drive options
		public Byte ACSlowSlew;           // non-zero if AC bus pins have slow slew
		public Byte ACSchmittInput;       // non-zero if AC bus pins are Schmitt input
		public Byte ACDriveCurrent;       // valid values are 4mA, 8mA, 12mA, 16mA
		public Byte ADSlowSlew;           // non-zero if AD bus pins have slow slew
		public Byte ADSchmittInput;       // non-zero if AD bus pins are Schmitt input
		public Byte ADDriveCurrent;       // valid values are 4mA, 8mA, 12mA, 16mA
										  // CBUS options
		public Byte Cbus0;                // Cbus Mux control
		public Byte Cbus1;                // Cbus Mux control
		public Byte Cbus2;                // Cbus Mux control
		public Byte Cbus3;                // Cbus Mux control
		public Byte Cbus4;                // Cbus Mux control
		public Byte Cbus5;                // Cbus Mux control
		public Byte Cbus6;                // Cbus Mux control
		public Byte Cbus7;                // Cbus Mux control
		public Byte Cbus8;                // Cbus Mux control
		public Byte Cbus9;                // Cbus Mux control
										  // FT1248 options
		public Byte FT1248Cpol;           // FT1248 clock polarity - clock idle high (1) or clock idle low (0)
		public Byte FT1248Lsb;            // FT1248 data is LSB (1) or MSB (0)
		public Byte FT1248FlowControl;    // FT1248 flow control enable
										  // Hardware options
		public Byte IsFifo;               // non-zero if interface is 245 FIFO
		public Byte IsFifoTar;            // non-zero if interface is 245 FIFO CPU target
		public Byte IsFastSer;            // non-zero if interface is Fast serial
		public Byte IsFT1248;         // non-zero if interface is FT1248
		public Byte PowerSaveEnable;      // 
										  // Driver option
		public Byte DriverType;           // non-zero if interface is to use VCP drivers
	}

	struct STRUCT_EEPROM_FT233HP
	{
		public STRUCT_EEPROM_FT232HP ft232h;
		public FT_EEPROM_PD pd;
	}


	// Power Delivery structures for use with FT_EEPROM_Read and FT_EEPROM_Program
	// PDO Configuration structure, mA supported values 0 to 10230mA, mV supported values 0 to 51100mV
	// This is part of the FT_EEPROM_PD structure.
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	struct FT_EEPROM_PD_PDO_mv_ma
	{
		public UInt16 PDO1ma;  // PDO1 mA
		public UInt16 PDO1mv;  // PDO1 mV
		public UInt16 PDO2ma;  // PDO2 mA
		public UInt16 PDO2mv;  // PDO2 mV
		public UInt16 PDO3ma;  // PDO3 mA
		public UInt16 PDO3mv;  // PDO3 mV
		public UInt16 PDO4ma;  // PDO4 mA
		public UInt16 PDO4mv;  // PDO4 mV
		public UInt16 PDO5ma;  // PDO5 mA
		public UInt16 PDO5mv;  // PDO5 mV
		public UInt16 PDO6ma;  // PDO6 mA
		public UInt16 PDO6mv;  // PDO6 mV
		public UInt16 PDO7ma;  // PDO7 mA
		public UInt16 PDO7mv;  // PDO7 mV
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	unsafe struct FT_EEPROM_PD
	{
		// Configuration
		public Byte srprs;        // non-zero to enable Sink Request Power Role Swap 
		public Byte sraprs;       // non-zero to enable Sink Accept PR Swap
		public Byte srrprs;       // non-zero to enable Source Request PR SWAP
		public Byte saprs;        // non-zero to enable Source Accept PR SWAP
		public Byte vconns;       // non-zero to enable vConn Swap
		public Byte passthru;     // non-zero to enable Pass Through
		public Byte extmcu;       // non-zero to enable External MCU
		public Byte pd2en;        // non-zero to enable PD2
		public Byte pd1autoclk;   // non-zero to enable PD1 Auto Clock
		public Byte pd2autoclk;   // non-zero to enable PD2 Auto Clock
		public Byte useefuse;     // non-zero to Use EFUSE
		public Byte extvconn;     // non-zero to enable External vConn

		// GPIO Configuration
		public Byte count;        // GPIO Count, supported values are 0 to 7 
		public Byte gpio1;        // GPIO Number 1, supported values are 0 to 7, 15 for N/A
		public Byte gpio2;        // GPIO Number 2, supported values are 0 to 7, 15 for N/A
		public Byte gpio3;        // GPIO Number 3, supported values are 0 to 7, 15 for N/A
		public Byte gpio4;        // GPIO Number 4, supported values are 0 to 7, 15 for N/A
		public Byte gpio5;        // GPIO Number 5, supported values are 0 to 7, 15 for N/A
		public Byte gpio6;        // GPIO Number 6, supported values are 0 to 7, 15 for N/A
		public Byte gpio7;        // GPIO Number 7, supported values are 0 to 7, 15 for N/A
		public Byte pd1lden;      // PD1 Load Enable, supported values are 0 to 7, 15 for N/A
		public Byte pd2lden;      // PD2 Load Enable, supported values are 0 to 7, 15 for N/A
		public Byte dispin;       // Discharge Pin, supported values are 0 to 7, 15 for N/A
		public Byte disenbm;      // Discharge Enable BM, 0 for "Drive Hi", 1 for "Drive Low", 2 for "Input Mode", 3 for "Don't Care"
		public Byte disdisbm;     // Discharge Disable BM, 0 for "Drive Hi", 1 for "Drive Low", 2 for "Input Mode", 3 for "Don't Care"
		public Byte ccselect;   // CC Select Indicator, supports device GPIO values

		// ISET Configuration
		public Byte iset1;        // ISET1, supported values are 0 to 7, 15 for N/A
		public Byte iset2;        // ISET2, supported values are 0 to 7, 15 for N/A
		public Byte iset3;        // ISET3, supported values are 0 to 7, 15 for N/A
		public Byte extiset;      // non-zero to enable EXTEND_ISET
		public Byte isetpd2;      // non-zero to enable ISET_PD2
		public Byte iseten;       // non-zero to set ISET_ENABLED

		// BM Configuration, 0 for "Drive Hi", 1 for "Drive Low", 2 for "Input Mode", 3 for "Don't Care"

		public fixed Byte PDO1_GPIO[7];     // PDO1 GPIO1 to GPIO7
        public fixed Byte PDO2_GPIO[7];     // PDO2 GPIO1 to GPIO7
		public fixed Byte PDO3_GPIO[7];     // PDO3 GPIO1 to GPIO7
		public fixed Byte PDO4_GPIO[7];     // PDO4 GPIO1 to GPIO7
		public fixed Byte PDO5_GPIO[7];     // PDO5 GPIO1 to GPIO7
		public fixed Byte PDO6_GPIO[7];     // PDO6 GPIO1 to GPIO7
		public fixed Byte PDO7_GPIO[7];     // PDO7 GPIO1 to GPIO7
		public fixed Byte VSET0V_GPIO[7];   // PDO7 GPIO1 to GPIO7
		public fixed Byte VSAFE5V_GPIO[7];  // PDO7 GPIO1 to GPIO7

		public FT_EEPROM_PD_PDO_mv_ma BM_PDO_Sink;
		public FT_EEPROM_PD_PDO_mv_ma BM_PDO_Source;
		public FT_EEPROM_PD_PDO_mv_ma BM_PDO_Sink_2;

		// PD Timers
		public Byte srt;          // Sender Response Timer
		public Byte hrt;          // Hard Reset Timer
		public Byte sct;          // Source Capability Timer
		public Byte dit;          // Discover Identity Timer
		public UInt16 srcrt;       // Source Recover Timer
		public UInt16 trt;         // Transition Timer
		public UInt16 sofft;       // Source off timer
		public UInt16 nrt;         // No Response Timer
		public UInt16 swct;        // Sink Wait Capability Timer
		public UInt16 snkrt;       // Sink Request Timer
		public Byte dt;           // Discharge Timer
		public Byte cnst;         // Chunk not supported timer
		public UInt16 it;          // Idle Timer

		// PD Control
		public Byte i2caddr;      // I2C Address (hex)
		public UInt32 prou;          // Power Reserved for OWN use
		public UInt32 trim1;         // TRIM1
		public UInt32 trim2;         // TRIM2
		public Byte extdc;        // non-zero to enable ETERNAL_DC_POWER
    }

	public class FT_EEPROM_DATA
	{
		//private const UInt32 Signature1     = 0x00000000;
		//private const UInt32 Signature2     = 0xFFFFFFFF;
		//private const UInt32 Version        = 0x00000002;
		/// <summary>
		/// Vendor ID as supplied by the USB Implementers Forum
		/// </summary>
		public UInt16 VendorID = 0x0403;
		/// <summary>
		/// Product ID
		/// </summary>
		public UInt16 ProductID = 0x6001;
		/// <summary>
		/// Manufacturer name string
		/// </summary>
		public string Manufacturer = "FTDI";
		/// <summary>
		/// Manufacturer name abbreviation to be used as a prefix for automatically generated serial numbers
		/// </summary>
		public string ManufacturerID = "FT";
		/// <summary>
		/// Device description string
		/// </summary>
		public string Description = "USB-Serial Converter";
		/// <summary>
		/// Device serial number string
		/// </summary>
		public string SerialNumber = "";
		/// <summary>
		/// Maximum power the device needs
		/// </summary>
		public UInt16 MaxPower = 0x0090;
		//private bool PnP                    = true;
		/// <summary>
		/// Indicates if the device has its own power supply (self-powered) or gets power from the USB port (bus-powered)
		/// </summary>
		public bool SelfPowered = false;
		/// <summary>
		/// Determines if the device can wake the host PC from suspend by toggling the RI line
		/// </summary>
		public bool RemoteWakeup = false;
	}

	// EEPROM class for FT232B and FT245B
	/// <summary>
	/// EEPROM structure specific to FT232B and FT245B devices.
	/// Inherits from FT_EEPROM_DATA.
	/// </summary>
	public class FT232B_EEPROM_STRUCTURE : FT_EEPROM_DATA
	{
		//private bool Rev4                   = true;
		//private bool IsoIn                  = false;
		//private bool IsoOut                 = false;
		/// <summary>
		/// Determines if IOs are pulled down when the device is in suspend
		/// </summary>
		public bool PullDownEnable = false;
		/// <summary>
		/// Determines if the serial number is enabled
		/// </summary>
		public bool SerNumEnable = true;
		/// <summary>
		/// Determines if the USB version number is enabled
		/// </summary>
		public bool USBVersionEnable = true;
		/// <summary>
		/// The USB version number.  Should be either 0x0110 (USB 1.1) or 0x0200 (USB 2.0)
		/// </summary>
		public UInt16 USBVersion = 0x0200;
	}

	// EEPROM class for FT2232C, FT2232L and FT2232D
	/// <summary>
	/// EEPROM structure specific to FT2232 devices.
	/// Inherits from FT_EEPROM_DATA.
	/// </summary>
	public class FT2232_EEPROM_STRUCTURE : FT_EEPROM_DATA
	{
		//private bool Rev5                   = true;
		//private bool IsoInA                 = false;
		//private bool IsoInB                 = false;
		//private bool IsoOutA                = false;
		//private bool IsoOutB                = false;
		/// <summary>
		/// Determines if IOs are pulled down when the device is in suspend
		/// </summary>
		public bool PullDownEnable = false;
		/// <summary>
		/// Determines if the serial number is enabled
		/// </summary>
		public bool SerNumEnable = true;
		/// <summary>
		/// Determines if the USB version number is enabled
		/// </summary>
		public bool USBVersionEnable = true;
		/// <summary>
		/// The USB version number.  Should be either 0x0110 (USB 1.1) or 0x0200 (USB 2.0)
		/// </summary>
		public UInt16 USBVersion = 0x0200;
		/// <summary>
		/// Enables high current IOs on channel A
		/// </summary>
		public bool AIsHighCurrent = false;
		/// <summary>
		/// Enables high current IOs on channel B
		/// </summary>
		public bool BIsHighCurrent = false;
		/// <summary>
		/// Determines if channel A is in FIFO mode
		/// </summary>
		public bool IFAIsFifo = false;
		/// <summary>
		/// Determines if channel A is in FIFO target mode
		/// </summary>
		public bool IFAIsFifoTar = false;
		/// <summary>
		/// Determines if channel A is in fast serial mode
		/// </summary>
		public bool IFAIsFastSer = false;
		/// <summary>
		/// Determines if channel A loads the VCP driver
		/// </summary>
		public bool AIsVCP = true;
		/// <summary>
		/// Determines if channel B is in FIFO mode
		/// </summary>
		public bool IFBIsFifo = false;
		/// <summary>
		/// Determines if channel B is in FIFO target mode
		/// </summary>
		public bool IFBIsFifoTar = false;
		/// <summary>
		/// Determines if channel B is in fast serial mode
		/// </summary>
		public bool IFBIsFastSer = false;
		/// <summary>
		/// Determines if channel B loads the VCP driver
		/// </summary>
		public bool BIsVCP = true;
	}

	// EEPROM class for FT232R and FT245R
	/// <summary>
	/// EEPROM structure specific to FT232R and FT245R devices.
	/// Inherits from FT_EEPROM_DATA.
	/// </summary>
	public class FT232R_EEPROM_STRUCTURE : FT_EEPROM_DATA
	{
		/// <summary>
		/// Disables the FT232R internal clock source.  
		/// If the device has external oscillator enabled it must have an external oscillator fitted to function
		/// </summary>
		public bool UseExtOsc = false;
		/// <summary>
		/// Enables high current IOs
		/// </summary>
		public bool HighDriveIOs = false;
		/// <summary>
		/// Sets the endpoint size.  This should always be set to 64
		/// </summary>
		public byte EndpointSize = 64;
		/// <summary>
		/// Determines if IOs are pulled down when the device is in suspend
		/// </summary>
		public bool PullDownEnable = false;
		/// <summary>
		/// Determines if the serial number is enabled
		/// </summary>
		public bool SerNumEnable = true;
		/// <summary>
		/// Inverts the sense of the TXD line
		/// </summary>
		public bool InvertTXD = false;
		/// <summary>
		/// Inverts the sense of the RXD line
		/// </summary>
		public bool InvertRXD = false;
		/// <summary>
		/// Inverts the sense of the RTS line
		/// </summary>
		public bool InvertRTS = false;
		/// <summary>
		/// Inverts the sense of the CTS line
		/// </summary>
		public bool InvertCTS = false;
		/// <summary>
		/// Inverts the sense of the DTR line
		/// </summary>
		public bool InvertDTR = false;
		/// <summary>
		/// Inverts the sense of the DSR line
		/// </summary>
		public bool InvertDSR = false;
		/// <summary>
		/// Inverts the sense of the DCD line
		/// </summary>
		public bool InvertDCD = false;
		/// <summary>
		/// Inverts the sense of the RI line
		/// </summary>
		public bool InvertRI = false;
		/// <summary>
		/// Sets the function of the CBUS0 pin for FT232R devices.
		/// Valid values are FT_CBUS_TXDEN, FT_CBUS_PWRON , FT_CBUS_RXLED, FT_CBUS_TXLED, 
		/// FT_CBUS_TXRXLED, FT_CBUS_SLEEP, FT_CBUS_CLK48, FT_CBUS_CLK24, FT_CBUS_CLK12, 
		/// FT_CBUS_CLK6, FT_CBUS_IOMODE, FT_CBUS_BITBANG_WR, FT_CBUS_BITBANG_RD
		/// </summary>
		public byte Cbus0 = FT_232R_CBUS_OPTIONS.FT_CBUS_SLEEP;
		/// <summary>
		/// Sets the function of the CBUS1 pin for FT232R devices.
		/// Valid values are FT_CBUS_TXDEN, FT_CBUS_PWRON , FT_CBUS_RXLED, FT_CBUS_TXLED, 
		/// FT_CBUS_TXRXLED, FT_CBUS_SLEEP, FT_CBUS_CLK48, FT_CBUS_CLK24, FT_CBUS_CLK12, 
		/// FT_CBUS_CLK6, FT_CBUS_IOMODE, FT_CBUS_BITBANG_WR, FT_CBUS_BITBANG_RD
		/// </summary>
		public byte Cbus1 = FT_232R_CBUS_OPTIONS.FT_CBUS_SLEEP;
		/// <summary>
		/// Sets the function of the CBUS2 pin for FT232R devices.
		/// Valid values are FT_CBUS_TXDEN, FT_CBUS_PWRON , FT_CBUS_RXLED, FT_CBUS_TXLED, 
		/// FT_CBUS_TXRXLED, FT_CBUS_SLEEP, FT_CBUS_CLK48, FT_CBUS_CLK24, FT_CBUS_CLK12, 
		/// FT_CBUS_CLK6, FT_CBUS_IOMODE, FT_CBUS_BITBANG_WR, FT_CBUS_BITBANG_RD
		/// </summary>
		public byte Cbus2 = FT_232R_CBUS_OPTIONS.FT_CBUS_SLEEP;
		/// <summary>
		/// Sets the function of the CBUS3 pin for FT232R devices.
		/// Valid values are FT_CBUS_TXDEN, FT_CBUS_PWRON , FT_CBUS_RXLED, FT_CBUS_TXLED, 
		/// FT_CBUS_TXRXLED, FT_CBUS_SLEEP, FT_CBUS_CLK48, FT_CBUS_CLK24, FT_CBUS_CLK12, 
		/// FT_CBUS_CLK6, FT_CBUS_IOMODE, FT_CBUS_BITBANG_WR, FT_CBUS_BITBANG_RD
		/// </summary>
		public byte Cbus3 = FT_232R_CBUS_OPTIONS.FT_CBUS_SLEEP;
		/// <summary>
		/// Sets the function of the CBUS4 pin for FT232R devices.
		/// Valid values are FT_CBUS_TXDEN, FT_CBUS_PWRON , FT_CBUS_RXLED, FT_CBUS_TXLED, 
		/// FT_CBUS_TXRXLED, FT_CBUS_SLEEP, FT_CBUS_CLK48, FT_CBUS_CLK24, FT_CBUS_CLK12, 
		/// FT_CBUS_CLK6
		/// </summary>
		public byte Cbus4 = FT_232R_CBUS_OPTIONS.FT_CBUS_SLEEP;
		/// <summary>
		/// Determines if the VCP driver is loaded
		/// </summary>
		public bool RIsD2XX = false;
	}

	// EEPROM class for FT2232H
	/// <summary>
	/// EEPROM structure specific to FT2232H devices.
	/// Inherits from FT_EEPROM_DATA.
	/// </summary>
	public class FT2232H_EEPROM_STRUCTURE : FT_EEPROM_DATA
	{
		/// <summary>
		/// Determines if IOs are pulled down when the device is in suspend
		/// </summary>
		public bool PullDownEnable = false;
		/// <summary>
		/// Determines if the serial number is enabled
		/// </summary>
		public bool SerNumEnable = true;
		/// <summary>
		/// Determines if AL pins have a slow slew rate
		/// </summary>
		public bool ALSlowSlew = false;
		/// <summary>
		/// Determines if the AL pins have a Schmitt input
		/// </summary>
		public bool ALSchmittInput = false;
		/// <summary>
		/// Determines the AL pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
		/// </summary>
		public byte ALDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;
		/// <summary>
		/// Determines if AH pins have a slow slew rate
		/// </summary>
		public bool AHSlowSlew = false;
		/// <summary>
		/// Determines if the AH pins have a Schmitt input
		/// </summary>
		public bool AHSchmittInput = false;
		/// <summary>
		/// Determines the AH pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
		/// </summary>
		public byte AHDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;
		/// <summary>
		/// Determines if BL pins have a slow slew rate
		/// </summary>
		public bool BLSlowSlew = false;
		/// <summary>
		/// Determines if the BL pins have a Schmitt input
		/// </summary>
		public bool BLSchmittInput = false;
		/// <summary>
		/// Determines the BL pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
		/// </summary>
		public byte BLDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;
		/// <summary>
		/// Determines if BH pins have a slow slew rate
		/// </summary>
		public bool BHSlowSlew = false;
		/// <summary>
		/// Determines if the BH pins have a Schmitt input
		/// </summary>
		public bool BHSchmittInput = false;
		/// <summary>
		/// Determines the BH pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
		/// </summary>
		public byte BHDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;
		/// <summary>
		/// Determines if channel A is in FIFO mode
		/// </summary>
		public bool IFAIsFifo = false;
		/// <summary>
		/// Determines if channel A is in FIFO target mode
		/// </summary>
		public bool IFAIsFifoTar = false;
		/// <summary>
		/// Determines if channel A is in fast serial mode
		/// </summary>
		public bool IFAIsFastSer = false;
		/// <summary>
		/// Determines if channel A loads the VCP driver
		/// </summary>
		public bool AIsVCP = true;
		/// <summary>
		/// Determines if channel B is in FIFO mode
		/// </summary>
		public bool IFBIsFifo = false;
		/// <summary>
		/// Determines if channel B is in FIFO target mode
		/// </summary>
		public bool IFBIsFifoTar = false;
		/// <summary>
		/// Determines if channel B is in fast serial mode
		/// </summary>
		public bool IFBIsFastSer = false;
		/// <summary>
		/// Determines if channel B loads the VCP driver
		/// </summary>
		public bool BIsVCP = true;
		/// <summary>
		/// For self-powered designs, keeps the FT2232H in low power state until BCBUS7 is high
		/// </summary>
		public bool PowerSaveEnable = false;
	}

	// EEPROM class for FT4232H
	/// <summary>
	/// EEPROM structure specific to FT4232H devices.
	/// Inherits from FT_EEPROM_DATA.
	/// </summary>
	public class FT4232H_EEPROM_STRUCTURE : FT_EEPROM_DATA
	{
		/// <summary>
		/// Determines if IOs are pulled down when the device is in suspend
		/// </summary>
		public bool PullDownEnable = false;
		/// <summary>
		/// Determines if the serial number is enabled
		/// </summary>
		public bool SerNumEnable = true;
		/// <summary>
		/// Determines if A pins have a slow slew rate
		/// </summary>
		public bool ASlowSlew = false;
		/// <summary>
		/// Determines if the A pins have a Schmitt input
		/// </summary>
		public bool ASchmittInput = false;
		/// <summary>
		/// Determines the A pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
		/// </summary>
		public byte ADriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;
		/// <summary>
		/// Determines if B pins have a slow slew rate
		/// </summary>
		public bool BSlowSlew = false;
		/// <summary>
		/// Determines if the B pins have a Schmitt input
		/// </summary>
		public bool BSchmittInput = false;
		/// <summary>
		/// Determines the B pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
		/// </summary>
		public byte BDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;
		/// <summary>
		/// Determines if C pins have a slow slew rate
		/// </summary>
		public bool CSlowSlew = false;
		/// <summary>
		/// Determines if the C pins have a Schmitt input
		/// </summary>
		public bool CSchmittInput = false;
		/// <summary>
		/// Determines the C pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
		/// </summary>
		public byte CDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;
		/// <summary>
		/// Determines if D pins have a slow slew rate
		/// </summary>
		public bool DSlowSlew = false;
		/// <summary>
		/// Determines if the D pins have a Schmitt input
		/// </summary>
		public bool DSchmittInput = false;
		/// <summary>
		/// Determines the D pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
		/// </summary>
		public byte DDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;
		/// <summary>
		/// RI of port A acts as RS485 transmit enable (TXDEN)
		/// </summary>
		public bool ARIIsTXDEN = false;
		/// <summary>
		/// RI of port B acts as RS485 transmit enable (TXDEN)
		/// </summary>
		public bool BRIIsTXDEN = false;
		/// <summary>
		/// RI of port C acts as RS485 transmit enable (TXDEN)
		/// </summary>
		public bool CRIIsTXDEN = false;
		/// <summary>
		/// RI of port D acts as RS485 transmit enable (TXDEN)
		/// </summary>
		public bool DRIIsTXDEN = false;
		/// <summary>
		/// Determines if channel A loads the VCP driver
		/// </summary>
		public bool AIsVCP = true;
		/// <summary>
		/// Determines if channel B loads the VCP driver
		/// </summary>
		public bool BIsVCP = true;
		/// <summary>
		/// Determines if channel C loads the VCP driver
		/// </summary>
		public bool CIsVCP = true;
		/// <summary>
		/// Determines if channel D loads the VCP driver
		/// </summary>
		public bool DIsVCP = true;
	}
	public class CEEPROM_FT2233HP
	{
		public UInt16 VendorID = 0x0403;
		public UInt16 ProductID = 0x6041;
		public bool SerNumEnable = false;
		public UInt16 MaxPower = 0x0064;
		public bool SelfPowered = false;
		public bool RemoteWakeup = false;
		public bool PullDownEnable = false;

		public string Manufacturer = "FTDI";
		public string ManufacturerID = "FT";
		public string Description = "USB-FT2233HP-DUAL";
		public string SerialNumber = "";

		public byte ALSlowSlew = 0;
		public byte ALSchmittInput = 0;
		public byte ALDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;

		public byte AHSlowSlew = 0;
		public byte AHSchmittInput = 0;
		public byte AHDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;

		public byte BLSlowSlew = 0;
		public byte BLSchmittInput = 0;
		public byte BLDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;

		public byte BHSlowSlew = 0;
		public byte BHSchmittInput = 0;
		public byte BHDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;

		public byte AIsFifo = 0;
		public byte AIsFifoTar = 0;
		public byte AIsFastSer = 0;
		public byte BIsFifo = 0;
		public byte BIsFifoTar = 0;
		public byte BIsFastSer = 0;
		public byte PowerSaveEnable = 0;

		public byte ADriverType = 1;
		public byte BDriverType = 1;


		//PD
		public byte srprs = 0;
		public byte sraprs = 0;
		public byte srrprs = 0;
		public byte saprs = 0;
		public byte vconns = 0;
		public byte passthru = 0;
		public byte extmcu = 0;
		public byte pd2en = 0;
		public byte pd1autoclk = 0;
		public byte pd2autoclk = 0;
		public byte useefuse = 0;
		public byte extvconn = 0;

		public byte count = 0;
		public byte gpio1 = 0;
		public byte gpio2 = 0;
		public byte gpio3 = 0;
		public byte gpio4 = 0;
		public byte gpio5 = 0;
		public byte gpio6 = 0;
		public byte gpio7 = 0;
		public byte pd1lden = 0;
		public byte pd2lden = 0;
		public byte dispin = 0;
		public byte disenbm = 0;
		public byte disdisbm = 0;
		public byte ccselect = 0;

		public byte iset1 = 0;
		public byte iset2 = 0;
		public byte iset3 = 0;
		public byte extiset = 0;
		public byte isetpd2 = 0;
		public byte iseten = 0;

		public byte[] PDO1_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] PDO2_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] PDO3_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] PDO4_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] PDO5_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] PDO6_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] PDO7_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] VSET0V_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] VSAFE5V_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };

		public CEEPROM_PD_PDO_MV_MA BM_PDO_Sink = new CEEPROM_PD_PDO_MV_MA();
		public CEEPROM_PD_PDO_MV_MA BM_PDO_Source = new CEEPROM_PD_PDO_MV_MA();
		public CEEPROM_PD_PDO_MV_MA BM_PDO_Sink_2 = new CEEPROM_PD_PDO_MV_MA();

		public byte srt = 0;
		public byte hrt = 0;
		public byte sct = 0;
		public byte dit = 0;
		public UInt16 srcrt = 0;
		public UInt16 trt = 0;
		public UInt16 sofft = 0;
		public UInt16 nrt = 0;
		public UInt16 swct = 0;
		public UInt16 snkrt = 0;
		public byte dt = 0;
		public byte cnst = 0;
		public UInt16 it = 0;

		public byte i2caddr = 0;
		public UInt32 prou = 0;
		public UInt32 trim1 = 0;
		public UInt32 trim2 = 0;
		public byte extdc = 0;

		//PROGRAMDATA.AIsVCP == FT_EEPROM_4232H.ADriverType
	}

	public class CEEPROM_FT4233H
	{
		public UInt16 VendorID = 0x0403;
		public UInt16 ProductID = 0x6041;
		public bool SerNumEnable = false;
		public UInt16 MaxPower = 0x0064;
		public bool SelfPowered = false;
		public bool RemoteWakeup = false;
		public bool PullDownEnable = false;

		public string Manufacturer = "FTDI";
		public string ManufacturerID = "FT";
		public string Description = "USB-FT4233HP-QUAD";
		public string SerialNumber = "";

		public byte ASlowSlew = 0;
		public byte ASchmittInput = 0;
		public byte ADriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;

		public byte BSlowSlew = 0;
		public byte BSchmittInput = 0;
		public byte BDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;
		
		public byte CSlowSlew = 0;
		public byte CSchmittInput = 0;
		public byte CDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;

		public byte DSlowSlew = 0;
		public byte DSchmittInput = 0;
		public byte DDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;
		
		public byte ARIIsTXDEN = 0;
		public byte BRIIsTXDEN = 0;
		public byte CRIIsTXDEN = 0;
		public byte DRIIsTXDEN = 0;
		public byte ADriverType = 1;
		public byte BDriverType = 1;
		public byte CDriverType = 1;
		public byte DDriverType = 1;

		//PD
		public byte srprs = 0;
		public byte sraprs = 0;
		public byte srrprs = 0;
		public byte saprs = 0;
		public byte vconns = 0;
		public byte passthru = 0;
		public byte extmcu = 0;
		public byte pd2en = 0;
		public byte pd1autoclk = 0;
		public byte pd2autoclk = 0;
		public byte useefuse = 0;
		public byte extvconn = 0;

		public byte count = 0;
		public byte gpio1 = 0;
		public byte gpio2 = 0;
		public byte gpio3 = 0;
		public byte gpio4 = 0;
		public byte gpio5 = 0;
		public byte gpio6 = 0;
		public byte gpio7 = 0;
		public byte pd1lden = 0;
		public byte pd2lden = 0;
		public byte dispin = 0;
		public byte disenbm = 0;
		public byte disdisbm = 0;
		public byte ccselect = 0;

		public byte iset1 = 0;
		public byte iset2 = 0;
		public byte iset3 = 0;
		public byte extiset = 0;
		public byte isetpd2 = 0;
		public byte iseten = 0;

		public byte[] PDO1_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] PDO2_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] PDO3_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] PDO4_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] PDO5_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] PDO6_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] PDO7_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] VSET0V_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] VSAFE5V_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };

		public CEEPROM_PD_PDO_MV_MA BM_PDO_Sink = new CEEPROM_PD_PDO_MV_MA();
		public CEEPROM_PD_PDO_MV_MA BM_PDO_Source = new CEEPROM_PD_PDO_MV_MA();
		public CEEPROM_PD_PDO_MV_MA BM_PDO_Sink_2 = new CEEPROM_PD_PDO_MV_MA();

		public byte srt = 0;
		public byte hrt = 0;
		public byte sct = 0;
		public byte dit = 0;
		public UInt16 srcrt = 0;
		public UInt16 trt = 0;
		public UInt16 sofft = 0;
		public UInt16 nrt = 0;
		public UInt16 swct = 0;
		public UInt16 snkrt = 0;
		public byte dt = 0;
		public byte cnst = 0;
		public UInt16 it = 0;

		public byte i2caddr = 0;
		public UInt32 prou = 0;
		public UInt32 trim1 = 0;
		public UInt32 trim2 = 0;
		public byte extdc = 0;

		//PROGRAMDATA.AIsVCP == FT_EEPROM_4232H.ADriverType
	}

	public class CEEPROM_FT233H
	{
		public UInt16 VendorID = 0x0403;
		public UInt16 ProductID = 0x6044;
		public bool SerNumEnable = false;
		public UInt16 MaxPower = 0x0064;
		public bool SelfPowered = false;
		public bool RemoteWakeup = false;
		public bool PullDownEnable = false;

		public string Manufacturer = "FTDI";
		public string ManufacturerID = "FT";
		public string Description = "USB-FT233HP";
		public string SerialNumber = "";

		public byte ACSlowSlew = 0;
		public byte ACSchmittInput = 0;
		public byte ACDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;

		public byte ADSlowSlew = 0;
		public byte ADSchmittInput = 0;
		public byte ADDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;

		public byte Cbus0 = 0;
		public byte Cbus1 = 0;
		public byte Cbus2 = 0;
		public byte Cbus3 = 0;
		public byte Cbus4 = 0;
		public byte Cbus5 = 0;
		public byte Cbus6 = 0;
		public byte Cbus7 = 0;
		public byte Cbus8 = 0;
		public byte Cbus9 = 0;

		public byte FT1248Cpol = 0;
		public byte FT1248Lsb = 0;
		public byte FT1248FlowControl = 0;

		public byte IsFifo = 0;
		public byte IsFifoTar = 0;
		public byte IsFastSer = 0;
		public byte IsFT1248 = 0;
		public byte PowerSaveEnable = 0;

		public byte DriverType = 0;

		//PD
		public byte srprs = 0;
		public byte sraprs = 0;
		public byte srrprs = 0;
		public byte saprs = 0;
		public byte vconns = 0;
		public byte passthru = 0;
		public byte extmcu = 0;
		public byte pd2en = 0;
		public byte pd1autoclk = 0;
		public byte pd2autoclk = 0;
		public byte useefuse = 0;
		public byte extvconn = 0;

		public byte count = 0;
		public byte gpio1 = 0;
		public byte gpio2 = 0;
		public byte gpio3 = 0;
		public byte gpio4 = 0;
		public byte gpio5 = 0;
		public byte gpio6 = 0;
		public byte gpio7 = 0;
		public byte pd1lden = 0;
		public byte pd2lden = 0;
		public byte dispin = 0;
		public byte disenbm = 0;
		public byte disdisbm = 0;
		public byte ccselect = 0;

		public byte iset1 = 0;
		public byte iset2 = 0;
		public byte iset3 = 0;
		public byte extiset = 0;
		public byte isetpd2 = 0;
		public byte iseten = 0;

		public byte[] PDO1_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] PDO2_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] PDO3_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] PDO4_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] PDO5_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] PDO6_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] PDO7_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] VSET0V_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };
		public byte[] VSAFE5V_GPIO = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };

		public CEEPROM_PD_PDO_MV_MA BM_PDO_Sink = new CEEPROM_PD_PDO_MV_MA();
		public CEEPROM_PD_PDO_MV_MA BM_PDO_Source = new CEEPROM_PD_PDO_MV_MA();
		public CEEPROM_PD_PDO_MV_MA BM_PDO_Sink_2 = new CEEPROM_PD_PDO_MV_MA();

		public byte srt = 0;
		public byte hrt = 0;
		public byte sct = 0;
		public byte dit = 0;
		public UInt16 srcrt = 0;
		public UInt16 trt = 0;
		public UInt16 sofft = 0;
		public UInt16 nrt = 0;
		public UInt16 swct = 0;
		public UInt16 snkrt = 0;
		public byte dt = 0;
		public byte cnst = 0;
		public UInt16 it = 0;

		public byte i2caddr = 0;
		public UInt32 prou = 0;
		public UInt32 trim1 = 0;
		public UInt32 trim2 = 0;
		public byte extdc = 0;
	}


	public class CEEPROM_PD_PDO_MV_MA
    {
		public UInt16 PDO1ma = 0;
		public UInt16 PDO1mv = 0;
		public UInt16 PDO2ma = 0;
		public UInt16 PDO2mv = 0;
		public UInt16 PDO3ma = 0;
		public UInt16 PDO3mv = 0;
		public UInt16 PDO4ma = 0;
		public UInt16 PDO4mv = 0;
		public UInt16 PDO5ma = 0;
		public UInt16 PDO5mv = 0;
		public UInt16 PDO6ma = 0;
		public UInt16 PDO6mv = 0;
		public UInt16 PDO7ma = 0;
		public UInt16 PDO7mv = 0;
	}

	// EEPROM class for FT232H
	/// <summary>
	/// EEPROM structure specific to FT232H devices.
	/// Inherits from FT_EEPROM_DATA.
	/// </summary>
	public class FT232H_EEPROM_STRUCTURE : FT_EEPROM_DATA
		{
			/// <summary>
			/// Determines if IOs are pulled down when the device is in suspend
			/// </summary>
			public bool PullDownEnable = false;
			/// <summary>
			/// Determines if the serial number is enabled
			/// </summary>
			public bool SerNumEnable = true;
			/// <summary>
			/// Determines if AC pins have a slow slew rate
			/// </summary>
			public bool ACSlowSlew = false;
			/// <summary>
			/// Determines if the AC pins have a Schmitt input
			/// </summary>
			public bool ACSchmittInput = false;
			/// <summary>
			/// Determines the AC pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
			/// </summary>
			public byte ACDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;
			/// <summary>
			/// Determines if AD pins have a slow slew rate
			/// </summary>
			public bool ADSlowSlew = false;
			/// <summary>
			/// Determines if the AD pins have a Schmitt input
			/// </summary>
			public bool ADSchmittInput = false;
			/// <summary>
			/// Determines the AD pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
			/// </summary>
			public byte ADDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;
			/// <summary>
			/// Sets the function of the CBUS0 pin for FT232H devices.
			/// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
			/// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TXDEN, FT_CBUS_CLK30,
			/// FT_CBUS_CLK15, FT_CBUS_CLK7_5
			/// </summary>
			public byte Cbus0 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;
			/// <summary>
			/// Sets the function of the CBUS1 pin for FT232H devices.
			/// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
			/// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TXDEN, FT_CBUS_CLK30,
			/// FT_CBUS_CLK15, FT_CBUS_CLK7_5
			/// </summary>
			public byte Cbus1 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;
			/// <summary>
			/// Sets the function of the CBUS2 pin for FT232H devices.
			/// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
			/// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TXDEN
			/// </summary>
			public byte Cbus2 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;
			/// <summary>
			/// Sets the function of the CBUS3 pin for FT232H devices.
			/// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
			/// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TXDEN
			/// </summary>
			public byte Cbus3 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;
			/// <summary>
			/// Sets the function of the CBUS4 pin for FT232H devices.
			/// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
			/// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TXDEN
			/// </summary>
			public byte Cbus4 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;
			/// <summary>
			/// Sets the function of the CBUS5 pin for FT232H devices.
			/// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
			/// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_IOMODE,
			/// FT_CBUS_TXDEN, FT_CBUS_CLK30, FT_CBUS_CLK15, FT_CBUS_CLK7_5
			/// </summary>
			public byte Cbus5 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;
			/// <summary>
			/// Sets the function of the CBUS6 pin for FT232H devices.
			/// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
			/// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_IOMODE,
			/// FT_CBUS_TXDEN, FT_CBUS_CLK30, FT_CBUS_CLK15, FT_CBUS_CLK7_5
			/// </summary>
			public byte Cbus6 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;
			/// <summary>
			/// Sets the function of the CBUS7 pin for FT232H devices.
			/// Valid values are FT_CBUS_TRISTATE
			/// </summary>
			public byte Cbus7 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;
			/// <summary>
			/// Sets the function of the CBUS8 pin for FT232H devices.
			/// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
			/// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_IOMODE,
			/// FT_CBUS_TXDEN, FT_CBUS_CLK30, FT_CBUS_CLK15, FT_CBUS_CLK7_5
			/// </summary>
			public byte Cbus8 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;
			/// <summary>
			/// Sets the function of the CBUS9 pin for FT232H devices.
			/// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
			/// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_IOMODE,
			/// FT_CBUS_TXDEN, FT_CBUS_CLK30, FT_CBUS_CLK15, FT_CBUS_CLK7_5
			/// </summary>
			public byte Cbus9 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;
			/// <summary>
			/// Determines if the device is in FIFO mode
			/// </summary>
			public bool IsFifo = false;
			/// <summary>
			/// Determines if the device is in FIFO target mode
			/// </summary>
			public bool IsFifoTar = false;
			/// <summary>
			/// Determines if the device is in fast serial mode
			/// </summary>
			public bool IsFastSer = false;
			/// <summary>
			/// Determines if the device is in FT1248 mode
			/// </summary>
			public bool IsFT1248 = false;
			/// <summary>
			/// Determines FT1248 mode clock polarity
			/// </summary>
			public bool FT1248Cpol = false;
			/// <summary>
			/// Determines if data is ent MSB (0) or LSB (1) in FT1248 mode
			/// </summary>
			public bool FT1248Lsb = false;
			/// <summary>
			/// Determines if FT1248 mode uses flow control
			/// </summary>
			public bool FT1248FlowControl = false;
			/// <summary>
			/// Determines if the VCP driver is loaded
			/// </summary>
			public bool IsVCP = true;
			/// <summary>
			/// For self-powered designs, keeps the FT232H in low power state until ACBUS7 is high
			/// </summary>
			public bool PowerSaveEnable = false;
		}

	

	/// <summary>
	/// EEPROM structure specific to X-Series devices.
	/// Inherits from FT_EEPROM_DATA.
	/// </summary>
	public class FT_XSERIES_EEPROM_STRUCTURE : FT_EEPROM_DATA
	{
		/// <summary>
		/// Determines if IOs are pulled down when the device is in suspend
		/// </summary>
		public bool PullDownEnable = false;
		/// <summary>
		/// Determines if the serial number is enabled
		/// </summary>
		public bool SerNumEnable = true;
		/// <summary>
		/// Determines if the USB version number is enabled
		/// </summary>
		public bool USBVersionEnable = true;
		/// <summary>
		/// The USB version number: 0x0200 (USB 2.0)
		/// </summary>
		public UInt16 USBVersion = 0x0200;
		/// <summary>
		/// Determines if AC pins have a slow slew rate
		/// </summary>
		public byte ACSlowSlew;
		/// <summary>
		/// Determines if the AC pins have a Schmitt input
		/// </summary>
		public byte ACSchmittInput;
		/// <summary>
		/// Determines the AC pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
		/// </summary>
		public byte ACDriveCurrent;
		/// <summary>
		/// Determines if AD pins have a slow slew rate
		/// </summary>
		public byte ADSlowSlew;
		/// <summary>
		/// Determines if AD pins have a schmitt input
		/// </summary>
		public byte ADSchmittInput;
		/// <summary>
		/// Determines the AD pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
		/// </summary>
		public byte ADDriveCurrent;
		/// <summary>
		/// Sets the function of the CBUS0 pin for FT232H devices.
		/// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
		/// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_GPIO, FT_CBUS_TXDEN, FT_CBUS_CLK24,
		/// FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
		/// FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
		/// </summary>
		public byte Cbus0;
		/// <summary>
		/// Sets the function of the CBUS1 pin for FT232H devices.
		/// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
		/// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_GPIO, FT_CBUS_TXDEN, FT_CBUS_CLK24,
		/// FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
		/// FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
		/// </summary>
		public byte Cbus1;
		/// <summary>
		/// Sets the function of the CBUS2 pin for FT232H devices.
		/// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
		/// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_GPIO, FT_CBUS_TXDEN, FT_CBUS_CLK24,
		/// FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
		/// FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
		/// </summary>
		public byte Cbus2;
		/// <summary>
		/// Sets the function of the CBUS3 pin for FT232H devices.
		/// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
		/// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_GPIO, FT_CBUS_TXDEN, FT_CBUS_CLK24,
		/// FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
		/// FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
		/// </summary>
		public byte Cbus3;
		/// <summary>
		/// Sets the function of the CBUS4 pin for FT232H devices.
		/// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
		/// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TXDEN, FT_CBUS_CLK24,
		/// FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
		/// FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
		/// </summary>
		public byte Cbus4;
		/// <summary>
		/// Sets the function of the CBUS5 pin for FT232H devices.
		/// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
		/// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TXDEN, FT_CBUS_CLK24,
		/// FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
		/// FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
		/// </summary>
		public byte Cbus5;
		/// <summary>
		/// Sets the function of the CBUS6 pin for FT232H devices.
		/// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
		/// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TXDEN, FT_CBUS_CLK24,
		/// FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
		/// FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
		/// </summary>
		public byte Cbus6;
		/// <summary>
		/// Inverts the sense of the TXD line
		/// </summary>
		public byte InvertTXD;
		/// <summary>
		/// Inverts the sense of the RXD line
		/// </summary>
		public byte InvertRXD;
		/// <summary>
		/// Inverts the sense of the RTS line
		/// </summary>
		public byte InvertRTS;
		/// <summary>
		/// Inverts the sense of the CTS line
		/// </summary>
		public byte InvertCTS;
		/// <summary>
		/// Inverts the sense of the DTR line
		/// </summary>
		public byte InvertDTR;
		/// <summary>
		/// Inverts the sense of the DSR line
		/// </summary>
		public byte InvertDSR;
		/// <summary>
		/// Inverts the sense of the DCD line
		/// </summary>
		public byte InvertDCD;
		/// <summary>
		/// Inverts the sense of the RI line
		/// </summary>
		public byte InvertRI;
		/// <summary>
		/// Determines whether the Battery Charge Detection option is enabled.
		/// </summary>
		public byte BCDEnable;
		/// <summary>
		/// Asserts the power enable signal on CBUS when charging port detected.
		/// </summary>
		public byte BCDForceCbusPWREN;
		/// <summary>
		/// Forces the device never to go into sleep mode.
		/// </summary>
		public byte BCDDisableSleep;
		/// <summary>
		/// I2C slave device address.
		/// </summary>
		public ushort I2CSlaveAddress;
		/// <summary>
		/// I2C device ID
		/// </summary>
		public UInt32 I2CDeviceId;
		/// <summary>
		/// Disable I2C Schmitt trigger.
		/// </summary>
		public byte I2CDisableSchmitt;
		/// <summary>
		/// FT1248 clock polarity - clock idle high (1) or clock idle low (0)
		/// </summary>
		public byte FT1248Cpol;
		/// <summary>
		/// FT1248 data is LSB (1) or MSB (0)
		/// </summary>
		public byte FT1248Lsb;
		/// <summary>
		/// FT1248 flow control enable.
		/// </summary>
		public byte FT1248FlowControl;
		/// <summary>
		/// Enable RS485 Echo Suppression
		/// </summary>
		public byte RS485EchoSuppress;
		/// <summary>
		/// Enable Power Save mode.
		/// </summary>
		public byte PowerSaveEnable;
		/// <summary>
		/// Determines whether the VCP driver is loaded.
		/// </summary>
		public byte IsVCP;
	}

	/// <summary>
	/// Exceptions thrown by errors within the FTDI class.
	/// </summary>
	[global::System.Serializable]
	public class FT_EXCEPTION : Exception
	{
		/// <summary>
		/// 
		/// </summary>
		public FT_EXCEPTION() { }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public FT_EXCEPTION(string message) : base(message) { }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="inner"></param>
		public FT_EXCEPTION(string message, Exception inner) : base(message, inner) { }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected FT_EXCEPTION(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

	public class FTDILib
	{
		private static FTDILib coInstance = null;
		private static Mutex ftdiLibMtx = new Mutex();

		public static FTDILib Instance
		{
			get
			{
				if (coInstance == null)
				{
					if (ftdiLibMtx.WaitOne(10000))
					{
						try
						{
							if (coInstance == null)
								coInstance = new FTDILib();
							return coInstance;
						}
						finally
						{
							ftdiLibMtx.ReleaseMutex();
						}
					}

					return null;
				}

				return coInstance;
			}
		}


#if LINUX

        [DllImport("libftd2xx.so")]
        private static extern IntPtr dlopen(string filename, int flags);

        [DllImport("libftd2xx.so")]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("libftd2xx.so")]
        private static extern int dlclose(IntPtr handle);

        private const int RTLD_LAZY = 1;

        public static IntPtr LoadLibrary(string lpFileName)
        {
            IntPtr handle = dlopen(lpFileName, RTLD_LAZY);
            if (handle == IntPtr.Zero)
            {
                Console.WriteLine("LoadLibrary handle is null");
                throw new PlatformNotSupportedException("LoadLibrary handle is null");
            }
            return handle;
        }	

        public static IntPtr GetProcAddress(IntPtr hModule, string lpProcName)
        {
            IntPtr symbol = dlsym(hModule, lpProcName);
            if (symbol == IntPtr.Zero)
            {
                Console.WriteLine("GetProcAddress symbol is null");
                throw new PlatformNotSupportedException("GetProcAddress symbol is null");
            }
            return symbol;
        }

        public static bool FreeLibrary(IntPtr hModule)
        {
            return dlclose(hModule) == 0;
        }
#endif
#if MACOS

        [DllImport("libftd2xx.dylib")]
        private static extern IntPtr dlopen(string filename, int flags);

        [DllImport("libftd2xx.dylib")]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("libftd2xx.dylib")]
        private static extern int dlclose(IntPtr handle);

        private const int RTLD_LAZY = 1;

        public static IntPtr LoadLibrary(string lpFileName)
        {
            IntPtr handle = dlopen(lpFileName, RTLD_LAZY);
            if (handle == IntPtr.Zero)
            {
                Console.WriteLine("LoadLibrary handle is null");
                throw new PlatformNotSupportedException("LoadLibrary handle is null");
            }
            return handle;
        }	

        public static IntPtr GetProcAddress(IntPtr hModule, string lpProcName)
        {
            IntPtr symbol = dlsym(hModule, lpProcName);
            if (symbol == IntPtr.Zero)
            {
                Console.WriteLine("GetProcAddress symbol is null");
                throw new PlatformNotSupportedException("GetProcAddress symbol is null");
            }
            return symbol;
        }

        public static bool FreeLibrary(IntPtr hModule)
        {
            return dlclose(hModule) == 0;
        }
#endif
		private FTDILib()
		{
			string fullPath = "";
			string libraryPathEv = Environment.GetEnvironmentVariable("LIBRARY_PATH");
			if (string.IsNullOrEmpty(libraryPathEv))
			{
				Console.WriteLine("LIBRARY_PATH environment variable is not set.");
			}
			Console.WriteLine($"Environment variable libraryPathEv: {libraryPathEv}");

#if LINUX || MACOS

            if (hFTD2XXDLL == IntPtr.Zero)
			{
				string libraryPath = "";
				
				// Determine the library name based on the platform
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					// Linux platform
					libraryPath = "libftd2xx.so";
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					// macOS platform
					libraryPath = "libftd2xx.dylib";
				}
				else
				{
					// Unsupported platform
					throw new PlatformNotSupportedException("Unsupported platform.");
				}
				
				// If LIBRARY_PATH is set, try loading from there
				if (!string.IsNullOrEmpty(libraryPathEv))
				{
					fullPath = Path.Combine(libraryPathEv, libraryPath);
					hFTD2XXDLL = LoadLibrary(fullPath);
					// Console.WriteLine($"LoadLibrary fullPath {fullPath}");
				}
				// Fallback: Load from local directory if not found
            	if (hFTD2XXDLL == IntPtr.Zero)
				{
					// Load the library
					string localPath = Path.GetDirectoryName(GetType().Assembly.Location);
					fullPath = Path.Combine(localPath, libraryPath);
					hFTD2XXDLL = LoadLibrary(fullPath);

					if (hFTD2XXDLL == IntPtr.Zero)
					{
						throw new ApplicationException($"Failed to load {libraryPath} from {fullPath}.. Driver D2XX not found/installed.");
					}
				}
                Console.WriteLine($"Loaded {fullPath}");
			}
#endif
#if WINDOWS
			if (hFTD2XXDLL == IntPtr.Zero)
			{
				// First, try to load from the path specified by LIBRARY_PATH on Windows
				if (!string.IsNullOrEmpty(libraryPathEv))
				{
					fullPath = Path.Combine(libraryPathEv, "FTD2XX.DLL");
					hFTD2XXDLL = LoadLibrary(fullPath);
					// Console.WriteLine($"Attempting to load from LIBRARY_PATH: {fullPath}");
					// If not found, try loading the DLL with a fallback name
					if (hFTD2XXDLL == IntPtr.Zero)
					{
						hFTD2XXDLL = LoadLibrary(@"FTD2XX.DLL");
					}
				}
				else
				{
					// Fallback: Load from local directory if not found
					if (hFTD2XXDLL == IntPtr.Zero)
					{
						string localPath = Path.GetDirectoryName(GetType().Assembly.Location);
						fullPath = Path.Combine(localPath, "ftd2xx.dll");
						hFTD2XXDLL = LoadLibrary(fullPath);

						if (hFTD2XXDLL == IntPtr.Zero)
						{
							hFTD2XXDLL = LoadLibrary(@"ftd2xx.DLL");
						}
					}
					// Console.WriteLine($"Attempting to load from Local Path: {fullPath}");
				}
				// If still not loaded, throw an exception
				if (hFTD2XXDLL == IntPtr.Zero)
				{
					throw new ApplicationException($"Failed to load {fullPath}. Driver D2XX not found/installed.");
				}

				Console.WriteLine($"Loaded {fullPath}");
			}            
#endif

#if LINUX || MACOS
			pFT_SetVIDPID = GetProcAddress(hFTD2XXDLL, "FT_SetVIDPID");
			pFT_GetVIDPID = GetProcAddress(hFTD2XXDLL, "FT_GetVIDPID");
			pFT_GetDeviceLocId = GetProcAddress(hFTD2XXDLL, "FT_GetDeviceLocId");
#endif
			// If we have succesfully loaded the library, get the function pointers set up
			// Set up our function pointers for use through our exported methods
			pFT_CreateDeviceInfoList = GetProcAddress(hFTD2XXDLL, "FT_CreateDeviceInfoList");
			pFT_GetDeviceInfoDetail = GetProcAddress(hFTD2XXDLL, "FT_GetDeviceInfoDetail");
			pFT_Open = GetProcAddress(hFTD2XXDLL, "FT_Open");
			pFT_OpenEx = GetProcAddress(hFTD2XXDLL, "FT_OpenEx");
			pFT_Close = GetProcAddress(hFTD2XXDLL, "FT_Close");
			pFT_Read = GetProcAddress(hFTD2XXDLL, "FT_Read");
			pFT_Write = GetProcAddress(hFTD2XXDLL, "FT_Write");
			pFT_GetQueueStatus = GetProcAddress(hFTD2XXDLL, "FT_GetQueueStatus");
			pFT_GetModemStatus = GetProcAddress(hFTD2XXDLL, "FT_GetModemStatus");
			pFT_GetStatus = GetProcAddress(hFTD2XXDLL, "FT_GetStatus");
			pFT_SetBaudRate = GetProcAddress(hFTD2XXDLL, "FT_SetBaudRate");
			pFT_SetDataCharacteristics = GetProcAddress(hFTD2XXDLL, "FT_SetDataCharacteristics");
			pFT_SetFlowControl = GetProcAddress(hFTD2XXDLL, "FT_SetFlowControl");
			pFT_SetDtr = GetProcAddress(hFTD2XXDLL, "FT_SetDtr");
			pFT_ClrDtr = GetProcAddress(hFTD2XXDLL, "FT_ClrDtr");
			pFT_SetRts = GetProcAddress(hFTD2XXDLL, "FT_SetRts");
			pFT_ClrRts = GetProcAddress(hFTD2XXDLL, "FT_ClrRts");
			pFT_ResetDevice = GetProcAddress(hFTD2XXDLL, "FT_ResetDevice");
			pFT_ResetPort = GetProcAddress(hFTD2XXDLL, "FT_ResetPort");
			pFT_CyclePort = GetProcAddress(hFTD2XXDLL, "FT_CyclePort");
			pFT_Rescan = GetProcAddress(hFTD2XXDLL, "FT_Rescan");
			pFT_Reload = GetProcAddress(hFTD2XXDLL, "FT_Reload");
			pFT_Purge = GetProcAddress(hFTD2XXDLL, "FT_Purge");
			pFT_SetTimeouts = GetProcAddress(hFTD2XXDLL, "FT_SetTimeouts");
			pFT_SetBreakOn = GetProcAddress(hFTD2XXDLL, "FT_SetBreakOn");
			pFT_SetBreakOff = GetProcAddress(hFTD2XXDLL, "FT_SetBreakOff");
			pFT_GetDeviceInfo = GetProcAddress(hFTD2XXDLL, "FT_GetDeviceInfo");
			pFT_SetResetPipeRetryCount = GetProcAddress(hFTD2XXDLL, "FT_SetResetPipeRetryCount");
			pFT_StopInTask = GetProcAddress(hFTD2XXDLL, "FT_StopInTask");
			pFT_RestartInTask = GetProcAddress(hFTD2XXDLL, "FT_RestartInTask");
			pFT_GetDriverVersion = GetProcAddress(hFTD2XXDLL, "FT_GetDriverVersion");
			pFT_GetLibraryVersion = GetProcAddress(hFTD2XXDLL, "FT_GetLibraryVersion");
			pFT_SetDeadmanTimeout = GetProcAddress(hFTD2XXDLL, "FT_SetDeadmanTimeout");
			pFT_SetChars = GetProcAddress(hFTD2XXDLL, "FT_SetChars");
			pFT_SetEventNotification = GetProcAddress(hFTD2XXDLL, "FT_SetEventNotification");
			pFT_GetComPortNumber = GetProcAddress(hFTD2XXDLL, "FT_GetComPortNumber");
			pFT_SetLatencyTimer = GetProcAddress(hFTD2XXDLL, "FT_SetLatencyTimer");
			pFT_GetLatencyTimer = GetProcAddress(hFTD2XXDLL, "FT_GetLatencyTimer");
			pFT_SetBitMode = GetProcAddress(hFTD2XXDLL, "FT_SetBitMode");
			pFT_GetBitMode = GetProcAddress(hFTD2XXDLL, "FT_GetBitMode");
			pFT_SetUSBParameters = GetProcAddress(hFTD2XXDLL, "FT_SetUSBParameters");
			pFT_ReadEE = GetProcAddress(hFTD2XXDLL, "FT_ReadEE");
			pFT_WriteEE = GetProcAddress(hFTD2XXDLL, "FT_WriteEE");
			pFT_EraseEE = GetProcAddress(hFTD2XXDLL, "FT_EraseEE");
			pFT_EE_UASize = GetProcAddress(hFTD2XXDLL, "FT_EE_UASize");
			pFT_EE_UARead = GetProcAddress(hFTD2XXDLL, "FT_EE_UARead");
			pFT_EE_UAWrite = GetProcAddress(hFTD2XXDLL, "FT_EE_UAWrite");
			pFT_EE_Read = GetProcAddress(hFTD2XXDLL, "FT_EE_Read");
			pFT_EE_Program = GetProcAddress(hFTD2XXDLL, "FT_EE_Program");
			pFT_EEPROM_Read = GetProcAddress(hFTD2XXDLL, "FT_EEPROM_Read");
			pFT_EEPROM_Program = GetProcAddress(hFTD2XXDLL, "FT_EEPROM_Program");

			pFT_W32_CreateFile = GetProcAddress(hFTD2XXDLL, "FT_W32_CreateFile");
			pFT_W32_CloseHandle = GetProcAddress(hFTD2XXDLL, "FT_W32_CloseHandle");
			pFT_W32_ReadFile = GetProcAddress(hFTD2XXDLL, "FT_W32_ReadFile");
			pFT_W32_WriteFile = GetProcAddress(hFTD2XXDLL, "FT_W32_WriteFile");
			pFT_W32_GetCommModemStatus = GetProcAddress(hFTD2XXDLL, "FT_W32_GetCommModemStatus");
			pFT_W32_ClearCommError = GetProcAddress(hFTD2XXDLL, "FT_W32_ClearCommError");
			pFT_W32_GetCommState = GetProcAddress(hFTD2XXDLL, "FT_W32_GetCommState");
			pFT_W32_GetCommTimeouts = GetProcAddress(hFTD2XXDLL, "FT_W32_GetCommTimeouts");
			pFT_W32_PurgeComm = GetProcAddress(hFTD2XXDLL, "FT_W32_PurgeComm");
			pFT_W32_SetCommState = GetProcAddress(hFTD2XXDLL, "FT_W32_SetCommState");
			pFT_W32_SetCommTimeouts = GetProcAddress(hFTD2XXDLL, "FT_W32_SetCommTimeouts");
		}

		~FTDILib()
		{
			// FreeLibrary here - we should only do this if we are completely finished
			FreeLibrary(hFTD2XXDLL);
			hFTD2XXDLL = IntPtr.Zero;
		}

		// Handle to our DLL - used with GetProcAddress to load all of our functions
		private IntPtr hFTD2XXDLL = IntPtr.Zero;
		// Declare pointers to each of the functions we are going to use in FT2DXX.DLL
		// These are assigned in our constructor and freed in our destructor.
#if LINUX || MACOS
		public IntPtr pFT_SetVIDPID = IntPtr.Zero;
		public IntPtr pFT_GetVIDPID = IntPtr.Zero;
		public IntPtr pFT_GetDeviceLocId = IntPtr.Zero;
#endif
		public IntPtr pFT_CreateDeviceInfoList = IntPtr.Zero;
		public IntPtr pFT_GetDeviceInfoDetail = IntPtr.Zero;
		public IntPtr pFT_Open = IntPtr.Zero;
		public IntPtr pFT_OpenEx = IntPtr.Zero;
		public IntPtr pFT_Close = IntPtr.Zero;
		public IntPtr pFT_Read = IntPtr.Zero;
		public IntPtr pFT_Write = IntPtr.Zero;
		public IntPtr pFT_GetQueueStatus = IntPtr.Zero;
		public IntPtr pFT_GetModemStatus = IntPtr.Zero;
		public IntPtr pFT_GetStatus = IntPtr.Zero;
		public IntPtr pFT_SetBaudRate = IntPtr.Zero;
		public IntPtr pFT_SetDataCharacteristics = IntPtr.Zero;
		public IntPtr pFT_SetFlowControl = IntPtr.Zero;
		public IntPtr pFT_SetDtr = IntPtr.Zero;
		public IntPtr pFT_ClrDtr = IntPtr.Zero;
		public IntPtr pFT_SetRts = IntPtr.Zero;
		public IntPtr pFT_ClrRts = IntPtr.Zero;
		public IntPtr pFT_ResetDevice = IntPtr.Zero;
		public IntPtr pFT_ResetPort = IntPtr.Zero;
		public IntPtr pFT_CyclePort = IntPtr.Zero;
		public IntPtr pFT_Rescan = IntPtr.Zero;
		public IntPtr pFT_Reload = IntPtr.Zero;
		public IntPtr pFT_Purge = IntPtr.Zero;
		public IntPtr pFT_SetTimeouts = IntPtr.Zero;
		public IntPtr pFT_SetBreakOn = IntPtr.Zero;
		public IntPtr pFT_SetBreakOff = IntPtr.Zero;
		public IntPtr pFT_GetDeviceInfo = IntPtr.Zero;
		public IntPtr pFT_SetResetPipeRetryCount = IntPtr.Zero;
		public IntPtr pFT_StopInTask = IntPtr.Zero;
		public IntPtr pFT_RestartInTask = IntPtr.Zero;
		public IntPtr pFT_GetDriverVersion = IntPtr.Zero;
		public IntPtr pFT_GetLibraryVersion = IntPtr.Zero;
		public IntPtr pFT_SetDeadmanTimeout = IntPtr.Zero;
		public IntPtr pFT_SetChars = IntPtr.Zero;
		public IntPtr pFT_SetEventNotification = IntPtr.Zero;
		public IntPtr pFT_GetComPortNumber = IntPtr.Zero;
		public IntPtr pFT_SetLatencyTimer = IntPtr.Zero;
		public IntPtr pFT_GetLatencyTimer = IntPtr.Zero;
		public IntPtr pFT_SetBitMode = IntPtr.Zero;
		public IntPtr pFT_GetBitMode = IntPtr.Zero;
		public IntPtr pFT_SetUSBParameters = IntPtr.Zero;
		public IntPtr pFT_ReadEE = IntPtr.Zero;
		public IntPtr pFT_WriteEE = IntPtr.Zero;
		public IntPtr pFT_EraseEE = IntPtr.Zero;
		public IntPtr pFT_EE_UASize = IntPtr.Zero;
		public IntPtr pFT_EE_UARead = IntPtr.Zero;
		public IntPtr pFT_EE_UAWrite = IntPtr.Zero;
		public IntPtr pFT_EE_Read = IntPtr.Zero;
		public IntPtr pFT_EE_Program = IntPtr.Zero;
		public IntPtr pFT_EEPROM_Read = IntPtr.Zero;
		public IntPtr pFT_EEPROM_Program = IntPtr.Zero;

		public IntPtr pFT_W32_CreateFile = IntPtr.Zero;
		public IntPtr pFT_W32_CloseHandle = IntPtr.Zero;
		public IntPtr pFT_W32_ReadFile = IntPtr.Zero;
		public IntPtr pFT_W32_WriteFile = IntPtr.Zero;
		public IntPtr pFT_W32_GetCommModemStatus = IntPtr.Zero;
		public IntPtr pFT_W32_ClearCommError = IntPtr.Zero;
		public IntPtr pFT_W32_GetCommState = IntPtr.Zero;
		public IntPtr pFT_W32_GetCommTimeouts = IntPtr.Zero;
		public IntPtr pFT_W32_PurgeComm = IntPtr.Zero;
		public IntPtr pFT_W32_SetCommState = IntPtr.Zero;
		public IntPtr pFT_W32_SetCommTimeouts = IntPtr.Zero;

        #if WINDOWS
        /// <summary>
        /// Built-in Windows API functions to allow us to dynamically load our own DLL.
        /// Will allow us to use old versions of the DLL that do not have all of these functions available.
        /// </summary>
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);
        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);
        #endif
	}

	public class FTDIDevice
	{
		public const Byte DBG = 1;
		public const uint DBG_BAUDRATE = 1000000;

        /*
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_CreateDeviceInfoList(ref UInt32 numdevs);

		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_Open(UInt32 index, ref IntPtr ftHandle);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_OpenEx(string devstring, UInt32 dwFlags, ref IntPtr ftHandle);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_OpenExLoc(UInt32 devloc, UInt32 dwFlags, ref IntPtr ftHandle);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_Close(IntPtr ftHandle);

		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern FT_STATUS FT_Read(IntPtr ftHandle, byte[] lpBuffer, UInt32 dwBytesToRead, ref UInt32 lpdwBytesReturned);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern FT_STATUS FT_Write(IntPtr ftHandle, byte[] lpBuffer, UInt32 dwBytesToWrite, ref UInt32 lpdwBytesWritten);

		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern FT_STATUS FT_GetQueueStatus(IntPtr ftHandle, ref UInt32 lpdwAmountInRxQueue);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_GetModemStatus(IntPtr ftHandle, ref UInt32 lpdwModemStatus);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_GetStatus(IntPtr ftHandle, ref UInt32 lpdwAmountInRxQueue, ref UInt32 lpdwAmountInTxQueue, ref UInt32 lpdwEventStatus);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_SetBaudRate(IntPtr ftHandle, UInt32 dwBaudRate);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_SetDataCharacteristics(IntPtr ftHandle, byte uWordLength, byte uStopBits, byte uParity);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_SetFlowControl(IntPtr ftHandle, UInt16 usFlowControl, byte uXon, byte uXoff);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_SetDtr(IntPtr ftHandle);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_ClrDtr(IntPtr ftHandle);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_SetRts(IntPtr ftHandle);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_ClrRts(IntPtr ftHandle);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_ResetDevice(IntPtr ftHandle);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_ResetPort(IntPtr ftHandle);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_CyclePort(IntPtr ftHandle);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_Rescan();
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_Reload(UInt16 wVID, UInt16 wPID);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_Purge(IntPtr ftHandle, UInt32 dwMask);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_SetTimeouts(IntPtr ftHandle, UInt32 dwReadTimeout, UInt32 dwWriteTimeout);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_SetBreakOn(IntPtr ftHandle);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_SetBreakOff(IntPtr ftHandle);

		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_SetResetPipeRetryCount(IntPtr ftHandle, UInt32 dwCount);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_StopInTask(IntPtr ftHandle);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_RestartInTask(IntPtr ftHandle);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_GetDriverVersion(IntPtr ftHandle, ref UInt32 lpdwDriverVersion);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_GetLibraryVersion(ref UInt32 lpdwLibraryVersion);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_SetDeadmanTimeout(IntPtr ftHandle, UInt32 dwDeadmanTimeout);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_SetChars(IntPtr ftHandle, byte uEventCh, byte uEventChEn, byte uErrorCh, byte uErrorChEn);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_SetEventNotification(IntPtr ftHandle, UInt32 dwEventMask, SafeHandle hEvent);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_GetComPortNumber(IntPtr ftHandle, ref Int32 dwComPortNumber);

		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_SetLatencyTimer(IntPtr ftHandle, byte ucLatency);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_GetLatencyTimer(IntPtr ftHandle, ref byte ucLatency);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_SetBitMode(IntPtr ftHandle, byte ucMask, byte ucMode);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_GetBitMode(IntPtr ftHandle, ref byte ucMode);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_SetUSBParameters(IntPtr ftHandle, UInt32 dwInTransferSize, UInt32 dwOutTransferSize);

		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_ReadEE(IntPtr ftHandle, UInt32 dwWordOffset, ref UInt16 lpwValue);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_WriteEE(IntPtr ftHandle, UInt32 dwWordOffset, UInt16 wValue);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_EraseEE(IntPtr ftHandle);

		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_EE_UASize(IntPtr ftHandle, ref UInt32 dwSize);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_EE_UARead(IntPtr ftHandle, byte[] pucData, Int32 dwDataLen, ref UInt32 lpdwDataRead);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_EE_UAWrite(IntPtr ftHandle, byte[] pucData, Int32 dwDataLen);

		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_EEPROM_Read(IntPtr ftHandle, IntPtr eepromData, UInt32 eepromDataSize, byte[] manufacturer, byte[] manufacturerID, byte[] description, byte[] serialnumber);
		[DllImport("ftd2xx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern FT_STATUS FT_EEPROM_Program(IntPtr ftHandle, IntPtr eepromData, UInt32 eepromDataSize, byte[] manufacturer, byte[] manufacturerID, byte[] description, byte[] serialnumber);
        */
		// constructor
		/// <summary>
		/// Constructor for the FTDI class.
		/// </summary>
		public FTDIDevice()
		{
		}

        public FTDIDevice(IntPtr handl)
        {
            ftHandle = handl;
        }

		/// <summary>
		/// Destructor for the FTDI class.
		/// </summary>
		~FTDIDevice()
		{
            /* Note: There are two ways a valid handle is acquired: From this device and the one
             * just assigned in the constructor above.
             * It is important NOT to destroy the handle assigned during construction,
             * MPSSE handles are not meant to be closed here. */
            //DoNotClose();
		}


#if LINUX || MACOS
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate FT_STATUS tFT_SetVIDPID(UInt32 dwVid, UInt32 dwPID);
		
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate FT_STATUS tFT_GetVIDPID(ref UInt32 dwVid, ref UInt32 dwPID);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate FT_STATUS tFT_GetDeviceLocId(IntPtr ftHandle,  ref UInt32 lpdwLocId);
#endif
        // Definitions for FTD2XX functions
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate FT_STATUS tFT_CreateDeviceInfoList(ref UInt32 numdevs);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate FT_STATUS tFT_GetDeviceInfoDetail(UInt32 index, ref UInt32 flags, ref FT_DEVICETYPE chiptype, ref UInt32 id, ref UInt32 locid, byte[] serialnumber, byte[] description, ref IntPtr ftHandle);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate FT_STATUS tFT_Open(UInt32 index, ref IntPtr ftHandle);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate FT_STATUS tFT_OpenEx(string devstring, UInt32 dwFlags, ref IntPtr ftHandle);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate FT_STATUS tFT_OpenExLoc(UInt32 devloc, UInt32 dwFlags, ref IntPtr ftHandle);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate FT_STATUS tFT_Close(IntPtr ftHandle);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate FT_STATUS tFT_Read(IntPtr ftHandle, byte[] lpBuffer, UInt32 dwBytesToRead, ref UInt32 lpdwBytesReturned);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate FT_STATUS tFT_Write(IntPtr ftHandle, byte[] lpBuffer, UInt32 dwBytesToWrite, ref UInt32 lpdwBytesWritten);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_GetQueueStatus(IntPtr ftHandle, ref UInt32 lpdwAmountInRxQueue);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_GetModemStatus(IntPtr ftHandle, ref UInt32 lpdwModemStatus);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_GetStatus(IntPtr ftHandle, ref UInt32 lpdwAmountInRxQueue, ref UInt32 lpdwAmountInTxQueue, ref UInt32 lpdwEventStatus);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_SetBaudRate(IntPtr ftHandle, UInt32 dwBaudRate);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_SetDataCharacteristics(IntPtr ftHandle, byte uWordLength, byte uStopBits, byte uParity);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_SetFlowControl(IntPtr ftHandle, UInt16 usFlowControl, byte uXon, byte uXoff);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_SetDtr(IntPtr ftHandle);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_ClrDtr(IntPtr ftHandle);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_SetRts(IntPtr ftHandle);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_ClrRts(IntPtr ftHandle);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_ResetDevice(IntPtr ftHandle);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_ResetPort(IntPtr ftHandle);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_CyclePort(IntPtr ftHandle);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_Rescan();
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_Reload(UInt16 wVID, UInt16 wPID);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_Purge(IntPtr ftHandle, UInt32 dwMask);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_SetTimeouts(IntPtr ftHandle, UInt32 dwReadTimeout, UInt32 dwWriteTimeout);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_SetBreakOn(IntPtr ftHandle);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_SetBreakOff(IntPtr ftHandle);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_GetDeviceInfo(IntPtr ftHandle, ref FT_DEVICETYPE pftType, ref UInt32 lpdwID, byte[] pcSerialNumber, byte[] pcDescription, IntPtr pvDummy);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_SetResetPipeRetryCount(IntPtr ftHandle, UInt32 dwCount);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_StopInTask(IntPtr ftHandle);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_RestartInTask(IntPtr ftHandle);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_GetDriverVersion(IntPtr ftHandle, ref UInt32 lpdwDriverVersion);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_GetLibraryVersion(ref UInt32 lpdwLibraryVersion);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_SetDeadmanTimeout(IntPtr ftHandle, UInt32 dwDeadmanTimeout);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_SetChars(IntPtr ftHandle, byte uEventCh, byte uEventChEn, byte uErrorCh, byte uErrorChEn);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_SetEventNotification(IntPtr ftHandle, UInt32 dwEventMask, SafeHandle hEvent);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_GetComPortNumber(IntPtr ftHandle, ref Int32 dwComPortNumber);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_SetLatencyTimer(IntPtr ftHandle, byte ucLatency);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_GetLatencyTimer(IntPtr ftHandle, ref byte ucLatency);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_SetBitMode(IntPtr ftHandle, byte ucMask, byte ucMode);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_GetBitMode(IntPtr ftHandle, ref byte ucMode);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_SetUSBParameters(IntPtr ftHandle, UInt32 dwInTransferSize, UInt32 dwOutTransferSize);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_ReadEE(IntPtr ftHandle, UInt32 dwWordOffset, ref UInt16 lpwValue);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_WriteEE(IntPtr ftHandle, UInt32 dwWordOffset, UInt16 wValue);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_EraseEE(IntPtr ftHandle);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_EE_UASize(IntPtr ftHandle, ref UInt32 dwSize);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_EE_UARead(IntPtr ftHandle, byte[] pucData, Int32 dwDataLen, ref UInt32 lpdwDataRead);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_EE_UAWrite(IntPtr ftHandle, byte[] pucData, Int32 dwDataLen);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_EE_Read(IntPtr ftHandle, FT_PROGRAM_DATA pData);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_EE_Program(IntPtr ftHandle, FT_PROGRAM_DATA pData);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_EEPROM_Read(IntPtr ftHandle, IntPtr eepromData, UInt32 eepromDataSize, byte[] manufacturer, byte[] manufacturerID, byte[] description, byte[] serialnumber);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_EEPROM_Program(IntPtr ftHandle, IntPtr eepromData, UInt32 eepromDataSize, byte[] manufacturer, byte[] manufacturerID, byte[] description, byte[] serialnumber);

		private const UInt32 FT_DEFAULT_BAUD_RATE = 9600;
		private const UInt32 FT_DEFAULT_DEADMAN_TIMEOUT = 5000;
		private const Int32 FT_COM_PORT_NOT_ASSIGNED = -1;
		private const UInt32 FT_DEFAULT_IN_TRANSFER_SIZE = 0x1000;
		private const UInt32 FT_DEFAULT_OUT_TRANSFER_SIZE = 0x1000;
		private const byte FT_DEFAULT_LATENCY = 16;
		private const UInt32 FT_DEFAULT_DEVICE_ID = 0x04036001;
		// Create private variables for the device within the class
		private IntPtr ftHandle = IntPtr.Zero;

        //**************************************************************************
		// GetNumberOfDevices
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the number of FTDI devices available.  
		/// </summary>
		/// <returns>FT_STATUS value from FT_CreateDeviceInfoList in FTD2XX.DLL</returns>
		/// <param name="devcount">The number of FTDI devices available.</param>
		public FT_STATUS GetNumberOfDevices(ref UInt32 devcount)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

			tFT_CreateDeviceInfoList FT_CreateDeviceInfoList = (tFT_CreateDeviceInfoList)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_CreateDeviceInfoList, typeof(tFT_CreateDeviceInfoList));

            Console.WriteLine($"GetNumberOfDevices {devcount}");
            // Call FT_CreateDeviceInfoList
            ftStatus = FT_CreateDeviceInfoList(ref devcount);

            Console.WriteLine("FT_CreateDeviceInfoList "+ ftStatus);
            return ftStatus;
		}


		//**************************************************************************
		// GetDeviceList
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets information on all of the FTDI devices available.  
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetDeviceInfoDetail in FTD2XX.DLL</returns>
		/// <param name="devicelist">An array of type FT_DEVICE_INFO_NODE to contain the device information for all available devices.</param>
		/// <exception cref="FT_EXCEPTION">Thrown when the supplied buffer is not large enough to contain the device info list.</exception>
        public FT_STATUS GetDeviceList(FT_DEVICE_INFO_NODE[] devicelist)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;
            FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

            UInt32 devcount = 0;

            tFT_CreateDeviceInfoList FT_CreateDeviceInfoList = (tFT_CreateDeviceInfoList)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_CreateDeviceInfoList, typeof(tFT_CreateDeviceInfoList));
            tFT_GetDeviceInfoDetail FT_GetDeviceInfoDetail = (tFT_GetDeviceInfoDetail)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_GetDeviceInfoDetail, typeof(tFT_GetDeviceInfoDetail));

            // Call FT_CreateDeviceInfoList
            ftStatus = FT_CreateDeviceInfoList(ref devcount);

            // Allocate the required storage for our list

            byte[] sernum = new byte[32];
            byte[] desc = new byte[64];

            if (devcount > 0)
            {
                // Check the size of the buffer passed in is big enough
                if (devicelist.Length < devcount)
                {
                    // Buffer not big enough
                    ftErrorCondition = FT_ERROR.FT_BUFFER_SIZE;
                    // Throw exception
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                // Instantiate the array elements as FT_DEVICE_INFO_NODE
                for (UInt32 i = 0; i < devcount; i++)
                {
                    devicelist[i] = new FT_DEVICE_INFO_NODE();
                    // Call FT_GetDeviceInfoDetail
                    ftStatus = FT_GetDeviceInfoDetail(i, ref devicelist[i].Flags, ref devicelist[i].Type, ref devicelist[i].ID, ref devicelist[i].LocId, sernum, desc, ref devicelist[i].ftHandle);
                    // Convert byte arrays to strings
                    devicelist[i].SerialNumber = Encoding.ASCII.GetString(sernum);
                    devicelist[i].Description = Encoding.ASCII.GetString(desc);

                    // Find the null terminator and trim the strings
                    int serialNumberNullIndex = Array.IndexOf(sernum, (byte)0);
                    int descriptionNullIndex = Array.IndexOf(desc, (byte)0);

                    if (serialNumberNullIndex >= 0)
                    {
                        devicelist[i].SerialNumber = devicelist[i].SerialNumber.Substring(0, serialNumberNullIndex);
                    }
                    if (descriptionNullIndex >= 0)
                    {
                        devicelist[i].Description = devicelist[i].Description.Substring(0, descriptionNullIndex);
                    }

                    Console.WriteLine($"SerialNumber: {devicelist[i].SerialNumber}");
                    Console.WriteLine($"Description: {devicelist[i].Description}");
                }
            }

            return ftStatus;
        }


		//**************************************************************************
		// OpenByIndex
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Opens the FTDI device with the specified index.  
		/// </summary>
		/// <returns>FT_STATUS value from FT_Open in FTD2XX.DLL</returns>
		/// <param name="index">Index of the device to open.
		/// Note that this cannot be guaranteed to open a specific device.</param>
		/// <remarks>Initialises the device to 8 data bits, 1 stop bit, no parity, no flow control and 9600 Baud.</remarks>
        public FT_STATUS OpenByIndex(UInt32 index)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            tFT_Open FT_Open = (tFT_Open)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_Open, typeof(tFT_Open));
            tFT_SetDataCharacteristics FT_SetDataCharacteristics = (tFT_SetDataCharacteristics)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetDataCharacteristics, typeof(tFT_SetDataCharacteristics));
            tFT_SetFlowControl FT_SetFlowControl = (tFT_SetFlowControl)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetFlowControl, typeof(tFT_SetFlowControl));
            tFT_SetBaudRate FT_SetBaudRate = (tFT_SetBaudRate)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetBaudRate, typeof(tFT_SetBaudRate));

            // Call FT_Open
            ftStatus = FT_Open(index, ref ftHandle);

            // Appears that the handle value can be non-NULL on a fail, so set it explicitly
            if (ftStatus != FT_STATUS.FT_OK)
                ftHandle = IntPtr.Zero;

            // Initialise port data characteristics
            byte WordLength = FT_DATA_BITS.FT_BITS_8;
            byte StopBits = FT_STOP_BITS.FT_STOP_BITS_1;
            byte Parity = FT_PARITY.FT_PARITY_NONE;
            ftStatus = FT_SetDataCharacteristics(ftHandle, WordLength, StopBits, Parity);
            // Initialise to no flow control
            UInt16 FlowControl = FT_FLOW_CONTROL.FT_FLOW_NONE;
            byte Xon = 0x11;
            byte Xoff = 0x13;
            ftStatus = FT_SetFlowControl(ftHandle, FlowControl, Xon, Xoff);
            // Initialise Baud rate
            UInt32 BaudRate = 9600;
            ftStatus = FT_SetBaudRate(ftHandle, BaudRate);
            return ftStatus;
        }


		//**************************************************************************
		// OpenBySerialNumber
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Opens the FTDI device with the specified serial number.  
		/// </summary>
		/// <returns>FT_STATUS value from FT_OpenEx in FTD2XX.DLL</returns>
		/// <param name="serialnumber">Serial number of the device to open.</param>
		/// <remarks>Initialises the device to 8 data bits, 1 stop bit, no parity, no flow control and 9600 Baud.</remarks>
        public FT_STATUS OpenBySerialNumber(string serialnumber)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;


            tFT_OpenEx FT_OpenEx = (tFT_OpenEx)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_OpenEx, typeof(tFT_OpenEx));
            tFT_SetDataCharacteristics FT_SetDataCharacteristics = (tFT_SetDataCharacteristics)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetDataCharacteristics, typeof(tFT_SetDataCharacteristics));
            tFT_SetFlowControl FT_SetFlowControl = (tFT_SetFlowControl)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetFlowControl, typeof(tFT_SetFlowControl));
            tFT_SetBaudRate FT_SetBaudRate = (tFT_SetBaudRate)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetBaudRate, typeof(tFT_SetBaudRate));

            // Call FT_OpenEx
            ftStatus = FT_OpenEx(serialnumber, (UInt32)FTDI.FT_OPENFLAGS.FT_OPEN_BY_SERIAL_NUMBER, ref ftHandle);

            // Appears that the handle value can be non-NULL on a fail, so set it explicitly
            if (ftStatus != FT_STATUS.FT_OK)
                ftHandle = IntPtr.Zero;

            // Initialise port data characteristics
            byte WordLength = FT_DATA_BITS.FT_BITS_8;
            byte StopBits = FT_STOP_BITS.FT_STOP_BITS_1;
            byte Parity = FT_PARITY.FT_PARITY_NONE;
            ftStatus = FT_SetDataCharacteristics(ftHandle, WordLength, StopBits, Parity);
            // Initialise to no flow control
            UInt16 FlowControl = FT_FLOW_CONTROL.FT_FLOW_NONE;
            byte Xon = 0x11;
            byte Xoff = 0x13;
            ftStatus = FT_SetFlowControl(ftHandle, FlowControl, Xon, Xoff);
            // Initialise Baud rate
            UInt32 BaudRate = 9600;
            ftStatus = FT_SetBaudRate(ftHandle, BaudRate);
            return ftStatus;
        }


		//**************************************************************************
		// OpenByDescription
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Opens the FTDI device with the specified description.  
		/// </summary>
		/// <returns>FT_STATUS value from FT_OpenEx in FTD2XX.DLL</returns>
		/// <param name="description">Description of the device to open.</param>
		/// <remarks>Initialises the device to 8 data bits, 1 stop bit, no parity, no flow control and 9600 Baud.</remarks>
        public FT_STATUS OpenByDescription(string description)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            tFT_OpenEx FT_OpenEx = (tFT_OpenEx)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_OpenEx, typeof(tFT_OpenEx));
            tFT_SetDataCharacteristics FT_SetDataCharacteristics = (tFT_SetDataCharacteristics)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetDataCharacteristics, typeof(tFT_SetDataCharacteristics));
            tFT_SetFlowControl FT_SetFlowControl = (tFT_SetFlowControl)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetFlowControl, typeof(tFT_SetFlowControl));
            tFT_SetBaudRate FT_SetBaudRate = (tFT_SetBaudRate)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetBaudRate, typeof(tFT_SetBaudRate));

            // Call FT_OpenEx
            ftStatus = FT_OpenEx(description, (UInt32)FTDI.FT_OPENFLAGS.FT_OPEN_BY_DESCRIPTION, ref ftHandle);

            // Appears that the handle value can be non-NULL on a fail, so set it explicitly
            if (ftStatus != FT_STATUS.FT_OK)
                ftHandle = IntPtr.Zero;

            // Initialise port data characteristics
            byte WordLength = FT_DATA_BITS.FT_BITS_8;
            byte StopBits = FT_STOP_BITS.FT_STOP_BITS_1;
            byte Parity = FT_PARITY.FT_PARITY_NONE;
            ftStatus = FT_SetDataCharacteristics(ftHandle, WordLength, StopBits, Parity);
            // Initialise to no flow control
            UInt16 FlowControl = FT_FLOW_CONTROL.FT_FLOW_NONE;
            byte Xon = 0x11;
            byte Xoff = 0x13;
            ftStatus = FT_SetFlowControl(ftHandle, FlowControl, Xon, Xoff);
            // Initialise Baud rate
            UInt32 BaudRate = 9600;
            ftStatus = FT_SetBaudRate(ftHandle, BaudRate);
            return ftStatus;
        }

		public FT_STATUS checkDeviceConnected(string description)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

			tFT_OpenEx FT_OpenEx = (tFT_OpenEx)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_OpenEx, typeof(tFT_OpenEx));
			tFT_SetDataCharacteristics FT_SetDataCharacteristics = (tFT_SetDataCharacteristics)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetDataCharacteristics, typeof(tFT_SetDataCharacteristics));
			tFT_SetFlowControl FT_SetFlowControl = (tFT_SetFlowControl)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetFlowControl, typeof(tFT_SetFlowControl));
			tFT_SetBaudRate FT_SetBaudRate = (tFT_SetBaudRate)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetBaudRate, typeof(tFT_SetBaudRate));

			// Call FT_OpenEx
			ftStatus = FT_OpenEx(description, (UInt32)FTDI.FT_OPENFLAGS.FT_OPEN_BY_DESCRIPTION, ref ftHandle);

			// Appears that the handle value can be non-NULL on a fail, so set it explicitly
			if (ftStatus != FT_STATUS.FT_OK)
				ftHandle = IntPtr.Zero;

			return ftStatus;
		}

		//**************************************************************************
		// OpenByLocation
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Opens the FTDI device at the specified physical location.  
		/// </summary>
		/// <returns>FT_STATUS value from FT_OpenEx in FTD2XX.DLL</returns>
		/// <param name="location">Location of the device to open.</param>
		/// <remarks>Initialises the device to 8 data bits, 1 stop bit, no parity, no flow control and 9600 Baud.</remarks>
		public FT_STATUS OpenByLocation(UInt32 location)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            tFT_OpenExLoc FT_OpenEx = (tFT_OpenExLoc)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_OpenEx, typeof(tFT_OpenExLoc));
            tFT_SetDataCharacteristics FT_SetDataCharacteristics = (tFT_SetDataCharacteristics)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetDataCharacteristics, typeof(tFT_SetDataCharacteristics));
            tFT_SetFlowControl FT_SetFlowControl = (tFT_SetFlowControl)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetFlowControl, typeof(tFT_SetFlowControl));
            tFT_SetBaudRate FT_SetBaudRate = (tFT_SetBaudRate)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetBaudRate, typeof(tFT_SetBaudRate));

            // Call FT_OpenEx
            ftStatus = FT_OpenEx(location, (UInt32)FTDI.FT_OPENFLAGS.FT_OPEN_BY_LOCATION, ref ftHandle);

            // Appears that the handle value can be non-NULL on a fail, so set it explicitly
            if (ftStatus != FT_STATUS.FT_OK)
                ftHandle = IntPtr.Zero;

            // Initialise port data characteristics
            byte WordLength = FT_DATA_BITS.FT_BITS_8;
            byte StopBits = FT_STOP_BITS.FT_STOP_BITS_1;
            byte Parity = FT_PARITY.FT_PARITY_NONE;
            ftStatus = FT_SetDataCharacteristics(ftHandle, WordLength, StopBits, Parity);
            // Initialise to no flow control
            UInt16 FlowControl = FT_FLOW_CONTROL.FT_FLOW_NONE;
            byte Xon = 0x11;
            byte Xoff = 0x13;
            ftStatus = FT_SetFlowControl(ftHandle, FlowControl, Xon, Xoff);
            // Initialise Baud rate
            UInt32 BaudRate = 9600;
            ftStatus = FT_SetBaudRate(ftHandle, BaudRate);
            return ftStatus;
        }

        public void Close()
        {
            if (ftHandle == IntPtr.Zero)
                return;

            tFT_Close FT_Close = (tFT_Close)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_Close, typeof(tFT_Close));
            FT_Close(ftHandle);
            ftHandle = IntPtr.Zero;
        }
#if LINUX || MACOS

		//**************************************************************************
		// SetVIDPID
		//**************************************************************************
		// Supported Operating Systems
		// Linux
		// Mac OS X (10.4 and later)
		// <Summary>
		// A command to include a custom VID and PID combination within the internal device list table. 
		// This will allow the driver to load for the specified VID and PID combination.
		// <param name="dwVID" Device Vendor ID (VID)
		// <param name="dwPID" Device Product ID (PID)
		// <returns> FT_OK if successful, otherwise the return value is an FT error code.
		// <remarks> By default, the driver will support a limited set of VID and PID matched devices (VID 0x0403 
		// with PIDs for standard FTDI devices only).
		// In order to use the driver with other VID and PID combinations the FT_SetVIDPID function 
		// must be used prior to calling FT_ListDevices, FT_Open, FT_OpenEx or FT_CreateDeviceInfoList.
		// @note Extra function for non-Windows platforms to compensate for lack of .INF file to specify 
		// Vendor and Product IDs.
		// 
        public FT_STATUS SetVIDPID(UInt32 dwVID, UInt32 dwPID)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            tFT_SetVIDPID FT_SetVIDPID = (tFT_SetVIDPID)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetVIDPID, typeof(tFT_SetVIDPID));
			
            // Call FT_SetVIDPID
            ftStatus = FT_SetVIDPID(dwVID, dwPID);
            return ftStatus;
        }

		//**************************************************************************
		// GetVIDPID
		//**************************************************************************
		// Supported Operating Systems
		// Linux
		// Mac OS X (10.4 and later)
		// <Summary>
		// A command to retrieve the current VID and PID combination from within the internal device list table.
		// <param name="pdwVID" Pointer to DWORD that will contain the internal VID
		// <param name="pdwPID" Pointer to DWORD that will contain the internal PID
		// <returns> FT_OK if successful, otherwise the return value is an FT error code.
		// <remarks> note Extra function for non-Windows platforms to compensate for lack of .INF file to specify Vendor and Product IDs.
		// FT_SetVIDPID.
		// 
        public FT_STATUS GetVIDPID(ref UInt32 dwVID, ref UInt32 dwPID)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            tFT_GetVIDPID FT_GetVIDPID = (tFT_GetVIDPID)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_GetVIDPID, typeof(tFT_GetVIDPID));

            // Call FT_GetVIDPID
            ftStatus = FT_GetVIDPID(ref dwVID, ref dwVID);
            return ftStatus;
        }

		//**************************************************************************
		// GetDeviceLocId
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Extra function for non-Windows platforms to compensate for lack of .INF file to specify Vendor and Product IDs.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetDeviceLocId in libftd2xx.lib/libftd2xx.dylib</returns>
		/// <param name="lpdwLocId">The device locationId of the current device.</param>
        public FT_STATUS GetDeviceLocId(ref UInt32 lpdwLocId)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_GetDeviceLocId FT_GetDeviceLocId = (tFT_GetDeviceLocId)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_GetDeviceLocId, typeof(tFT_GetDeviceLocId));

            // Call FT_GetDeviceLocId
            ftStatus = FT_GetDeviceLocId(ftHandle, ref lpdwLocId);
            return ftStatus;
        } 
#endif
		//**************************************************************************
		// Read
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Read data from an open FTDI device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_Read in FTD2XX.DLL</returns>
		/// <param name="dataBuffer">An array of bytes which will be populated with the data read from the device.</param>
		/// <param name="numBytesToRead">The number of bytes requested from the device.</param>
		/// <param name="numBytesRead">The number of bytes actually read.</param>
        public FT_STATUS Read(byte[] dataBuffer, UInt32 numBytesToRead, ref UInt32 numBytesRead)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;


            tFT_Read FT_Read = (tFT_Read)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_Read, typeof(tFT_Read));

            // If the buffer is not big enough to receive the amount of data requested, adjust the number of bytes to read
            if (dataBuffer.Length < numBytesToRead)
            {
                numBytesToRead = (uint)dataBuffer.Length;
            }

            // Call FT_Read
            ftStatus = FT_Read(ftHandle, dataBuffer, numBytesToRead, ref numBytesRead);
            return ftStatus;
        }

		// Intellisense comments
		/// <summary>
		/// Read data from an open FTDI device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_Read in FTD2XX.DLL</returns>
		/// <param name="dataBuffer">A string containing the data read</param>
		/// <param name="numBytesToRead">The number of bytes requested from the device.</param>
		/// <param name="numBytesRead">The number of bytes actually read.</param>
        public FT_STATUS Read(out string dataBuffer, UInt32 numBytesToRead, ref UInt32 numBytesRead)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // As dataBuffer is an OUT parameter, needs to be assigned before returning
            dataBuffer = string.Empty;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_Read FT_Read = (tFT_Read)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_Read, typeof(tFT_Read));

            byte[] byteDataBuffer = new byte[numBytesToRead];

            // Call FT_Read
            ftStatus = FT_Read(ftHandle, byteDataBuffer, numBytesToRead, ref numBytesRead);

            // Convert ASCII byte array back to Unicode string for passing back
            dataBuffer = Encoding.ASCII.GetString(byteDataBuffer);
            // Trim buffer to actual bytes read
            dataBuffer = dataBuffer.Substring(0, (int)numBytesRead);
            return ftStatus;
        }

		//**************************************************************************
		// Write
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Write data to an open FTDI device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_Write in FTD2XX.DLL</returns>
		/// <param name="dataBuffer">An array of bytes which contains the data to be written to the device.</param>
		/// <param name="numBytesToWrite">The number of bytes to be written to the device.</param>
		/// <param name="numBytesWritten">The number of bytes actually written to the device.</param>
        public FT_STATUS Write(byte[] dataBuffer, Int32 numBytesToWrite, ref UInt32 numBytesWritten)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_Write FT_Write = (tFT_Write)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_Write, typeof(tFT_Write));

            // Call FT_Write
            ftStatus = FT_Write(ftHandle, dataBuffer, (UInt32)numBytesToWrite, ref numBytesWritten);
            return ftStatus;
        }
		
        public byte ReadPin()
        {
            if (ftHandle == IntPtr.Zero)
				throw new ApplicationException("no valid ftdi handle");
			tFT_GetBitMode d1 = (tFT_GetBitMode)Marshal
			       .GetDelegateForFunctionPointer(FTDILib.Instance.pFT_GetBitMode, typeof(tFT_GetBitMode));
		    byte pins = 0x00;
            if (d1(ftHandle, ref pins) == FT_STATUS.FT_OK)
				return pins;
			else
				throw new System.IO.IOException("error: FT_GetBitMode");
        }
		

        public void WritePin(byte pins)
        {
            if (ftHandle == IntPtr.Zero)
                throw new ApplicationException("no valid ftdi handle");
            tFT_Write d1 = (tFT_Write)Marshal
                .GetDelegateForFunctionPointer(FTDILib.Instance.pFT_Write, typeof(tFT_Write));
            UInt32 lpdwBytesWritten = 0;
            byte[] lpBuffer = new byte[1] { pins };
            if (d1(ftHandle, lpBuffer, 1, ref lpdwBytesWritten) != FT_STATUS.FT_OK)
                throw new System.IO.IOException("error: FT_Write");
        }

        // Intellisense comments
		/// <summary>
		/// Write data to an open FTDI device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_Write in FTD2XX.DLL</returns>
		/// <param name="dataBuffer">An array of bytes which contains the data to be written to the device.</param>
		/// <param name="numBytesToWrite">The number of bytes to be written to the device.</param>
		/// <param name="numBytesWritten">The number of bytes actually written to the device.</param>
        public FT_STATUS Write(byte[] dataBuffer, UInt32 numBytesToWrite, ref UInt32 numBytesWritten)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_Write FT_Write = (tFT_Write)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_Write, typeof(tFT_Write));

            // Call FT_Write
            ftStatus = FT_Write(ftHandle, dataBuffer, numBytesToWrite, ref numBytesWritten);
            return ftStatus;
        }

		// Intellisense comments
		/// <summary>
		/// Write data to an open FTDI device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_Write in FTD2XX.DLL</returns>
		/// <param name="dataBuffer">A  string which contains the data to be written to the device.</param>
		/// <param name="numBytesToWrite">The number of bytes to be written to the device.</param>
		/// <param name="numBytesWritten">The number of bytes actually written to the device.</param>
        public FT_STATUS Write(string dataBuffer, Int32 numBytesToWrite, ref UInt32 numBytesWritten)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_Write FT_Write = (tFT_Write)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_Write, typeof(tFT_Write));

            // Convert Unicode string to ASCII byte array
            byte[] byteDataBuffer = Encoding.ASCII.GetBytes(dataBuffer);

            // Call FT_Write
            ftStatus = FT_Write(ftHandle, byteDataBuffer, (UInt32)numBytesToWrite, ref numBytesWritten);
            return ftStatus;
        }

		// Intellisense comments
		/// <summary>
		/// Write data to an open FTDI device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_Write in FTD2XX.DLL</returns>
		/// <param name="dataBuffer">A  string which contains the data to be written to the device.</param>
		/// <param name="numBytesToWrite">The number of bytes to be written to the device.</param>
		/// <param name="numBytesWritten">The number of bytes actually written to the device.</param>
        public FT_STATUS Write(string dataBuffer, UInt32 numBytesToWrite, ref UInt32 numBytesWritten)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_Write FT_Write = (tFT_Write)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_Write, typeof(tFT_Write));

            // Convert Unicode string to ASCII byte array
            byte[] byteDataBuffer = Encoding.ASCII.GetBytes(dataBuffer);

            // Call FT_Write
            ftStatus = FT_Write(ftHandle, byteDataBuffer, numBytesToWrite, ref numBytesWritten);
            return ftStatus;
        }

		//**************************************************************************
		// ResetDevice
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Reset an open FTDI device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_ResetDevice in FTD2XX.DLL</returns>
        public FT_STATUS ResetDevice()
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_ResetDevice FT_ResetDevice = (tFT_ResetDevice)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_ResetDevice, typeof(tFT_ResetDevice));

            // Call FT_ResetDevice
            ftStatus = FT_ResetDevice(ftHandle);
            return ftStatus;
        }

		//**************************************************************************
		// Purge
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Purge data from the devices transmit and/or receive buffers.
		/// </summary>
		/// <returns>FT_STATUS value from FT_Purge in FTD2XX.DLL</returns>
		/// <param name="purgemask">Specifies which buffer(s) to be purged.  Valid values are any combination of the following flags: FT_PURGE_RX, FT_PURGE_TX</param>
        public FT_STATUS Purge(UInt32 purgemask)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_Purge FT_Purge = (tFT_Purge)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_Purge, typeof(tFT_Purge));

            // Call FT_Purge
            ftStatus = FT_Purge(ftHandle, purgemask);
            return ftStatus;
        }

		//**************************************************************************
		// SetEventNotification
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Register for event notification.
		/// </summary>
		/// <returns>FT_STATUS value from FT_SetEventNotification in FTD2XX.DLL</returns>
		/// <remarks>After setting event notification, the event can be caught by executing the WaitOne() method of the EventWaitHandle.  If multiple event types are being monitored, the event that fired can be determined from the GetEventType method.</remarks>
		/// <param name="eventmask">The type of events to signal.  Can be any combination of the following: FT_EVENT_RXCHAR, FT_EVENT_MODEM_STATUS, FT_EVENT_LINE_STATUS</param>
		/// <param name="eventhandle">Handle to the event that will receive the notification</param>
        public FT_STATUS SetEventNotification(UInt32 eventmask, EventWaitHandle eventhandle)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_SetEventNotification FT_SetEventNotification = (tFT_SetEventNotification)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetEventNotification, typeof(tFT_SetEventNotification));

            // Call FT_SetSetEventNotification
            ftStatus = FT_SetEventNotification(ftHandle, eventmask, eventhandle.SafeWaitHandle);
            return ftStatus;
        }

		//**************************************************************************
		// StopInTask
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Stops the driver issuing USB in requests.
		/// </summary>
		/// <returns>FT_STATUS value from FT_StopInTask in FTD2XX.DLL</returns>
        public FT_STATUS StopInTask()
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_StopInTask FT_StopInTask = (tFT_StopInTask)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_StopInTask, typeof(tFT_StopInTask));

            // Call FT_StopInTask
            ftStatus = FT_StopInTask(ftHandle);
            return ftStatus;
        }

		//**************************************************************************
		// RestartInTask
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Resumes the driver issuing USB in requests.
		/// </summary>
		/// <returns>FT_STATUS value from FT_RestartInTask in FTD2XX.DLL</returns>
        public FT_STATUS RestartInTask()
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_RestartInTask FT_RestartInTask = (tFT_RestartInTask)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_RestartInTask, typeof(tFT_RestartInTask));

            // Call FT_RestartInTask
            ftStatus = FT_RestartInTask(ftHandle);

            return ftStatus;
        }

		//**************************************************************************
		// ResetPort
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Resets the device port.
		/// </summary>
		/// <returns>FT_STATUS value from FT_ResetPort in FTD2XX.DLL</returns>
        public FT_STATUS ResetPort()
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_ResetPort FT_ResetPort = (tFT_ResetPort)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_ResetPort, typeof(tFT_ResetPort));

            // Call FT_ResetPort
            ftStatus = FT_ResetPort(ftHandle);
            return ftStatus;
        }

		//**************************************************************************
		// CyclePort
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Causes the device to be re-enumerated on the USB bus.  This is equivalent to unplugging and replugging the device.
		/// Also calls FT_Close if FT_CyclePort is successful, so no need to call this separately in the application.
		/// </summary>
		/// <returns>FT_STATUS value from FT_CyclePort in FTD2XX.DLL</returns>
        public FT_STATUS CyclePort()
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_CyclePort FT_CyclePort = (tFT_CyclePort)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_CyclePort, typeof(tFT_CyclePort));
            tFT_Close FT_Close = (tFT_Close)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_Close, typeof(tFT_Close));

            // Call FT_CyclePort
            ftStatus = FT_CyclePort(ftHandle);
            if (ftStatus == FT_STATUS.FT_OK)
            {
                // If successful, call FT_Close
                ftStatus = FT_Close(ftHandle);
                if (ftStatus == FT_STATUS.FT_OK)
                {
                    ftHandle = IntPtr.Zero;
                }
            }
            return ftStatus;
        }

		//**************************************************************************
		// Rescan
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Causes the system to check for USB hardware changes.  This is equivalent to clicking on the "Scan for hardware changes" button in the Device Manager.
		/// </summary>
		/// <returns>FT_STATUS value from FT_Rescan in FTD2XX.DLL</returns>
        public FT_STATUS Rescan()
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;


            tFT_Rescan FT_Rescan = (tFT_Rescan)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_Rescan, typeof(tFT_Rescan));

            // Call FT_Rescan
            ftStatus = FT_Rescan();
            return ftStatus;
        }

		//**************************************************************************
		// Reload
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Forces a reload of the driver for devices with a specific VID and PID combination.
		/// </summary>
		/// <returns>FT_STATUS value from FT_Reload in FTD2XX.DLL</returns>
		/// <remarks>If the VID and PID parameters are 0, the drivers for USB root hubs will be reloaded, causing all USB devices connected to reload their drivers</remarks>
		/// <param name="VendorID">Vendor ID of the devices to have the driver reloaded</param>
		/// <param name="ProductID">Product ID of the devices to have the driver reloaded</param>
        public FT_STATUS Reload(UInt16 VendorID, UInt16 ProductID)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_Reload FT_Reload = (tFT_Reload)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_Reload, typeof(tFT_Reload));

            // Call FT_Reload
            ftStatus = FT_Reload(VendorID, ProductID);
            return ftStatus;
        }

		//**************************************************************************
		// SetBitMode
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Puts the device in a mode other than the default UART or FIFO mode.
		/// </summary>
		/// <returns>FT_STATUS value from FT_SetBitMode in FTD2XX.DLL</returns>
		/// <param name="Mask">Sets up which bits are inputs and which are outputs.  A bit value of 0 sets the corresponding pin to an input, a bit value of 1 sets the corresponding pin to an output.
		/// In the case of CBUS Bit Bang, the upper nibble of this value controls which pins are inputs and outputs, while the lower nibble controls which of the outputs are high and low.</param>
		/// <param name="BitMode"> For FT232H devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE, FT_BIT_MODE_SYNC_BITBANG, FT_BIT_MODE_CBUS_BITBANG, FT_BIT_MODE_MCU_HOST, FT_BIT_MODE_FAST_SERIAL, FT_BIT_MODE_SYNC_FIFO.
		/// For FT2232H devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE, FT_BIT_MODE_SYNC_BITBANG, FT_BIT_MODE_MCU_HOST, FT_BIT_MODE_FAST_SERIAL, FT_BIT_MODE_SYNC_FIFO.
		/// For FT4232H devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE, FT_BIT_MODE_SYNC_BITBANG.
		/// For FT232R devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_SYNC_BITBANG, FT_BIT_MODE_CBUS_BITBANG.
		/// For FT245R devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_SYNC_BITBANG.
		/// For FT2232 devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE, FT_BIT_MODE_SYNC_BITBANG, FT_BIT_MODE_MCU_HOST, FT_BIT_MODE_FAST_SERIAL.
		/// For FT232B and FT245B devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG.</param>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not support the requested bit mode.</exception>
        public FT_STATUS SetBitMode(byte Mask, byte BitMode)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;
            FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_SetBitMode FT_SetBitMode = (tFT_SetBitMode)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetBitMode, typeof(tFT_SetBitMode));


            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            // Set Bit Mode does not apply to FT8U232AM, FT8U245AM or FT8U100AX devices
            GetDeviceType(ref DeviceType);
            if (DeviceType == FT_DEVICETYPE.FT_DEVICE_AM)
            {
                // Throw an exception
                ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                ErrorHandler(ftStatus, ftErrorCondition);
            }
            else if (DeviceType == FT_DEVICETYPE.FT_DEVICE_100AX)
            {
                // Throw an exception
                ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                ErrorHandler(ftStatus, ftErrorCondition);
            }
            else if ((DeviceType == FT_DEVICETYPE.FT_DEVICE_BM) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
            {
                if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG)) == 0)
                {
                    // Throw an exception
                    ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
            }
            else if ((DeviceType == FT_DEVICETYPE.FT_DEVICE_2232C) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
            {
                if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MPSSE | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MCU_HOST | FT_BIT_MODES.FT_BIT_MODE_FAST_SERIAL)) == 0)
                {
                    // Throw an exception
                    ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
                if ((BitMode == FT_BIT_MODES.FT_BIT_MODE_MPSSE) & (InterfaceIdentifier != "A"))
                {
                    // MPSSE mode is only available on channel A
                    // Throw an exception
                    ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
            }
            else if ((DeviceType == FT_DEVICETYPE.FT_DEVICE_232R) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
            {
                if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_CBUS_BITBANG)) == 0)
                {
                    // Throw an exception
                    ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
            }
            else if ((DeviceType == FT_DEVICETYPE.FT_DEVICE_2232H) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
            {
                if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MPSSE | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MCU_HOST | FT_BIT_MODES.FT_BIT_MODE_FAST_SERIAL | FT_BIT_MODES.FT_BIT_MODE_SYNC_FIFO)) == 0)
                {
                    // Throw an exception
                    ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
                if (((BitMode == FT_BIT_MODES.FT_BIT_MODE_MCU_HOST) | (BitMode == FT_BIT_MODES.FT_BIT_MODE_SYNC_FIFO)) & (InterfaceIdentifier != "A"))
                {
                    // MCU Host Emulation and Single channel synchronous 245 FIFO mode is only available on channel A
                    // Throw an exception
                    ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
            }
            else if ((DeviceType == FT_DEVICETYPE.FT_DEVICE_4232H) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
            {
                if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MPSSE | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG)) == 0)
                {
                    // Throw an exception
                    ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
                if ((BitMode == FT_BIT_MODES.FT_BIT_MODE_MPSSE) & ((InterfaceIdentifier != "A") & (InterfaceIdentifier != "B")))
                {
                    // MPSSE mode is only available on channel A and B
                    // Throw an exception
                    ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
            }
            else if ((DeviceType == FT_DEVICETYPE.FT_DEVICE_232H) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
            {
                // FT232H supports all current bit modes!
                if (BitMode > FT_BIT_MODES.FT_BIT_MODE_SYNC_FIFO)
                {
                    // Throw an exception
                    ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
            }

            // Requested bit mode is supported
            // Note FT_BIT_MODES.FT_BIT_MODE_RESET falls through to here - no bits set so cannot check for AND
            // Call FT_SetBitMode
            ftStatus = FT_SetBitMode(ftHandle, Mask, BitMode);

            return ftStatus;
        }

		//**************************************************************************
		// GetPinStates
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the instantaneous state of the device IO pins.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetBitMode in FTD2XX.DLL</returns>
		/// <param name="BitMode">A bitmap value containing the instantaneous state of the device IO pins</param>
        public FT_STATUS GetPinStates(ref byte BitMode)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_GetBitMode FT_GetBitMode = (tFT_GetBitMode)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_GetBitMode, typeof(tFT_GetBitMode));

            // Call FT_GetBitMode
            ftStatus = FT_GetBitMode(ftHandle, ref BitMode);
            return ftStatus;
        }

		//**************************************************************************
		// ReadEEPROMLocation
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Reads an individual word value from a specified location in the device's EEPROM.
		/// </summary>
		/// <returns>FT_STATUS value from FT_ReadEE in FTD2XX.DLL</returns>
		/// <param name="Address">The EEPROM location to read data from</param>
		/// <param name="EEValue">The WORD value read from the EEPROM location specified in the Address paramter</param>
        public FT_STATUS ReadEEPROMLocation(UInt32 Address, ref UInt16 EEValue)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_ReadEE FT_ReadEE = (tFT_ReadEE)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_ReadEE, typeof(tFT_ReadEE));

            // Call FT_ReadEE
            ftStatus = FT_ReadEE(ftHandle, Address, ref EEValue);
            return ftStatus;
        }

		//**************************************************************************
		// WriteEEPROMLocation
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Writes an individual word value to a specified location in the device's EEPROM.
		/// </summary>
		/// <returns>FT_STATUS value from FT_WriteEE in FTD2XX.DLL</returns>
		/// <param name="Address">The EEPROM location to read data from</param>
		/// <param name="EEValue">The WORD value to write to the EEPROM location specified by the Address parameter</param>
        public FT_STATUS WriteEEPROMLocation(UInt32 Address, UInt16 EEValue)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_WriteEE FT_WriteEE = (tFT_WriteEE)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_WriteEE, typeof(tFT_WriteEE));

            // Call FT_WriteEE
            ftStatus = FT_WriteEE(ftHandle, Address, EEValue);
            return ftStatus;
        }

		//**************************************************************************
		// EraseEEPROM
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Erases the device EEPROM.
		/// </summary>
		/// <returns>FT_STATUS value from FT_EraseEE in FTD2XX.DLL</returns>
		/// <exception cref="FT_EXCEPTION">Thrown when attempting to erase the EEPROM of a device with an internal EEPROM such as an FT232R or FT245R.</exception>
        public FT_STATUS EraseEEPROM()
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;
            FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_EraseEE FT_EraseEE = (tFT_EraseEE)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EraseEE, typeof(tFT_EraseEE));

            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            // Check that it is not an FT232R or FT245R that we are trying to erase
            GetDeviceType(ref DeviceType);
            if (DeviceType == FT_DEVICETYPE.FT_DEVICE_232R)
            {
                // If it is a device with an internal EEPROM, throw an exception
                ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            // Call FT_EraseEE
            ftStatus = FT_EraseEE(ftHandle);
            return ftStatus;
        }

		//**************************************************************************
		// ReadFT232BEEPROM
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Reads the EEPROM contents of an FT232B or FT245B device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
		/// <param name="ee232b">An FT232B_EEPROM_STRUCTURE which contains only the relevant information for an FT232B and FT245B device.</param>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS Read_FT232B_EEPROM(FT232B_EEPROM_STRUCTURE ee232b)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;
            FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_EE_Read FT_EE_Read = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_Read, typeof(tFT_EE_Read));

            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            // Check that it is an FT232B or FT245B that we are trying to read
            GetDeviceType(ref DeviceType);
            if (DeviceType != FT_DEVICETYPE.FT_DEVICE_BM)
            {
                // If it is not, throw an exception
                ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

            // Set up structure headers
            eedata.Signature1 = 0x00000000;
            eedata.Signature2 = 0xFFFFFFFF;
            eedata.Version = 2;

            // Allocate space from unmanaged heap
            eedata.Manufacturer = Marshal.AllocHGlobal(32);
            eedata.ManufacturerID = Marshal.AllocHGlobal(16);
            eedata.Description = Marshal.AllocHGlobal(64);
            eedata.SerialNumber = Marshal.AllocHGlobal(16);

            // Call FT_EE_Read
            ftStatus = FT_EE_Read(ftHandle, eedata);

            // Retrieve string values
            ee232b.Manufacturer = Marshal.PtrToStringAnsi(eedata.Manufacturer);
            ee232b.ManufacturerID = Marshal.PtrToStringAnsi(eedata.ManufacturerID);
            ee232b.Description = Marshal.PtrToStringAnsi(eedata.Description);
            ee232b.SerialNumber = Marshal.PtrToStringAnsi(eedata.SerialNumber);

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eedata.Manufacturer);
            Marshal.FreeHGlobal(eedata.ManufacturerID);
            Marshal.FreeHGlobal(eedata.Description);
            Marshal.FreeHGlobal(eedata.SerialNumber);

            // Map non-string elements to structure to be returned
            // Standard elements
            ee232b.VendorID = eedata.VendorID;
            ee232b.ProductID = eedata.ProductID;
            ee232b.MaxPower = eedata.MaxPower;
            ee232b.SelfPowered = Convert.ToBoolean(eedata.SelfPowered);
            ee232b.RemoteWakeup = Convert.ToBoolean(eedata.RemoteWakeup);
            // B specific fields
            ee232b.PullDownEnable = Convert.ToBoolean(eedata.PullDownEnable);
            ee232b.SerNumEnable = Convert.ToBoolean(eedata.SerNumEnable);
            ee232b.USBVersionEnable = Convert.ToBoolean(eedata.USBVersionEnable);
            ee232b.USBVersion = eedata.USBVersion;

            return ftStatus;
        }

		//**************************************************************************
		// ReadFT2232EEPROM
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Reads the EEPROM contents of an FT2232 device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
		/// <param name="ee2232">An FT2232_EEPROM_STRUCTURE which contains only the relevant information for an FT2232 device.</param>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS Read_FT2232_EEPROM(FT2232_EEPROM_STRUCTURE ee2232)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;
            FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_EE_Read FT_EE_Read = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_Read, typeof(tFT_EE_Read));

            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            // Check that it is an FT2232 that we are trying to read
            GetDeviceType(ref DeviceType);
            if (DeviceType != FT_DEVICETYPE.FT_DEVICE_2232C)
            {
                // If it is not, throw an exception
                ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

            // Set up structure headers
            eedata.Signature1 = 0x00000000;
            eedata.Signature2 = 0xFFFFFFFF;
            eedata.Version = 2;

            // Allocate space from unmanaged heap
            eedata.Manufacturer = Marshal.AllocHGlobal(32);
            eedata.ManufacturerID = Marshal.AllocHGlobal(16);
            eedata.Description = Marshal.AllocHGlobal(64);
            eedata.SerialNumber = Marshal.AllocHGlobal(16);

            // Call FT_EE_Read
            ftStatus = FT_EE_Read(ftHandle, eedata);

            // Retrieve string values
            ee2232.Manufacturer = Marshal.PtrToStringAnsi(eedata.Manufacturer);
            ee2232.ManufacturerID = Marshal.PtrToStringAnsi(eedata.ManufacturerID);
            ee2232.Description = Marshal.PtrToStringAnsi(eedata.Description);
            ee2232.SerialNumber = Marshal.PtrToStringAnsi(eedata.SerialNumber);

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eedata.Manufacturer);
            Marshal.FreeHGlobal(eedata.ManufacturerID);
            Marshal.FreeHGlobal(eedata.Description);
            Marshal.FreeHGlobal(eedata.SerialNumber);

            // Map non-string elements to structure to be returned
            // Standard elements
            ee2232.VendorID = eedata.VendorID;
            ee2232.ProductID = eedata.ProductID;
            ee2232.MaxPower = eedata.MaxPower;
            ee2232.SelfPowered = Convert.ToBoolean(eedata.SelfPowered);
            ee2232.RemoteWakeup = Convert.ToBoolean(eedata.RemoteWakeup);
            // 2232 specific fields
            ee2232.PullDownEnable = Convert.ToBoolean(eedata.PullDownEnable5);
            ee2232.SerNumEnable = Convert.ToBoolean(eedata.SerNumEnable5);
            ee2232.USBVersionEnable = Convert.ToBoolean(eedata.USBVersionEnable5);
            ee2232.USBVersion = eedata.USBVersion5;
            ee2232.AIsHighCurrent = Convert.ToBoolean(eedata.AIsHighCurrent);
            ee2232.BIsHighCurrent = Convert.ToBoolean(eedata.BIsHighCurrent);
            ee2232.IFAIsFifo = Convert.ToBoolean(eedata.IFAIsFifo);
            ee2232.IFAIsFifoTar = Convert.ToBoolean(eedata.IFAIsFifoTar);
            ee2232.IFAIsFastSer = Convert.ToBoolean(eedata.IFAIsFastSer);
            ee2232.AIsVCP = Convert.ToBoolean(eedata.AIsVCP);
            ee2232.IFBIsFifo = Convert.ToBoolean(eedata.IFBIsFifo);
            ee2232.IFBIsFifoTar = Convert.ToBoolean(eedata.IFBIsFifoTar);
            ee2232.IFBIsFastSer = Convert.ToBoolean(eedata.IFBIsFastSer);
            ee2232.BIsVCP = Convert.ToBoolean(eedata.BIsVCP);

            return ftStatus;
        }

		//**************************************************************************
		// ReadFT232REEPROM
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Reads the EEPROM contents of an FT232R or FT245R device.
		/// Calls FT_EE_Read in FTD2XX DLL
		/// </summary>
		/// <returns>An FT232R_EEPROM_STRUCTURE which contains only the relevant information for an FT232R and FT245R device.</returns>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS Read_FT232R_EEPROM(FT232R_EEPROM_STRUCTURE ee232r)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;
            FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_EE_Read FT_EE_Read = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_Read, typeof(tFT_EE_Read));

            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            // Check that it is an FT232R or FT245R that we are trying to read
            GetDeviceType(ref DeviceType);
            if (DeviceType != FT_DEVICETYPE.FT_DEVICE_232R)
            {
                // If it is not, throw an exception
                ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

            // Set up structure headers
            eedata.Signature1 = 0x00000000;
            eedata.Signature2 = 0xFFFFFFFF;
            eedata.Version = 2;

            // Allocate space from unmanaged heap
            eedata.Manufacturer = Marshal.AllocHGlobal(32);
            eedata.ManufacturerID = Marshal.AllocHGlobal(16);
            eedata.Description = Marshal.AllocHGlobal(64);
            eedata.SerialNumber = Marshal.AllocHGlobal(16);

            // Call FT_EE_Read
            ftStatus = FT_EE_Read(ftHandle, eedata);

            // Retrieve string values
            ee232r.Manufacturer = Marshal.PtrToStringAnsi(eedata.Manufacturer);
            ee232r.ManufacturerID = Marshal.PtrToStringAnsi(eedata.ManufacturerID);
            ee232r.Description = Marshal.PtrToStringAnsi(eedata.Description);
            ee232r.SerialNumber = Marshal.PtrToStringAnsi(eedata.SerialNumber);

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eedata.Manufacturer);
            Marshal.FreeHGlobal(eedata.ManufacturerID);
            Marshal.FreeHGlobal(eedata.Description);
            Marshal.FreeHGlobal(eedata.SerialNumber);

            // Map non-string elements to structure to be returned
            // Standard elements
            ee232r.VendorID = eedata.VendorID;
            ee232r.ProductID = eedata.ProductID;
            ee232r.MaxPower = eedata.MaxPower;
            ee232r.SelfPowered = Convert.ToBoolean(eedata.SelfPowered);
            ee232r.RemoteWakeup = Convert.ToBoolean(eedata.RemoteWakeup);
            // 232R specific fields
            ee232r.UseExtOsc = Convert.ToBoolean(eedata.UseExtOsc);
            ee232r.HighDriveIOs = Convert.ToBoolean(eedata.HighDriveIOs);
            ee232r.EndpointSize = eedata.EndpointSize;
            ee232r.PullDownEnable = Convert.ToBoolean(eedata.PullDownEnableR);
            ee232r.SerNumEnable = Convert.ToBoolean(eedata.SerNumEnableR);
            ee232r.InvertTXD = Convert.ToBoolean(eedata.InvertTXD);
            ee232r.InvertRXD = Convert.ToBoolean(eedata.InvertRXD);
            ee232r.InvertRTS = Convert.ToBoolean(eedata.InvertRTS);
            ee232r.InvertCTS = Convert.ToBoolean(eedata.InvertCTS);
            ee232r.InvertDTR = Convert.ToBoolean(eedata.InvertDTR);
            ee232r.InvertDSR = Convert.ToBoolean(eedata.InvertDSR);
            ee232r.InvertDCD = Convert.ToBoolean(eedata.InvertDCD);
            ee232r.InvertRI = Convert.ToBoolean(eedata.InvertRI);
            ee232r.Cbus0 = eedata.Cbus0;
            ee232r.Cbus1 = eedata.Cbus1;
            ee232r.Cbus2 = eedata.Cbus2;
            ee232r.Cbus3 = eedata.Cbus3;
            ee232r.Cbus4 = eedata.Cbus4;
            ee232r.RIsD2XX = Convert.ToBoolean(eedata.RIsD2XX);

            return ftStatus;
        }

		//**************************************************************************
		// ReadFT2232HEEPROM
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Reads the EEPROM contents of an FT2232H device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
		/// <param name="ee2232h">An FT2232H_EEPROM_STRUCTURE which contains only the relevant information for an FT2232H device.</param>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS Read_FT2232H_EEPROM(ref FT2232H_EEPROM_STRUCTURE ee2232h, ref string strError)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;
            FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_EE_Read FT_EE_Read = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_Read, typeof(tFT_EE_Read));

            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            // Check that it is an FT2232H that we are trying to read
            GetDeviceType(ref DeviceType);
            if (DeviceType != FT_DEVICETYPE.FT_DEVICE_2232H)
            {
                // If it is not, throw an exception
                ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

            // Set up structure headers
            eedata.Signature1 = 0x00000000;
            eedata.Signature2 = 0xFFFFFFFF;
            eedata.Version = 3;

            // Allocate space from unmanaged heap
            eedata.Manufacturer = Marshal.AllocHGlobal(32);
            eedata.ManufacturerID = Marshal.AllocHGlobal(16);
            eedata.Description = Marshal.AllocHGlobal(64);
            eedata.SerialNumber = Marshal.AllocHGlobal(16);

            // Call FT_EE_Read
            ftStatus = FT_EE_Read(ftHandle, eedata);

            // Retrieve string values
            ee2232h.Manufacturer = Marshal.PtrToStringAnsi(eedata.Manufacturer);
            ee2232h.ManufacturerID = Marshal.PtrToStringAnsi(eedata.ManufacturerID);
            ee2232h.Description = Marshal.PtrToStringAnsi(eedata.Description);
            ee2232h.SerialNumber = Marshal.PtrToStringAnsi(eedata.SerialNumber);

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eedata.Manufacturer);
            Marshal.FreeHGlobal(eedata.ManufacturerID);
            Marshal.FreeHGlobal(eedata.Description);
            Marshal.FreeHGlobal(eedata.SerialNumber);

            // Map non-string elements to structure to be returned
            // Standard elements
            ee2232h.VendorID = eedata.VendorID;
            ee2232h.ProductID = eedata.ProductID;
            ee2232h.MaxPower = eedata.MaxPower;
            ee2232h.SelfPowered = Convert.ToBoolean(eedata.SelfPowered);
            ee2232h.RemoteWakeup = Convert.ToBoolean(eedata.RemoteWakeup);
            // 2232H specific fields
            ee2232h.PullDownEnable = Convert.ToBoolean(eedata.PullDownEnable7);
            ee2232h.SerNumEnable = Convert.ToBoolean(eedata.SerNumEnable7);
            ee2232h.ALSlowSlew = Convert.ToBoolean(eedata.ALSlowSlew);
            ee2232h.ALSchmittInput = Convert.ToBoolean(eedata.ALSchmittInput);
            ee2232h.ALDriveCurrent = eedata.ALDriveCurrent;
            ee2232h.AHSlowSlew = Convert.ToBoolean(eedata.AHSlowSlew);
            ee2232h.AHSchmittInput = Convert.ToBoolean(eedata.AHSchmittInput);
            ee2232h.AHDriveCurrent = eedata.AHDriveCurrent;
            ee2232h.BLSlowSlew = Convert.ToBoolean(eedata.BLSlowSlew);
            ee2232h.BLSchmittInput = Convert.ToBoolean(eedata.BLSchmittInput);
            ee2232h.BLDriveCurrent = eedata.BLDriveCurrent;
            ee2232h.BHSlowSlew = Convert.ToBoolean(eedata.BHSlowSlew);
            ee2232h.BHSchmittInput = Convert.ToBoolean(eedata.BHSchmittInput);
            ee2232h.BHDriveCurrent = eedata.BHDriveCurrent;
            ee2232h.IFAIsFifo = Convert.ToBoolean(eedata.IFAIsFifo7);
            ee2232h.IFAIsFifoTar = Convert.ToBoolean(eedata.IFAIsFifoTar7);
            ee2232h.IFAIsFastSer = Convert.ToBoolean(eedata.IFAIsFastSer7);
            ee2232h.AIsVCP = Convert.ToBoolean(eedata.AIsVCP7);
            ee2232h.IFBIsFifo = Convert.ToBoolean(eedata.IFBIsFifo7);
            ee2232h.IFBIsFifoTar = Convert.ToBoolean(eedata.IFBIsFifoTar7);
            ee2232h.IFBIsFastSer = Convert.ToBoolean(eedata.IFBIsFastSer7);
            ee2232h.BIsVCP = Convert.ToBoolean(eedata.BIsVCP7);
            ee2232h.PowerSaveEnable = Convert.ToBoolean(eedata.PowerSaveEnable);

            return ftStatus;
        }

		//**************************************************************************
		// ReadFT4232HEEPROM
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Reads the EEPROM contents of an FT4232H device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
		/// <param name="ee4232h">An FT4232H_EEPROM_STRUCTURE which contains only the relevant information for an FT4232H device.</param>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS Read_FT4232H_EEPROM(FT4232H_EEPROM_STRUCTURE ee4232h)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;
            FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_EE_Read FT_EE_Read = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_Read, typeof(tFT_EE_Read));

            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            // Check that it is an FT4232H that we are trying to read
            GetDeviceType(ref DeviceType);
            if (DeviceType != FT_DEVICETYPE.FT_DEVICE_4232H)
            {
                // If it is not, throw an exception
                ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

            // Set up structure headers
            eedata.Signature1 = 0x00000000;
            eedata.Signature2 = 0xFFFFFFFF;
            eedata.Version = 4;

            // Allocate space from unmanaged heap
            eedata.Manufacturer = Marshal.AllocHGlobal(32);
            eedata.ManufacturerID = Marshal.AllocHGlobal(16);
            eedata.Description = Marshal.AllocHGlobal(64);
            eedata.SerialNumber = Marshal.AllocHGlobal(16);

            // Call FT_EE_Read
            ftStatus = FT_EE_Read(ftHandle, eedata);

            // Retrieve string values
            ee4232h.Manufacturer = Marshal.PtrToStringAnsi(eedata.Manufacturer);
            ee4232h.ManufacturerID = Marshal.PtrToStringAnsi(eedata.ManufacturerID);
            ee4232h.Description = Marshal.PtrToStringAnsi(eedata.Description);
            ee4232h.SerialNumber = Marshal.PtrToStringAnsi(eedata.SerialNumber);

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eedata.Manufacturer);
            Marshal.FreeHGlobal(eedata.ManufacturerID);
            Marshal.FreeHGlobal(eedata.Description);
            Marshal.FreeHGlobal(eedata.SerialNumber);

            // Map non-string elements to structure to be returned
            // Standard elements
            ee4232h.VendorID = eedata.VendorID;
            ee4232h.ProductID = eedata.ProductID;
            ee4232h.MaxPower = eedata.MaxPower;
            ee4232h.SelfPowered = Convert.ToBoolean(eedata.SelfPowered);
            ee4232h.RemoteWakeup = Convert.ToBoolean(eedata.RemoteWakeup);
            // 4232H specific fields
            ee4232h.PullDownEnable = Convert.ToBoolean(eedata.PullDownEnable8);
            ee4232h.SerNumEnable = Convert.ToBoolean(eedata.SerNumEnable8);
            ee4232h.ASlowSlew = Convert.ToBoolean(eedata.ASlowSlew);
            ee4232h.ASchmittInput = Convert.ToBoolean(eedata.ASchmittInput);
            ee4232h.ADriveCurrent = eedata.ADriveCurrent;
            ee4232h.BSlowSlew = Convert.ToBoolean(eedata.BSlowSlew);
            ee4232h.BSchmittInput = Convert.ToBoolean(eedata.BSchmittInput);
            ee4232h.BDriveCurrent = eedata.BDriveCurrent;
            ee4232h.CSlowSlew = Convert.ToBoolean(eedata.CSlowSlew);
            ee4232h.CSchmittInput = Convert.ToBoolean(eedata.CSchmittInput);
            ee4232h.CDriveCurrent = eedata.CDriveCurrent;
            ee4232h.DSlowSlew = Convert.ToBoolean(eedata.DSlowSlew);
            ee4232h.DSchmittInput = Convert.ToBoolean(eedata.DSchmittInput);
            ee4232h.DDriveCurrent = eedata.DDriveCurrent;
            ee4232h.ARIIsTXDEN = Convert.ToBoolean(eedata.ARIIsTXDEN);
            ee4232h.BRIIsTXDEN = Convert.ToBoolean(eedata.BRIIsTXDEN);
            ee4232h.CRIIsTXDEN = Convert.ToBoolean(eedata.CRIIsTXDEN);
            ee4232h.DRIIsTXDEN = Convert.ToBoolean(eedata.DRIIsTXDEN);
            ee4232h.AIsVCP = Convert.ToBoolean(eedata.AIsVCP8);
            ee4232h.BIsVCP = Convert.ToBoolean(eedata.BIsVCP8);
            ee4232h.CIsVCP = Convert.ToBoolean(eedata.CIsVCP8);
            ee4232h.DIsVCP = Convert.ToBoolean(eedata.DIsVCP8);

            return ftStatus;
        }

		//**************************************************************************
		// ReadFT4232HAEEPROM
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Reads the EEPROM contents of an FT4232HA device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
		/// <param name="ee4232ha">An FT4232HA_EEPROM_STRUCTURE which contains only the relevant information for an FT4232HA device.</param>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
		public FT_STATUS Read_FT4232HA_EEPROM(ref FT4232H_EEPROM_STRUCTURE ee4232ha)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;
			FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (ftHandle == IntPtr.Zero)
				return ftStatus;

			tFT_EE_Read FT_EE_Read = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_Read, typeof(tFT_EE_Read));

			FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
			// Check that it is an FT4232HA that we are trying to read
			GetDeviceType(ref DeviceType);
			if (DeviceType != FT_DEVICETYPE.FT_DEVICE_4232HA)
			{
				// If it is not, throw an exception
				ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
				ErrorHandler(ftStatus, ftErrorCondition);
			}

			FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

			// Set up structure headers
			eedata.Signature1 = 0x00000000;
			eedata.Signature2 = 0xFFFFFFFF;
			eedata.Version = 4;

			// Allocate space from unmanaged heap
			eedata.Manufacturer = Marshal.AllocHGlobal(32);
			eedata.ManufacturerID = Marshal.AllocHGlobal(16);
			eedata.Description = Marshal.AllocHGlobal(64);
			eedata.SerialNumber = Marshal.AllocHGlobal(16);

			// Call FT_EE_Read
			ftStatus = FT_EE_Read(ftHandle, eedata);

			// Retrieve string values
			ee4232ha.Manufacturer = Marshal.PtrToStringAnsi(eedata.Manufacturer);
			ee4232ha.ManufacturerID = Marshal.PtrToStringAnsi(eedata.ManufacturerID);
			ee4232ha.Description = Marshal.PtrToStringAnsi(eedata.Description);
			ee4232ha.SerialNumber = Marshal.PtrToStringAnsi(eedata.SerialNumber);

			// Free unmanaged buffers
			Marshal.FreeHGlobal(eedata.Manufacturer);
			Marshal.FreeHGlobal(eedata.ManufacturerID);
			Marshal.FreeHGlobal(eedata.Description);
			Marshal.FreeHGlobal(eedata.SerialNumber);

			// Map non-string elements to structure to be returned
			// Standard elements
			ee4232ha.VendorID = eedata.VendorID;
			ee4232ha.ProductID = eedata.ProductID;
			ee4232ha.MaxPower = eedata.MaxPower;
			ee4232ha.SelfPowered = Convert.ToBoolean(eedata.SelfPowered);
			ee4232ha.RemoteWakeup = Convert.ToBoolean(eedata.RemoteWakeup);
			// 4232HA specific fields
			ee4232ha.PullDownEnable = Convert.ToBoolean(eedata.PullDownEnable8);
			ee4232ha.SerNumEnable = Convert.ToBoolean(eedata.SerNumEnable8);
			ee4232ha.ASlowSlew = Convert.ToBoolean(eedata.ASlowSlew);
			ee4232ha.ASchmittInput = Convert.ToBoolean(eedata.ASchmittInput);
			ee4232ha.ADriveCurrent = eedata.ADriveCurrent;
			ee4232ha.BSlowSlew = Convert.ToBoolean(eedata.BSlowSlew);
			ee4232ha.BSchmittInput = Convert.ToBoolean(eedata.BSchmittInput);
			ee4232ha.BDriveCurrent = eedata.BDriveCurrent;
			ee4232ha.CSlowSlew = Convert.ToBoolean(eedata.CSlowSlew);
			ee4232ha.CSchmittInput = Convert.ToBoolean(eedata.CSchmittInput);
			ee4232ha.CDriveCurrent = eedata.CDriveCurrent;
			ee4232ha.DSlowSlew = Convert.ToBoolean(eedata.DSlowSlew);
			ee4232ha.DSchmittInput = Convert.ToBoolean(eedata.DSchmittInput);
			ee4232ha.DDriveCurrent = eedata.DDriveCurrent;
			ee4232ha.ARIIsTXDEN = Convert.ToBoolean(eedata.ARIIsTXDEN);
			ee4232ha.BRIIsTXDEN = Convert.ToBoolean(eedata.BRIIsTXDEN);
			ee4232ha.CRIIsTXDEN = Convert.ToBoolean(eedata.CRIIsTXDEN);
			ee4232ha.DRIIsTXDEN = Convert.ToBoolean(eedata.DRIIsTXDEN);
			ee4232ha.AIsVCP = Convert.ToBoolean(eedata.AIsVCP8);
			ee4232ha.BIsVCP = Convert.ToBoolean(eedata.BIsVCP8);
			ee4232ha.CIsVCP = Convert.ToBoolean(eedata.CIsVCP8);
			ee4232ha.DIsVCP = Convert.ToBoolean(eedata.DIsVCP8);

			return ftStatus;
		}

		//**************************************************************************
		// ReadFT4233H EEPROM
		//**************************************************************************
		//read EEPROM
		//read Manufacturer
		//read ManufacturerID
		//read Description
		//read serial number
		//FT_EEPROM_Read
		public FT_STATUS Read_FT4233H_EEPROM(ref CEEPROM_FT4233H class_FT4233H_eeData  , ref string strError)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

			//check valid handle - DLL loaded
			if (ftHandle == IntPtr.Zero)
			{
				strError = "Error - Invalid Hnadle";
				return FT_STATUS.FT_INVALID_HANDLE;
			}

			//Check correct device FT4233H
			FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
			GetDeviceType(ref DeviceType);
			if (DeviceType != FT_DEVICETYPE.FT_DEVICE_4233HP)
			{
				strError = "Error - Device loaded=" + DeviceType.ToString() + "  -  Read Device=" + FT_DEVICETYPE.FT_DEVICE_4233HP.ToString();
				return FT_STATUS.FT_INVALID_PARAMETER;
			}

			tFT_EEPROM_Read FT_EEPROM_Read = (tFT_EEPROM_Read)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EEPROM_Read, typeof(tFT_EEPROM_Read));

			STRUCT_EEPROM_FT4233H stEEData = new STRUCT_EEPROM_FT4233H();
			FT_EEPROM_HEADER eeHeader = new FT_EEPROM_HEADER();

			byte[] manufacturer = new byte[32];
			byte[] manufacturerID = new byte[16];
			byte[] description = new byte[64];
			byte[] serialNumber = new byte[16];

			eeHeader.deviceType = (uint)FT_DEVICETYPE.FT_DEVICE_4233HP;
			stEEData.ft4232h.common = eeHeader;

			// Calculate the size of our data structure...
			int size = Marshal.SizeOf(stEEData);

			// Allocate space for our pointer...
			IntPtr ptrMarshalStructDataEE = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(stEEData, ptrMarshalStructDataEE, false);

			// Call FT_EEPROM_Read
			ftStatus = FT_EEPROM_Read(ftHandle, ptrMarshalStructDataEE, (uint)size, manufacturer, manufacturerID, description, serialNumber);

			if(ftStatus != FT_STATUS.FT_OK)
            {
				strError = "Error - Read EEPROM - " + ftStatus.ToString();
				return ftStatus;
			}

			if (ftStatus == FT_STATUS.FT_OK)
			{
				// Get the data back from the pointer...
				stEEData = (STRUCT_EEPROM_FT4233H)Marshal.PtrToStructure(ptrMarshalStructDataEE, typeof(STRUCT_EEPROM_FT4233H));
				//stEEData = (STRUCT_EEPROM_FT4232H)Marshal.PtrToStructure(ptrMarshalStructDataEE, typeof(STRUCT_EEPROM_FT4232H));

				class_FT4233H_eeData.Manufacturer = Encoding.UTF8.GetString(manufacturer).TrimEnd('\0');
				class_FT4233H_eeData.ManufacturerID = Encoding.UTF8.GetString(manufacturerID).TrimEnd('\0');
				class_FT4233H_eeData.Description = Encoding.UTF8.GetString(description).TrimEnd('\0');
				class_FT4233H_eeData.SerialNumber = Encoding.UTF8.GetString(serialNumber).TrimEnd('\0');

				class_FT4233H_eeData.VendorID = stEEData.ft4232h.common.VendorId;
				class_FT4233H_eeData.ProductID = stEEData.ft4232h.common.ProductId;
				class_FT4233H_eeData.SerNumEnable = Convert.ToBoolean(stEEData.ft4232h.common.SerNumEnable);
				class_FT4233H_eeData.MaxPower = stEEData.ft4232h.common.MaxPower;
				class_FT4233H_eeData.SelfPowered = Convert.ToBoolean(stEEData.ft4232h.common.SelfPowered);
				class_FT4233H_eeData.RemoteWakeup = Convert.ToBoolean(stEEData.ft4232h.common.RemoteWakeup);
				class_FT4233H_eeData.PullDownEnable = Convert.ToBoolean(stEEData.ft4232h.common.PullDownEnable);

				class_FT4233H_eeData.ASlowSlew = stEEData.ft4232h.ASlowSlew;
				class_FT4233H_eeData.ASchmittInput = stEEData.ft4232h.ASchmittInput;
				class_FT4233H_eeData.ADriveCurrent = stEEData.ft4232h.ADriveCurrent;
				class_FT4233H_eeData.BSlowSlew = stEEData.ft4232h.BSlowSlew;
				class_FT4233H_eeData.BSchmittInput = stEEData.ft4232h.BSchmittInput;
				class_FT4233H_eeData.BDriveCurrent = stEEData.ft4232h.BDriveCurrent;
				class_FT4233H_eeData.CSlowSlew = stEEData.ft4232h.CSlowSlew;
				class_FT4233H_eeData.CSchmittInput = stEEData.ft4232h.CSchmittInput;
				class_FT4233H_eeData.CDriveCurrent = stEEData.ft4232h.CDriveCurrent;
				class_FT4233H_eeData.DSlowSlew = stEEData.ft4232h.DSlowSlew;
				class_FT4233H_eeData.DSchmittInput = stEEData.ft4232h.DSchmittInput;
				class_FT4233H_eeData.DDriveCurrent = stEEData.ft4232h.DDriveCurrent;
				class_FT4233H_eeData.ARIIsTXDEN = stEEData.ft4232h.ARIIsTXDEN;
				class_FT4233H_eeData.BRIIsTXDEN = stEEData.ft4232h.BRIIsTXDEN;
				class_FT4233H_eeData.CRIIsTXDEN = stEEData.ft4232h.CRIIsTXDEN;
				class_FT4233H_eeData.DRIIsTXDEN = stEEData.ft4232h.DRIIsTXDEN;
				class_FT4233H_eeData.ADriverType = stEEData.ft4232h.ADriverType;
				class_FT4233H_eeData.BDriverType = stEEData.ft4232h.BDriverType;
				class_FT4233H_eeData.CDriverType = stEEData.ft4232h.CDriverType;
				class_FT4233H_eeData.DDriverType = stEEData.ft4232h.DDriverType;

				//PD

				class_FT4233H_eeData.srprs = stEEData.pd.srprs;				//Sink Request Power Role Swap
				class_FT4233H_eeData.sraprs = stEEData.pd.sraprs;			//Sink Accept Power Role Swap
				class_FT4233H_eeData.srrprs = stEEData.pd.srrprs;			//Source Request Power Role Swap
				class_FT4233H_eeData.saprs = stEEData.pd.saprs;				//Source Accept Power Role Swap
				class_FT4233H_eeData.vconns = stEEData.pd.vconns;			//VConn Swap
				class_FT4233H_eeData.passthru = stEEData.pd.passthru;		//Pass Through
				class_FT4233H_eeData.extmcu = stEEData.pd.extmcu;			//External MCU
				class_FT4233H_eeData.pd2en = stEEData.pd.pd2en;				//PD2 Enable
				class_FT4233H_eeData.pd1autoclk = stEEData.pd.pd1autoclk;	//PD1 Auto Clock
				class_FT4233H_eeData.pd2autoclk = stEEData.pd.pd2autoclk;	//PD2 Auto Clock
				class_FT4233H_eeData.useefuse = stEEData.pd.useefuse;		//Use EFuse
				class_FT4233H_eeData.extvconn = stEEData.pd.extvconn;		//External VConn

				class_FT4233H_eeData.count = stEEData.pd.count;				//GPIO Count
				class_FT4233H_eeData.gpio1 = stEEData.pd.gpio1;				//
				class_FT4233H_eeData.gpio2 = stEEData.pd.gpio2;
				class_FT4233H_eeData.gpio3 = stEEData.pd.gpio3;
				class_FT4233H_eeData.gpio4 = stEEData.pd.gpio4;
				class_FT4233H_eeData.gpio5 = stEEData.pd.gpio5;
				class_FT4233H_eeData.gpio6 = stEEData.pd.gpio6;
				class_FT4233H_eeData.gpio7 = stEEData.pd.gpio7;
				class_FT4233H_eeData.pd1lden = stEEData.pd.pd1lden;			//PD1 load Enable GPIO Pin Number
				class_FT4233H_eeData.pd2lden = stEEData.pd.pd2lden;         //PD2 load Enable GPIO Pin Number
				class_FT4233H_eeData.dispin = stEEData.pd.dispin;			//GPIO Pin enable/disable discharge circuit
				class_FT4233H_eeData.disenbm = stEEData.pd.disenbm;			//
				class_FT4233H_eeData.disdisbm = stEEData.pd.disdisbm;
				class_FT4233H_eeData.ccselect = stEEData.pd.ccselect;

				class_FT4233H_eeData.iset1 = stEEData.pd.iset1;
				class_FT4233H_eeData.iset2 = stEEData.pd.iset2;
				class_FT4233H_eeData.iset3 = stEEData.pd.iset3;
				class_FT4233H_eeData.extiset = stEEData.pd.extiset;
				class_FT4233H_eeData.isetpd2 = stEEData.pd.isetpd2;
				class_FT4233H_eeData.iseten = stEEData.pd.iseten;

				unsafe
				{
					int iLoop;
					for (iLoop = 0; iLoop < 7; iLoop++)
					{
						class_FT4233H_eeData.PDO1_GPIO[iLoop] = stEEData.pd.PDO1_GPIO[iLoop];
					}
					for (iLoop = 0; iLoop < 7; iLoop++)
					{
						class_FT4233H_eeData.PDO2_GPIO[iLoop] = stEEData.pd.PDO2_GPIO[iLoop];
					}
					for (iLoop = 0; iLoop < 7; iLoop++)
					{
						class_FT4233H_eeData.PDO3_GPIO[iLoop] = stEEData.pd.PDO3_GPIO[iLoop];
					}
					for (iLoop = 0; iLoop < 7; iLoop++)
					{
						class_FT4233H_eeData.PDO4_GPIO[iLoop] = stEEData.pd.PDO4_GPIO[iLoop];
					}
					for (iLoop = 0; iLoop < 7; iLoop++)
					{
						class_FT4233H_eeData.PDO5_GPIO[iLoop] = stEEData.pd.PDO5_GPIO[iLoop];
					}
					for (iLoop = 0; iLoop < 7; iLoop++)
					{
						class_FT4233H_eeData.PDO6_GPIO[iLoop] = stEEData.pd.PDO6_GPIO[iLoop];
					}
					for (iLoop = 0; iLoop < 7; iLoop++)
					{
						class_FT4233H_eeData.PDO7_GPIO[iLoop] = stEEData.pd.PDO7_GPIO[iLoop];
					}

					for (iLoop = 0; iLoop < 7; iLoop++)
					{
						class_FT4233H_eeData.VSET0V_GPIO[iLoop] = stEEData.pd.VSET0V_GPIO[iLoop];
					}
					for (iLoop = 0; iLoop < 7; iLoop++)
					{
						class_FT4233H_eeData.VSAFE5V_GPIO[iLoop] = stEEData.pd.VSAFE5V_GPIO[iLoop];
					}
				}

				class_FT4233H_eeData.BM_PDO_Sink.PDO1ma = stEEData.pd.BM_PDO_Sink.PDO1ma;
				class_FT4233H_eeData.BM_PDO_Sink.PDO1mv = stEEData.pd.BM_PDO_Sink.PDO1mv;
				class_FT4233H_eeData.BM_PDO_Sink.PDO2ma = stEEData.pd.BM_PDO_Sink.PDO2ma;
				class_FT4233H_eeData.BM_PDO_Sink.PDO2mv = stEEData.pd.BM_PDO_Sink.PDO2mv;
				class_FT4233H_eeData.BM_PDO_Sink.PDO3ma = stEEData.pd.BM_PDO_Sink.PDO3ma;
				class_FT4233H_eeData.BM_PDO_Sink.PDO3mv = stEEData.pd.BM_PDO_Sink.PDO3mv;
				class_FT4233H_eeData.BM_PDO_Sink.PDO4ma = stEEData.pd.BM_PDO_Sink.PDO4ma;
				class_FT4233H_eeData.BM_PDO_Sink.PDO4mv = stEEData.pd.BM_PDO_Sink.PDO4mv;
				class_FT4233H_eeData.BM_PDO_Sink.PDO5ma = stEEData.pd.BM_PDO_Sink.PDO5ma;
				class_FT4233H_eeData.BM_PDO_Sink.PDO5mv = stEEData.pd.BM_PDO_Sink.PDO5mv;
				class_FT4233H_eeData.BM_PDO_Sink.PDO6ma = stEEData.pd.BM_PDO_Sink.PDO6ma;
				class_FT4233H_eeData.BM_PDO_Sink.PDO6mv = stEEData.pd.BM_PDO_Sink.PDO6mv;
				class_FT4233H_eeData.BM_PDO_Sink.PDO7ma = stEEData.pd.BM_PDO_Sink.PDO7ma;
				class_FT4233H_eeData.BM_PDO_Sink.PDO7mv = stEEData.pd.BM_PDO_Sink.PDO7mv;

				class_FT4233H_eeData.BM_PDO_Source.PDO1ma = stEEData.pd.BM_PDO_Source.PDO1ma;
				class_FT4233H_eeData.BM_PDO_Source.PDO1mv = stEEData.pd.BM_PDO_Source.PDO1mv;
				class_FT4233H_eeData.BM_PDO_Source.PDO2ma = stEEData.pd.BM_PDO_Source.PDO2ma;
				class_FT4233H_eeData.BM_PDO_Source.PDO2mv = stEEData.pd.BM_PDO_Source.PDO2mv;
				class_FT4233H_eeData.BM_PDO_Source.PDO3ma = stEEData.pd.BM_PDO_Source.PDO3ma;
				class_FT4233H_eeData.BM_PDO_Source.PDO3mv = stEEData.pd.BM_PDO_Source.PDO3mv;
				class_FT4233H_eeData.BM_PDO_Source.PDO4ma = stEEData.pd.BM_PDO_Source.PDO4ma;
				class_FT4233H_eeData.BM_PDO_Source.PDO4mv = stEEData.pd.BM_PDO_Source.PDO4mv;
				class_FT4233H_eeData.BM_PDO_Source.PDO5ma = stEEData.pd.BM_PDO_Source.PDO5ma;
				class_FT4233H_eeData.BM_PDO_Source.PDO5mv = stEEData.pd.BM_PDO_Source.PDO5mv;
				class_FT4233H_eeData.BM_PDO_Source.PDO6ma = stEEData.pd.BM_PDO_Source.PDO6ma;
				class_FT4233H_eeData.BM_PDO_Source.PDO6mv = stEEData.pd.BM_PDO_Source.PDO6mv;
				class_FT4233H_eeData.BM_PDO_Source.PDO7ma = stEEData.pd.BM_PDO_Source.PDO7ma;
				class_FT4233H_eeData.BM_PDO_Source.PDO7mv = stEEData.pd.BM_PDO_Source.PDO7mv;

				class_FT4233H_eeData.BM_PDO_Sink_2.PDO1ma = stEEData.pd.BM_PDO_Sink_2.PDO1ma;
				class_FT4233H_eeData.BM_PDO_Sink_2.PDO1mv = stEEData.pd.BM_PDO_Sink_2.PDO1mv;
				class_FT4233H_eeData.BM_PDO_Sink_2.PDO2ma = stEEData.pd.BM_PDO_Sink_2.PDO2ma;
				class_FT4233H_eeData.BM_PDO_Sink_2.PDO2mv = stEEData.pd.BM_PDO_Sink_2.PDO2mv;
				class_FT4233H_eeData.BM_PDO_Sink_2.PDO3ma = stEEData.pd.BM_PDO_Sink_2.PDO3ma;
				class_FT4233H_eeData.BM_PDO_Sink_2.PDO3mv = stEEData.pd.BM_PDO_Sink_2.PDO3mv;
				class_FT4233H_eeData.BM_PDO_Sink_2.PDO4ma = stEEData.pd.BM_PDO_Sink_2.PDO4ma;
				class_FT4233H_eeData.BM_PDO_Sink_2.PDO4mv = stEEData.pd.BM_PDO_Sink_2.PDO4mv;
				class_FT4233H_eeData.BM_PDO_Sink_2.PDO5ma = stEEData.pd.BM_PDO_Sink_2.PDO5ma;
				class_FT4233H_eeData.BM_PDO_Sink_2.PDO5mv = stEEData.pd.BM_PDO_Sink_2.PDO5mv;
				class_FT4233H_eeData.BM_PDO_Sink_2.PDO6ma = stEEData.pd.BM_PDO_Sink_2.PDO6ma;
				class_FT4233H_eeData.BM_PDO_Sink_2.PDO6mv = stEEData.pd.BM_PDO_Sink_2.PDO6mv;
				class_FT4233H_eeData.BM_PDO_Sink_2.PDO7ma = stEEData.pd.BM_PDO_Sink_2.PDO7ma;
				class_FT4233H_eeData.BM_PDO_Sink_2.PDO7mv = stEEData.pd.BM_PDO_Sink_2.PDO7mv;

				class_FT4233H_eeData.srt = stEEData.pd.srt;		// Sender Response Timer
				class_FT4233H_eeData.hrt = stEEData.pd.hrt;		// Hard Reset Timer
				class_FT4233H_eeData.sct = stEEData.pd.sct;		// Source Capability Timer
				class_FT4233H_eeData.dit = stEEData.pd.dit;		// Discover Identity Timer
				class_FT4233H_eeData.srcrt = stEEData.pd.srcrt;	//Source Recover Timer
				class_FT4233H_eeData.trt = stEEData.pd.trt;		//Transition Timer
				class_FT4233H_eeData.sofft = stEEData.pd.sofft;	//Source Off Timer
				class_FT4233H_eeData.nrt = stEEData.pd.nrt;		//No Response Timer
				class_FT4233H_eeData.swct = stEEData.pd.swct;	//Sink Wait Capability Timer
				class_FT4233H_eeData.snkrt = stEEData.pd.snkrt;	//Sink Request Timer
				class_FT4233H_eeData.dt = stEEData.pd.dt;		//Discharge Timer
				class_FT4233H_eeData.cnst = stEEData.pd.cnst;	//Chunk not Supported Timer
				class_FT4233H_eeData.it = stEEData.pd.it;		//Idle Timer

				class_FT4233H_eeData.i2caddr = stEEData.pd.i2caddr;		//
				class_FT4233H_eeData.prou = stEEData.pd.prou;
				class_FT4233H_eeData.trim1 = stEEData.pd.trim1;
				class_FT4233H_eeData.trim2 = stEEData.pd.trim2;
				class_FT4233H_eeData.extdc = stEEData.pd.extdc;

			}

			return ftStatus;
		}

		//read 4233HP using EE read
		public FT_STATUS Read_FT4233H_EE(CEEPROM_FT4233H class_FT4233H_eeData, ref string strError)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

			//check valid handle - DLL loaded
			if (ftHandle == IntPtr.Zero)
			{
				strError = "Error - Invalid Hnadle";
				return FT_STATUS.FT_INVALID_HANDLE;
			}

			//Check correct device FT4233H
			FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
			GetDeviceType(ref DeviceType);
			if (DeviceType != FT_DEVICETYPE.FT_DEVICE_4233HP)
			{
				strError = "Error - Device loaded=" + DeviceType.ToString() + "  -  Read Device=" + FT_DEVICETYPE.FT_DEVICE_4233HP.ToString();
				return FT_STATUS.FT_INVALID_PARAMETER;
			}

			tFT_EE_Read FT_EE_Read = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_Read, typeof(tFT_EE_Read));

			FT_PROGRAM_DATA classEEData = new FT_PROGRAM_DATA();

			// Set up structure headers
			classEEData.Signature1 = 0x00000000;
			classEEData.Signature2 = 0xFFFFFFFF;
			classEEData.Version = 4;

			// Allocate space from unmanaged heap
			classEEData.Manufacturer = Marshal.AllocHGlobal(32);
			classEEData.ManufacturerID = Marshal.AllocHGlobal(16);
			classEEData.Description = Marshal.AllocHGlobal(64);
			classEEData.SerialNumber = Marshal.AllocHGlobal(16);

			// Call FT_EE_Read
			ftStatus = FT_EE_Read(ftHandle, classEEData);

			// Retrieve string values
			class_FT4233H_eeData.Manufacturer = Marshal.PtrToStringAnsi(classEEData.Manufacturer);
			class_FT4233H_eeData.ManufacturerID = Marshal.PtrToStringAnsi(classEEData.ManufacturerID);
			class_FT4233H_eeData.Description = Marshal.PtrToStringAnsi(classEEData.Description);
			class_FT4233H_eeData.SerialNumber = Marshal.PtrToStringAnsi(classEEData.SerialNumber);

			// Free unmanaged buffers
			Marshal.FreeHGlobal(classEEData.Manufacturer);
			Marshal.FreeHGlobal(classEEData.ManufacturerID);
			Marshal.FreeHGlobal(classEEData.Description);
			Marshal.FreeHGlobal(classEEData.SerialNumber);

			// Map non-string elements to structure to be returned
			// Standard elements
			class_FT4233H_eeData.VendorID = classEEData.VendorID;
			class_FT4233H_eeData.ProductID = classEEData.ProductID;
			class_FT4233H_eeData.MaxPower = classEEData.MaxPower;
			class_FT4233H_eeData.SelfPowered = Convert.ToBoolean(classEEData.SelfPowered);
			class_FT4233H_eeData.RemoteWakeup = Convert.ToBoolean(classEEData.RemoteWakeup);


			return ftStatus;
		}



		//**************************************************************************
		// ReadFT232HEEPROM
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Reads the EEPROM contents of an FT232H device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
		/// <param name="ee232h">An FT232H_EEPROM_STRUCTURE which contains only the relevant information for an FT232H device.</param>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
		public FT_STATUS Read_FT232H_EEPROM(ref FT232H_EEPROM_STRUCTURE ee232h)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;
            FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_EE_Read FT_EE_Read = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_Read, typeof(tFT_EE_Read));

            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            // Check that it is an FT232H that we are trying to read
            GetDeviceType(ref DeviceType);
            if (DeviceType != FT_DEVICETYPE.FT_DEVICE_232H)
            {
                // If it is not, throw an exception
                ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

            // Set up structure headers
            eedata.Signature1 = 0x00000000;
            eedata.Signature2 = 0xFFFFFFFF;
            eedata.Version = 5;

            // Allocate space from unmanaged heap
            eedata.Manufacturer = Marshal.AllocHGlobal(32);
            eedata.ManufacturerID = Marshal.AllocHGlobal(16);
            eedata.Description = Marshal.AllocHGlobal(64);
            eedata.SerialNumber = Marshal.AllocHGlobal(16);

            // Call FT_EE_Read
            ftStatus = FT_EE_Read(ftHandle, eedata);

            // Retrieve string values
            ee232h.Manufacturer = Marshal.PtrToStringAnsi(eedata.Manufacturer);
            ee232h.ManufacturerID = Marshal.PtrToStringAnsi(eedata.ManufacturerID);
            ee232h.Description = Marshal.PtrToStringAnsi(eedata.Description);
            ee232h.SerialNumber = Marshal.PtrToStringAnsi(eedata.SerialNumber);

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eedata.Manufacturer);
            Marshal.FreeHGlobal(eedata.ManufacturerID);
            Marshal.FreeHGlobal(eedata.Description);
            Marshal.FreeHGlobal(eedata.SerialNumber);

            // Map non-string elements to structure to be returned
            // Standard elements
            ee232h.VendorID = eedata.VendorID;
            ee232h.ProductID = eedata.ProductID;
            ee232h.MaxPower = eedata.MaxPower;
            ee232h.SelfPowered = Convert.ToBoolean(eedata.SelfPowered);
            ee232h.RemoteWakeup = Convert.ToBoolean(eedata.RemoteWakeup);
            // 232H specific fields
            ee232h.PullDownEnable = Convert.ToBoolean(eedata.PullDownEnableH);
            ee232h.SerNumEnable = Convert.ToBoolean(eedata.SerNumEnableH);
            ee232h.ACSlowSlew = Convert.ToBoolean(eedata.ACSlowSlewH);
            ee232h.ACSchmittInput = Convert.ToBoolean(eedata.ACSchmittInputH);
            ee232h.ACDriveCurrent = eedata.ACDriveCurrentH;
            ee232h.ADSlowSlew = Convert.ToBoolean(eedata.ADSlowSlewH);
            ee232h.ADSchmittInput = Convert.ToBoolean(eedata.ADSchmittInputH);
            ee232h.ADDriveCurrent = eedata.ADDriveCurrentH;
            ee232h.Cbus0 = eedata.Cbus0H;
            ee232h.Cbus1 = eedata.Cbus1H;
            ee232h.Cbus2 = eedata.Cbus2H;
            ee232h.Cbus3 = eedata.Cbus3H;
            ee232h.Cbus4 = eedata.Cbus4H;
            ee232h.Cbus5 = eedata.Cbus5H;
            ee232h.Cbus6 = eedata.Cbus6H;
            ee232h.Cbus7 = eedata.Cbus7H;
            ee232h.Cbus8 = eedata.Cbus8H;
            ee232h.Cbus9 = eedata.Cbus9H;
            ee232h.IsFifo = Convert.ToBoolean(eedata.IsFifoH);
            ee232h.IsFifoTar = Convert.ToBoolean(eedata.IsFifoTarH);
            ee232h.IsFastSer = Convert.ToBoolean(eedata.IsFastSerH);
            ee232h.IsFT1248 = Convert.ToBoolean(eedata.IsFT1248H);
            ee232h.FT1248Cpol = Convert.ToBoolean(eedata.FT1248CpolH);
            ee232h.FT1248Lsb = Convert.ToBoolean(eedata.FT1248LsbH);
            ee232h.FT1248FlowControl = Convert.ToBoolean(eedata.FT1248FlowControlH);
            ee232h.IsVCP = Convert.ToBoolean(eedata.IsVCPH);
            ee232h.PowerSaveEnable = Convert.ToBoolean(eedata.PowerSaveEnableH);

            return ftStatus;
        }

		//**************************************************************************
		// ReadFT233HEEPROM
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Reads the EEPROM contents of an FT233H device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_EEPROM_Read in FTD2XX DLL</returns>
		/// <param name="ee233h">An FT233H_EEPROM_STRUCTURE which contains only the relevant information for an FT233H device.</param>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
		public FT_STATUS Read_FT233H_EEPROM(ref CEEPROM_FT233H class_FT233H_eeData, ref string strError)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

			//check valid handle - DLL loaded
			if (ftHandle == IntPtr.Zero)
			{
				strError = "Error - Invalid Hnadle";
				return FT_STATUS.FT_INVALID_HANDLE;
			}

			//Check correct device FT233H
			FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
			GetDeviceType(ref DeviceType);
			if (DeviceType != FT_DEVICETYPE.FT_DEVICE_233HP)
			{
				strError = "Error - Device loaded=" + DeviceType.ToString() + "  -  Read Device=" + FT_DEVICETYPE.FT_DEVICE_233HP.ToString();
				return FT_STATUS.FT_INVALID_PARAMETER;
			}

			tFT_EEPROM_Read FT_EEPROM_Read = (tFT_EEPROM_Read)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EEPROM_Read, typeof(tFT_EEPROM_Read));

			STRUCT_EEPROM_FT233HP stEEPROMData = new STRUCT_EEPROM_FT233HP();
			FT_EEPROM_HEADER eeHeader = new FT_EEPROM_HEADER();

			byte[] manufacturer = new byte[32];
			byte[] manufacturerID = new byte[16];
			byte[] description = new byte[64];
			byte[] serialNumber = new byte[16];

			eeHeader.deviceType = (uint)FT_DEVICETYPE.FT_DEVICE_233HP;
			stEEPROMData.ft232h.common = eeHeader;

			// Calculate the size of our data structure...
			int size = Marshal.SizeOf(stEEPROMData);

			// Allocate space for our pointer...
			IntPtr ptrMarshalStructDataEE = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(stEEPROMData, ptrMarshalStructDataEE, false);

			// Call FT_EEPROM_Read
			ftStatus = FT_EEPROM_Read(ftHandle, ptrMarshalStructDataEE, (uint)size, manufacturer, manufacturerID, description, serialNumber);

			if (ftStatus != FT_STATUS.FT_OK)
			{
				strError = "Error - Read EEPROM - " + ftStatus.ToString();
				return ftStatus;
			}

			if (ftStatus == FT_STATUS.FT_OK)
			{
				// Get the data back from the pointer...
				stEEPROMData = (STRUCT_EEPROM_FT233HP)Marshal.PtrToStructure(ptrMarshalStructDataEE, typeof(STRUCT_EEPROM_FT233HP));
				//stEEData = (STRUCT_EEPROM_FT4232H)Marshal.PtrToStructure(ptrMarshalStructDataEE, typeof(STRUCT_EEPROM_FT4232H));

				class_FT233H_eeData.Manufacturer = Encoding.UTF8.GetString(manufacturer).TrimEnd('\0');
				class_FT233H_eeData.ManufacturerID = Encoding.UTF8.GetString(manufacturerID).TrimEnd('\0');
				class_FT233H_eeData.Description = Encoding.UTF8.GetString(description).TrimEnd('\0');
				class_FT233H_eeData.SerialNumber = Encoding.UTF8.GetString(serialNumber).TrimEnd('\0');

				class_FT233H_eeData.VendorID = stEEPROMData.ft232h.common.VendorId;
				class_FT233H_eeData.ProductID = stEEPROMData.ft232h.common.ProductId;
				class_FT233H_eeData.SerNumEnable = Convert.ToBoolean(stEEPROMData.ft232h.common.SerNumEnable);
				class_FT233H_eeData.MaxPower = stEEPROMData.ft232h.common.MaxPower;
				class_FT233H_eeData.SelfPowered = Convert.ToBoolean(stEEPROMData.ft232h.common.SelfPowered);
				class_FT233H_eeData.RemoteWakeup = Convert.ToBoolean(stEEPROMData.ft232h.common.RemoteWakeup);
				class_FT233H_eeData.PullDownEnable = Convert.ToBoolean(stEEPROMData.ft232h.common.PullDownEnable);

				class_FT233H_eeData.ACSlowSlew = stEEPROMData.ft232h.ACSlowSlew;
				class_FT233H_eeData.ACSchmittInput = stEEPROMData.ft232h.ACSchmittInput;
				class_FT233H_eeData.ACDriveCurrent = stEEPROMData.ft232h.ACDriveCurrent;
				class_FT233H_eeData.ADSlowSlew = stEEPROMData.ft232h.ACSlowSlew;
				class_FT233H_eeData.ADSchmittInput = stEEPROMData.ft232h.ADSchmittInput;
				class_FT233H_eeData.ADDriveCurrent = stEEPROMData.ft232h.ADDriveCurrent;

				class_FT233H_eeData.Cbus0 = stEEPROMData.ft232h.Cbus0;
				class_FT233H_eeData.Cbus1 = stEEPROMData.ft232h.Cbus1;
				class_FT233H_eeData.Cbus2 = stEEPROMData.ft232h.Cbus2;
				class_FT233H_eeData.Cbus3 = stEEPROMData.ft232h.Cbus3;
				class_FT233H_eeData.Cbus4 = stEEPROMData.ft232h.Cbus4;
				class_FT233H_eeData.Cbus5 = stEEPROMData.ft232h.Cbus5;
				class_FT233H_eeData.Cbus6 = stEEPROMData.ft232h.Cbus6;
				class_FT233H_eeData.Cbus7 = stEEPROMData.ft232h.Cbus7;
				class_FT233H_eeData.Cbus8 = stEEPROMData.ft232h.Cbus8;
				class_FT233H_eeData.Cbus9 = stEEPROMData.ft232h.Cbus9;
				class_FT233H_eeData.IsFifo = (stEEPROMData.ft232h.IsFifo);
				class_FT233H_eeData.IsFifoTar = (stEEPROMData.ft232h.IsFifoTar);
				class_FT233H_eeData.IsFastSer = (stEEPROMData.ft232h.IsFastSer);
				class_FT233H_eeData.IsFT1248 = (stEEPROMData.ft232h.IsFT1248);
				class_FT233H_eeData.FT1248Cpol = (stEEPROMData.ft232h.FT1248Cpol);
				class_FT233H_eeData.FT1248Lsb = (stEEPROMData.ft232h.FT1248Lsb);
				class_FT233H_eeData.FT1248FlowControl = (stEEPROMData.ft232h.FT1248FlowControl);
				class_FT233H_eeData.DriverType = (stEEPROMData.ft232h.DriverType);
				class_FT233H_eeData.PowerSaveEnable = (stEEPROMData.ft232h.PowerSaveEnable);

				//PD

				class_FT233H_eeData.srprs = stEEPROMData.pd.srprs;             //Sink Request Power Role Swap
				class_FT233H_eeData.sraprs = stEEPROMData.pd.sraprs;           //Sink Accept Power Role Swap
				class_FT233H_eeData.srrprs = stEEPROMData.pd.srrprs;           //Source Request Power Role Swap
				class_FT233H_eeData.saprs = stEEPROMData.pd.saprs;             //Source Accept Power Role Swap
				class_FT233H_eeData.vconns = stEEPROMData.pd.vconns;           //VConn Swap
				class_FT233H_eeData.passthru = stEEPROMData.pd.passthru;       //Pass Through
				class_FT233H_eeData.extmcu = stEEPROMData.pd.extmcu;           //External MCU
				class_FT233H_eeData.pd2en = stEEPROMData.pd.pd2en;             //PD2 Enable
				class_FT233H_eeData.pd1autoclk = stEEPROMData.pd.pd1autoclk;   //PD1 Auto Clock
				class_FT233H_eeData.pd2autoclk = stEEPROMData.pd.pd2autoclk;   //PD2 Auto Clock
				class_FT233H_eeData.useefuse = stEEPROMData.pd.useefuse;       //Use EFuse
				class_FT233H_eeData.extvconn = stEEPROMData.pd.extvconn;       //External VConn

				class_FT233H_eeData.count = stEEPROMData.pd.count;             //GPIO Count
				class_FT233H_eeData.gpio1 = stEEPROMData.pd.gpio1;             //
				class_FT233H_eeData.gpio2 = stEEPROMData.pd.gpio2;
				class_FT233H_eeData.gpio3 = stEEPROMData.pd.gpio3;
				class_FT233H_eeData.gpio4 = stEEPROMData.pd.gpio4;
				class_FT233H_eeData.gpio5 = stEEPROMData.pd.gpio5;
				class_FT233H_eeData.gpio6 = stEEPROMData.pd.gpio6;
				class_FT233H_eeData.gpio7 = stEEPROMData.pd.gpio7;
				class_FT233H_eeData.pd1lden = stEEPROMData.pd.pd1lden;         //PD1 load Enable GPIO Pin Number
				class_FT233H_eeData.pd2lden = stEEPROMData.pd.pd2lden;         //PD2 load Enable GPIO Pin Number
				class_FT233H_eeData.dispin = stEEPROMData.pd.dispin;           //GPIO Pin enable/disable discharge circuit
				class_FT233H_eeData.disenbm = stEEPROMData.pd.disenbm;         //
				class_FT233H_eeData.disdisbm = stEEPROMData.pd.disdisbm;
				class_FT233H_eeData.ccselect = stEEPROMData.pd.ccselect;

				class_FT233H_eeData.iset1 = stEEPROMData.pd.iset1;
				class_FT233H_eeData.iset2 = stEEPROMData.pd.iset2;
				class_FT233H_eeData.iset3 = stEEPROMData.pd.iset3;
				class_FT233H_eeData.extiset = stEEPROMData.pd.extiset;
				class_FT233H_eeData.isetpd2 = stEEPROMData.pd.isetpd2;
				class_FT233H_eeData.iseten = stEEPROMData.pd.iseten;

				unsafe
				{
					int iLoop;
					for (iLoop = 0; iLoop < 7; iLoop++)
					{
						class_FT233H_eeData.PDO1_GPIO[iLoop] = stEEPROMData.pd.PDO1_GPIO[iLoop];
					}
					for (iLoop = 0; iLoop < 7; iLoop++)
					{
						class_FT233H_eeData.PDO2_GPIO[iLoop] = stEEPROMData.pd.PDO2_GPIO[iLoop];
					}
					for (iLoop = 0; iLoop < 7; iLoop++)
					{
						class_FT233H_eeData.PDO3_GPIO[iLoop] = stEEPROMData.pd.PDO3_GPIO[iLoop];
					}
					for (iLoop = 0; iLoop < 7; iLoop++)
					{
						class_FT233H_eeData.PDO4_GPIO[iLoop] = stEEPROMData.pd.PDO4_GPIO[iLoop];
					}
					for (iLoop = 0; iLoop < 7; iLoop++)
					{
						class_FT233H_eeData.PDO5_GPIO[iLoop] = stEEPROMData.pd.PDO5_GPIO[iLoop];
					}
					for (iLoop = 0; iLoop < 7; iLoop++)
					{
						class_FT233H_eeData.PDO6_GPIO[iLoop] = stEEPROMData.pd.PDO6_GPIO[iLoop];
					}
					for (iLoop = 0; iLoop < 7; iLoop++)
					{
						class_FT233H_eeData.PDO7_GPIO[iLoop] = stEEPROMData.pd.PDO7_GPIO[iLoop];
					}

					for (iLoop = 0; iLoop < 7; iLoop++)
					{
						class_FT233H_eeData.VSET0V_GPIO[iLoop] = stEEPROMData.pd.VSET0V_GPIO[iLoop];
					}
					for (iLoop = 0; iLoop < 7; iLoop++)
					{
						class_FT233H_eeData.VSAFE5V_GPIO[iLoop] = stEEPROMData.pd.VSAFE5V_GPIO[iLoop];
					}
				}

				class_FT233H_eeData.BM_PDO_Sink.PDO1ma = stEEPROMData.pd.BM_PDO_Sink.PDO1ma;
				class_FT233H_eeData.BM_PDO_Sink.PDO1mv = stEEPROMData.pd.BM_PDO_Sink.PDO1mv;
				class_FT233H_eeData.BM_PDO_Sink.PDO2ma = stEEPROMData.pd.BM_PDO_Sink.PDO2ma;
				class_FT233H_eeData.BM_PDO_Sink.PDO2mv = stEEPROMData.pd.BM_PDO_Sink.PDO2mv;
				class_FT233H_eeData.BM_PDO_Sink.PDO3ma = stEEPROMData.pd.BM_PDO_Sink.PDO3ma;
				class_FT233H_eeData.BM_PDO_Sink.PDO3mv = stEEPROMData.pd.BM_PDO_Sink.PDO3mv;
				class_FT233H_eeData.BM_PDO_Sink.PDO4ma = stEEPROMData.pd.BM_PDO_Sink.PDO4ma;
				class_FT233H_eeData.BM_PDO_Sink.PDO4mv = stEEPROMData.pd.BM_PDO_Sink.PDO4mv;
				class_FT233H_eeData.BM_PDO_Sink.PDO5ma = stEEPROMData.pd.BM_PDO_Sink.PDO5ma;
				class_FT233H_eeData.BM_PDO_Sink.PDO5mv = stEEPROMData.pd.BM_PDO_Sink.PDO5mv;
				class_FT233H_eeData.BM_PDO_Sink.PDO6ma = stEEPROMData.pd.BM_PDO_Sink.PDO6ma;
				class_FT233H_eeData.BM_PDO_Sink.PDO6mv = stEEPROMData.pd.BM_PDO_Sink.PDO6mv;
				class_FT233H_eeData.BM_PDO_Sink.PDO7ma = stEEPROMData.pd.BM_PDO_Sink.PDO7ma;
				class_FT233H_eeData.BM_PDO_Sink.PDO7mv = stEEPROMData.pd.BM_PDO_Sink.PDO7mv;

				class_FT233H_eeData.BM_PDO_Source.PDO1ma = stEEPROMData.pd.BM_PDO_Source.PDO1ma;
				class_FT233H_eeData.BM_PDO_Source.PDO1mv = stEEPROMData.pd.BM_PDO_Source.PDO1mv;
				class_FT233H_eeData.BM_PDO_Source.PDO2ma = stEEPROMData.pd.BM_PDO_Source.PDO2ma;
				class_FT233H_eeData.BM_PDO_Source.PDO2mv = stEEPROMData.pd.BM_PDO_Source.PDO2mv;
				class_FT233H_eeData.BM_PDO_Source.PDO3ma = stEEPROMData.pd.BM_PDO_Source.PDO3ma;
				class_FT233H_eeData.BM_PDO_Source.PDO3mv = stEEPROMData.pd.BM_PDO_Source.PDO3mv;
				class_FT233H_eeData.BM_PDO_Source.PDO4ma = stEEPROMData.pd.BM_PDO_Source.PDO4ma;
				class_FT233H_eeData.BM_PDO_Source.PDO4mv = stEEPROMData.pd.BM_PDO_Source.PDO4mv;
				class_FT233H_eeData.BM_PDO_Source.PDO5ma = stEEPROMData.pd.BM_PDO_Source.PDO5ma;
				class_FT233H_eeData.BM_PDO_Source.PDO5mv = stEEPROMData.pd.BM_PDO_Source.PDO5mv;
				class_FT233H_eeData.BM_PDO_Source.PDO6ma = stEEPROMData.pd.BM_PDO_Source.PDO6ma;
				class_FT233H_eeData.BM_PDO_Source.PDO6mv = stEEPROMData.pd.BM_PDO_Source.PDO6mv;
				class_FT233H_eeData.BM_PDO_Source.PDO7ma = stEEPROMData.pd.BM_PDO_Source.PDO7ma;
				class_FT233H_eeData.BM_PDO_Source.PDO7mv = stEEPROMData.pd.BM_PDO_Source.PDO7mv;

				class_FT233H_eeData.BM_PDO_Sink_2.PDO1ma = stEEPROMData.pd.BM_PDO_Sink_2.PDO1ma;
				class_FT233H_eeData.BM_PDO_Sink_2.PDO1mv = stEEPROMData.pd.BM_PDO_Sink_2.PDO1mv;
				class_FT233H_eeData.BM_PDO_Sink_2.PDO2ma = stEEPROMData.pd.BM_PDO_Sink_2.PDO2ma;
				class_FT233H_eeData.BM_PDO_Sink_2.PDO2mv = stEEPROMData.pd.BM_PDO_Sink_2.PDO2mv;
				class_FT233H_eeData.BM_PDO_Sink_2.PDO3ma = stEEPROMData.pd.BM_PDO_Sink_2.PDO3ma;
				class_FT233H_eeData.BM_PDO_Sink_2.PDO3mv = stEEPROMData.pd.BM_PDO_Sink_2.PDO3mv;
				class_FT233H_eeData.BM_PDO_Sink_2.PDO4ma = stEEPROMData.pd.BM_PDO_Sink_2.PDO4ma;
				class_FT233H_eeData.BM_PDO_Sink_2.PDO4mv = stEEPROMData.pd.BM_PDO_Sink_2.PDO4mv;
				class_FT233H_eeData.BM_PDO_Sink_2.PDO5ma = stEEPROMData.pd.BM_PDO_Sink_2.PDO5ma;
				class_FT233H_eeData.BM_PDO_Sink_2.PDO5mv = stEEPROMData.pd.BM_PDO_Sink_2.PDO5mv;
				class_FT233H_eeData.BM_PDO_Sink_2.PDO6ma = stEEPROMData.pd.BM_PDO_Sink_2.PDO6ma;
				class_FT233H_eeData.BM_PDO_Sink_2.PDO6mv = stEEPROMData.pd.BM_PDO_Sink_2.PDO6mv;
				class_FT233H_eeData.BM_PDO_Sink_2.PDO7ma = stEEPROMData.pd.BM_PDO_Sink_2.PDO7ma;
				class_FT233H_eeData.BM_PDO_Sink_2.PDO7mv = stEEPROMData.pd.BM_PDO_Sink_2.PDO7mv;

				class_FT233H_eeData.srt = stEEPROMData.pd.srt;     // Sender Response Timer
				class_FT233H_eeData.hrt = stEEPROMData.pd.hrt;     // Hard Reset Timer
				class_FT233H_eeData.sct = stEEPROMData.pd.sct;     // Source Capability Timer
				class_FT233H_eeData.dit = stEEPROMData.pd.dit;     // Discover Identity Timer
				class_FT233H_eeData.srcrt = stEEPROMData.pd.srcrt; //Source Recover Timer
				class_FT233H_eeData.trt = stEEPROMData.pd.trt;     //Transition Timer
				class_FT233H_eeData.sofft = stEEPROMData.pd.sofft; //Source Off Timer
				class_FT233H_eeData.nrt = stEEPROMData.pd.nrt;     //No Response Timer
				class_FT233H_eeData.swct = stEEPROMData.pd.swct;   //Sink Wait Capability Timer
				class_FT233H_eeData.snkrt = stEEPROMData.pd.snkrt; //Sink Request Timer
				class_FT233H_eeData.dt = stEEPROMData.pd.dt;       //Discharge Timer
				class_FT233H_eeData.cnst = stEEPROMData.pd.cnst;   //Chunk not Supported Timer
				class_FT233H_eeData.it = stEEPROMData.pd.it;       //Idle Timer

				class_FT233H_eeData.i2caddr = stEEPROMData.pd.i2caddr;     //
				class_FT233H_eeData.prou = stEEPROMData.pd.prou;
				class_FT233H_eeData.trim1 = stEEPROMData.pd.trim1;
				class_FT233H_eeData.trim2 = stEEPROMData.pd.trim2;
				class_FT233H_eeData.extdc = stEEPROMData.pd.extdc;

			}

			return ftStatus;
		}






		//**************************************************************************
		// ReadXSeriesEEPROM
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Reads the EEPROM contents of an X-Series device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_EEPROM_Read in FTD2XX DLL</returns>
		/// <param name="eeX">An FT_XSERIES_EEPROM_STRUCTURE which contains only the relevant information for an X-Series device.</param>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
		public FT_STATUS Read_XSeries_EEPROM(FT_XSERIES_EEPROM_STRUCTURE eeX)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;
            FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_EEPROM_Read FT_EEPROM_Read = (tFT_EEPROM_Read)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EEPROM_Read, typeof(tFT_EEPROM_Read));

            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            // Check that it is an FT232H that we are trying to read
            GetDeviceType(ref DeviceType);
            if (DeviceType != FT_DEVICETYPE.FT_DEVICE_X_SERIES)
            {
                // If it is not, throw an exception
                ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            FT_XSERIES_DATA eeData = new FT_XSERIES_DATA();
            FT_EEPROM_HEADER eeHeader = new FT_EEPROM_HEADER();

            byte[] manufacturer = new byte[32];
            byte[] manufacturerID = new byte[16];
            byte[] description = new byte[64];
            byte[] serialNumber = new byte[16];

            eeHeader.deviceType = (uint)FT_DEVICETYPE.FT_DEVICE_X_SERIES;
            eeData.common = eeHeader;

            // Calculate the size of our data structure...
            int size = Marshal.SizeOf(eeData);

            // Allocate space for our pointer...
            IntPtr eeDataMarshal = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(eeData, eeDataMarshal, false);

            // Call FT_EEPROM_Read
            ftStatus = FT_EEPROM_Read(ftHandle, eeDataMarshal, (uint)size, manufacturer, manufacturerID, description, serialNumber);

            if (ftStatus == FT_STATUS.FT_OK)
            {
                // Get the data back from the pointer...
                eeData = (FT_XSERIES_DATA)Marshal.PtrToStructure(eeDataMarshal, typeof(FT_XSERIES_DATA));

                // Retrieve string values
                System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
                eeX.Manufacturer = enc.GetString(manufacturer);
                eeX.ManufacturerID = enc.GetString(manufacturerID);
                eeX.Description = enc.GetString(description);
                eeX.SerialNumber = enc.GetString(serialNumber);
                // Map non-string elements to structure to be returned
                // Standard elements
                eeX.VendorID = eeData.common.VendorId;
                eeX.ProductID = eeData.common.ProductId;
                eeX.MaxPower = eeData.common.MaxPower;
                eeX.SelfPowered = Convert.ToBoolean(eeData.common.SelfPowered);
                eeX.RemoteWakeup = Convert.ToBoolean(eeData.common.RemoteWakeup);
                eeX.SerNumEnable = Convert.ToBoolean(eeData.common.SerNumEnable);
                eeX.PullDownEnable = Convert.ToBoolean(eeData.common.PullDownEnable);
                // X-Series specific fields
                // CBUS
                eeX.Cbus0 = eeData.Cbus0;
                eeX.Cbus1 = eeData.Cbus1;
                eeX.Cbus2 = eeData.Cbus2;
                eeX.Cbus3 = eeData.Cbus3;
                eeX.Cbus4 = eeData.Cbus4;
                eeX.Cbus5 = eeData.Cbus5;
                eeX.Cbus6 = eeData.Cbus6;
                // Drive Options
                eeX.ACDriveCurrent = eeData.ACDriveCurrent;
                eeX.ACSchmittInput = eeData.ACSchmittInput;
                eeX.ACSlowSlew = eeData.ACSlowSlew;
                eeX.ADDriveCurrent = eeData.ADDriveCurrent;
                eeX.ADSchmittInput = eeData.ADSchmittInput;
                eeX.ADSlowSlew = eeData.ADSlowSlew;
                // BCD
                eeX.BCDDisableSleep = eeData.BCDDisableSleep;
                eeX.BCDEnable = eeData.BCDEnable;
                eeX.BCDForceCbusPWREN = eeData.BCDForceCbusPWREN;
                // FT1248
                eeX.FT1248Cpol = eeData.FT1248Cpol;
                eeX.FT1248FlowControl = eeData.FT1248FlowControl;
                eeX.FT1248Lsb = eeData.FT1248Lsb;
                // I2C
                eeX.I2CDeviceId = eeData.I2CDeviceId;
                eeX.I2CDisableSchmitt = eeData.I2CDisableSchmitt;
                eeX.I2CSlaveAddress = eeData.I2CSlaveAddress;
                // RS232 Signals
                eeX.InvertCTS = eeData.InvertCTS;
                eeX.InvertDCD = eeData.InvertDCD;
                eeX.InvertDSR = eeData.InvertDSR;
                eeX.InvertDTR = eeData.InvertDTR;
                eeX.InvertRI = eeData.InvertRI;
                eeX.InvertRTS = eeData.InvertRTS;
                eeX.InvertRXD = eeData.InvertRXD;
                eeX.InvertTXD = eeData.InvertTXD;
                // Hardware Options
                eeX.PowerSaveEnable = eeData.PowerSaveEnable;
                eeX.RS485EchoSuppress = eeData.RS485EchoSuppress;
                // Driver Option
                eeX.IsVCP = eeData.DriverType;
            }

            return ftStatus;
        }

		//**************************************************************************
		// WriteFT232BEEPROM
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Writes the specified values to the EEPROM of an FT232B or FT245B device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
		/// <param name="ee232b">The EEPROM settings to be written to the device</param>
		/// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS Write_FT232B_EEPROM(FT232B_EEPROM_STRUCTURE ee232b)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;
            FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_EE_Program FT_EE_Program = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_Program, typeof(tFT_EE_Program));

            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            // Check that it is an FT232B or FT245B that we are trying to write
            GetDeviceType(ref DeviceType);
            if (DeviceType != FT_DEVICETYPE.FT_DEVICE_BM)
            {
                // If it is not, throw an exception
                ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            // Check for VID and PID of 0x0000
            if ((ee232b.VendorID == 0x0000) | (ee232b.ProductID == 0x0000))
            {
                // Do not allow users to program the device with VID or PID of 0x0000
                return FT_STATUS.FT_INVALID_PARAMETER;
            }

            FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

            // Set up structure headers
            eedata.Signature1 = 0x00000000;
            eedata.Signature2 = 0xFFFFFFFF;
            eedata.Version = 2;

            // Allocate space from unmanaged heap
            eedata.Manufacturer = Marshal.AllocHGlobal(32);
            eedata.ManufacturerID = Marshal.AllocHGlobal(16);
            eedata.Description = Marshal.AllocHGlobal(64);
            eedata.SerialNumber = Marshal.AllocHGlobal(16);

            // Check lengths of strings to make sure that they are within our limits
            // If not, trim them to make them our maximum length
            if (ee232b.Manufacturer.Length > 32)
                ee232b.Manufacturer = ee232b.Manufacturer.Substring(0, 32);
            if (ee232b.ManufacturerID.Length > 16)
                ee232b.ManufacturerID = ee232b.ManufacturerID.Substring(0, 16);
            if (ee232b.Description.Length > 64)
                ee232b.Description = ee232b.Description.Substring(0, 64);
            if (ee232b.SerialNumber.Length > 16)
                ee232b.SerialNumber = ee232b.SerialNumber.Substring(0, 16);

            // Set string values
            eedata.Manufacturer = Marshal.StringToHGlobalAnsi(ee232b.Manufacturer);
            eedata.ManufacturerID = Marshal.StringToHGlobalAnsi(ee232b.ManufacturerID);
            eedata.Description = Marshal.StringToHGlobalAnsi(ee232b.Description);
            eedata.SerialNumber = Marshal.StringToHGlobalAnsi(ee232b.SerialNumber);

            // Map non-string elements to structure
            // Standard elements
            eedata.VendorID = ee232b.VendorID;
            eedata.ProductID = ee232b.ProductID;
            eedata.MaxPower = ee232b.MaxPower;
            eedata.SelfPowered = Convert.ToUInt16(ee232b.SelfPowered);
            eedata.RemoteWakeup = Convert.ToUInt16(ee232b.RemoteWakeup);
            // B specific fields
            eedata.Rev4 = Convert.ToByte(true);
            eedata.PullDownEnable = Convert.ToByte(ee232b.PullDownEnable);
            eedata.SerNumEnable = Convert.ToByte(ee232b.SerNumEnable);
            eedata.USBVersionEnable = Convert.ToByte(ee232b.USBVersionEnable);
            eedata.USBVersion = ee232b.USBVersion;

            // Call FT_EE_Program
            ftStatus = FT_EE_Program(ftHandle, eedata);

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eedata.Manufacturer);
            Marshal.FreeHGlobal(eedata.ManufacturerID);
            Marshal.FreeHGlobal(eedata.Description);
            Marshal.FreeHGlobal(eedata.SerialNumber);

            return ftStatus;
        }

		//**************************************************************************
		// WriteFT2232EEPROM
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Writes the specified values to the EEPROM of an FT2232 device.
		/// Calls FT_EE_Program in FTD2XX DLL
		/// </summary>
		/// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
		/// <param name="ee2232">The EEPROM settings to be written to the device</param>
		/// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS Write_FT2232_EEPROM(FT2232_EEPROM_STRUCTURE ee2232)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;
            FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_EE_Program FT_EE_Program = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_Program, typeof(tFT_EE_Program));

            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            // Check that it is an FT2232 that we are trying to write
            GetDeviceType(ref DeviceType);
            if (DeviceType != FT_DEVICETYPE.FT_DEVICE_2232C)
            {
                // If it is not, throw an exception
                ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            // Check for VID and PID of 0x0000
            if ((ee2232.VendorID == 0x0000) | (ee2232.ProductID == 0x0000))
            {
                // Do not allow users to program the device with VID or PID of 0x0000
                return FT_STATUS.FT_INVALID_PARAMETER;
            }

            FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

            // Set up structure headers
            eedata.Signature1 = 0x00000000;
            eedata.Signature2 = 0xFFFFFFFF;
            eedata.Version = 2;

            // Allocate space from unmanaged heap
            eedata.Manufacturer = Marshal.AllocHGlobal(32);
            eedata.ManufacturerID = Marshal.AllocHGlobal(16);
            eedata.Description = Marshal.AllocHGlobal(64);
            eedata.SerialNumber = Marshal.AllocHGlobal(16);

            // Check lengths of strings to make sure that they are within our limits
            // If not, trim them to make them our maximum length
            if (ee2232.Manufacturer.Length > 32)
                ee2232.Manufacturer = ee2232.Manufacturer.Substring(0, 32);
            if (ee2232.ManufacturerID.Length > 16)
                ee2232.ManufacturerID = ee2232.ManufacturerID.Substring(0, 16);
            if (ee2232.Description.Length > 64)
                ee2232.Description = ee2232.Description.Substring(0, 64);
            if (ee2232.SerialNumber.Length > 16)
                ee2232.SerialNumber = ee2232.SerialNumber.Substring(0, 16);

            // Set string values
            eedata.Manufacturer = Marshal.StringToHGlobalAnsi(ee2232.Manufacturer);
            eedata.ManufacturerID = Marshal.StringToHGlobalAnsi(ee2232.ManufacturerID);
            eedata.Description = Marshal.StringToHGlobalAnsi(ee2232.Description);
            eedata.SerialNumber = Marshal.StringToHGlobalAnsi(ee2232.SerialNumber);

            // Map non-string elements to structure
            // Standard elements
            eedata.VendorID = ee2232.VendorID;
            eedata.ProductID = ee2232.ProductID;
            eedata.MaxPower = ee2232.MaxPower;
            eedata.SelfPowered = Convert.ToUInt16(ee2232.SelfPowered);
            eedata.RemoteWakeup = Convert.ToUInt16(ee2232.RemoteWakeup);
            // 2232 specific fields
            eedata.Rev5 = Convert.ToByte(true);
            eedata.PullDownEnable5 = Convert.ToByte(ee2232.PullDownEnable);
            eedata.SerNumEnable5 = Convert.ToByte(ee2232.SerNumEnable);
            eedata.USBVersionEnable5 = Convert.ToByte(ee2232.USBVersionEnable);
            eedata.USBVersion5 = ee2232.USBVersion;
            eedata.AIsHighCurrent = Convert.ToByte(ee2232.AIsHighCurrent);
            eedata.BIsHighCurrent = Convert.ToByte(ee2232.BIsHighCurrent);
            eedata.IFAIsFifo = Convert.ToByte(ee2232.IFAIsFifo);
            eedata.IFAIsFifoTar = Convert.ToByte(ee2232.IFAIsFifoTar);
            eedata.IFAIsFastSer = Convert.ToByte(ee2232.IFAIsFastSer);
            eedata.AIsVCP = Convert.ToByte(ee2232.AIsVCP);
            eedata.IFBIsFifo = Convert.ToByte(ee2232.IFBIsFifo);
            eedata.IFBIsFifoTar = Convert.ToByte(ee2232.IFBIsFifoTar);
            eedata.IFBIsFastSer = Convert.ToByte(ee2232.IFBIsFastSer);
            eedata.BIsVCP = Convert.ToByte(ee2232.BIsVCP);

            // Call FT_EE_Program
            ftStatus = FT_EE_Program(ftHandle, eedata);

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eedata.Manufacturer);
            Marshal.FreeHGlobal(eedata.ManufacturerID);
            Marshal.FreeHGlobal(eedata.Description);
            Marshal.FreeHGlobal(eedata.SerialNumber);

            return ftStatus;
        }

		//**************************************************************************
		// WriteFT232REEPROM
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Writes the specified values to the EEPROM of an FT232R or FT245R device.
		/// Calls FT_EE_Program in FTD2XX DLL
		/// </summary>
		/// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
		/// <param name="ee232r">The EEPROM settings to be written to the device</param>
		/// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS Write_FT232R_EEPROM(FT232R_EEPROM_STRUCTURE ee232r)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;
            FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_EE_Program FT_EE_Program = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_Program, typeof(tFT_EE_Program));

            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            // Check that it is an FT232R or FT245R that we are trying to write
            GetDeviceType(ref DeviceType);
            if (DeviceType != FT_DEVICETYPE.FT_DEVICE_232R)
            {
                // If it is not, throw an exception
                ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            // Check for VID and PID of 0x0000
            if ((ee232r.VendorID == 0x0000) | (ee232r.ProductID == 0x0000))
            {
                // Do not allow users to program the device with VID or PID of 0x0000
                return FT_STATUS.FT_INVALID_PARAMETER;
            }

            FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

            // Set up structure headers
            eedata.Signature1 = 0x00000000;
            eedata.Signature2 = 0xFFFFFFFF;
            eedata.Version = 2;

            // Allocate space from unmanaged heap
            eedata.Manufacturer = Marshal.AllocHGlobal(32);
            eedata.ManufacturerID = Marshal.AllocHGlobal(16);
            eedata.Description = Marshal.AllocHGlobal(64);
            eedata.SerialNumber = Marshal.AllocHGlobal(16);

            // Check lengths of strings to make sure that they are within our limits
            // If not, trim them to make them our maximum length
            if (ee232r.Manufacturer.Length > 32)
                ee232r.Manufacturer = ee232r.Manufacturer.Substring(0, 32);
            if (ee232r.ManufacturerID.Length > 16)
                ee232r.ManufacturerID = ee232r.ManufacturerID.Substring(0, 16);
            if (ee232r.Description.Length > 64)
                ee232r.Description = ee232r.Description.Substring(0, 64);
            if (ee232r.SerialNumber.Length > 16)
                ee232r.SerialNumber = ee232r.SerialNumber.Substring(0, 16);

            // Set string values
            eedata.Manufacturer = Marshal.StringToHGlobalAnsi(ee232r.Manufacturer);
            eedata.ManufacturerID = Marshal.StringToHGlobalAnsi(ee232r.ManufacturerID);
            eedata.Description = Marshal.StringToHGlobalAnsi(ee232r.Description);
            eedata.SerialNumber = Marshal.StringToHGlobalAnsi(ee232r.SerialNumber);

            // Map non-string elements to structure
            // Standard elements
            eedata.VendorID = ee232r.VendorID;
            eedata.ProductID = ee232r.ProductID;
            eedata.MaxPower = ee232r.MaxPower;
            eedata.SelfPowered = Convert.ToUInt16(ee232r.SelfPowered);
            eedata.RemoteWakeup = Convert.ToUInt16(ee232r.RemoteWakeup);
            // 232R specific fields
            eedata.PullDownEnableR = Convert.ToByte(ee232r.PullDownEnable);
            eedata.SerNumEnableR = Convert.ToByte(ee232r.SerNumEnable);
            eedata.UseExtOsc = Convert.ToByte(ee232r.UseExtOsc);
            eedata.HighDriveIOs = Convert.ToByte(ee232r.HighDriveIOs);
            // Override any endpoint size the user has selected and force 64 bytes
            // Some users have been known to wreck devices by setting 0 here...
            eedata.EndpointSize = 64;
            eedata.PullDownEnableR = Convert.ToByte(ee232r.PullDownEnable);
            eedata.SerNumEnableR = Convert.ToByte(ee232r.SerNumEnable);
            eedata.InvertTXD = Convert.ToByte(ee232r.InvertTXD);
            eedata.InvertRXD = Convert.ToByte(ee232r.InvertRXD);
            eedata.InvertRTS = Convert.ToByte(ee232r.InvertRTS);
            eedata.InvertCTS = Convert.ToByte(ee232r.InvertCTS);
            eedata.InvertDTR = Convert.ToByte(ee232r.InvertDTR);
            eedata.InvertDSR = Convert.ToByte(ee232r.InvertDSR);
            eedata.InvertDCD = Convert.ToByte(ee232r.InvertDCD);
            eedata.InvertRI = Convert.ToByte(ee232r.InvertRI);
            eedata.Cbus0 = ee232r.Cbus0;
            eedata.Cbus1 = ee232r.Cbus1;
            eedata.Cbus2 = ee232r.Cbus2;
            eedata.Cbus3 = ee232r.Cbus3;
            eedata.Cbus4 = ee232r.Cbus4;
            eedata.RIsD2XX = Convert.ToByte(ee232r.RIsD2XX);

            // Call FT_EE_Program
            ftStatus = FT_EE_Program(ftHandle, eedata);

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eedata.Manufacturer);
            Marshal.FreeHGlobal(eedata.ManufacturerID);
            Marshal.FreeHGlobal(eedata.Description);
            Marshal.FreeHGlobal(eedata.SerialNumber);

            return ftStatus;
        }

		//**************************************************************************
		// WriteFT2232HEEPROM
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Writes the specified values to the EEPROM of an FT2232H device.
		/// Calls FT_EE_Program in FTD2XX DLL
		/// </summary>
		/// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
		/// <param name="ee2232h">The EEPROM settings to be written to the device</param>
		/// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS Write_FT2232H_EEPROM(FT2232H_EEPROM_STRUCTURE ee2232h)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;
            FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_EE_Program FT_EE_Program = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_Program, typeof(tFT_EE_Program));

            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            // Check that it is an FT2232H that we are trying to write
            GetDeviceType(ref DeviceType);
            if (DeviceType != FT_DEVICETYPE.FT_DEVICE_2232H)
            {
                // If it is not, throw an exception
                ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            // Check for VID and PID of 0x0000
            if ((ee2232h.VendorID == 0x0000) | (ee2232h.ProductID == 0x0000))
            {
                // Do not allow users to program the device with VID or PID of 0x0000
                return FT_STATUS.FT_INVALID_PARAMETER;
            }

            FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

            // Set up structure headers
            eedata.Signature1 = 0x00000000;
            eedata.Signature2 = 0xFFFFFFFF;
            eedata.Version = 3;

            // Allocate space from unmanaged heap
            eedata.Manufacturer = Marshal.AllocHGlobal(32);
            eedata.ManufacturerID = Marshal.AllocHGlobal(16);
            eedata.Description = Marshal.AllocHGlobal(64);
            eedata.SerialNumber = Marshal.AllocHGlobal(16);

            // Check lengths of strings to make sure that they are within our limits
            // If not, trim them to make them our maximum length
            if (ee2232h.Manufacturer.Length > 32)
                ee2232h.Manufacturer = ee2232h.Manufacturer.Substring(0, 32);
            if (ee2232h.ManufacturerID.Length > 16)
                ee2232h.ManufacturerID = ee2232h.ManufacturerID.Substring(0, 16);
            if (ee2232h.Description.Length > 64)
                ee2232h.Description = ee2232h.Description.Substring(0, 64);
            if (ee2232h.SerialNumber.Length > 16)
                ee2232h.SerialNumber = ee2232h.SerialNumber.Substring(0, 16);

            // Set string values
            eedata.Manufacturer = Marshal.StringToHGlobalAnsi(ee2232h.Manufacturer);
            eedata.ManufacturerID = Marshal.StringToHGlobalAnsi(ee2232h.ManufacturerID);
            eedata.Description = Marshal.StringToHGlobalAnsi(ee2232h.Description);
            eedata.SerialNumber = Marshal.StringToHGlobalAnsi(ee2232h.SerialNumber);

            // Map non-string elements to structure
            // Standard elements
            eedata.VendorID = ee2232h.VendorID;
            eedata.ProductID = ee2232h.ProductID;
            eedata.MaxPower = ee2232h.MaxPower;
            eedata.SelfPowered = Convert.ToUInt16(ee2232h.SelfPowered);
            eedata.RemoteWakeup = Convert.ToUInt16(ee2232h.RemoteWakeup);
            // 2232H specific fields
            eedata.PullDownEnable7 = Convert.ToByte(ee2232h.PullDownEnable);
            eedata.SerNumEnable7 = Convert.ToByte(ee2232h.SerNumEnable);
            eedata.ALSlowSlew = Convert.ToByte(ee2232h.ALSlowSlew);
            eedata.ALSchmittInput = Convert.ToByte(ee2232h.ALSchmittInput);
            eedata.ALDriveCurrent = ee2232h.ALDriveCurrent;
            eedata.AHSlowSlew = Convert.ToByte(ee2232h.AHSlowSlew);
            eedata.AHSchmittInput = Convert.ToByte(ee2232h.AHSchmittInput);
            eedata.AHDriveCurrent = ee2232h.AHDriveCurrent;
            eedata.BLSlowSlew = Convert.ToByte(ee2232h.BLSlowSlew);
            eedata.BLSchmittInput = Convert.ToByte(ee2232h.BLSchmittInput);
            eedata.BLDriveCurrent = ee2232h.BLDriveCurrent;
            eedata.BHSlowSlew = Convert.ToByte(ee2232h.BHSlowSlew);
            eedata.BHSchmittInput = Convert.ToByte(ee2232h.BHSchmittInput);
            eedata.BHDriveCurrent = ee2232h.BHDriveCurrent;
            eedata.IFAIsFifo7 = Convert.ToByte(ee2232h.IFAIsFifo);
            eedata.IFAIsFifoTar7 = Convert.ToByte(ee2232h.IFAIsFifoTar);
            eedata.IFAIsFastSer7 = Convert.ToByte(ee2232h.IFAIsFastSer);
            eedata.AIsVCP7 = Convert.ToByte(ee2232h.AIsVCP);
            eedata.IFBIsFifo7 = Convert.ToByte(ee2232h.IFBIsFifo);
            eedata.IFBIsFifoTar7 = Convert.ToByte(ee2232h.IFBIsFifoTar);
            eedata.IFBIsFastSer7 = Convert.ToByte(ee2232h.IFBIsFastSer);
            eedata.BIsVCP7 = Convert.ToByte(ee2232h.BIsVCP);
            eedata.PowerSaveEnable = Convert.ToByte(ee2232h.PowerSaveEnable);

            // Call FT_EE_Program
            ftStatus = FT_EE_Program(ftHandle, eedata);

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eedata.Manufacturer);
            Marshal.FreeHGlobal(eedata.ManufacturerID);
            Marshal.FreeHGlobal(eedata.Description);
            Marshal.FreeHGlobal(eedata.SerialNumber);

            return ftStatus;
        }

		//**************************************************************************
		// WriteFT4232HEEPROM
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Writes the specified values to the EEPROM of an FT4232H device.
		/// Calls FT_EE_Program in FTD2XX DLL
		/// </summary>
		/// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
		/// <param name="ee4232h">The EEPROM settings to be written to the device</param>
		/// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS Write_FT4232H_EEPROM(FT4232H_EEPROM_STRUCTURE ee4232h)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;
            FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_EE_Program FT_EE_Program = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_Program, typeof(tFT_EE_Program));

            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            // Check that it is an FT4232H that we are trying to write
            GetDeviceType(ref DeviceType);
            if (DeviceType != FT_DEVICETYPE.FT_DEVICE_4232H)
            {
                // If it is not, throw an exception
                ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            // Check for VID and PID of 0x0000
            if ((ee4232h.VendorID == 0x0000) | (ee4232h.ProductID == 0x0000))
            {
                // Do not allow users to program the device with VID or PID of 0x0000
                return FT_STATUS.FT_INVALID_PARAMETER;
            }

            FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

            // Set up structure headers
            eedata.Signature1 = 0x00000000;
            eedata.Signature2 = 0xFFFFFFFF;
            eedata.Version = 4;

            // Allocate space from unmanaged heap
            eedata.Manufacturer = Marshal.AllocHGlobal(32);
            eedata.ManufacturerID = Marshal.AllocHGlobal(16);
            eedata.Description = Marshal.AllocHGlobal(64);
            eedata.SerialNumber = Marshal.AllocHGlobal(16);

            // Check lengths of strings to make sure that they are within our limits
            // If not, trim them to make them our maximum length
            if (ee4232h.Manufacturer.Length > 32)
                ee4232h.Manufacturer = ee4232h.Manufacturer.Substring(0, 32);
            if (ee4232h.ManufacturerID.Length > 16)
                ee4232h.ManufacturerID = ee4232h.ManufacturerID.Substring(0, 16);
            if (ee4232h.Description.Length > 64)
                ee4232h.Description = ee4232h.Description.Substring(0, 64);
            if (ee4232h.SerialNumber.Length > 16)
                ee4232h.SerialNumber = ee4232h.SerialNumber.Substring(0, 16);

            // Set string values
            eedata.Manufacturer = Marshal.StringToHGlobalAnsi(ee4232h.Manufacturer);
            eedata.ManufacturerID = Marshal.StringToHGlobalAnsi(ee4232h.ManufacturerID);
            eedata.Description = Marshal.StringToHGlobalAnsi(ee4232h.Description);
            eedata.SerialNumber = Marshal.StringToHGlobalAnsi(ee4232h.SerialNumber);

            // Map non-string elements to structure
            // Standard elements
            eedata.VendorID = ee4232h.VendorID;
            eedata.ProductID = ee4232h.ProductID;
            eedata.MaxPower = ee4232h.MaxPower;
            eedata.SelfPowered = Convert.ToUInt16(ee4232h.SelfPowered);
            eedata.RemoteWakeup = Convert.ToUInt16(ee4232h.RemoteWakeup);
            // 4232H specific fields
            eedata.PullDownEnable8 = Convert.ToByte(ee4232h.PullDownEnable);
            eedata.SerNumEnable8 = Convert.ToByte(ee4232h.SerNumEnable);
            eedata.ASlowSlew = Convert.ToByte(ee4232h.ASlowSlew);
            eedata.ASchmittInput = Convert.ToByte(ee4232h.ASchmittInput);
            eedata.ADriveCurrent = ee4232h.ADriveCurrent;
            eedata.BSlowSlew = Convert.ToByte(ee4232h.BSlowSlew);
            eedata.BSchmittInput = Convert.ToByte(ee4232h.BSchmittInput);
            eedata.BDriveCurrent = ee4232h.BDriveCurrent;
            eedata.CSlowSlew = Convert.ToByte(ee4232h.CSlowSlew);
            eedata.CSchmittInput = Convert.ToByte(ee4232h.CSchmittInput);
            eedata.CDriveCurrent = ee4232h.CDriveCurrent;
            eedata.DSlowSlew = Convert.ToByte(ee4232h.DSlowSlew);
            eedata.DSchmittInput = Convert.ToByte(ee4232h.DSchmittInput);
            eedata.DDriveCurrent = ee4232h.DDriveCurrent;
            eedata.ARIIsTXDEN = Convert.ToByte(ee4232h.ARIIsTXDEN);
            eedata.BRIIsTXDEN = Convert.ToByte(ee4232h.BRIIsTXDEN);
            eedata.CRIIsTXDEN = Convert.ToByte(ee4232h.CRIIsTXDEN);
            eedata.DRIIsTXDEN = Convert.ToByte(ee4232h.DRIIsTXDEN);
            eedata.AIsVCP8 = Convert.ToByte(ee4232h.AIsVCP);
            eedata.BIsVCP8 = Convert.ToByte(ee4232h.BIsVCP);
            eedata.CIsVCP8 = Convert.ToByte(ee4232h.CIsVCP);
            eedata.DIsVCP8 = Convert.ToByte(ee4232h.DIsVCP);

            // Call FT_EE_Program
            ftStatus = FT_EE_Program(ftHandle, eedata);

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eedata.Manufacturer);
            Marshal.FreeHGlobal(eedata.ManufacturerID);
            Marshal.FreeHGlobal(eedata.Description);
            Marshal.FreeHGlobal(eedata.SerialNumber);

            return ftStatus;
        }

		//**************************************************************************
		// WriteFT4232HAEEPROM
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Writes the specified values to the EEPROM of an FT4232HA device.
		/// Calls FT_EE_Program in FTD2XX DLL
		/// </summary>
		/// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
		/// <param name="ee4232ha">The EEPROM settings to be written to the device</param>
		/// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
		public FT_STATUS Write_FT4232HA_EEPROM(FT4232H_EEPROM_STRUCTURE ee4232ha)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;
			FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (ftHandle == IntPtr.Zero)
				return ftStatus;

			tFT_EE_Program FT_EE_Program = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_Program, typeof(tFT_EE_Program));

			FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
			// Check that it is an FT4232HA that we are trying to write
			GetDeviceType(ref DeviceType);
			if (DeviceType != FT_DEVICETYPE.FT_DEVICE_4232HA)
			{
				// If it is not, throw an exception
				ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
				ErrorHandler(ftStatus, ftErrorCondition);
			}

			// Check for VID and PID of 0x0000
			if ((ee4232ha.VendorID == 0x0000) | (ee4232ha.ProductID == 0x0000))
			{
				// Do not allow users to program the device with VID or PID of 0x0000
				return FT_STATUS.FT_INVALID_PARAMETER;
			}

			FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

			// Set up structure headers
			eedata.Signature1 = 0x00000000;
			eedata.Signature2 = 0xFFFFFFFF;
			eedata.Version = 4;

			// Allocate space from unmanaged heap
			eedata.Manufacturer = Marshal.AllocHGlobal(32);
			eedata.ManufacturerID = Marshal.AllocHGlobal(16);
			eedata.Description = Marshal.AllocHGlobal(64);
			eedata.SerialNumber = Marshal.AllocHGlobal(16);

			// Check lengths of strings to make sure that they are within our limits
			// If not, trim them to make them our maximum length
			if (ee4232ha.Manufacturer.Length > 32)
				ee4232ha.Manufacturer = ee4232ha.Manufacturer.Substring(0, 32);
			if (ee4232ha.ManufacturerID.Length > 16)
				ee4232ha.ManufacturerID = ee4232ha.ManufacturerID.Substring(0, 16);
			if (ee4232ha.Description.Length > 64)
				ee4232ha.Description = ee4232ha.Description.Substring(0, 64);
			if (ee4232ha.SerialNumber.Length > 16)
				ee4232ha.SerialNumber = ee4232ha.SerialNumber.Substring(0, 16);

			// Set string values
			eedata.Manufacturer = Marshal.StringToHGlobalAnsi(ee4232ha.Manufacturer);
			eedata.ManufacturerID = Marshal.StringToHGlobalAnsi(ee4232ha.ManufacturerID);
			eedata.Description = Marshal.StringToHGlobalAnsi(ee4232ha.Description);
			eedata.SerialNumber = Marshal.StringToHGlobalAnsi(ee4232ha.SerialNumber);

			// Map non-string elements to structure
			// Standard elements
			eedata.VendorID = ee4232ha.VendorID;
			eedata.ProductID = ee4232ha.ProductID;
			eedata.MaxPower = ee4232ha.MaxPower;
			eedata.SelfPowered = Convert.ToUInt16(ee4232ha.SelfPowered);
			eedata.RemoteWakeup = Convert.ToUInt16(ee4232ha.RemoteWakeup);
			// 4232HA specific fields
			eedata.PullDownEnable8 = Convert.ToByte(ee4232ha.PullDownEnable);
			eedata.SerNumEnable8 = Convert.ToByte(ee4232ha.SerNumEnable);
			eedata.ASlowSlew = Convert.ToByte(ee4232ha.ASlowSlew);
			eedata.ASchmittInput = Convert.ToByte(ee4232ha.ASchmittInput);
			eedata.ADriveCurrent = ee4232ha.ADriveCurrent;
			eedata.BSlowSlew = Convert.ToByte(ee4232ha.BSlowSlew);
			eedata.BSchmittInput = Convert.ToByte(ee4232ha.BSchmittInput);
			eedata.BDriveCurrent = ee4232ha.BDriveCurrent;
			eedata.CSlowSlew = Convert.ToByte(ee4232ha.CSlowSlew);
			eedata.CSchmittInput = Convert.ToByte(ee4232ha.CSchmittInput);
			eedata.CDriveCurrent = ee4232ha.CDriveCurrent;
			eedata.DSlowSlew = Convert.ToByte(ee4232ha.DSlowSlew);
			eedata.DSchmittInput = Convert.ToByte(ee4232ha.DSchmittInput);
			eedata.DDriveCurrent = ee4232ha.DDriveCurrent;
			eedata.ARIIsTXDEN = Convert.ToByte(ee4232ha.ARIIsTXDEN);
			eedata.BRIIsTXDEN = Convert.ToByte(ee4232ha.BRIIsTXDEN);
			eedata.CRIIsTXDEN = Convert.ToByte(ee4232ha.CRIIsTXDEN);
			eedata.DRIIsTXDEN = Convert.ToByte(ee4232ha.DRIIsTXDEN);
			eedata.AIsVCP8 = Convert.ToByte(ee4232ha.AIsVCP);
			eedata.BIsVCP8 = Convert.ToByte(ee4232ha.BIsVCP);
			eedata.CIsVCP8 = Convert.ToByte(ee4232ha.CIsVCP);
			eedata.DIsVCP8 = Convert.ToByte(ee4232ha.DIsVCP);

			// Call FT_EE_Program
			ftStatus = FT_EE_Program(ftHandle, eedata);

			// Free unmanaged buffers
			Marshal.FreeHGlobal(eedata.Manufacturer);
			Marshal.FreeHGlobal(eedata.ManufacturerID);
			Marshal.FreeHGlobal(eedata.Description);
			Marshal.FreeHGlobal(eedata.SerialNumber);

			return ftStatus;
		}

		//**************************************************************************
		// WriteFT232HEEPROM
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Writes the specified values to the EEPROM of an FT232H device.
		/// Calls FT_EE_Program in FTD2XX DLL
		/// </summary>
		/// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
		/// <param name="ee232h">The EEPROM settings to be written to the device</param>
		/// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
		public FT_STATUS Write_FT232H_EEPROM(FT232H_EEPROM_STRUCTURE ee232h)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;
            FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_EE_Program FT_EE_Program = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_Program, typeof(tFT_EE_Program));

            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            // Check that it is an FT232H that we are trying to write
            GetDeviceType(ref DeviceType);
            if (DeviceType != FT_DEVICETYPE.FT_DEVICE_232H)
            {
                // If it is not, throw an exception
                ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            // Check for VID and PID of 0x0000
            if ((ee232h.VendorID == 0x0000) | (ee232h.ProductID == 0x0000))
            {
                // Do not allow users to program the device with VID or PID of 0x0000
                return FT_STATUS.FT_INVALID_PARAMETER;
            }

            FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

            // Set up structure headers
            eedata.Signature1 = 0x00000000;
            eedata.Signature2 = 0xFFFFFFFF;
            eedata.Version = 5;

            // Allocate space from unmanaged heap
            eedata.Manufacturer = Marshal.AllocHGlobal(32);
            eedata.ManufacturerID = Marshal.AllocHGlobal(16);
            eedata.Description = Marshal.AllocHGlobal(64);
            eedata.SerialNumber = Marshal.AllocHGlobal(16);

            // Check lengths of strings to make sure that they are within our limits
            // If not, trim them to make them our maximum length
            if (ee232h.Manufacturer.Length > 32)
                ee232h.Manufacturer = ee232h.Manufacturer.Substring(0, 32);
            if (ee232h.ManufacturerID.Length > 16)
                ee232h.ManufacturerID = ee232h.ManufacturerID.Substring(0, 16);
            if (ee232h.Description.Length > 64)
                ee232h.Description = ee232h.Description.Substring(0, 64);
            if (ee232h.SerialNumber.Length > 16)
                ee232h.SerialNumber = ee232h.SerialNumber.Substring(0, 16);

            // Set string values
            eedata.Manufacturer = Marshal.StringToHGlobalAnsi(ee232h.Manufacturer);
            eedata.ManufacturerID = Marshal.StringToHGlobalAnsi(ee232h.ManufacturerID);
            eedata.Description = Marshal.StringToHGlobalAnsi(ee232h.Description);
            eedata.SerialNumber = Marshal.StringToHGlobalAnsi(ee232h.SerialNumber);

            // Map non-string elements to structure
            // Standard elements
            eedata.VendorID = ee232h.VendorID;
            eedata.ProductID = ee232h.ProductID;
            eedata.MaxPower = ee232h.MaxPower;
            eedata.SelfPowered = Convert.ToUInt16(ee232h.SelfPowered);
            eedata.RemoteWakeup = Convert.ToUInt16(ee232h.RemoteWakeup);
            // 232H specific fields
            eedata.PullDownEnableH = Convert.ToByte(ee232h.PullDownEnable);
            eedata.SerNumEnableH = Convert.ToByte(ee232h.SerNumEnable);
            eedata.ACSlowSlewH = Convert.ToByte(ee232h.ACSlowSlew);
            eedata.ACSchmittInputH = Convert.ToByte(ee232h.ACSchmittInput);
            eedata.ACDriveCurrentH = Convert.ToByte(ee232h.ACDriveCurrent);
            eedata.ADSlowSlewH = Convert.ToByte(ee232h.ADSlowSlew);
            eedata.ADSchmittInputH = Convert.ToByte(ee232h.ADSchmittInput);
            eedata.ADDriveCurrentH = Convert.ToByte(ee232h.ADDriveCurrent);
            eedata.Cbus0H = Convert.ToByte(ee232h.Cbus0);
            eedata.Cbus1H = Convert.ToByte(ee232h.Cbus1);
            eedata.Cbus2H = Convert.ToByte(ee232h.Cbus2);
            eedata.Cbus3H = Convert.ToByte(ee232h.Cbus3);
            eedata.Cbus4H = Convert.ToByte(ee232h.Cbus4);
            eedata.Cbus5H = Convert.ToByte(ee232h.Cbus5);
            eedata.Cbus6H = Convert.ToByte(ee232h.Cbus6);
            eedata.Cbus7H = Convert.ToByte(ee232h.Cbus7);
            eedata.Cbus8H = Convert.ToByte(ee232h.Cbus8);
            eedata.Cbus9H = Convert.ToByte(ee232h.Cbus9);
            eedata.IsFifoH = Convert.ToByte(ee232h.IsFifo);
            eedata.IsFifoTarH = Convert.ToByte(ee232h.IsFifoTar);
            eedata.IsFastSerH = Convert.ToByte(ee232h.IsFastSer);
            eedata.IsFT1248H = Convert.ToByte(ee232h.IsFT1248);
            eedata.FT1248CpolH = Convert.ToByte(ee232h.FT1248Cpol);
            eedata.FT1248LsbH = Convert.ToByte(ee232h.FT1248Lsb);
            eedata.FT1248FlowControlH = Convert.ToByte(ee232h.FT1248FlowControl);
            eedata.IsVCPH = Convert.ToByte(ee232h.IsVCP);
            eedata.PowerSaveEnableH = Convert.ToByte(ee232h.PowerSaveEnable);

            // Call FT_EE_Program
            ftStatus = FT_EE_Program(ftHandle, eedata);

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eedata.Manufacturer);
            Marshal.FreeHGlobal(eedata.ManufacturerID);
            Marshal.FreeHGlobal(eedata.Description);
            Marshal.FreeHGlobal(eedata.SerialNumber);

            return ftStatus;
        }

		//**************************************************************************
		// WriteFT233HEEPROM
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Writes the specified values to the EEPROM of an FT233H device.
		/// Calls FT_EE_Program in FTD2XX DLL
		/// </summary>
		/// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
		/// <param name="ee233h">The EEPROM settings to be written to the device</param>
		/// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
		public FT_STATUS Write_EEPROM_FT233HP(CEEPROM_FT233H classEE233h, ref string strError)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

			// If the DLL hasn't been loaded, just return here
			if (ftHandle == IntPtr.Zero)
			{
				strError = "DLL handle is null - DLL not loaded";
				return FT_STATUS.FT_OTHER_ERROR;
			}

			tFT_EEPROM_Program FT_EEPROM_Program = (tFT_EEPROM_Program)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EEPROM_Program, typeof(tFT_EEPROM_Program));

			FT_DEVICETYPE deviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
			GetDeviceType(ref deviceType);
			if (deviceType != FT_DEVICETYPE.FT_DEVICE_233HP)
			{
				strError = "Error - incorrect device type. Expected Device=FT_DEVICE_233HP. Connected device=" + deviceType.ToString();
				return FT_STATUS.FT_OTHER_ERROR;
			}

			if (classEE233h.VendorID == 0x0000)
			{
				strError = "Error - VendorID = 0";
				return FT_STATUS.FT_INVALID_PARAMETER;
			}
			if (classEE233h.ProductID == 0x0000)
			{
				strError = "Error - ProductID = 0";
				return FT_STATUS.FT_INVALID_PARAMETER;
			}

			STRUCT_EEPROM_FT233HP stEEPROM233HP = new STRUCT_EEPROM_FT233HP();

			byte[] manufacturer = new byte[32];
			byte[] manufacturerID = new byte[16];
			byte[] description = new byte[64];
			byte[] serialNumber = new byte[16];

			// Check lengths of strings to make sure that they are within our limits
			// If not, trim them to make them our maximum length
			if (classEE233h.Manufacturer.Length > 32)
				classEE233h.Manufacturer = classEE233h.Manufacturer.Substring(0, 32);
			if (classEE233h.ManufacturerID.Length > 16)
				classEE233h.ManufacturerID = classEE233h.ManufacturerID.Substring(0, 16);
			if (classEE233h.Description.Length > 64)
				classEE233h.Description = classEE233h.Description.Substring(0, 64);
			if (classEE233h.SerialNumber.Length > 16)
				classEE233h.SerialNumber = classEE233h.SerialNumber.Substring(0, 16);

			// Set string values
			System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
			manufacturer = encoding.GetBytes(classEE233h.Manufacturer);
			manufacturerID = encoding.GetBytes(classEE233h.ManufacturerID);
			description = encoding.GetBytes(classEE233h.Description);
			serialNumber = encoding.GetBytes(classEE233h.SerialNumber);

			stEEPROM233HP.ft232h.common.deviceType = (uint)FT_DEVICETYPE.FT_DEVICE_233HP;
			stEEPROM233HP.ft232h.common.VendorId = classEE233h.VendorID;
			stEEPROM233HP.ft232h.common.ProductId = classEE233h.ProductID;
			stEEPROM233HP.ft232h.common.SerNumEnable = Convert.ToByte(classEE233h.SerNumEnable);
			stEEPROM233HP.ft232h.common.MaxPower = classEE233h.MaxPower;
			stEEPROM233HP.ft232h.common.SelfPowered = Convert.ToByte(classEE233h.SelfPowered);
			stEEPROM233HP.ft232h.common.RemoteWakeup = Convert.ToByte(classEE233h.RemoteWakeup);
			stEEPROM233HP.ft232h.common.PullDownEnable = Convert.ToByte(classEE233h.PullDownEnable);

			stEEPROM233HP.ft232h.ACSlowSlew = Convert.ToByte(classEE233h.ACSlowSlew);
			stEEPROM233HP.ft232h.ACSchmittInput = Convert.ToByte(classEE233h.ACSchmittInput);
			stEEPROM233HP.ft232h.ACDriveCurrent = Convert.ToByte(classEE233h.ACDriveCurrent);
			stEEPROM233HP.ft232h.ADSlowSlew = Convert.ToByte(classEE233h.ADSlowSlew);
			stEEPROM233HP.ft232h.ADSchmittInput = Convert.ToByte(classEE233h.ADSchmittInput);
			stEEPROM233HP.ft232h.ADDriveCurrent = Convert.ToByte(classEE233h.ADDriveCurrent);
			stEEPROM233HP.ft232h.Cbus0 = Convert.ToByte(classEE233h.Cbus0);
			stEEPROM233HP.ft232h.Cbus1 = Convert.ToByte(classEE233h.Cbus1);
			stEEPROM233HP.ft232h.Cbus2 = Convert.ToByte(classEE233h.Cbus2);
			stEEPROM233HP.ft232h.Cbus3 = Convert.ToByte(classEE233h.Cbus3);
			stEEPROM233HP.ft232h.Cbus4 = Convert.ToByte(classEE233h.Cbus4);
			stEEPROM233HP.ft232h.Cbus5 = Convert.ToByte(classEE233h.Cbus5);
			stEEPROM233HP.ft232h.Cbus6 = Convert.ToByte(classEE233h.Cbus6);
			stEEPROM233HP.ft232h.Cbus7 = Convert.ToByte(classEE233h.Cbus7);
			stEEPROM233HP.ft232h.Cbus8 = Convert.ToByte(classEE233h.Cbus8);
			stEEPROM233HP.ft232h.Cbus9 = Convert.ToByte(classEE233h.Cbus9);
			stEEPROM233HP.ft232h.IsFifo = Convert.ToByte(classEE233h.IsFifo);
			stEEPROM233HP.ft232h.IsFifoTar = Convert.ToByte(classEE233h.IsFifoTar);
			stEEPROM233HP.ft232h.IsFastSer = Convert.ToByte(classEE233h.IsFastSer);
			stEEPROM233HP.ft232h.IsFT1248 = Convert.ToByte(classEE233h.IsFT1248);
			stEEPROM233HP.ft232h.FT1248Cpol = Convert.ToByte(classEE233h.FT1248Cpol);
			stEEPROM233HP.ft232h.FT1248Lsb = Convert.ToByte(classEE233h.FT1248Lsb);
			stEEPROM233HP.ft232h.FT1248FlowControl = Convert.ToByte(classEE233h.FT1248FlowControl);
			stEEPROM233HP.ft232h.DriverType = Convert.ToByte(classEE233h.DriverType);
			stEEPROM233HP.ft232h.PowerSaveEnable = Convert.ToByte(classEE233h.PowerSaveEnable);

			//PD
			stEEPROM233HP.pd.srprs = classEE233h.srprs;        // non-zero to enable Sink Request Power Role Swap 
			stEEPROM233HP.pd.sraprs = classEE233h.sraprs;       // non-zero to enable Sink Accept PR Swap
			stEEPROM233HP.pd.srrprs = classEE233h.srrprs;       // non-zero to enable Source Request PR SWAP
			stEEPROM233HP.pd.saprs = classEE233h.saprs;        // non-zero to enable Source Accept PR SWAP
			stEEPROM233HP.pd.vconns = classEE233h.vconns;       // non-zero to enable vConn Swap
			stEEPROM233HP.pd.passthru = classEE233h.passthru;     // non-zero to enable Pass Through
			stEEPROM233HP.pd.extmcu = classEE233h.extmcu;       // non-zero to enable External MCU
			stEEPROM233HP.pd.pd2en = classEE233h.pd2en;        // non-zero to enable PD2
			stEEPROM233HP.pd.pd1autoclk = classEE233h.pd1autoclk;   // non-zero to enable PD1 Auto Clock
			stEEPROM233HP.pd.pd2autoclk = classEE233h.pd2autoclk;   // non-zero to enable PD2 Auto Clock
			stEEPROM233HP.pd.useefuse = classEE233h.useefuse;     // non-zero to Use EFUSE
			stEEPROM233HP.pd.extvconn = classEE233h.extvconn;     // non-zero to enable External vConn
																  //stEEPROM4233.pwrmgmt = classEE4233h.pwrmgmt;      // Power Management setting, supported values are 0 to 4

			// GPIO Configuration
			stEEPROM233HP.pd.count = classEE233h.count;        // GPIO Count, supported values are 0 to 7 
			stEEPROM233HP.pd.gpio1 = classEE233h.gpio1;        // GPIO Number 1, supported values are 0 to 7, 15 for N/A
			stEEPROM233HP.pd.gpio2 = classEE233h.gpio2;        // GPIO Number 2, supported values are 0 to 7, 15 for N/A
			stEEPROM233HP.pd.gpio3 = classEE233h.gpio3;        // GPIO Number 3, supported values are 0 to 7, 15 for N/A
			stEEPROM233HP.pd.gpio4 = classEE233h.gpio4;        // GPIO Number 4, supported values are 0 to 7, 15 for N/A
			stEEPROM233HP.pd.gpio5 = classEE233h.gpio5;        // GPIO Number 5, supported values are 0 to 7, 15 for N/A
			stEEPROM233HP.pd.gpio6 = classEE233h.gpio6;        // GPIO Number 6, supported values are 0 to 7, 15 for N/A
			stEEPROM233HP.pd.gpio7 = classEE233h.gpio7;        // GPIO Number 7, supported values are 0 to 7, 15 for N/A
			stEEPROM233HP.pd.pd1lden = classEE233h.pd1lden;      // PD1 Load Enable, supported values are 0 to 7, 15 for N/A
			stEEPROM233HP.pd.pd2lden = classEE233h.pd2lden;      // PD2 Load Enable, supported values are 0 to 7, 15 for N/A
			stEEPROM233HP.pd.dispin = classEE233h.dispin;       // Discharge Pin, supported values are 0 to 7, 15 for N/A
			stEEPROM233HP.pd.disenbm = classEE233h.disenbm;      // Discharge Enable BM, 0 for "Drive Hi", 1 for "Drive Low", 2 for "Input Mode", 3 for "Don't Care"
			stEEPROM233HP.pd.disdisbm = classEE233h.disdisbm;     // Discharge Disable BM, 0 for "Drive Hi", 1 for "Drive Low", 2 for "Input Mode", 3 for "Don't Care"
			stEEPROM233HP.pd.ccselect = classEE233h.ccselect;

			// ISET Configuration
			stEEPROM233HP.pd.iset1 = classEE233h.iset1;        // ISET1, supported values are 0 to 7, 15 for N/A
			stEEPROM233HP.pd.iset2 = classEE233h.iset2;        // ISET2, supported values are 0 to 7, 15 for N/A
			stEEPROM233HP.pd.iset3 = classEE233h.iset3;        // ISET3, supported values are 0 to 7, 15 for N/A
			stEEPROM233HP.pd.extiset = classEE233h.extiset;      // non-zero to enable EXTEND_ISET
			stEEPROM233HP.pd.isetpd2 = classEE233h.isetpd2;      // non-zero to enable ISET_PD2
			stEEPROM233HP.pd.iseten = classEE233h.iseten;       // non-zero to set ISET_ENABLED

			unsafe
			{
				int i;
				for (i = 0; i < 7; i++)
				{
					stEEPROM233HP.pd.PDO1_GPIO[i] = classEE233h.PDO1_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM233HP.pd.PDO2_GPIO[i] = classEE233h.PDO2_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM233HP.pd.PDO3_GPIO[i] = classEE233h.PDO3_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM233HP.pd.PDO4_GPIO[i] = classEE233h.PDO4_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM233HP.pd.PDO5_GPIO[i] = classEE233h.PDO5_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM233HP.pd.PDO6_GPIO[i] = classEE233h.PDO6_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM233HP.pd.PDO7_GPIO[i] = classEE233h.PDO7_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM233HP.pd.VSET0V_GPIO[i] = classEE233h.VSET0V_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM233HP.pd.VSAFE5V_GPIO[i] = classEE233h.VSAFE5V_GPIO[i];
				}
			}

			stEEPROM233HP.pd.BM_PDO_Sink.PDO1ma = classEE233h.BM_PDO_Sink.PDO1ma;
			stEEPROM233HP.pd.BM_PDO_Sink.PDO1mv = classEE233h.BM_PDO_Sink.PDO1mv;
			stEEPROM233HP.pd.BM_PDO_Sink.PDO2ma = classEE233h.BM_PDO_Sink.PDO2ma;
			stEEPROM233HP.pd.BM_PDO_Sink.PDO2mv = classEE233h.BM_PDO_Sink.PDO2mv;
			stEEPROM233HP.pd.BM_PDO_Sink.PDO3ma = classEE233h.BM_PDO_Sink.PDO3ma;
			stEEPROM233HP.pd.BM_PDO_Sink.PDO3mv = classEE233h.BM_PDO_Sink.PDO3mv;
			stEEPROM233HP.pd.BM_PDO_Sink.PDO4ma = classEE233h.BM_PDO_Sink.PDO4ma;
			stEEPROM233HP.pd.BM_PDO_Sink.PDO4mv = classEE233h.BM_PDO_Sink.PDO4mv;
			stEEPROM233HP.pd.BM_PDO_Sink.PDO5ma = classEE233h.BM_PDO_Sink.PDO5ma;
			stEEPROM233HP.pd.BM_PDO_Sink.PDO5mv = classEE233h.BM_PDO_Sink.PDO5mv;
			stEEPROM233HP.pd.BM_PDO_Sink.PDO6ma = classEE233h.BM_PDO_Sink.PDO6ma;
			stEEPROM233HP.pd.BM_PDO_Sink.PDO6mv = classEE233h.BM_PDO_Sink.PDO6mv;
			stEEPROM233HP.pd.BM_PDO_Sink.PDO7ma = classEE233h.BM_PDO_Sink.PDO7ma;
			stEEPROM233HP.pd.BM_PDO_Sink.PDO7mv = classEE233h.BM_PDO_Sink.PDO7mv;

			stEEPROM233HP.pd.BM_PDO_Source.PDO1ma = classEE233h.BM_PDO_Source.PDO1ma;
			stEEPROM233HP.pd.BM_PDO_Source.PDO1mv = classEE233h.BM_PDO_Source.PDO1mv;
			stEEPROM233HP.pd.BM_PDO_Source.PDO2ma = classEE233h.BM_PDO_Source.PDO2ma;
			stEEPROM233HP.pd.BM_PDO_Source.PDO2mv = classEE233h.BM_PDO_Source.PDO2mv;
			stEEPROM233HP.pd.BM_PDO_Source.PDO3ma = classEE233h.BM_PDO_Source.PDO3ma;
			stEEPROM233HP.pd.BM_PDO_Source.PDO3mv = classEE233h.BM_PDO_Source.PDO3mv;
			stEEPROM233HP.pd.BM_PDO_Source.PDO4ma = classEE233h.BM_PDO_Source.PDO4ma;
			stEEPROM233HP.pd.BM_PDO_Source.PDO4mv = classEE233h.BM_PDO_Source.PDO4mv;
			stEEPROM233HP.pd.BM_PDO_Source.PDO5ma = classEE233h.BM_PDO_Source.PDO5ma;
			stEEPROM233HP.pd.BM_PDO_Source.PDO5mv = classEE233h.BM_PDO_Source.PDO5mv;
			stEEPROM233HP.pd.BM_PDO_Source.PDO6ma = classEE233h.BM_PDO_Source.PDO6ma;
			stEEPROM233HP.pd.BM_PDO_Source.PDO6mv = classEE233h.BM_PDO_Source.PDO6mv;
			stEEPROM233HP.pd.BM_PDO_Source.PDO7ma = classEE233h.BM_PDO_Source.PDO7ma;
			stEEPROM233HP.pd.BM_PDO_Source.PDO7mv = classEE233h.BM_PDO_Source.PDO7mv;

			stEEPROM233HP.pd.BM_PDO_Sink_2.PDO1ma = classEE233h.BM_PDO_Sink_2.PDO1ma;
			stEEPROM233HP.pd.BM_PDO_Sink_2.PDO1mv = classEE233h.BM_PDO_Sink_2.PDO1mv;
			stEEPROM233HP.pd.BM_PDO_Sink_2.PDO2ma = classEE233h.BM_PDO_Sink_2.PDO2ma;
			stEEPROM233HP.pd.BM_PDO_Sink_2.PDO2mv = classEE233h.BM_PDO_Sink_2.PDO2mv;
			stEEPROM233HP.pd.BM_PDO_Sink_2.PDO3ma = classEE233h.BM_PDO_Sink_2.PDO3ma;
			stEEPROM233HP.pd.BM_PDO_Sink_2.PDO3mv = classEE233h.BM_PDO_Sink_2.PDO3mv;
			stEEPROM233HP.pd.BM_PDO_Sink_2.PDO4ma = classEE233h.BM_PDO_Sink_2.PDO4ma;
			stEEPROM233HP.pd.BM_PDO_Sink_2.PDO4mv = classEE233h.BM_PDO_Sink_2.PDO4mv;
			stEEPROM233HP.pd.BM_PDO_Sink_2.PDO5ma = classEE233h.BM_PDO_Sink_2.PDO5ma;
			stEEPROM233HP.pd.BM_PDO_Sink_2.PDO5mv = classEE233h.BM_PDO_Sink_2.PDO5mv;
			stEEPROM233HP.pd.BM_PDO_Sink_2.PDO6ma = classEE233h.BM_PDO_Sink_2.PDO6ma;
			stEEPROM233HP.pd.BM_PDO_Sink_2.PDO6mv = classEE233h.BM_PDO_Sink_2.PDO6mv;
			stEEPROM233HP.pd.BM_PDO_Sink_2.PDO7ma = classEE233h.BM_PDO_Sink_2.PDO7ma;
			stEEPROM233HP.pd.BM_PDO_Sink_2.PDO7mv = classEE233h.BM_PDO_Sink_2.PDO7mv;

			stEEPROM233HP.pd.srt = classEE233h.srt;          // Sender Response Timer
			stEEPROM233HP.pd.hrt = classEE233h.hrt;          // Hard Reset Timer
			stEEPROM233HP.pd.sct = classEE233h.sct;          // Source Capability Timer
			stEEPROM233HP.pd.dit = classEE233h.dit;          // Discover Identity Timer
			stEEPROM233HP.pd.srcrt = classEE233h.srcrt;       // Source Recover Timer
			stEEPROM233HP.pd.trt = classEE233h.trt;         // Transition Timer
			stEEPROM233HP.pd.sofft = classEE233h.sofft;       // Source off timer
			stEEPROM233HP.pd.nrt = classEE233h.nrt;         // No Response Timer
			stEEPROM233HP.pd.swct = classEE233h.swct;        // Sink Wait Capability Timer
			stEEPROM233HP.pd.snkrt = classEE233h.snkrt;       // Sink Request Timer
			stEEPROM233HP.pd.dt = classEE233h.dt;           // Discharge Timer
			stEEPROM233HP.pd.cnst = classEE233h.cnst;         // Chunk not supported timer
			stEEPROM233HP.pd.it = classEE233h.it;          // Idle Timer

			// PD Control
			stEEPROM233HP.pd.i2caddr = classEE233h.i2caddr;      // I2C Address (hex)
			stEEPROM233HP.pd.prou = classEE233h.prou;          // Power Reserved for OWN use
			stEEPROM233HP.pd.trim1 = classEE233h.trim1;         // TRIM1
			stEEPROM233HP.pd.trim2 = classEE233h.trim2;         // TRIM2
			stEEPROM233HP.pd.extdc = classEE233h.extdc;        // non-zero to enable ETERNAL_DC_POWER

			// Check the size of the structure...
			int size = Marshal.SizeOf(stEEPROM233HP);
			// Allocate space for our pointer...
			IntPtr eeDataMarshal = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(stEEPROM233HP, eeDataMarshal, false);

			ftStatus = FT_EEPROM_Program(ftHandle, eeDataMarshal, (uint)size, manufacturer, manufacturerID, description, serialNumber);
			strError = ftStatus.ToString();

			return ftStatus;
		}





		//**************************************************************************
		// WriteXSeriesEEPROM
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Writes the specified values to the EEPROM of an X-Series device.
		/// Calls FT_EEPROM_Program in FTD2XX DLL
		/// </summary>
		/// <returns>FT_STATUS value from FT_EEPROM_Program in FTD2XX DLL</returns>
		/// <param name="eeX">The EEPROM settings to be written to the device</param>
		/// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not match the type required by this method.</exception>
		public FT_STATUS Write_XSeries_EEPROM(FT_XSERIES_EEPROM_STRUCTURE eeX)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;
            FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

            byte[] manufacturer, manufacturerID, description, serialNumber;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_EEPROM_Program FT_EEPROM_Program = (tFT_EEPROM_Program)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EEPROM_Program, typeof(tFT_EEPROM_Program));

            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            // Check that it is an FT232H that we are trying to write
            GetDeviceType(ref DeviceType);
            if (DeviceType != FT_DEVICETYPE.FT_DEVICE_X_SERIES)
            {
                // If it is not, throw an exception
                ftErrorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            // Check for VID and PID of 0x0000
            if ((eeX.VendorID == 0x0000) | (eeX.ProductID == 0x0000))
            {
                // Do not allow users to program the device with VID or PID of 0x0000
                return FT_STATUS.FT_INVALID_PARAMETER;
            }

            FT_XSERIES_DATA eeData = new FT_XSERIES_DATA();

            // String manipulation...
            // Allocate space from unmanaged heap
            manufacturer = new byte[32];
            manufacturerID = new byte[16];
            description = new byte[64];
            serialNumber = new byte[16];

            // Check lengths of strings to make sure that they are within our limits
            // If not, trim them to make them our maximum length
            if (eeX.Manufacturer.Length > 32)
                eeX.Manufacturer = eeX.Manufacturer.Substring(0, 32);
            if (eeX.ManufacturerID.Length > 16)
                eeX.ManufacturerID = eeX.ManufacturerID.Substring(0, 16);
            if (eeX.Description.Length > 64)
                eeX.Description = eeX.Description.Substring(0, 64);
            if (eeX.SerialNumber.Length > 16)
                eeX.SerialNumber = eeX.SerialNumber.Substring(0, 16);

            // Set string values
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            manufacturer = encoding.GetBytes(eeX.Manufacturer);
            manufacturerID = encoding.GetBytes(eeX.ManufacturerID);
            description = encoding.GetBytes(eeX.Description);
            serialNumber = encoding.GetBytes(eeX.SerialNumber);

            // Map non-string elements to structure to be returned
            // Standard elements
            eeData.common.deviceType = (uint)FT_DEVICETYPE.FT_DEVICE_X_SERIES;
            eeData.common.VendorId = eeX.VendorID;
            eeData.common.ProductId = eeX.ProductID;
            eeData.common.MaxPower = eeX.MaxPower;
            eeData.common.SelfPowered = Convert.ToByte(eeX.SelfPowered);
            eeData.common.RemoteWakeup = Convert.ToByte(eeX.RemoteWakeup);
            eeData.common.SerNumEnable = Convert.ToByte(eeX.SerNumEnable);
            eeData.common.PullDownEnable = Convert.ToByte(eeX.PullDownEnable);
            // X-Series specific fields
            // CBUS
            eeData.Cbus0 = eeX.Cbus0;
            eeData.Cbus1 = eeX.Cbus1;
            eeData.Cbus2 = eeX.Cbus2;
            eeData.Cbus3 = eeX.Cbus3;
            eeData.Cbus4 = eeX.Cbus4;
            eeData.Cbus5 = eeX.Cbus5;
            eeData.Cbus6 = eeX.Cbus6;
            // Drive Options
            eeData.ACDriveCurrent = eeX.ACDriveCurrent;
            eeData.ACSchmittInput = eeX.ACSchmittInput;
            eeData.ACSlowSlew = eeX.ACSlowSlew;
            eeData.ADDriveCurrent = eeX.ADDriveCurrent;
            eeData.ADSchmittInput = eeX.ADSchmittInput;
            eeData.ADSlowSlew = eeX.ADSlowSlew;
            // BCD
            eeData.BCDDisableSleep = eeX.BCDDisableSleep;
            eeData.BCDEnable = eeX.BCDEnable;
            eeData.BCDForceCbusPWREN = eeX.BCDForceCbusPWREN;
            // FT1248
            eeData.FT1248Cpol = eeX.FT1248Cpol;
            eeData.FT1248FlowControl = eeX.FT1248FlowControl;
            eeData.FT1248Lsb = eeX.FT1248Lsb;
            // I2C
            eeData.I2CDeviceId = eeX.I2CDeviceId;
            eeData.I2CDisableSchmitt = eeX.I2CDisableSchmitt;
            eeData.I2CSlaveAddress = eeX.I2CSlaveAddress;
            // RS232 Signals
            eeData.InvertCTS = eeX.InvertCTS;
            eeData.InvertDCD = eeX.InvertDCD;
            eeData.InvertDSR = eeX.InvertDSR;
            eeData.InvertDTR = eeX.InvertDTR;
            eeData.InvertRI = eeX.InvertRI;
            eeData.InvertRTS = eeX.InvertRTS;
            eeData.InvertRXD = eeX.InvertRXD;
            eeData.InvertTXD = eeX.InvertTXD;
            // Hardware Options
            eeData.PowerSaveEnable = eeX.PowerSaveEnable;
            eeData.RS485EchoSuppress = eeX.RS485EchoSuppress;
            // Driver Option
            eeData.DriverType = eeX.IsVCP;

            // Check the size of the structure...
            int size = Marshal.SizeOf(eeData);
            // Allocate space for our pointer...
            IntPtr eeDataMarshal = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(eeData, eeDataMarshal, false);

            ftStatus = FT_EEPROM_Program(ftHandle, eeDataMarshal, (uint)size, manufacturer, manufacturerID, description, serialNumber);


            return ftStatus;
        }


		public FT_STATUS Write_EE_FT4233H(CEEPROM_FT4233H classEE4233h, ref string strError)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

			// If the DLL hasn't been loaded, just return here
			if (ftHandle == IntPtr.Zero)
            {
				strError = "DLL handle is null - DLL not loaded";
				return FT_STATUS.FT_OTHER_ERROR;
			}

			tFT_EE_Program FT_EE_Program = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_Program, typeof(tFT_EE_Program));

			FT_DEVICETYPE deviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
			GetDeviceType(ref deviceType);
			if (deviceType != FT_DEVICETYPE.FT_DEVICE_4233HP)
			{
				strError = "Error - incorrect device type. Expected Device=FT_DEVICE_4233HP(18). Connected device=" + deviceType.ToString();
				return FT_STATUS.FT_OTHER_ERROR;
			}

			if ((classEE4233h.VendorID == 0x0000) || (classEE4233h.ProductID == 0x0000))
			{
				strError = "Error - VendorID or ProductID = 0";
				return FT_STATUS.FT_INVALID_PARAMETER;
			}

			FT_PROGRAM_DATA prgmData = new FT_PROGRAM_DATA();

			// Set up structure headers
			prgmData.Signature1 = 0x00000000;
			prgmData.Signature2 = 0xFFFFFFFF;
			prgmData.Version = 4;
			prgmData.VendorID = classEE4233h.VendorID;
			prgmData.ProductID = classEE4233h.ProductID;

			// Allocate space from unmanaged heap
			prgmData.Manufacturer = Marshal.AllocHGlobal(32);
			prgmData.ManufacturerID = Marshal.AllocHGlobal(16);
			prgmData.Description = Marshal.AllocHGlobal(64);
			prgmData.SerialNumber = Marshal.AllocHGlobal(16);

			// Check lengths of strings to make sure that they are within our limits
			// If not, trim them to make them our maximum length
			if (classEE4233h.Manufacturer.Length > 32)
				classEE4233h.Manufacturer = classEE4233h.Manufacturer.Substring(0, 32);
			if (classEE4233h.ManufacturerID.Length > 16)
				classEE4233h.ManufacturerID = classEE4233h.ManufacturerID.Substring(0, 16);
			if (classEE4233h.Description.Length > 64)
				classEE4233h.Description = classEE4233h.Description.Substring(0, 64);
			if (classEE4233h.SerialNumber.Length > 16)
				classEE4233h.SerialNumber = classEE4233h.SerialNumber.Substring(0, 16);

			// Set string values
			prgmData.Manufacturer = Marshal.StringToHGlobalAnsi(classEE4233h.Manufacturer);
			prgmData.ManufacturerID = Marshal.StringToHGlobalAnsi(classEE4233h.ManufacturerID);
			prgmData.Description = Marshal.StringToHGlobalAnsi(classEE4233h.Description);
			prgmData.SerialNumber = Marshal.StringToHGlobalAnsi(classEE4233h.SerialNumber);

			// Map non-string elements to structure
			// Standard elements

			prgmData.MaxPower = classEE4233h.MaxPower;
			prgmData.PnP = 0;
			prgmData.SelfPowered = Convert.ToUInt16(classEE4233h.SelfPowered);
			prgmData.RemoteWakeup = Convert.ToUInt16(classEE4233h.RemoteWakeup);

			prgmData.Rev4 = 0;
			prgmData.IsoIn = 0;
			prgmData.IsoOut = 0;
			prgmData.PullDownEnable = 0;
			prgmData.SerNumEnable = Convert.ToByte(classEE4233h.SerNumEnable);
			prgmData.USBVersionEnable = 0;
			prgmData.USBVersion = 0x0200;

			prgmData.Rev5 = 0;
			prgmData.IsoInA = 0;
			prgmData.Rev5 = 0;
			prgmData.IsoInA = 0;
			prgmData.IsoInB = 0;
			prgmData.IsoOutA = 0;
			prgmData.IsoOutB = 0;
			prgmData.PullDownEnable5 = 0;
			prgmData.SerNumEnable5 = 0;
			prgmData.USBVersionEnable5 = 0;
			prgmData.USBVersion5 = 0;
			prgmData.AIsHighCurrent = 0;
			prgmData.BIsHighCurrent = 0;
			prgmData.IFAIsFifo = 0;
			prgmData.IFAIsFifoTar = 0;
			prgmData.IFAIsFastSer = 0;
			prgmData.AIsVCP = 0;
			prgmData.IFBIsFifo = 0;
			prgmData.IFBIsFifoTar = 0;
			prgmData.IFBIsFastSer = 0;
			prgmData.BIsVCP = 0;

			prgmData.UseExtOsc = 0;
			prgmData.HighDriveIOs = 0;
			prgmData.EndpointSize = 0;
			prgmData.PullDownEnableR = 0;
			prgmData.SerNumEnableR = 0;
			prgmData.InvertTXD = 0;
			prgmData.InvertRXD = 0;
			prgmData.InvertRTS = 0;
			prgmData.InvertCTS = 0;
			prgmData.InvertDTR = 0;
			prgmData.InvertDSR = 0;
			prgmData.InvertDCD = 0;
			prgmData.InvertRI = 0;
			prgmData.Cbus0 = 0;
			prgmData.Cbus1 = 0;
			prgmData.Cbus2 = 0;
			prgmData.Cbus3 = 0;
			prgmData.Cbus4 = 0;
			prgmData.RIsD2XX = 0;

			prgmData.PullDownEnable7 = 0;
			prgmData.SerNumEnable7 = 0;
			prgmData.ALSlowSlew = 0;
			prgmData.ALSchmittInput = 0;
			prgmData.ALDriveCurrent = 0;
			prgmData.AHSlowSlew = 0;
			prgmData.AHSchmittInput = 0;
			prgmData.AHDriveCurrent = 0;
			prgmData.BLSlowSlew = 0;
			prgmData.BLSchmittInput = 0;
			prgmData.BLDriveCurrent = 0;
			prgmData.BHSlowSlew = 0;
			prgmData.BHSchmittInput = 0;
			prgmData.BHDriveCurrent = 0;
			prgmData.IFAIsFifo7 = 0;
			prgmData.IFAIsFifoTar7 = 0;
			prgmData.IFAIsFastSer7 = 0;
			prgmData.AIsVCP7 = 0;
			prgmData.IFBIsFifo7 = 0;
			prgmData.IFBIsFifoTar7 = 0;
			prgmData.IFBIsFastSer7 = 0;
			prgmData.BIsVCP7 = 0;
			prgmData.PowerSaveEnable = 0;


			// 4232H specific fields
			prgmData.PullDownEnable8 = Convert.ToByte(classEE4233h.PullDownEnable);
			prgmData.SerNumEnable8 = Convert.ToByte(classEE4233h.SerNumEnable);
			prgmData.ASlowSlew = Convert.ToByte(classEE4233h.ASlowSlew);
			prgmData.ASchmittInput = Convert.ToByte(classEE4233h.ASchmittInput);
			prgmData.ADriveCurrent = classEE4233h.ADriveCurrent;
			prgmData.BSlowSlew = Convert.ToByte(classEE4233h.BSlowSlew);
			prgmData.BSchmittInput = Convert.ToByte(classEE4233h.BSchmittInput);
			prgmData.BDriveCurrent = classEE4233h.BDriveCurrent;
			prgmData.CSlowSlew = Convert.ToByte(classEE4233h.CSlowSlew);
			prgmData.CSchmittInput = Convert.ToByte(classEE4233h.CSchmittInput);
			prgmData.CDriveCurrent = classEE4233h.CDriveCurrent;
			prgmData.DSlowSlew = Convert.ToByte(classEE4233h.DSlowSlew);
			prgmData.DSchmittInput = Convert.ToByte(classEE4233h.DSchmittInput);
			prgmData.DDriveCurrent = classEE4233h.DDriveCurrent;
			prgmData.ARIIsTXDEN = Convert.ToByte(classEE4233h.ARIIsTXDEN);
			prgmData.BRIIsTXDEN = Convert.ToByte(classEE4233h.BRIIsTXDEN);
			prgmData.CRIIsTXDEN = Convert.ToByte(classEE4233h.CRIIsTXDEN);
			prgmData.DRIIsTXDEN = Convert.ToByte(classEE4233h.DRIIsTXDEN);
			prgmData.AIsVCP8 = Convert.ToByte(classEE4233h.ADriverType);
			prgmData.BIsVCP8 = Convert.ToByte(classEE4233h.BDriverType);
			prgmData.CIsVCP8 = Convert.ToByte(classEE4233h.CDriverType);
			prgmData.DIsVCP8 = Convert.ToByte(classEE4233h.DDriverType);


			prgmData.PullDownEnableH = 0;
			prgmData.SerNumEnableH = 0;
			prgmData.ACSlowSlewH = 0;
			prgmData.ACSchmittInputH = 0;
			prgmData.ACDriveCurrentH = 0;
			prgmData.ADSlowSlewH = 0;
			prgmData.ADSchmittInputH = 0;
			prgmData.ADDriveCurrentH = 0;
			prgmData.Cbus0H = 0;
			prgmData.Cbus1H = 0;
			prgmData.Cbus2H = 0;
			prgmData.Cbus3H = 0;
			prgmData.Cbus4H = 0;
			prgmData.Cbus5H = 0;
			prgmData.Cbus6H = 0;
			prgmData.Cbus7H = 0;
			prgmData.Cbus8H = 0;
			prgmData.Cbus9H = 0;
			prgmData.IsFifoH = 0;
			prgmData.IsFifoTarH = 0;
			prgmData.IsFastSerH = 0;
			prgmData.IsFT1248H = 0;
			prgmData.FT1248CpolH = 0;
			prgmData.FT1248LsbH = 0;
			prgmData.FT1248FlowControlH = 0;
			prgmData.IsVCPH = 0;
			prgmData.PowerSaveEnableH = 0;


			// Call FT_EE_Program
			ftStatus = FT_EE_Program(ftHandle, prgmData);

			// Free unmanaged buffers
			Marshal.FreeHGlobal(prgmData.Manufacturer);
			Marshal.FreeHGlobal(prgmData.ManufacturerID);
			Marshal.FreeHGlobal(prgmData.Description);
			Marshal.FreeHGlobal(prgmData.SerialNumber);

			return ftStatus;
		}
		/*		
				public FT_STATUS Write_EEPROM_FT2233H(CEEPROM_FT2233H classEE2233h, ref string strError)
				{
					// Initialise ftStatus to something other than FT_OK
					FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

					// If the DLL hasn't been loaded, just return here
					if (ftHandle == IntPtr.Zero)
					{
						strError = "DLL handle is null - DLL not loaded";
						return FT_STATUS.FT_OTHER_ERROR;
					}

					tFT_EEPROM_Program FT_EEPROM_Program = (tFT_EEPROM_Program)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EEPROM_Program, typeof(tFT_EEPROM_Program));

					FT_DEVICETYPE deviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
					GetDeviceType(ref deviceType);
					if (deviceType != FT_DEVICETYPE.FT_DEVICE_2233H)
					{
						strError = "Error - incorrect device type. Expected Device=FT_DEVICE_2233H(18). Connected device=" + deviceType.ToString();
						return FT_STATUS.FT_OTHER_ERROR;
					}

					if (classEE2233h.VendorID == 0x0000)
					{
						strError = "Error - VendorID = 0";
						return FT_STATUS.FT_INVALID_PARAMETER;
					}
					if (classEE2233h.ProductID == 0x0000)
					{
						strError = "Error - ProductID = 0";
						return FT_STATUS.FT_INVALID_PARAMETER;
					}

					STRUCT_EEPROM_FT2233H stEEPROM2233 = new STRUCT_EEPROM_FT2233H();

					byte[] manufacturer = new byte[32];
					byte[] manufacturerID = new byte[16];
					byte[] description = new byte[64];
					byte[] serialNumber = new byte[16];

					if (classEE2233h.Manufacturer.Length > 32)
						classEE2233h.Manufacturer = classEE2233h.Manufacturer.Substring(0, 32);
					if (classEE2233h.ManufacturerID.Length > 16)
						classEE2233h.ManufacturerID = classEE2233h.ManufacturerID.Substring(0, 16);
					if (classEE2233h.Description.Length > 64)
						classEE2233h.Description = classEE2233h.Description.Substring(0, 64);
					if (classEE2233h.SerialNumber.Length > 16)
						classEE2233h.SerialNumber = classEE2233h.SerialNumber.Substring(0, 16);

					// Set string values
					System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
					manufacturer = encoding.GetBytes(classEE2233h.Manufacturer);
					manufacturerID = encoding.GetBytes(classEE2233h.ManufacturerID);
					description = encoding.GetBytes(classEE2233h.Description);
					serialNumber = encoding.GetBytes(classEE2233h.SerialNumber);

					stEEPROM2233.ft4232h.common.deviceType = (uint)FT_DEVICETYPE.FT_DEVICE_2233H;
					stEEPROM2233.ft4232h.common.VendorId = classEE2233h.VendorID;
					stEEPROM2233.ft4232h.common.ProductId = classEE2233h.ProductID;
					stEEPROM2233.ft4232h.common.SerNumEnable = Convert.ToByte(classEE2233h.SerNumEnable);
					stEEPROM2233.ft4232h.common.MaxPower = classEE2233h.MaxPower;
					stEEPROM2233.ft4232h.common.SelfPowered = Convert.ToByte(classEE2233h.SelfPowered);
					stEEPROM2233.ft4232h.common.RemoteWakeup = Convert.ToByte(classEE2233h.RemoteWakeup);
					stEEPROM2233.ft4232h.common.PullDownEnable = Convert.ToByte(classEE2233h.PullDownEnable);

					stEEPROM2233.ft4232h.ASlowSlew = classEE2233h.ASlowSlew;
					stEEPROM2233.ft4232h.ASchmittInput = classEE2233h.ASchmittInput;
					stEEPROM2233.ft4232h.ADriveCurrent = classEE2233h.ADriveCurrent;
					stEEPROM2233.ft4232h.BSlowSlew = classEE2233h.BSlowSlew;
					stEEPROM2233.ft4232h.BSchmittInput = classEE2233h.BSchmittInput;
					stEEPROM2233.ft4232h.BDriveCurrent = classEE2233h.BDriveCurrent;
					stEEPROM2233.ft4232h.ARIIsTXDEN = classEE2233h.ARIIsTXDEN;
					stEEPROM2233.ft4232h.BRIIsTXDEN = classEE2233h.BRIIsTXDEN;
					stEEPROM2233.ft4232h.ADriverType = classEE2233h.ADriverType;
					stEEPROM2233.ft4232h.BDriverType = classEE2233h.BDriverType;

					//PD
					stEEPROM2233.pd.srprs = classEE2233h.srprs;        // non-zero to enable Sink Request Power Role Swap 
					stEEPROM2233.pd.sraprs = classEE2233h.sraprs;       // non-zero to enable Sink Accept PR Swap
					stEEPROM2233.pd.srrprs = classEE2233h.srrprs;       // non-zero to enable Source Request PR SWAP
					stEEPROM2233.pd.saprs = classEE2233h.saprs;        // non-zero to enable Source Accept PR SWAP
					stEEPROM2233.pd.vconns = classEE2233h.vconns;       // non-zero to enable vConn Swap
					stEEPROM2233.pd.passthru = classEE2233h.passthru;     // non-zero to enable Pass Through
					stEEPROM2233.pd.extmcu = classEE2233h.extmcu;       // non-zero to enable External MCU
					stEEPROM2233.pd.pd2en = classEE2233h.pd2en;        // non-zero to enable PD2
					stEEPROM2233.pd.pd1autoclk = classEE2233h.pd1autoclk;   // non-zero to enable PD1 Auto Clock
					stEEPROM2233.pd.pd2autoclk = classEE2233h.pd2autoclk;   // non-zero to enable PD2 Auto Clock
					stEEPROM2233.pd.useefuse = classEE2233h.useefuse;     // non-zero to Use EFUSE
					stEEPROM2233.pd.extvconn = classEE2233h.extvconn;     // non-zero to enable External vConn


					// GPIO Configuration
					stEEPROM2233.pd.count = classEE2233h.count;        // GPIO Count, supported values are 0 to 7 
					stEEPROM2233.pd.gpio1 = classEE2233h.gpio1;        // GPIO Number 1, supported values are 0 to 7, 15 for N/A
					stEEPROM2233.pd.gpio2 = classEE2233h.gpio2;        // GPIO Number 2, supported values are 0 to 7, 15 for N/A
					stEEPROM2233.pd.gpio3 = classEE2233h.gpio3;        // GPIO Number 3, supported values are 0 to 7, 15 for N/A
					stEEPROM2233.pd.gpio4 = classEE2233h.gpio4;        // GPIO Number 4, supported values are 0 to 7, 15 for N/A
					stEEPROM2233.pd.gpio5 = classEE2233h.gpio5;        // GPIO Number 5, supported values are 0 to 7, 15 for N/A
					stEEPROM2233.pd.gpio6 = classEE2233h.gpio6;        // GPIO Number 6, supported values are 0 to 7, 15 for N/A
					stEEPROM2233.pd.gpio7 = classEE2233h.gpio7;        // GPIO Number 7, supported values are 0 to 7, 15 for N/A
					stEEPROM2233.pd.pd1lden = classEE2233h.pd1lden;      // PD1 Load Enable, supported values are 0 to 7, 15 for N/A
					stEEPROM2233.pd.pd2lden = classEE2233h.pd2lden;      // PD2 Load Enable, supported values are 0 to 7, 15 for N/A
					stEEPROM2233.pd.dispin = classEE2233h.dispin;       // Discharge Pin, supported values are 0 to 7, 15 for N/A
					stEEPROM2233.pd.disenbm = classEE2233h.disenbm;      // Discharge Enable BM, 0 for "Drive Hi", 1 for "Drive Low", 2 for "Input Mode", 3 for "Don't Care"
					stEEPROM2233.pd.disdisbm = classEE2233h.disdisbm;     // Discharge Disable BM, 0 for "Drive Hi", 1 for "Drive Low", 2 for "Input Mode", 3 for "Don't Care"
					stEEPROM2233.pd.ccselect = classEE2233h.ccselect;

					// ISET Configuration
					stEEPROM2233.pd.iset1 = classEE2233h.iset1;        // ISET1, supported values are 0 to 7, 15 for N/A
					stEEPROM2233.pd.iset2 = classEE2233h.iset2;        // ISET2, supported values are 0 to 7, 15 for N/A
					stEEPROM2233.pd.iset3 = classEE2233h.iset3;        // ISET3, supported values are 0 to 7, 15 for N/A
					stEEPROM2233.pd.extiset = classEE2233h.extiset;      // non-zero to enable EXTEND_ISET
					stEEPROM2233.pd.isetpd2 = classEE2233h.isetpd2;      // non-zero to enable ISET_PD2
					stEEPROM2233.pd.iseten = classEE2233h.iseten;       // non-zero to set ISET_ENABLED

					unsafe
					{
						int i;
						for (i = 0; i < 7; i++)
						{
							stEEPROM2233.pd.PDO1_GPIO[i] = classEE2233h.PDO1_GPIO[i];
						}
						for (i = 0; i < 7; i++)
						{
							stEEPROM2233.pd.PDO2_GPIO[i] = classEE2233h.PDO2_GPIO[i];
						}
						for (i = 0; i < 7; i++)
						{
							stEEPROM2233.pd.PDO3_GPIO[i] = classEE2233h.PDO3_GPIO[i];
						}
						for (i = 0; i < 7; i++)
						{
							stEEPROM2233.pd.PDO4_GPIO[i] = classEE2233h.PDO4_GPIO[i];
						}
						for (i = 0; i < 7; i++)
						{
							stEEPROM2233.pd.PDO5_GPIO[i] = classEE2233h.PDO5_GPIO[i];
						}
						for (i = 0; i < 7; i++)
						{
							stEEPROM2233.pd.PDO6_GPIO[i] = classEE2233h.PDO6_GPIO[i];
						}
						for (i = 0; i < 7; i++)
						{
							stEEPROM2233.pd.PDO7_GPIO[i] = classEE2233h.PDO7_GPIO[i];
						}
						for (i = 0; i < 7; i++)
						{
							stEEPROM2233.pd.VSET0V_GPIO[i] = classEE2233h.VSET0V_GPIO[i];
						}
						for (i = 0; i < 7; i++)
						{
							stEEPROM2233.pd.VSAFE5V_GPIO[i] = classEE2233h.VSAFE5V_GPIO[i];
						}
					}

					stEEPROM2233.pd.BM_PDO_Sink.PDO1ma = classEE2233h.BM_PDO_Sink.PDO1ma;
					stEEPROM2233.pd.BM_PDO_Sink.PDO1mv = classEE2233h.BM_PDO_Sink.PDO1mv;
					stEEPROM2233.pd.BM_PDO_Sink.PDO2ma = classEE2233h.BM_PDO_Sink.PDO2ma;
					stEEPROM2233.pd.BM_PDO_Sink.PDO2mv = classEE2233h.BM_PDO_Sink.PDO2mv;
					stEEPROM2233.pd.BM_PDO_Sink.PDO3ma = classEE2233h.BM_PDO_Sink.PDO3ma;
					stEEPROM2233.pd.BM_PDO_Sink.PDO3mv = classEE2233h.BM_PDO_Sink.PDO3mv;
					stEEPROM2233.pd.BM_PDO_Sink.PDO4ma = classEE2233h.BM_PDO_Sink.PDO4ma;
					stEEPROM2233.pd.BM_PDO_Sink.PDO4mv = classEE2233h.BM_PDO_Sink.PDO4mv;
					stEEPROM2233.pd.BM_PDO_Sink.PDO5ma = classEE2233h.BM_PDO_Sink.PDO5ma;
					stEEPROM2233.pd.BM_PDO_Sink.PDO5mv = classEE2233h.BM_PDO_Sink.PDO5mv;
					stEEPROM2233.pd.BM_PDO_Sink.PDO6ma = classEE2233h.BM_PDO_Sink.PDO6ma;
					stEEPROM2233.pd.BM_PDO_Sink.PDO6mv = classEE2233h.BM_PDO_Sink.PDO6mv;
					stEEPROM2233.pd.BM_PDO_Sink.PDO7ma = classEE2233h.BM_PDO_Sink.PDO7ma;
					stEEPROM2233.pd.BM_PDO_Sink.PDO7mv = classEE2233h.BM_PDO_Sink.PDO7mv;

					stEEPROM2233.pd.BM_PDO_Source.PDO1ma = classEE2233h.BM_PDO_Source.PDO1ma;
					stEEPROM2233.pd.BM_PDO_Source.PDO1mv = classEE2233h.BM_PDO_Source.PDO1mv;
					stEEPROM2233.pd.BM_PDO_Source.PDO2ma = classEE2233h.BM_PDO_Source.PDO2ma;
					stEEPROM2233.pd.BM_PDO_Source.PDO2mv = classEE2233h.BM_PDO_Source.PDO2mv;
					stEEPROM2233.pd.BM_PDO_Source.PDO3ma = classEE2233h.BM_PDO_Source.PDO3ma;
					stEEPROM2233.pd.BM_PDO_Source.PDO3mv = classEE2233h.BM_PDO_Source.PDO3mv;
					stEEPROM2233.pd.BM_PDO_Source.PDO4ma = classEE2233h.BM_PDO_Source.PDO4ma;
					stEEPROM2233.pd.BM_PDO_Source.PDO4mv = classEE2233h.BM_PDO_Source.PDO4mv;
					stEEPROM2233.pd.BM_PDO_Source.PDO5ma = classEE2233h.BM_PDO_Source.PDO5ma;
					stEEPROM2233.pd.BM_PDO_Source.PDO5mv = classEE2233h.BM_PDO_Source.PDO5mv;
					stEEPROM2233.pd.BM_PDO_Source.PDO6ma = classEE2233h.BM_PDO_Source.PDO6ma;
					stEEPROM2233.pd.BM_PDO_Source.PDO6mv = classEE2233h.BM_PDO_Source.PDO6mv;
					stEEPROM2233.pd.BM_PDO_Source.PDO7ma = classEE2233h.BM_PDO_Source.PDO7ma;
					stEEPROM2233.pd.BM_PDO_Source.PDO7mv = classEE2233h.BM_PDO_Source.PDO7mv;

					stEEPROM2233.pd.BM_PDO_Sink_2.PDO1ma = classEE2233h.BM_PDO_Sink_2.PDO1ma;
					stEEPROM2233.pd.BM_PDO_Sink_2.PDO1mv = classEE2233h.BM_PDO_Sink_2.PDO1mv;
					stEEPROM2233.pd.BM_PDO_Sink_2.PDO2ma = classEE2233h.BM_PDO_Sink_2.PDO2ma;
					stEEPROM2233.pd.BM_PDO_Sink_2.PDO2mv = classEE2233h.BM_PDO_Sink_2.PDO2mv;
					stEEPROM2233.pd.BM_PDO_Sink_2.PDO3ma = classEE2233h.BM_PDO_Sink_2.PDO3ma;
					stEEPROM2233.pd.BM_PDO_Sink_2.PDO3mv = classEE2233h.BM_PDO_Sink_2.PDO3mv;
					stEEPROM2233.pd.BM_PDO_Sink_2.PDO4ma = classEE2233h.BM_PDO_Sink_2.PDO4ma;
					stEEPROM2233.pd.BM_PDO_Sink_2.PDO4mv = classEE2233h.BM_PDO_Sink_2.PDO4mv;
					stEEPROM2233.pd.BM_PDO_Sink_2.PDO5ma = classEE2233h.BM_PDO_Sink_2.PDO5ma;
					stEEPROM2233.pd.BM_PDO_Sink_2.PDO5mv = classEE2233h.BM_PDO_Sink_2.PDO5mv;
					stEEPROM2233.pd.BM_PDO_Sink_2.PDO6ma = classEE2233h.BM_PDO_Sink_2.PDO6ma;
					stEEPROM2233.pd.BM_PDO_Sink_2.PDO6mv = classEE2233h.BM_PDO_Sink_2.PDO6mv;
					stEEPROM2233.pd.BM_PDO_Sink_2.PDO7ma = classEE2233h.BM_PDO_Sink_2.PDO7ma;
					stEEPROM2233.pd.BM_PDO_Sink_2.PDO7mv = classEE2233h.BM_PDO_Sink_2.PDO7mv;

					stEEPROM2233.pd.srt = classEE2233h.srt;          // Sender Response Timer
					stEEPROM2233.pd.hrt = classEE2233h.hrt;          // Hard Reset Timer
					stEEPROM2233.pd.sct = classEE2233h.sct;          // Source Capability Timer
					stEEPROM2233.pd.dit = classEE2233h.dit;          // Discover Identity Timer
					stEEPROM2233.pd.srcrt = classEE2233h.srcrt;       // Source Recover Timer
					stEEPROM2233.pd.trt = classEE2233h.trt;         // Transition Timer
					stEEPROM2233.pd.sofft = classEE2233h.sofft;       // Source off timer
					stEEPROM2233.pd.nrt = classEE2233h.nrt;         // No Response Timer
					stEEPROM2233.pd.swct = classEE2233h.swct;        // Sink Wait Capability Timer
					stEEPROM2233.pd.snkrt = classEE2233h.snkrt;       // Sink Request Timer
					stEEPROM2233.pd.dt = classEE2233h.dt;           // Discharge Timer
					stEEPROM2233.pd.cnst = classEE2233h.cnst;         // Chunk not supported timer
					stEEPROM2233.pd.it = classEE2233h.it;          // Idle Timer

					// PD Control
					stEEPROM2233.pd.i2caddr = classEE2233h.i2caddr;      // I2C Address (hex)
					stEEPROM2233.pd.prou = classEE2233h.prou;          // Power Reserved for OWN use
					stEEPROM2233.pd.trim1 = classEE2233h.trim1;         // TRIM1
					stEEPROM2233.pd.trim2 = classEE2233h.trim2;         // TRIM2
					stEEPROM2233.pd.extdc = classEE2233h.extdc;        // non-zero to enable ETERNAL_DC_POWER

					// Check the size of the structure...
					int size = Marshal.SizeOf(stEEPROM2233);
					// Allocate space for our pointer...
					IntPtr eeDataMarshal = Marshal.AllocHGlobal(size);
					Marshal.StructureToPtr(stEEPROM2233, eeDataMarshal, false);

					ftStatus = FT_EEPROM_Program(ftHandle, eeDataMarshal, (uint)size, manufacturer, manufacturerID, description, serialNumber);

					return ftStatus;
				}
		*/

		public FT_STATUS Write_EEPROM_FT2233HP(CEEPROM_FT2233HP classEE2233HP, ref string strError)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

			// If the DLL hasn't been loaded, just return here
			if (ftHandle == IntPtr.Zero)
			{
				strError = "DLL handle is null - DLL not loaded";
				return FT_STATUS.FT_OTHER_ERROR;
			}

			tFT_EEPROM_Program FT_EEPROM_Program = (tFT_EEPROM_Program)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EEPROM_Program, typeof(tFT_EEPROM_Program));

			FT_DEVICETYPE deviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
			GetDeviceType(ref deviceType);
			if (deviceType != FT_DEVICETYPE.FT_DEVICE_2233H)
			{
				strError = "Error - incorrect device type. Expected Device=FT_DEVICE_2233H(17). Connected device=" + deviceType.ToString();
				return FT_STATUS.FT_OTHER_ERROR;
			}

			if (classEE2233HP.VendorID == 0x0000)
			{
				strError = "Error - VendorID = 0";
				return FT_STATUS.FT_INVALID_PARAMETER;
			}
			if (classEE2233HP.ProductID == 0x0000)
			{
				strError = "Error - ProductID = 0";
				return FT_STATUS.FT_INVALID_PARAMETER;
			}

			STRUCT_EEPROM_FT2233HP stEEPROM2233HP = new STRUCT_EEPROM_FT2233HP();

			byte[] manufacturer = new byte[32];
			byte[] manufacturerID = new byte[16];
			byte[] description = new byte[64];
			byte[] serialNumber = new byte[16];

			if (classEE2233HP.Manufacturer.Length > 32)
				classEE2233HP.Manufacturer = classEE2233HP.Manufacturer.Substring(0, 32);
			if (classEE2233HP.ManufacturerID.Length > 16)
				classEE2233HP.ManufacturerID = classEE2233HP.ManufacturerID.Substring(0, 16);
			if (classEE2233HP.Description.Length > 64)
				classEE2233HP.Description = classEE2233HP.Description.Substring(0, 64);
			if (classEE2233HP.SerialNumber.Length > 16)
				classEE2233HP.SerialNumber = classEE2233HP.SerialNumber.Substring(0, 16);

			// Set string values
			System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
			manufacturer = encoding.GetBytes(classEE2233HP.Manufacturer);
			manufacturerID = encoding.GetBytes(classEE2233HP.ManufacturerID);
			description = encoding.GetBytes(classEE2233HP.Description);
			serialNumber = encoding.GetBytes(classEE2233HP.SerialNumber);

			stEEPROM2233HP.ft2232h.common.deviceType = (uint)FT_DEVICETYPE.FT_DEVICE_2233H;
			stEEPROM2233HP.ft2232h.common.VendorId = classEE2233HP.VendorID;
			stEEPROM2233HP.ft2232h.common.ProductId = classEE2233HP.ProductID;
			stEEPROM2233HP.ft2232h.common.SerNumEnable = Convert.ToByte(classEE2233HP.SerNumEnable);
			stEEPROM2233HP.ft2232h.common.MaxPower = classEE2233HP.MaxPower;
			stEEPROM2233HP.ft2232h.common.SelfPowered = Convert.ToByte(classEE2233HP.SelfPowered);
			stEEPROM2233HP.ft2232h.common.RemoteWakeup = Convert.ToByte(classEE2233HP.RemoteWakeup);
			stEEPROM2233HP.ft2232h.common.PullDownEnable = Convert.ToByte(classEE2233HP.PullDownEnable);

			stEEPROM2233HP.ft2232h.ALSlowSlew = classEE2233HP.ALSlowSlew;
			stEEPROM2233HP.ft2232h.ALSchmittInput = classEE2233HP.ALSchmittInput;
			stEEPROM2233HP.ft2232h.ALDriveCurrent = classEE2233HP.ALDriveCurrent;
			stEEPROM2233HP.ft2232h.AHSlowSlew = classEE2233HP.AHSlowSlew;
			stEEPROM2233HP.ft2232h.AHSchmittInput = classEE2233HP.AHSchmittInput;
			stEEPROM2233HP.ft2232h.AHDriveCurrent = classEE2233HP.AHDriveCurrent;

			stEEPROM2233HP.ft2232h.BLSlowSlew = classEE2233HP.BLSlowSlew;
			stEEPROM2233HP.ft2232h.BLSchmittInput = classEE2233HP.BLSchmittInput;
			stEEPROM2233HP.ft2232h.BLDriveCurrent = classEE2233HP.BLDriveCurrent;
			stEEPROM2233HP.ft2232h.BHSlowSlew = classEE2233HP.BHSlowSlew;
			stEEPROM2233HP.ft2232h.BHSchmittInput = classEE2233HP.BHSchmittInput;
			stEEPROM2233HP.ft2232h.BHDriveCurrent = classEE2233HP.BHDriveCurrent;

			stEEPROM2233HP.ft2232h.AIsFifo = classEE2233HP.AIsFifo;
			stEEPROM2233HP.ft2232h.AIsFifoTar = classEE2233HP.AIsFifoTar;
			stEEPROM2233HP.ft2232h.AIsFastSer = classEE2233HP.AIsFastSer;
			stEEPROM2233HP.ft2232h.BIsFifo = classEE2233HP.BIsFifo;
			stEEPROM2233HP.ft2232h.BIsFifoTar = classEE2233HP.BIsFifoTar;
			stEEPROM2233HP.ft2232h.BIsFastSer = classEE2233HP.BIsFastSer;
			stEEPROM2233HP.ft2232h.PowerSaveEnable = classEE2233HP.PowerSaveEnable;

			stEEPROM2233HP.ft2232h.ADriverType = classEE2233HP.ADriverType;
			stEEPROM2233HP.ft2232h.BDriverType = classEE2233HP.BDriverType;


			//PD
			stEEPROM2233HP.pd.srprs = classEE2233HP.srprs;        // non-zero to enable Sink Request Power Role Swap 
			stEEPROM2233HP.pd.sraprs = classEE2233HP.sraprs;       // non-zero to enable Sink Accept PR Swap
			stEEPROM2233HP.pd.srrprs = classEE2233HP.srrprs;       // non-zero to enable Source Request PR SWAP
			stEEPROM2233HP.pd.saprs = classEE2233HP.saprs;        // non-zero to enable Source Accept PR SWAP
			stEEPROM2233HP.pd.vconns = classEE2233HP.vconns;       // non-zero to enable vConn Swap
			stEEPROM2233HP.pd.passthru = classEE2233HP.passthru;     // non-zero to enable Pass Through
			stEEPROM2233HP.pd.extmcu = classEE2233HP.extmcu;       // non-zero to enable External MCU
			stEEPROM2233HP.pd.pd2en = classEE2233HP.pd2en;        // non-zero to enable PD2
			stEEPROM2233HP.pd.pd1autoclk = classEE2233HP.pd1autoclk;   // non-zero to enable PD1 Auto Clock
			stEEPROM2233HP.pd.pd2autoclk = classEE2233HP.pd2autoclk;   // non-zero to enable PD2 Auto Clock
			stEEPROM2233HP.pd.useefuse = classEE2233HP.useefuse;     // non-zero to Use EFUSE
			stEEPROM2233HP.pd.extvconn = classEE2233HP.extvconn;     // non-zero to enable External vConn
																  //stEEPROM2233HP.pwrmgmt = classEE2233HP.pwrmgmt;      // Power Management setting, supported values are 0 to 4

			// GPIO Configuration
			stEEPROM2233HP.pd.count = classEE2233HP.count;        // GPIO Count, supported values are 0 to 7 
			stEEPROM2233HP.pd.gpio1 = classEE2233HP.gpio1;        // GPIO Number 1, supported values are 0 to 7, 15 for N/A
			stEEPROM2233HP.pd.gpio2 = classEE2233HP.gpio2;        // GPIO Number 2, supported values are 0 to 7, 15 for N/A
			stEEPROM2233HP.pd.gpio3 = classEE2233HP.gpio3;        // GPIO Number 3, supported values are 0 to 7, 15 for N/A
			stEEPROM2233HP.pd.gpio4 = classEE2233HP.gpio4;        // GPIO Number 4, supported values are 0 to 7, 15 for N/A
			stEEPROM2233HP.pd.gpio5 = classEE2233HP.gpio5;        // GPIO Number 5, supported values are 0 to 7, 15 for N/A
			stEEPROM2233HP.pd.gpio6 = classEE2233HP.gpio6;        // GPIO Number 6, supported values are 0 to 7, 15 for N/A
			stEEPROM2233HP.pd.gpio7 = classEE2233HP.gpio7;        // GPIO Number 7, supported values are 0 to 7, 15 for N/A
			stEEPROM2233HP.pd.pd1lden = classEE2233HP.pd1lden;      // PD1 Load Enable, supported values are 0 to 7, 15 for N/A
			stEEPROM2233HP.pd.pd2lden = classEE2233HP.pd2lden;      // PD2 Load Enable, supported values are 0 to 7, 15 for N/A
			stEEPROM2233HP.pd.dispin = classEE2233HP.dispin;       // Discharge Pin, supported values are 0 to 7, 15 for N/A
			stEEPROM2233HP.pd.disenbm = classEE2233HP.disenbm;      // Discharge Enable BM, 0 for "Drive Hi", 1 for "Drive Low", 2 for "Input Mode", 3 for "Don't Care"
			stEEPROM2233HP.pd.disdisbm = classEE2233HP.disdisbm;     // Discharge Disable BM, 0 for "Drive Hi", 1 for "Drive Low", 2 for "Input Mode", 3 for "Don't Care"
			stEEPROM2233HP.pd.ccselect = classEE2233HP.ccselect;

			// ISET Configuration
			stEEPROM2233HP.pd.iset1 = classEE2233HP.iset1;        // ISET1, supported values are 0 to 7, 15 for N/A
			stEEPROM2233HP.pd.iset2 = classEE2233HP.iset2;        // ISET2, supported values are 0 to 7, 15 for N/A
			stEEPROM2233HP.pd.iset3 = classEE2233HP.iset3;        // ISET3, supported values are 0 to 7, 15 for N/A
			stEEPROM2233HP.pd.extiset = classEE2233HP.extiset;      // non-zero to enable EXTEND_ISET
			stEEPROM2233HP.pd.isetpd2 = classEE2233HP.isetpd2;      // non-zero to enable ISET_PD2
			stEEPROM2233HP.pd.iseten = classEE2233HP.iseten;       // non-zero to set ISET_ENABLED

			unsafe
			{
				int i;
				for (i = 0; i < 7; i++)
				{
					stEEPROM2233HP.pd.PDO1_GPIO[i] = classEE2233HP.PDO1_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM2233HP.pd.PDO2_GPIO[i] = classEE2233HP.PDO2_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM2233HP.pd.PDO3_GPIO[i] = classEE2233HP.PDO3_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM2233HP.pd.PDO4_GPIO[i] = classEE2233HP.PDO4_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM2233HP.pd.PDO5_GPIO[i] = classEE2233HP.PDO5_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM2233HP.pd.PDO6_GPIO[i] = classEE2233HP.PDO6_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM2233HP.pd.PDO7_GPIO[i] = classEE2233HP.PDO7_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM2233HP.pd.VSET0V_GPIO[i] = classEE2233HP.VSET0V_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM2233HP.pd.VSAFE5V_GPIO[i] = classEE2233HP.VSAFE5V_GPIO[i];
				}
			}

			stEEPROM2233HP.pd.BM_PDO_Sink.PDO1ma = classEE2233HP.BM_PDO_Sink.PDO1ma;
			stEEPROM2233HP.pd.BM_PDO_Sink.PDO1mv = classEE2233HP.BM_PDO_Sink.PDO1mv;
			stEEPROM2233HP.pd.BM_PDO_Sink.PDO2ma = classEE2233HP.BM_PDO_Sink.PDO2ma;
			stEEPROM2233HP.pd.BM_PDO_Sink.PDO2mv = classEE2233HP.BM_PDO_Sink.PDO2mv;
			stEEPROM2233HP.pd.BM_PDO_Sink.PDO3ma = classEE2233HP.BM_PDO_Sink.PDO3ma;
			stEEPROM2233HP.pd.BM_PDO_Sink.PDO3mv = classEE2233HP.BM_PDO_Sink.PDO3mv;
			stEEPROM2233HP.pd.BM_PDO_Sink.PDO4ma = classEE2233HP.BM_PDO_Sink.PDO4ma;
			stEEPROM2233HP.pd.BM_PDO_Sink.PDO4mv = classEE2233HP.BM_PDO_Sink.PDO4mv;
			stEEPROM2233HP.pd.BM_PDO_Sink.PDO5ma = classEE2233HP.BM_PDO_Sink.PDO5ma;
			stEEPROM2233HP.pd.BM_PDO_Sink.PDO5mv = classEE2233HP.BM_PDO_Sink.PDO5mv;
			stEEPROM2233HP.pd.BM_PDO_Sink.PDO6ma = classEE2233HP.BM_PDO_Sink.PDO6ma;
			stEEPROM2233HP.pd.BM_PDO_Sink.PDO6mv = classEE2233HP.BM_PDO_Sink.PDO6mv;
			stEEPROM2233HP.pd.BM_PDO_Sink.PDO7ma = classEE2233HP.BM_PDO_Sink.PDO7ma;
			stEEPROM2233HP.pd.BM_PDO_Sink.PDO7mv = classEE2233HP.BM_PDO_Sink.PDO7mv;

			stEEPROM2233HP.pd.BM_PDO_Source.PDO1ma = classEE2233HP.BM_PDO_Source.PDO1ma;
			stEEPROM2233HP.pd.BM_PDO_Source.PDO1mv = classEE2233HP.BM_PDO_Source.PDO1mv;
			stEEPROM2233HP.pd.BM_PDO_Source.PDO2ma = classEE2233HP.BM_PDO_Source.PDO2ma;
			stEEPROM2233HP.pd.BM_PDO_Source.PDO2mv = classEE2233HP.BM_PDO_Source.PDO2mv;
			stEEPROM2233HP.pd.BM_PDO_Source.PDO3ma = classEE2233HP.BM_PDO_Source.PDO3ma;
			stEEPROM2233HP.pd.BM_PDO_Source.PDO3mv = classEE2233HP.BM_PDO_Source.PDO3mv;
			stEEPROM2233HP.pd.BM_PDO_Source.PDO4ma = classEE2233HP.BM_PDO_Source.PDO4ma;
			stEEPROM2233HP.pd.BM_PDO_Source.PDO4mv = classEE2233HP.BM_PDO_Source.PDO4mv;
			stEEPROM2233HP.pd.BM_PDO_Source.PDO5ma = classEE2233HP.BM_PDO_Source.PDO5ma;
			stEEPROM2233HP.pd.BM_PDO_Source.PDO5mv = classEE2233HP.BM_PDO_Source.PDO5mv;
			stEEPROM2233HP.pd.BM_PDO_Source.PDO6ma = classEE2233HP.BM_PDO_Source.PDO6ma;
			stEEPROM2233HP.pd.BM_PDO_Source.PDO6mv = classEE2233HP.BM_PDO_Source.PDO6mv;
			stEEPROM2233HP.pd.BM_PDO_Source.PDO7ma = classEE2233HP.BM_PDO_Source.PDO7ma;
			stEEPROM2233HP.pd.BM_PDO_Source.PDO7mv = classEE2233HP.BM_PDO_Source.PDO7mv;

			stEEPROM2233HP.pd.BM_PDO_Sink_2.PDO1ma = classEE2233HP.BM_PDO_Sink_2.PDO1ma;
			stEEPROM2233HP.pd.BM_PDO_Sink_2.PDO1mv = classEE2233HP.BM_PDO_Sink_2.PDO1mv;
			stEEPROM2233HP.pd.BM_PDO_Sink_2.PDO2ma = classEE2233HP.BM_PDO_Sink_2.PDO2ma;
			stEEPROM2233HP.pd.BM_PDO_Sink_2.PDO2mv = classEE2233HP.BM_PDO_Sink_2.PDO2mv;
			stEEPROM2233HP.pd.BM_PDO_Sink_2.PDO3ma = classEE2233HP.BM_PDO_Sink_2.PDO3ma;
			stEEPROM2233HP.pd.BM_PDO_Sink_2.PDO3mv = classEE2233HP.BM_PDO_Sink_2.PDO3mv;
			stEEPROM2233HP.pd.BM_PDO_Sink_2.PDO4ma = classEE2233HP.BM_PDO_Sink_2.PDO4ma;
			stEEPROM2233HP.pd.BM_PDO_Sink_2.PDO4mv = classEE2233HP.BM_PDO_Sink_2.PDO4mv;
			stEEPROM2233HP.pd.BM_PDO_Sink_2.PDO5ma = classEE2233HP.BM_PDO_Sink_2.PDO5ma;
			stEEPROM2233HP.pd.BM_PDO_Sink_2.PDO5mv = classEE2233HP.BM_PDO_Sink_2.PDO5mv;
			stEEPROM2233HP.pd.BM_PDO_Sink_2.PDO6ma = classEE2233HP.BM_PDO_Sink_2.PDO6ma;
			stEEPROM2233HP.pd.BM_PDO_Sink_2.PDO6mv = classEE2233HP.BM_PDO_Sink_2.PDO6mv;
			stEEPROM2233HP.pd.BM_PDO_Sink_2.PDO7ma = classEE2233HP.BM_PDO_Sink_2.PDO7ma;
			stEEPROM2233HP.pd.BM_PDO_Sink_2.PDO7mv = classEE2233HP.BM_PDO_Sink_2.PDO7mv;

			stEEPROM2233HP.pd.srt = classEE2233HP.srt;          // Sender Response Timer
			stEEPROM2233HP.pd.hrt = classEE2233HP.hrt;          // Hard Reset Timer
			stEEPROM2233HP.pd.sct = classEE2233HP.sct;          // Source Capability Timer
			stEEPROM2233HP.pd.dit = classEE2233HP.dit;          // Discover Identity Timer
			stEEPROM2233HP.pd.srcrt = classEE2233HP.srcrt;       // Source Recover Timer
			stEEPROM2233HP.pd.trt = classEE2233HP.trt;         // Transition Timer
			stEEPROM2233HP.pd.sofft = classEE2233HP.sofft;       // Source off timer
			stEEPROM2233HP.pd.nrt = classEE2233HP.nrt;         // No Response Timer
			stEEPROM2233HP.pd.swct = classEE2233HP.swct;        // Sink Wait Capability Timer
			stEEPROM2233HP.pd.snkrt = classEE2233HP.snkrt;       // Sink Request Timer
			stEEPROM2233HP.pd.dt = classEE2233HP.dt;           // Discharge Timer
			stEEPROM2233HP.pd.cnst = classEE2233HP.cnst;         // Chunk not supported timer
			stEEPROM2233HP.pd.it = classEE2233HP.it;          // Idle Timer

			// PD Control
			stEEPROM2233HP.pd.i2caddr = classEE2233HP.i2caddr;      // I2C Address (hex)
			stEEPROM2233HP.pd.prou = classEE2233HP.prou;          // Power Reserved for OWN use
			stEEPROM2233HP.pd.trim1 = classEE2233HP.trim1;         // TRIM1
			stEEPROM2233HP.pd.trim2 = classEE2233HP.trim2;         // TRIM2
			stEEPROM2233HP.pd.extdc = classEE2233HP.extdc;        // non-zero to enable ETERNAL_DC_POWER

			// Check the size of the structure...
			int size = Marshal.SizeOf(stEEPROM2233HP);
			// Allocate space for our pointer...
			IntPtr eeDataMarshal = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(stEEPROM2233HP, eeDataMarshal, false);

			ftStatus = FT_EEPROM_Program(ftHandle, eeDataMarshal, (uint)size, manufacturer, manufacturerID, description, serialNumber);
			strError = ftStatus.ToString();

			return ftStatus;
		}


		public FT_STATUS Write_EEPROM_FT4233H(CEEPROM_FT4233H classEE4233h, ref string strError)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

			// If the DLL hasn't been loaded, just return here
			if (ftHandle == IntPtr.Zero)
			{
				strError = "DLL handle is null - DLL not loaded";
				return FT_STATUS.FT_OTHER_ERROR;
			}

			tFT_EEPROM_Program FT_EEPROM_Program = (tFT_EEPROM_Program)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EEPROM_Program, typeof(tFT_EEPROM_Program));

			FT_DEVICETYPE deviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
			GetDeviceType(ref deviceType);
			if (deviceType != FT_DEVICETYPE.FT_DEVICE_4233HP)
			{
				strError = "Error - incorrect device type. Expected Device=FT_DEVICE_4233HP(18). Connected device=" + deviceType.ToString();
				return FT_STATUS.FT_OTHER_ERROR;
			}

			if(classEE4233h.VendorID == 0x0000)
			{
				strError = "Error - VendorID = 0";
				return FT_STATUS.FT_INVALID_PARAMETER;
			}
			if (classEE4233h.ProductID == 0x0000)
			{
				strError = "Error - ProductID = 0";
				return FT_STATUS.FT_INVALID_PARAMETER;
			}

			STRUCT_EEPROM_FT4233H stEEPROM4233 = new STRUCT_EEPROM_FT4233H();
			
			byte[] manufacturer = new byte[32];
			byte[] manufacturerID = new byte[16];
			byte[] description = new byte[64];
			byte[] serialNumber = new byte[16];

			if (classEE4233h.Manufacturer.Length > 32)
				classEE4233h.Manufacturer = classEE4233h.Manufacturer.Substring(0, 32);
			if (classEE4233h.ManufacturerID.Length > 16)
				classEE4233h.ManufacturerID = classEE4233h.ManufacturerID.Substring(0, 16);
			if (classEE4233h.Description.Length > 64)
				classEE4233h.Description = classEE4233h.Description.Substring(0, 64);
			if (classEE4233h.SerialNumber.Length > 16)
				classEE4233h.SerialNumber = classEE4233h.SerialNumber.Substring(0, 16);

			// Set string values
			System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
			manufacturer = encoding.GetBytes(classEE4233h.Manufacturer);
			manufacturerID = encoding.GetBytes(classEE4233h.ManufacturerID);
			description = encoding.GetBytes(classEE4233h.Description);
			serialNumber = encoding.GetBytes(classEE4233h.SerialNumber);

			stEEPROM4233.ft4232h.common.deviceType = (uint)FT_DEVICETYPE.FT_DEVICE_4233HP;
			stEEPROM4233.ft4232h.common.VendorId = classEE4233h.VendorID;
			stEEPROM4233.ft4232h.common.ProductId = classEE4233h.ProductID;
			stEEPROM4233.ft4232h.common.SerNumEnable = Convert.ToByte(classEE4233h.SerNumEnable);
			stEEPROM4233.ft4232h.common.MaxPower = classEE4233h.MaxPower;
			stEEPROM4233.ft4232h.common.SelfPowered = Convert.ToByte(classEE4233h.SelfPowered);
			stEEPROM4233.ft4232h.common.RemoteWakeup = Convert.ToByte(classEE4233h.RemoteWakeup);
			stEEPROM4233.ft4232h.common.PullDownEnable = Convert.ToByte(classEE4233h.PullDownEnable);

			stEEPROM4233.ft4232h.ASlowSlew = classEE4233h.ASlowSlew;
			stEEPROM4233.ft4232h.ASchmittInput = classEE4233h.ASchmittInput;
			stEEPROM4233.ft4232h.ADriveCurrent = classEE4233h.ADriveCurrent;
			stEEPROM4233.ft4232h.BSlowSlew = classEE4233h.BSlowSlew;
			stEEPROM4233.ft4232h.BSchmittInput = classEE4233h.BSchmittInput;
			stEEPROM4233.ft4232h.BDriveCurrent = classEE4233h.BDriveCurrent;
			stEEPROM4233.ft4232h.CSlowSlew = classEE4233h.CSlowSlew;
			stEEPROM4233.ft4232h.CSchmittInput = classEE4233h.CSchmittInput;
			stEEPROM4233.ft4232h.CDriveCurrent = classEE4233h.CDriveCurrent;
			stEEPROM4233.ft4232h.DSlowSlew = classEE4233h.DSlowSlew;
			stEEPROM4233.ft4232h.DSchmittInput = classEE4233h.DSchmittInput;
			stEEPROM4233.ft4232h.DDriveCurrent = classEE4233h.DDriveCurrent;
			stEEPROM4233.ft4232h.ARIIsTXDEN = classEE4233h.ARIIsTXDEN;
			stEEPROM4233.ft4232h.BRIIsTXDEN = classEE4233h.BRIIsTXDEN;
			stEEPROM4233.ft4232h.CRIIsTXDEN = classEE4233h.CRIIsTXDEN;
			stEEPROM4233.ft4232h.DRIIsTXDEN = classEE4233h.DRIIsTXDEN;
			stEEPROM4233.ft4232h.ADriverType = classEE4233h.ADriverType;
			stEEPROM4233.ft4232h.BDriverType = classEE4233h.BDriverType;
			stEEPROM4233.ft4232h.CDriverType = classEE4233h.CDriverType;
			stEEPROM4233.ft4232h.DDriverType = classEE4233h.DDriverType;

//PD
			stEEPROM4233.pd.srprs = classEE4233h.srprs;        // non-zero to enable Sink Request Power Role Swap 
			stEEPROM4233.pd.sraprs = classEE4233h.sraprs;       // non-zero to enable Sink Accept PR Swap
			stEEPROM4233.pd.srrprs = classEE4233h.srrprs;       // non-zero to enable Source Request PR SWAP
			stEEPROM4233.pd.saprs = classEE4233h.saprs;        // non-zero to enable Source Accept PR SWAP
			stEEPROM4233.pd.vconns = classEE4233h.vconns;       // non-zero to enable vConn Swap
			stEEPROM4233.pd.passthru = classEE4233h.passthru;     // non-zero to enable Pass Through
			stEEPROM4233.pd.extmcu = classEE4233h.extmcu;       // non-zero to enable External MCU
			stEEPROM4233.pd.pd2en = classEE4233h.pd2en;        // non-zero to enable PD2
			stEEPROM4233.pd.pd1autoclk = classEE4233h.pd1autoclk;   // non-zero to enable PD1 Auto Clock
			stEEPROM4233.pd.pd2autoclk = classEE4233h.pd2autoclk;   // non-zero to enable PD2 Auto Clock
			stEEPROM4233.pd.useefuse = classEE4233h.useefuse;     // non-zero to Use EFUSE
			stEEPROM4233.pd.extvconn = classEE4233h.extvconn;     // non-zero to enable External vConn
			//stEEPROM4233.pwrmgmt = classEE4233h.pwrmgmt;      // Power Management setting, supported values are 0 to 4

			// GPIO Configuration
			stEEPROM4233.pd.count = classEE4233h.count;        // GPIO Count, supported values are 0 to 7 
			stEEPROM4233.pd.gpio1 = classEE4233h.gpio1;        // GPIO Number 1, supported values are 0 to 7, 15 for N/A
			stEEPROM4233.pd.gpio2 = classEE4233h.gpio2;        // GPIO Number 2, supported values are 0 to 7, 15 for N/A
			stEEPROM4233.pd.gpio3 = classEE4233h.gpio3;        // GPIO Number 3, supported values are 0 to 7, 15 for N/A
			stEEPROM4233.pd.gpio4 = classEE4233h.gpio4;        // GPIO Number 4, supported values are 0 to 7, 15 for N/A
			stEEPROM4233.pd.gpio5 = classEE4233h.gpio5;        // GPIO Number 5, supported values are 0 to 7, 15 for N/A
			stEEPROM4233.pd.gpio6 = classEE4233h.gpio6;        // GPIO Number 6, supported values are 0 to 7, 15 for N/A
			stEEPROM4233.pd.gpio7 = classEE4233h.gpio7;        // GPIO Number 7, supported values are 0 to 7, 15 for N/A
			stEEPROM4233.pd.pd1lden = classEE4233h.pd1lden;      // PD1 Load Enable, supported values are 0 to 7, 15 for N/A
			stEEPROM4233.pd.pd2lden = classEE4233h.pd2lden;      // PD2 Load Enable, supported values are 0 to 7, 15 for N/A
			stEEPROM4233.pd.dispin = classEE4233h.dispin;       // Discharge Pin, supported values are 0 to 7, 15 for N/A
			stEEPROM4233.pd.disenbm = classEE4233h.disenbm;      // Discharge Enable BM, 0 for "Drive Hi", 1 for "Drive Low", 2 for "Input Mode", 3 for "Don't Care"
			stEEPROM4233.pd.disdisbm = classEE4233h.disdisbm;     // Discharge Disable BM, 0 for "Drive Hi", 1 for "Drive Low", 2 for "Input Mode", 3 for "Don't Care"
			stEEPROM4233.pd.ccselect = classEE4233h.ccselect;

			// ISET Configuration
			stEEPROM4233.pd.iset1 = classEE4233h.iset1;        // ISET1, supported values are 0 to 7, 15 for N/A
			stEEPROM4233.pd.iset2 = classEE4233h.iset2;        // ISET2, supported values are 0 to 7, 15 for N/A
			stEEPROM4233.pd.iset3 = classEE4233h.iset3;        // ISET3, supported values are 0 to 7, 15 for N/A
			stEEPROM4233.pd.extiset = classEE4233h.extiset;      // non-zero to enable EXTEND_ISET
			stEEPROM4233.pd.isetpd2 = classEE4233h.isetpd2;      // non-zero to enable ISET_PD2
			stEEPROM4233.pd.iseten = classEE4233h.iseten;       // non-zero to set ISET_ENABLED

			unsafe
			{
				int i;
				for (i = 0; i < 7; i++)
				{
					stEEPROM4233.pd.PDO1_GPIO[i] = classEE4233h.PDO1_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM4233.pd.PDO2_GPIO[i] = classEE4233h.PDO2_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM4233.pd.PDO3_GPIO[i] = classEE4233h.PDO3_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM4233.pd.PDO4_GPIO[i] = classEE4233h.PDO4_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM4233.pd.PDO5_GPIO[i] = classEE4233h.PDO5_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM4233.pd.PDO6_GPIO[i] = classEE4233h.PDO6_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM4233.pd.PDO7_GPIO[i] = classEE4233h.PDO7_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM4233.pd.VSET0V_GPIO[i] = classEE4233h.VSET0V_GPIO[i];
				}
				for (i = 0; i < 7; i++)
				{
					stEEPROM4233.pd.VSAFE5V_GPIO[i] = classEE4233h.VSAFE5V_GPIO[i];
				}
			}

			stEEPROM4233.pd.BM_PDO_Sink.PDO1ma = classEE4233h.BM_PDO_Sink.PDO1ma;
			stEEPROM4233.pd.BM_PDO_Sink.PDO1mv = classEE4233h.BM_PDO_Sink.PDO1mv;
			stEEPROM4233.pd.BM_PDO_Sink.PDO2ma = classEE4233h.BM_PDO_Sink.PDO2ma;
			stEEPROM4233.pd.BM_PDO_Sink.PDO2mv = classEE4233h.BM_PDO_Sink.PDO2mv;
			stEEPROM4233.pd.BM_PDO_Sink.PDO3ma = classEE4233h.BM_PDO_Sink.PDO3ma;
			stEEPROM4233.pd.BM_PDO_Sink.PDO3mv = classEE4233h.BM_PDO_Sink.PDO3mv;
			stEEPROM4233.pd.BM_PDO_Sink.PDO4ma = classEE4233h.BM_PDO_Sink.PDO4ma;
			stEEPROM4233.pd.BM_PDO_Sink.PDO4mv = classEE4233h.BM_PDO_Sink.PDO4mv;
			stEEPROM4233.pd.BM_PDO_Sink.PDO5ma = classEE4233h.BM_PDO_Sink.PDO5ma;
			stEEPROM4233.pd.BM_PDO_Sink.PDO5mv = classEE4233h.BM_PDO_Sink.PDO5mv;
			stEEPROM4233.pd.BM_PDO_Sink.PDO6ma = classEE4233h.BM_PDO_Sink.PDO6ma;
			stEEPROM4233.pd.BM_PDO_Sink.PDO6mv = classEE4233h.BM_PDO_Sink.PDO6mv;
			stEEPROM4233.pd.BM_PDO_Sink.PDO7ma = classEE4233h.BM_PDO_Sink.PDO7ma;
			stEEPROM4233.pd.BM_PDO_Sink.PDO7mv = classEE4233h.BM_PDO_Sink.PDO7mv;

			stEEPROM4233.pd.BM_PDO_Source.PDO1ma = classEE4233h.BM_PDO_Source.PDO1ma;
			stEEPROM4233.pd.BM_PDO_Source.PDO1mv = classEE4233h.BM_PDO_Source.PDO1mv;
			stEEPROM4233.pd.BM_PDO_Source.PDO2ma = classEE4233h.BM_PDO_Source.PDO2ma;
			stEEPROM4233.pd.BM_PDO_Source.PDO2mv = classEE4233h.BM_PDO_Source.PDO2mv;
			stEEPROM4233.pd.BM_PDO_Source.PDO3ma = classEE4233h.BM_PDO_Source.PDO3ma;
			stEEPROM4233.pd.BM_PDO_Source.PDO3mv = classEE4233h.BM_PDO_Source.PDO3mv;
			stEEPROM4233.pd.BM_PDO_Source.PDO4ma = classEE4233h.BM_PDO_Source.PDO4ma;
			stEEPROM4233.pd.BM_PDO_Source.PDO4mv = classEE4233h.BM_PDO_Source.PDO4mv;
			stEEPROM4233.pd.BM_PDO_Source.PDO5ma = classEE4233h.BM_PDO_Source.PDO5ma;
			stEEPROM4233.pd.BM_PDO_Source.PDO5mv = classEE4233h.BM_PDO_Source.PDO5mv;
			stEEPROM4233.pd.BM_PDO_Source.PDO6ma = classEE4233h.BM_PDO_Source.PDO6ma;
			stEEPROM4233.pd.BM_PDO_Source.PDO6mv = classEE4233h.BM_PDO_Source.PDO6mv;
			stEEPROM4233.pd.BM_PDO_Source.PDO7ma = classEE4233h.BM_PDO_Source.PDO7ma;
			stEEPROM4233.pd.BM_PDO_Source.PDO7mv = classEE4233h.BM_PDO_Source.PDO7mv;

			stEEPROM4233.pd.BM_PDO_Sink_2.PDO1ma = classEE4233h.BM_PDO_Sink_2.PDO1ma;
			stEEPROM4233.pd.BM_PDO_Sink_2.PDO1mv = classEE4233h.BM_PDO_Sink_2.PDO1mv;
			stEEPROM4233.pd.BM_PDO_Sink_2.PDO2ma = classEE4233h.BM_PDO_Sink_2.PDO2ma;
			stEEPROM4233.pd.BM_PDO_Sink_2.PDO2mv = classEE4233h.BM_PDO_Sink_2.PDO2mv;
			stEEPROM4233.pd.BM_PDO_Sink_2.PDO3ma = classEE4233h.BM_PDO_Sink_2.PDO3ma;
			stEEPROM4233.pd.BM_PDO_Sink_2.PDO3mv = classEE4233h.BM_PDO_Sink_2.PDO3mv;
			stEEPROM4233.pd.BM_PDO_Sink_2.PDO4ma = classEE4233h.BM_PDO_Sink_2.PDO4ma;
			stEEPROM4233.pd.BM_PDO_Sink_2.PDO4mv = classEE4233h.BM_PDO_Sink_2.PDO4mv;
			stEEPROM4233.pd.BM_PDO_Sink_2.PDO5ma = classEE4233h.BM_PDO_Sink_2.PDO5ma;
			stEEPROM4233.pd.BM_PDO_Sink_2.PDO5mv = classEE4233h.BM_PDO_Sink_2.PDO5mv;
			stEEPROM4233.pd.BM_PDO_Sink_2.PDO6ma = classEE4233h.BM_PDO_Sink_2.PDO6ma;
			stEEPROM4233.pd.BM_PDO_Sink_2.PDO6mv = classEE4233h.BM_PDO_Sink_2.PDO6mv;
			stEEPROM4233.pd.BM_PDO_Sink_2.PDO7ma = classEE4233h.BM_PDO_Sink_2.PDO7ma;
			stEEPROM4233.pd.BM_PDO_Sink_2.PDO7mv = classEE4233h.BM_PDO_Sink_2.PDO7mv;

			stEEPROM4233.pd.srt = classEE4233h.srt;          // Sender Response Timer
			stEEPROM4233.pd.hrt = classEE4233h.hrt;          // Hard Reset Timer
			stEEPROM4233.pd.sct = classEE4233h.sct;          // Source Capability Timer
			stEEPROM4233.pd.dit = classEE4233h.dit;          // Discover Identity Timer
			stEEPROM4233.pd.srcrt = classEE4233h.srcrt;       // Source Recover Timer
			stEEPROM4233.pd.trt = classEE4233h.trt;         // Transition Timer
			stEEPROM4233.pd.sofft = classEE4233h.sofft;       // Source off timer
			stEEPROM4233.pd.nrt = classEE4233h.nrt;         // No Response Timer
			stEEPROM4233.pd.swct = classEE4233h.swct;        // Sink Wait Capability Timer
			stEEPROM4233.pd.snkrt = classEE4233h.snkrt;       // Sink Request Timer
			stEEPROM4233.pd.dt = classEE4233h.dt;           // Discharge Timer
			stEEPROM4233.pd.cnst = classEE4233h.cnst;         // Chunk not supported timer
			stEEPROM4233.pd.it = classEE4233h.it;          // Idle Timer

			// PD Control
			stEEPROM4233.pd.i2caddr = classEE4233h.i2caddr;      // I2C Address (hex)
			stEEPROM4233.pd.prou = classEE4233h.prou;          // Power Reserved for OWN use
			stEEPROM4233.pd.trim1 = classEE4233h.trim1;         // TRIM1
			stEEPROM4233.pd.trim2 = classEE4233h.trim2;         // TRIM2
			stEEPROM4233.pd.extdc = classEE4233h.extdc;        // non-zero to enable ETERNAL_DC_POWER

			// Check the size of the structure...
			int size = Marshal.SizeOf(stEEPROM4233);
			// Allocate space for our pointer...
			IntPtr eeDataMarshal = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(stEEPROM4233, eeDataMarshal, false);

			ftStatus = FT_EEPROM_Program(ftHandle, eeDataMarshal, (uint)size, manufacturer, manufacturerID, description, serialNumber);
			strError = ftStatus.ToString();

			return ftStatus;
		}


		//**************************************************************************
		// EEReadUserArea
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Reads data from the user area of the device EEPROM.
		/// </summary>
		/// <returns>FT_STATUS from FT_UARead in FTD2XX.DLL</returns>
		/// <param name="UserAreaDataBuffer">An array of bytes which will be populated with the data read from the device EEPROM user area.</param>
		/// <param name="numBytesRead">The number of bytes actually read from the EEPROM user area.</param>
		public FT_STATUS EEReadUserArea(byte[] UserAreaDataBuffer, ref UInt32 numBytesRead)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_EE_UASize FT_EE_UASize = (tFT_EE_UASize)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_UASize, typeof(tFT_EE_UASize));
            tFT_EE_UARead FT_EE_UARead = (tFT_EE_UARead)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_UARead, typeof(tFT_EE_UARead));

            UInt32 UASize = 0;
            // Get size of user area to allocate an array of the correct size.
            // The application must also get the UA size for its copy
            ftStatus = FT_EE_UASize(ftHandle, ref UASize);

            // Make sure we have enough storage for the whole user area
            if (UserAreaDataBuffer.Length >= UASize)
            {
                // Call FT_EE_UARead
                ftStatus = FT_EE_UARead(ftHandle, UserAreaDataBuffer, UserAreaDataBuffer.Length, ref numBytesRead);
            }
            return ftStatus;
        }

		//**************************************************************************
		// EEWriteUserArea
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Writes data to the user area of the device EEPROM.
		/// </summary>
		/// <returns>FT_STATUS value from FT_UAWrite in FTD2XX.DLL</returns>
		/// <param name="UserAreaDataBuffer">An array of bytes which will be written to the device EEPROM user area.</param>
        public FT_STATUS EEWriteUserArea(byte[] UserAreaDataBuffer)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_EE_UASize FT_EE_UASize = (tFT_EE_UASize)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_UASize, typeof(tFT_EE_UASize));
            tFT_EE_UAWrite FT_EE_UAWrite = (tFT_EE_UAWrite)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_UAWrite, typeof(tFT_EE_UAWrite));

            UInt32 UASize = 0;
            // Get size of user area to allocate an array of the correct size.
            // The application must also get the UA size for its copy
            ftStatus = FT_EE_UASize(ftHandle, ref UASize);

            // Make sure we have enough storage for all the data in the EEPROM
            if (UserAreaDataBuffer.Length <= UASize)
            {
                // Call FT_EE_UAWrite
                ftStatus = FT_EE_UAWrite(ftHandle, UserAreaDataBuffer, UserAreaDataBuffer.Length);
            }
            return ftStatus;
        }

		//**************************************************************************
		// GetDeviceType
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the chip type of the current device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetDeviceInfo in FTD2XX.DLL</returns>
		/// <param name="DeviceType">The FTDI chip type of the current device.</param>
        public FT_STATUS GetDeviceType(ref FT_DEVICETYPE DeviceType)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_GetDeviceInfo FT_GetDeviceInfo = (tFT_GetDeviceInfo)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_GetDeviceInfo, typeof(tFT_GetDeviceInfo));

            UInt32 DeviceID = 0;
            byte[] sernum = new byte[16];
            byte[] desc = new byte[64];

            DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;

            // Call FT_GetDeviceInfo
            ftStatus = FT_GetDeviceInfo(ftHandle, ref DeviceType, ref DeviceID, sernum, desc, IntPtr.Zero);
            return ftStatus;
        }

		//**************************************************************************
		// GetDeviceID
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the Vendor ID and Product ID of the current device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetDeviceInfo in FTD2XX.DLL</returns>
		/// <param name="DeviceID">The device ID (Vendor ID and Product ID) of the current device.</param>
        public FT_STATUS GetDeviceID(ref UInt32 DeviceID)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_GetDeviceInfo FT_GetDeviceInfo = (tFT_GetDeviceInfo)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_GetDeviceInfo, typeof(tFT_GetDeviceInfo));

            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            byte[] sernum = new byte[16];
            byte[] desc = new byte[64];

            // Call FT_GetDeviceInfo
            ftStatus = FT_GetDeviceInfo(ftHandle, ref DeviceType, ref DeviceID, sernum, desc, IntPtr.Zero);
            return ftStatus;
        }

		//**************************************************************************
		// GetDescription
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the description of the current device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetDeviceInfo in FTD2XX.DLL</returns>
		/// <param name="Description">The description of the current device.</param>
        public FT_STATUS GetDescription(out string Description)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            Description = String.Empty;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;


            tFT_GetDeviceInfo FT_GetDeviceInfo = (tFT_GetDeviceInfo)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_GetDeviceInfo, typeof(tFT_GetDeviceInfo));

            UInt32 DeviceID = 0;
            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            byte[] sernum = new byte[16];
            byte[] desc = new byte[64];

            // Call FT_GetDeviceInfo
            ftStatus = FT_GetDeviceInfo(ftHandle, ref DeviceType, ref DeviceID, sernum, desc, IntPtr.Zero);
            Description = Encoding.ASCII.GetString(desc);
            Description = Description.Substring(0, Description.IndexOf("\0"));
            return ftStatus;
        }

		//**************************************************************************
		// GetSerialNumber
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the serial number of the current device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetDeviceInfo in FTD2XX.DLL</returns>
		/// <param name="SerialNumber">The serial number of the current device.</param>
        public FT_STATUS GetSerialNumber(out string SerialNumber)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            SerialNumber = String.Empty;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_GetDeviceInfo FT_GetDeviceInfo = (tFT_GetDeviceInfo)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_GetDeviceInfo, typeof(tFT_GetDeviceInfo));

            UInt32 DeviceID = 0;
            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            byte[] sernum = new byte[16];
            byte[] desc = new byte[64];

            // Call FT_GetDeviceInfo
            ftStatus = FT_GetDeviceInfo(ftHandle, ref DeviceType, ref DeviceID, sernum, desc, IntPtr.Zero);
            SerialNumber = Encoding.ASCII.GetString(sernum);
            SerialNumber = SerialNumber.Substring(0, SerialNumber.IndexOf("\0"));
            return ftStatus;
        }

		//**************************************************************************
		// GetRxBytesAvailable
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the number of bytes available in the receive buffer.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetQueueStatus in FTD2XX.DLL</returns>
		/// <param name="RxQueue">The number of bytes available to be read.</param>
        public FT_STATUS GetRxBytesAvailable(ref UInt32 RxQueue)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_GetQueueStatus FT_GetQueueStatus = (tFT_GetQueueStatus)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_GetQueueStatus, typeof(tFT_GetQueueStatus));

            // Call FT_GetQueueStatus
            ftStatus = FT_GetQueueStatus(ftHandle, ref RxQueue);
            return ftStatus;
        }

		//**************************************************************************
		// GetTxBytesWaiting
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the number of bytes waiting in the transmit buffer.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetStatus in FTD2XX.DLL</returns>
		/// <param name="TxQueue">The number of bytes waiting to be sent.</param>
        public FT_STATUS GetTxBytesWaiting(ref UInt32 TxQueue)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_GetStatus FT_GetStatus = (tFT_GetStatus)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_GetStatus, typeof(tFT_GetStatus));

            UInt32 RxQueue = 0;
            UInt32 EventStatus = 0;

            // Call FT_GetStatus
            ftStatus = FT_GetStatus(ftHandle, ref RxQueue, ref TxQueue, ref EventStatus);
            return ftStatus;
        }

		//ywk add
		public FT_STATUS GetStatus(ref UInt32 RxQueue, ref UInt32 TxQueue, ref UInt32 EventStatus)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

			// If the DLL hasn't been loaded, just return here
			if (ftHandle == IntPtr.Zero)
				return ftStatus;

			tFT_GetStatus FT_GetStatus = (tFT_GetStatus)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_GetStatus, typeof(tFT_GetStatus));

			// Call FT_GetStatus
			ftStatus = FT_GetStatus(ftHandle, ref RxQueue, ref TxQueue, ref EventStatus);
			return ftStatus;
		}
		//ywk end

		//**************************************************************************
		// GetEventType
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the event type after an event has fired.  Can be used to distinguish which event has been triggered when waiting on multiple event types.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetStatus in FTD2XX.DLL</returns>
		/// <param name="EventType">The type of event that has occurred.</param>
		public FT_STATUS GetEventType(ref UInt32 EventType)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_GetStatus FT_GetStatus = (tFT_GetStatus)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_GetStatus, typeof(tFT_GetStatus));

            UInt32 RxQueue = 0;
            UInt32 TxQueue = 0;

            // Call FT_GetStatus
            ftStatus = FT_GetStatus(ftHandle, ref RxQueue, ref TxQueue, ref EventType);
            return ftStatus;
        }

		//**************************************************************************
		// GetModemStatus
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the current modem status.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetModemStatus in FTD2XX.DLL</returns>
		/// <param name="ModemStatus">A bit map representaion of the current modem status.</param>
        public FT_STATUS GetModemStatus(ref byte ModemStatus)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_GetModemStatus FT_GetModemStatus = (tFT_GetModemStatus)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_GetModemStatus, typeof(tFT_GetModemStatus));

            UInt32 ModemLineStatus = 0;

            // Call FT_GetModemStatus
            ftStatus = FT_GetModemStatus(ftHandle, ref ModemLineStatus);

            ModemStatus = Convert.ToByte(ModemLineStatus & 0x000000FF);
            return ftStatus;
        }

		//**************************************************************************
		// GetLineStatus
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the current line status.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetModemStatus in FTD2XX.DLL</returns>
		/// <param name="LineStatus">A bit map representaion of the current line status.</param>
        public FT_STATUS GetLineStatus(ref byte LineStatus)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_GetModemStatus FT_GetModemStatus = (tFT_GetModemStatus)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_GetModemStatus, typeof(tFT_GetModemStatus));

            UInt32 ModemLineStatus = 0;

            // Call FT_GetModemStatus
            ftStatus = FT_GetModemStatus(ftHandle, ref ModemLineStatus);
            LineStatus = Convert.ToByte((ModemLineStatus >> 8) & 0x000000FF);
            return ftStatus;
        }

		//**************************************************************************
		// SetBaudRate
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Sets the current Baud rate.
		/// </summary>
		/// <returns>FT_STATUS value from FT_SetBaudRate in FTD2XX.DLL</returns>
		/// <param name="BaudRate">The desired Baud rate for the device.</param>
        public FT_STATUS SetBaudRate(UInt32 BaudRate)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_SetBaudRate FT_SetBaudRate = (tFT_SetBaudRate)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetBaudRate, typeof(tFT_SetBaudRate));

            // Call FT_SetBaudRate
            ftStatus = FT_SetBaudRate(ftHandle, BaudRate);
            return ftStatus;
        }

		//**************************************************************************
		// SetDataCharacteristics
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Sets the data bits, stop bits and parity for the device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_SetDataCharacteristics in FTD2XX.DLL</returns>
		/// <param name="DataBits">The number of data bits for UART data.  Valid values are FT_DATA_BITS.FT_DATA_7 or FT_DATA_BITS.FT_BITS_8</param>
		/// <param name="StopBits">The number of stop bits for UART data.  Valid values are FT_STOP_BITS.FT_STOP_BITS_1 or FT_STOP_BITS.FT_STOP_BITS_2</param>
		/// <param name="Parity">The parity of the UART data.  Valid values are FT_PARITY.FT_PARITY_NONE, FT_PARITY.FT_PARITY_ODD, FT_PARITY.FT_PARITY_EVEN, FT_PARITY.FT_PARITY_MARK or FT_PARITY.FT_PARITY_SPACE</param>
        public FT_STATUS SetDataCharacteristics(byte DataBits, byte StopBits, byte Parity)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_SetDataCharacteristics FT_SetDataCharacteristics = (tFT_SetDataCharacteristics)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetDataCharacteristics, typeof(tFT_SetDataCharacteristics));

            // Call FT_SetDataCharacteristics
            ftStatus = FT_SetDataCharacteristics(ftHandle, DataBits, StopBits, Parity);
            return ftStatus;
        }

		//**************************************************************************
		// SetFlowControl
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Sets the flow control type.
		/// </summary>
		/// <returns>FT_STATUS value from FT_SetFlowControl in FTD2XX.DLL</returns>
		/// <param name="FlowControl">The type of flow control for the UART.  Valid values are FT_FLOW_CONTROL.FT_FLOW_NONE, FT_FLOW_CONTROL.FT_FLOW_RTS_CTS, FT_FLOW_CONTROL.FT_FLOW_DTR_DSR or FT_FLOW_CONTROL.FT_FLOW_XON_XOFF</param>
		/// <param name="Xon">The Xon character for Xon/Xoff flow control.  Ignored if not using Xon/XOff flow control.</param>
		/// <param name="Xoff">The Xoff character for Xon/Xoff flow control.  Ignored if not using Xon/XOff flow control.</param>
        public FT_STATUS SetFlowControl(UInt16 FlowControl, byte Xon, byte Xoff)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_SetFlowControl FT_SetFlowControl = (tFT_SetFlowControl)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetFlowControl, typeof(tFT_SetFlowControl));

            // Call FT_SetFlowControl
            ftStatus = FT_SetFlowControl(ftHandle, FlowControl, Xon, Xoff);
            return ftStatus;
        }

		//**************************************************************************
		// SetRTS
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Asserts or de-asserts the Request To Send (RTS) line.
		/// </summary>
		/// <returns>FT_STATUS value from FT_SetRts or FT_ClrRts in FTD2XX.DLL</returns>
		/// <param name="Enable">If true, asserts RTS.  If false, de-asserts RTS</param>
        public FT_STATUS SetRTS(bool Enable)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_SetRts FT_SetRts = (tFT_SetRts)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetRts, typeof(tFT_SetRts));
            tFT_ClrRts FT_ClrRts = (tFT_ClrRts)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_ClrRts, typeof(tFT_ClrRts));

            if (Enable)
            {
                // Call FT_SetRts
                ftStatus = FT_SetRts(ftHandle);
            }
            else
            {
                // Call FT_ClrRts
                ftStatus = FT_ClrRts(ftHandle);
            }
            return ftStatus;
        }

		//**************************************************************************
		// SetDTR
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Asserts or de-asserts the Data Terminal Ready (DTR) line.
		/// </summary>
		/// <returns>FT_STATUS value from FT_SetDtr or FT_ClrDtr in FTD2XX.DLL</returns>
		/// <param name="Enable">If true, asserts DTR.  If false, de-asserts DTR.</param>
        public FT_STATUS SetDTR(bool Enable)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_SetDtr FT_SetDtr = (tFT_SetDtr)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetDtr, typeof(tFT_SetDtr));
            tFT_ClrDtr FT_ClrDtr = (tFT_ClrDtr)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_ClrDtr, typeof(tFT_ClrDtr));

            if (Enable)
            {
                // Call FT_SetDtr
                ftStatus = FT_SetDtr(ftHandle);
            }
            else
            {
                // Call FT_ClrDtr
                ftStatus = FT_ClrDtr(ftHandle);
            }
            return ftStatus;
        }

		//**************************************************************************
		// SetTimeouts
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Sets the read and write timeout values.
		/// </summary>
		/// <returns>FT_STATUS value from FT_SetTimeouts in FTD2XX.DLL</returns>
		/// <param name="ReadTimeout">Read timeout value in ms.  A value of 0 indicates an infinite timeout.</param>
		/// <param name="WriteTimeout">Write timeout value in ms.  A value of 0 indicates an infinite timeout.</param>
        public FT_STATUS SetTimeouts(UInt32 ReadTimeout, UInt32 WriteTimeout)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_SetTimeouts FT_SetTimeouts = (tFT_SetTimeouts)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetTimeouts, typeof(tFT_SetTimeouts));

            // Call FT_SetTimeouts
            ftStatus = FT_SetTimeouts(ftHandle, ReadTimeout, WriteTimeout);
            return ftStatus;
        }

		//**************************************************************************
		// SetBreak
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Sets or clears the break state.
		/// </summary>
		/// <returns>FT_STATUS value from FT_SetBreakOn or FT_SetBreakOff in FTD2XX.DLL</returns>
		/// <param name="Enable">If true, sets break on.  If false, sets break off.</param>
        public FT_STATUS SetBreak(bool Enable)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_SetBreakOn FT_SetBreakOn = (tFT_SetBreakOn)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetBreakOn, typeof(tFT_SetBreakOn));
            tFT_SetBreakOff FT_SetBreakOff = (tFT_SetBreakOff)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetBreakOff, typeof(tFT_SetBreakOff));

            if (Enable)
            {
                // Call FT_SetBreakOn
                ftStatus = FT_SetBreakOn(ftHandle);
            }
            else
            {
                // Call FT_SetBreakOff
                ftStatus = FT_SetBreakOff(ftHandle);
            }
            return ftStatus;
        }

		//**************************************************************************
		// SetResetPipeRetryCount
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets or sets the reset pipe retry count.  Default value is 50.
		/// </summary>
		/// <returns>FT_STATUS vlaue from FT_SetResetPipeRetryCount in FTD2XX.DLL</returns>
		/// <param name="ResetPipeRetryCount">The reset pipe retry count.  
		/// Electrically noisy environments may benefit from a larger value.</param>
        public FT_STATUS SetResetPipeRetryCount(UInt32 ResetPipeRetryCount)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_SetResetPipeRetryCount FT_SetResetPipeRetryCount = (tFT_SetResetPipeRetryCount)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetResetPipeRetryCount, typeof(tFT_SetResetPipeRetryCount));

            // Call FT_SetResetPipeRetryCount
            ftStatus = FT_SetResetPipeRetryCount(ftHandle, ResetPipeRetryCount);
            return ftStatus;
        }

		//**************************************************************************
		// GetDriverVersion
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the current FTDIBUS.SYS driver version number.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetDriverVersion in FTD2XX.DLL</returns>
		/// <param name="DriverVersion">The current driver version number.</param>
        public FT_STATUS GetDriverVersion(ref UInt32 DriverVersion)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_GetDriverVersion FT_GetDriverVersion = (tFT_GetDriverVersion)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_GetDriverVersion, typeof(tFT_GetDriverVersion));

            // Call FT_GetDriverVersion
            ftStatus = FT_GetDriverVersion(ftHandle, ref DriverVersion);
            return ftStatus;
        }

		//**************************************************************************
		// GetLibraryVersion
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the current FTD2XX.DLL driver version number.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetLibraryVersion in FTD2XX.DLL</returns>
		/// <param name="LibraryVersion">The current library version.</param>
        public FT_STATUS GetLibraryVersion(ref UInt32 LibraryVersion)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            tFT_GetLibraryVersion FT_GetLibraryVersion = (tFT_GetLibraryVersion)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_GetLibraryVersion, typeof(tFT_GetLibraryVersion));

            // Call FT_GetLibraryVersion
            ftStatus = FT_GetLibraryVersion(ref LibraryVersion);
            return ftStatus;
        }

		//**************************************************************************
		// SetDeadmanTimeout
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Sets the USB deadman timeout value.  Default is 5000ms.
		/// </summary>
		/// <returns>FT_STATUS value from FT_SetDeadmanTimeout in FTD2XX.DLL</returns>
		/// <param name="DeadmanTimeout">The deadman timeout value in ms.  Default is 5000ms.</param>
        public FT_STATUS SetDeadmanTimeout(UInt32 DeadmanTimeout)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_SetDeadmanTimeout FT_SetDeadmanTimeout = (tFT_SetDeadmanTimeout)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetDeadmanTimeout, typeof(tFT_SetDeadmanTimeout));

            // Call FT_SetDeadmanTimeout
            ftStatus = FT_SetDeadmanTimeout(ftHandle, DeadmanTimeout);
            return ftStatus;
        }

		//**************************************************************************
		// SetLatency
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Sets the value of the latency timer.  Default value is 16ms.
		/// </summary>
		/// <returns>FT_STATUS value from FT_SetLatencyTimer in FTD2XX.DLL</returns>
		/// <param name="Latency">The latency timer value in ms.
		/// Valid values are 2ms - 255ms for FT232BM, FT245BM and FT2232 devices.
		/// Valid values are 0ms - 255ms for other devices.</param>
        public FT_STATUS SetLatency(byte Latency)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_SetLatencyTimer FT_SetLatencyTimer = (tFT_SetLatencyTimer)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetLatencyTimer, typeof(tFT_SetLatencyTimer));

            FT_DEVICETYPE DeviceType = FT_DEVICETYPE.FT_DEVICE_UNKNOWN;
            // Set Bit Mode does not apply to FT8U232AM, FT8U245AM or FT8U100AX devices
            GetDeviceType(ref DeviceType);
            if ((DeviceType == FT_DEVICETYPE.FT_DEVICE_BM) || (DeviceType == FT_DEVICETYPE.FT_DEVICE_2232C))
            {
                // Do not allow latency of 1ms or 0ms for older devices
                // since this can cause problems/lock up due to buffering mechanism
                if (Latency < 2)
                    Latency = 2;
            }

            // Call FT_SetLatencyTimer
            ftStatus = FT_SetLatencyTimer(ftHandle, Latency);
            return ftStatus;
        }

		//**************************************************************************
		// GetLatency
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the value of the latency timer.  Default value is 16ms.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetLatencyTimer in FTD2XX.DLL</returns>
		/// <param name="Latency">The latency timer value in ms.</param>
        public FT_STATUS GetLatency(ref byte Latency)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_GetLatencyTimer FT_GetLatencyTimer = (tFT_GetLatencyTimer)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_GetLatencyTimer, typeof(tFT_GetLatencyTimer));

            // Call FT_GetLatencyTimer
            ftStatus = FT_GetLatencyTimer(ftHandle, ref Latency);
            return ftStatus;
        }

		//**************************************************************************
		// SetUSBTransferSizes
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Sets the USB IN and OUT transfer sizes.
		/// </summary>
		/// <returns>FT_STATUS value from FT_SetUSBParameters in FTD2XX.DLL</returns>
		/// <param name="InTransferSize">The USB IN transfer size in bytes.</param>
        public FT_STATUS InTransferSize(UInt32 InTransferSize)
        // Only support IN transfer sizes at the moment
        //public UInt32 InTransferSize(UInt32 InTransferSize, UInt32 OutTransferSize)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_SetUSBParameters FT_SetUSBParameters = (tFT_SetUSBParameters)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetUSBParameters, typeof(tFT_SetUSBParameters));

            UInt32 OutTransferSize = InTransferSize;

            // Call FT_SetUSBParameters
            ftStatus = FT_SetUSBParameters(ftHandle, InTransferSize, OutTransferSize);
            return ftStatus;
        }

		//**************************************************************************
		// SetCharacters
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Sets an event character, an error character and enables or disables them.
		/// </summary>
		/// <returns>FT_STATUS value from FT_SetChars in FTD2XX.DLL</returns>
		/// <param name="EventChar">A character that will be tigger an IN to the host when this character is received.</param>
		/// <param name="EventCharEnable">Determines if the EventChar is enabled or disabled.</param>
		/// <param name="ErrorChar">A character that will be inserted into the data stream to indicate that an error has occurred.</param>
		/// <param name="ErrorCharEnable">Determines if the ErrorChar is enabled or disabled.</param>
        public FT_STATUS SetCharacters(byte EventChar, bool EventCharEnable, byte ErrorChar, bool ErrorCharEnable)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_SetChars FT_SetChars = (tFT_SetChars)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_SetChars, typeof(tFT_SetChars));

            // Call FT_SetChars
            ftStatus = FT_SetChars(ftHandle, EventChar, Convert.ToByte(EventCharEnable), ErrorChar, Convert.ToByte(ErrorCharEnable));
            return ftStatus;
        }

		//**************************************************************************
		// GetEEUserAreaSize
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the size of the EEPROM user area.
		/// </summary>
		/// <returns>FT_STATUS value from FT_EE_UASize in FTD2XX.DLL</returns>
		/// <param name="UASize">The EEPROM user area size in bytes.</param>
        public FT_STATUS EEUserAreaSize(ref UInt32 UASize)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;

            tFT_EE_UASize FT_EE_UASize = (tFT_EE_UASize)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_EE_UASize, typeof(tFT_EE_UASize));

            ftStatus = FT_EE_UASize(ftHandle, ref UASize);
            return ftStatus;
        }

		//**************************************************************************
		// GetCOMPort
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the corresponding COM port number for the current device.  If no COM port is exposed, a negative 1 is returned.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetComPortNumber in FTD2XX.DLL</returns>
		/// <param name="ComPortName">The COM port name corresponding to the current device.  If no COM port is installed, an empty string is passed back.</param>
        public FT_STATUS GetCOMPort(out int PortNum)
        {
            PortNum = -1;

            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_INVALID_HANDLE;

            // If the DLL hasn't been loaded, just return here
            if (ftHandle == IntPtr.Zero)
                return ftStatus;


            tFT_GetComPortNumber FT_GetComPortNumber = (tFT_GetComPortNumber)Marshal.GetDelegateForFunctionPointer(FTDILib.Instance.pFT_GetComPortNumber, typeof(tFT_GetComPortNumber));

            Int32 port = -1;
            ftStatus = FT_GetComPortNumber(ftHandle, ref port);
            if (ftStatus == FT_STATUS.FT_OK)
                PortNum = port;

            return ftStatus;
        }

		//**************************************************************************
		// IsOpen
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the open status of the device.
		/// </summary>
		public bool IsOpen
		{
			get
			{
				if (ftHandle == IntPtr.Zero)
					return false;
				else
					return true;
			}
		}

		//**************************************************************************
		// InterfaceIdentifier
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the interface identifier.
		/// </summary>
		private string InterfaceIdentifier
		{
			get
			{
				string Identifier;
				Identifier = String.Empty;
				if (IsOpen)
				{
					FT_DEVICETYPE deviceType = FT_DEVICETYPE.FT_DEVICE_BM;
					GetDeviceType(ref deviceType);
					if ((deviceType == FT_DEVICETYPE.FT_DEVICE_2232C) | (deviceType == FT_DEVICETYPE.FT_DEVICE_2232H) | (deviceType == FT_DEVICETYPE.FT_DEVICE_4232H))
					{
						string Description;
						GetDescription(out Description);
						Identifier = Description.Substring((Description.Length - 1));
						return Identifier;
					}
				}
				return Identifier;
			}
		}

		//**************************************************************************
		// ErrorHandler
		//**************************************************************************
		/// <summary>
		/// Method to check ftStatus and ftErrorCondition values for error conditions and throw exceptions accordingly.
		/// </summary>
		private void ErrorHandler(FT_STATUS ftStatus, FT_ERROR ftErrorCondition)
		{
			if (ftStatus != FT_STATUS.FT_OK)
			{
				// Check FT_STATUS values returned from FTD2XX DLL calls
				switch (ftStatus)
				{
					case FT_STATUS.FT_DEVICE_NOT_FOUND:
						{
							throw new FT_EXCEPTION("FTDI device not found.");
						}
					case FT_STATUS.FT_DEVICE_NOT_OPENED:
						{
							throw new FT_EXCEPTION("FTDI device not opened.");
						}
					case FT_STATUS.FT_DEVICE_NOT_OPENED_FOR_ERASE:
						{
							throw new FT_EXCEPTION("FTDI device not opened for erase.");
						}
					case FT_STATUS.FT_DEVICE_NOT_OPENED_FOR_WRITE:
						{
							throw new FT_EXCEPTION("FTDI device not opened for write.");
						}
					case FT_STATUS.FT_EEPROM_ERASE_FAILED:
						{
							throw new FT_EXCEPTION("Failed to erase FTDI device EEPROM.");
						}
					case FT_STATUS.FT_EEPROM_NOT_PRESENT:
						{
							throw new FT_EXCEPTION("No EEPROM fitted to FTDI device.");
						}
					case FT_STATUS.FT_EEPROM_NOT_PROGRAMMED:
						{
							throw new FT_EXCEPTION("FTDI device EEPROM not programmed.");
						}
					case FT_STATUS.FT_EEPROM_READ_FAILED:
						{
							throw new FT_EXCEPTION("Failed to read FTDI device EEPROM.");
						}
					case FT_STATUS.FT_EEPROM_WRITE_FAILED:
						{
							throw new FT_EXCEPTION("Failed to write FTDI device EEPROM.");
						}
					case FT_STATUS.FT_FAILED_TO_WRITE_DEVICE:
						{
							throw new FT_EXCEPTION("Failed to write to FTDI device.");
						}
					case FT_STATUS.FT_INSUFFICIENT_RESOURCES:
						{
							throw new FT_EXCEPTION("Insufficient resources.");
						}
					case FT_STATUS.FT_INVALID_ARGS:
						{
							throw new FT_EXCEPTION("Invalid arguments for FTD2XX function call.");
						}
					case FT_STATUS.FT_INVALID_BAUD_RATE:
						{
							throw new FT_EXCEPTION("Invalid Baud rate for FTDI device.");
						}
					case FT_STATUS.FT_INVALID_HANDLE:
						{
							throw new FT_EXCEPTION("Invalid handle for FTDI device.");
						}
					case FT_STATUS.FT_INVALID_PARAMETER:
						{
							throw new FT_EXCEPTION("Invalid parameter for FTD2XX function call.");
						}
					case FT_STATUS.FT_IO_ERROR:
						{
							throw new FT_EXCEPTION("FTDI device IO error.");
						}
					case FT_STATUS.FT_OTHER_ERROR:
						{
							throw new FT_EXCEPTION("An unexpected error has occurred when trying to communicate with the FTDI device.");
						}
					default:
						break;
				}
			}
			if (ftErrorCondition != FT_ERROR.FT_NO_ERROR)
			{
				// Check for other error conditions not handled by FTD2XX DLL
				switch (ftErrorCondition)
				{
					case FT_ERROR.FT_INCORRECT_DEVICE:
						{
							throw new FT_EXCEPTION("The current device type does not match the EEPROM structure.");
						}
					case FT_ERROR.FT_INVALID_BITMODE:
						{
							throw new FT_EXCEPTION("The requested bit mode is not valid for the current device.");
						}
					case FT_ERROR.FT_BUFFER_SIZE:
						{
							throw new FT_EXCEPTION("The supplied buffer is not big enough.");
						}

					default:
						break;
				}

			}

			return;
		}
	}
}

