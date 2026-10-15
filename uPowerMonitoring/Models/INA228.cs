
using System;

namespace uPowerMonitoring.Models;

public record Ina228Measurement
(
    DateTime Timestamp,
    double VoltageV,
    double CurrentA,
    double PowerW,
    double? Temperature
);

public class Ina228Config
{
    // 0 = 85.19 mV Range, 1 = 204.8 mV Range (Beispiel je nach INA228 Spec)
    public int AdcRange { get; set; } = 0; 
    
    // Conversion times (z.B. für VBUS und Shunt)
    public int ConversionTimeVbus { get; set; } = 4; // z.B. 280 µs
    public int ConversionTimeShunt { get; set; } = 4;
    public int ConversionTimeTemperature { get; set; } = 4;
    
    // ADC Sample Averaging Count (z.B. 1, 4, 16, 64, 128...)
    public int AveragingCount { get; set; } = 4; 
}

