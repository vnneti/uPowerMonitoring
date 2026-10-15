using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Defaults;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using uPowerMonitoring.Models;
using uPowerMonitoring.Services;

namespace uPowerMonitoring.ViewModels;
public partial class DashBoardViewModel : ObservableObject
{
    private readonly ISampleService _sampleService;
    private readonly IIna228Service _ina228Service;

    // --- Indikator Properties für Textanzeigen ---
    [ObservableProperty] private string _voltageText = "0.00 V";
    [ObservableProperty] private string _currentText = "0.00 A";
    [ObservableProperty] private string _powerText = "0.00 W";
    [ObservableProperty] private string _temperatureText = "0.0 °C";

    // --- Konfigurations-Properties (Bindings für die UI) ---
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanApplyConfiguration))] private bool _isRunning = false;

    public bool CanApplyConfiguration => !IsRunning;
    [ObservableProperty] private double _shuntResistance = 0.001; // in Ohm
    [ObservableProperty] private int _samplingIntervalMs = 50;     // 20ms bis 1000ms
    
    // INA228 Optionen (Auswahlmöglichkeiten)
    [ObservableProperty] private string _selectedAdcRange = "±163.84 mV";
    [ObservableProperty] private string _selectedConversionTime = "1050 µs";
    [ObservableProperty] private string _selectedAveraging = "16";

    public string[] AdcRangeOptions { get; } = { "±163.84 mV" , "±41.94 mV"};
    public string[] ConversionTimeOptions { get; } = { "50 µs", "84 µs", "150 µs", "280 µs", "540 µs", "1050 µs", "2110 µs", "4120 µs" };
    public string[] AveragingOptions { get; } = { "1", "4", "16", "64", "128", "256", "512", "1024" };

    // --- Chart Serien (Datenreihen) ---
    public ISeries[] VoltageSeries { get; set; }
    public ISeries[] CurrentSeries { get; set; }
    public ISeries[] PowerSeries { get; set; }

    private readonly ObservableCollection<ObservablePoint> _voltageValues = new();
    private readonly ObservableCollection<ObservablePoint> _currentValues = new();
    private readonly ObservableCollection<ObservablePoint> _powerValues = new();

    private const int MaxHistoryCount = 100; // Maximale Anzahl Punkte im Chart

    private int _currentIndex = 0;
    public DashBoardViewModel(ISampleService sampleService, IIna228Service ina228Service)
    {
        _sampleService = sampleService;
        _ina228Service = ina228Service;
        // Charts initialisieren
        VoltageSeries = new ISeries[] { new LineSeries<ObservablePoint> { Values = _voltageValues, Fill = null, LineSmoothness = 0, GeometrySize = 0 } };
        CurrentSeries = new ISeries[] { new LineSeries<ObservablePoint> { Values = _currentValues, Fill = null, LineSmoothness = 0, GeometrySize = 0 } };
        PowerSeries   = new ISeries[] { new LineSeries<ObservablePoint> { Values = _powerValues, Fill = null, LineSmoothness = 0, GeometrySize = 0} };

        // Auf die 50Hz Messwerte vom SampleService lauschen
        _sampleService.OnDataReceived += OnMeasurementReceived;
    }

    [RelayCommand]
    private void StartMeasurement()
    {
        if (IsRunning) return;
        
        _sampleService.StartSampling();
        IsRunning = true;
    }

    [RelayCommand]
    private void StopMeasurement()
    {
        if (!IsRunning) return;
     
        _sampleService.StopSampling(); 
           IsRunning = false;
    }

    [RelayCommand]
    private async Task ApplyConfigurationAsync()
    {
        _sampleService.AbtastTimer = SamplingIntervalMs;
        int ADCRange = Array.IndexOf(AdcRangeOptions, SelectedAdcRange);
        _ina228Service.SetConversionFactor(ADCRange);
        _ina228Service.SetShuntResistor(ShuntResistance);
        int conversionTimeIndex = Array.IndexOf(ConversionTimeOptions, SelectedConversionTime);
        int averaging = Array.IndexOf(AveragingOptions, SelectedAveraging);
        await _ina228Service.SetConfig((UInt16)(Ina228I2cService.INA228_REG_CFG_CONVDLY(1) | (ADCRange << Ina228I2cService.INA228_REG_CFG_ADCRANGE_POS)));
        await _ina228Service.SetADCConfig((UInt16)(Ina228I2cService.INA228_REG_ACFG_MODE(Ina228I2cService.INA228_ACFG_MODE_VSHUNT_BUS_TEMP_CON)
            |Ina228I2cService.INA228_REG_ACFG_VBUSCT((byte)conversionTimeIndex)|Ina228I2cService.INA228_REG_ACFG_VSHCT((byte)conversionTimeIndex)
            | Ina228I2cService.INA228_REG_ACFG_VTCT((byte)conversionTimeIndex)
            | Ina228I2cService.INA228_REG_ACFG_AVG((byte)averaging)));
        // Hier wird die INA228 Konfiguration an die Hardware übergeben
        // Z.B. _inaService.Configure(ShuntResistance, SelectedAdcRange, SelectedConversionTime, SelectedAveraging, SamplingIntervalMs);
    }

    private void OnMeasurementReceived(Ina228Measurement m)
    {
        // 2. Werte für die Live-Charts hinzufügen (Performance-schonend im UI-Thread)
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            // 1. Textwerte für die Indikatoren aktualisieren
            VoltageText = $"{m.VoltageV:F3} V";
            CurrentText = $"{m.CurrentA:F6} A";
            PowerText = $"{m.PowerW:F2} W";
            
            if (m.Temperature.HasValue)
            {
                TemperatureText = $"{m.Temperature.Value:F1} °C";
            }

            // 2. Werte für die Live-Charts hinzufügen
            _currentIndex += 1;
            _voltageValues.Add(new ObservablePoint(_currentIndex, m.VoltageV));
            _currentValues.Add(new ObservablePoint(_currentIndex, m.CurrentA));
            _powerValues.Add(new ObservablePoint(_currentIndex, m.PowerW));

            // Begrenzen, damit der Speicher nicht vollläuft
            if (_voltageValues.Count > MaxHistoryCount) _voltageValues.RemoveAt(0);
            if (_currentValues.Count > MaxHistoryCount) _currentValues.RemoveAt(0);
            if (_powerValues.Count > MaxHistoryCount) _powerValues.RemoveAt(0);
        });
    }
}