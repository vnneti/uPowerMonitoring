    using System;
    using uPowerMonitoring.Services;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.ComponentModel;

    namespace uPowerMonitoring.ViewModels;

    public partial class MainViewModel : ViewModelBase
    {
        private readonly IIna228Service _inaService;
        private readonly ISampleService _sampleService;
        private readonly IDataLoggerService _DataLoggerService;

        public DashBoardViewModel DashBoardViewModel { get; }
        public MainViewModel(IIna228Service inaService, ISampleService sampleService, IDataLoggerService dataloggerservice, DashBoardViewModel dashBoardViewModel)
        {
            _inaService = inaService;
            _sampleService = sampleService;
            _DataLoggerService = dataloggerservice;
            DashBoardViewModel = dashBoardViewModel;
            // Starte die Initialisierung im Hintergrund beim Start der App
            _ = InitializeHardwareAsync();
        }

        private async Task InitializeHardwareAsync()
        {
            try
            {
                // Hier übergibst du später deine Werte (Seriennummer, Shunt in Ohm, I2C-Adresse)
                await _inaService.InitializeAsync("UM232H", 0.0005, 0x40);
                await _inaService.SetConfig((UInt16)(Ina228I2cService.INA228_REG_CFG_CONVDLY(1) | Ina228I2cService.INA228_REG_CFG_ADCRANGE(1)));
                await _inaService.SetADCConfig((UInt16)(Ina228I2cService.INA228_REG_ACFG_MODE(Ina228I2cService.INA228_ACFG_MODE_VSHUNT_BUS_TEMP_CON)
                |Ina228I2cService.INA228_REG_ACFG_VBUSCT(5)|Ina228I2cService.INA228_REG_ACFG_VSHCT(5)
                | Ina228I2cService.INA228_REG_ACFG_VTCT(5)
                | Ina228I2cService.INA228_REG_ACFG_AVG(3)));
                _inaService.SetConversionFactor(1);
                // Wenn erfolgreich, direkt den Abtast-Loop starten
            }
            catch (Exception ex)
            {
                // Fehler abfangen (z.B. wenn kein USB-Gerät angeschlossen ist)
                AppLogger.AppLogger.Error($"Initialisierungsfehler: {ex.Message}", "Init");
            }
        }
        [ObservableProperty]
        public partial string Greeting { get; set; } = "Welcome to Avalonia!";
    }
