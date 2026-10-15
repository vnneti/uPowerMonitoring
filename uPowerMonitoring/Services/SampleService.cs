using System;
using System.Threading;
using System.Threading.Tasks;
using uPowerMonitoring.Models;

namespace uPowerMonitoring.Services;


public interface ISampleService
{
    int AbtastTimer {get; set;}
    bool IsRunning { get; }
    void StartSampling();
     void StopSampling();
    event Action<Ina228Measurement> OnDataReceived;    
}
public class SampleService: ISampleService
{
    private IIna228Service _INA228Service;
    private CancellationTokenSource? _cts;
    private bool _IsRunning = false;

    private double _CachedTemperature = 0;
    public bool IsRunning => _IsRunning;


    public int AbtastTimer {get; set;} = 20;
    public event Action<Ina228Measurement>? OnDataReceived;
    
    public SampleService(IIna228Service inaService)
    {
        _INA228Service = inaService;
    }

    public void StartSampling()
    {
        if(_IsRunning)
            return;
        _cts = new CancellationTokenSource();
        _IsRunning = true;
        _= Task.Run(() => RunSamplingLoopAsync(_cts.Token));
    }

    public void StopSampling()
    {
        if (!_IsRunning) return;
        _cts?.Cancel();
        _cts.Dispose();
    }

    private async Task RunSamplingLoopAsync(CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(AbtastTimer));
        int tickCounter = 0;
        try
        {
            while (await timer.WaitForNextTickAsync(token))
            {
                // 1. Immer alle 20ms: Spannung, Strom, Leistung auslesen
                var measurement = await _INA228Service.ReadMeasuredDataAsync();

                // 2. Nur alle 50 Ticks (50 * 20ms = 1 Sekunde) die Temperatur abfragen
                tickCounter++;
                if (tickCounter >= 250)
                {
                    tickCounter = 0;
                    try
                    {
                        _CachedTemperature = await _INA228Service.ReadTemperature();
                    }
                    catch
                    {
                        // Stiller Fallback, falls die Temperaturabfrage einen Schluckauf hat
                    }
                }

                // 3. Neues Record mit den aktuellen Werten (inkl. der gemachten oder gecachten Temperatur) erstellen
                var fullMeasurement = new Ina228Measurement(
                    Timestamp: measurement.Timestamp,
                    VoltageV: measurement.VoltageV,
                    CurrentA: measurement.CurrentA,
                    PowerW: measurement.PowerW,
                    Temperature: _CachedTemperature
                );

                // 4. Event an die UI / das ViewModel feuern
                OnDataReceived?.Invoke(fullMeasurement);
            }
        }
        catch (OperationCanceledException)
        {
            // Gewolltes Beenden
        }
        catch (Exception ex)
        {
            AppLogger.AppLogger.Error($"Fehler im Abtast-Loop: {ex.Message}", "Sampling");
        }
        finally
        {
            _IsRunning = false;
            
        }
    }

}