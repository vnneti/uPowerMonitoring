
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using AppLogger;
namespace uPowerMonitoring.Services;
public interface IDataLoggerService : IAsyncDisposable
{
    Task StartAsync();
    Task StopAsync();
    Task LogAsync(string[] values);
    Task LogAsync(string[] values, DateTime t);
    Task LogAsync(string[] values, string time);
    Task LogAsyncIndex(string[] values);
    void SetFileHeader(string h);
    void SetFilePrefix(string filePrefix);
    void SetColumnHeader(string[] ch);
    void SetDirectory(string dir);
    void SetMaxFileSize(int sizeMB);
    bool Enabled { get; }
    string FilePath {  get; }
    event Action<string> NewFileEnrolled;
  
}
public class DataLogger : IDataLoggerService
{
    private Channel<string>? _channel;
    private CancellationTokenSource _cts = new();
    private Task? _backgroundTask;
    private int _iD;
    private StreamWriter? _writer;
    private string _directory;
    private string? _filePath;
    private string _filePrefix;
    private long _maxFileSizeBytes;
    private long _currentFileSize;
    private string[] _headerColumns;
    private string _fileHeader = "";
    private readonly char _separator = ';';
    private int _LogLineIndex = 0;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public event Action<string> NewFileEnrolled;
    public bool Enabled { get; private set; } = false;

    public string FilePath => _filePath;
    public int ID => _iD;
    public uint DiagnosticCode { get; private set; } = 0;
    
    public DataLogger()
    {}

    public void SetDirectory(string dir)
    {
        _directory = dir;
    }

    public void SetMaxFileSize(int sizeMB)
    {
        _maxFileSizeBytes = sizeMB * 1024 * 1024;
    }

    public void SetColumnHeader(string[] ch)
    {
        _headerColumns = ch;
    }

    public void SetFileHeader(string h)
    {
        _fileHeader = h;
    }

    public void SetFilePrefix(string filePrefix)
    {
        _filePrefix = filePrefix;
    }

    public async Task StartAsync()
    {
        if (Enabled) return;
        _channel = Channel.CreateBounded<string>(new BoundedChannelOptions(500)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
        _cts = new CancellationTokenSource();
        await _semaphore.WaitAsync();
        try
        {
            await CreateNewFile();
        }
        finally
        {
            _semaphore.Release();
        }
        
        _backgroundTask =  Task.Run(ProcessChannelAsync);
        Enabled = true;
    }

    public async Task LogAsync(string[] values)
    {
        if (!Enabled || _channel == null)
            return;

        var timeStamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var sb = new StringBuilder();
        sb.Append(timeStamp).Append(_separator);

        foreach (var value in values)
        {
            sb.Append(value).Append(_separator);
        }

        try
        {
            await _channel.Writer.WriteAsync(sb.ToString());
        }
        catch (ChannelClosedException)
        {
            // ignorieren
        }
    }

    public async Task LogAsyncIndex(string[] values)
    {
        if (!Enabled || _channel == null)
            return;

        // Erhöht sicher den Zähler im Speicher und gibt den neuen Wert zurück
        // Wenn _LogLineIndex vorher 0 war, ist nextIndex jetzt 1.
        int nextIndex = System.Threading.Interlocked.Increment(ref _LogLineIndex);
        var sb = new StringBuilder();
        sb.Append(nextIndex.ToString()).Append(_separator);

        foreach (var value in values)
        {
            sb.Append(value).Append(_separator);
        }

        try
        {
            await _channel.Writer.WriteAsync(sb.ToString());
        }
        catch (ChannelClosedException)
        {
            // ignorieren
        }
    }

    public async Task LogAsync(string[] values, DateTime t)
    {
        if (!Enabled || _channel == null)
            return;

        var timeStamp = t.ToString("HH:mm:ss.fff");
        var sb = new StringBuilder();
        sb.Append(timeStamp).Append(_separator);

        foreach (var value in values)
        {
            sb.Append(value).Append(_separator);
        }

        try
        {
            await _channel.Writer.WriteAsync(sb.ToString());
        }
        catch (ChannelClosedException)
        {
            // ignorieren
        }
    }

    public async Task LogAsync(string[] values, string time)
    {
        if (!Enabled || _channel == null)
            return;
        var sb = new StringBuilder();
        sb.Append(time).Append(_separator);

        foreach (var value in values)
        {
            sb.Append(value).Append(_separator);
        }

        try
        {
            await _channel.Writer.WriteAsync(sb.ToString());
        }
        catch (ChannelClosedException)
        {
            // ignorieren
        }
    }

    private async Task ProcessChannelAsync()
    {
        try
        {
            while (await _channel.Reader.WaitToReadAsync(_cts.Token))
            {
                while (_channel.Reader.TryRead(out var line))
                {
                    
                    try
                    {
                        await WriteLineAsync(line);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        AppLogger.AppLogger.Error(ex.ToString(), "DataLogger");
                        await Task.Delay(1000);
                        Enabled = false;
                        return;
                    }
                    catch (IOException ex)
                    {
                     
                        AppLogger.AppLogger.Error($"Schreibfehler (Disk voll/weg?): {ex.Message}", "DataLogger");
                        await Task.Delay(1000);
                        Enabled = false;
                        return;

                    }
                    catch (Exception ex)
                    {
                      
                        AppLogger.AppLogger.Error($"Allgemeiner Fehler beim Schreiben: {ex.Message}", "DataLogger");
                        await Task.Delay(1000);
                        Enabled = false;
                        return;
                    }
                    
                }
            }
        }
        catch (OperationCanceledException ex)
        {
            //ReportDiagnostic(DiagnosticCodeTableT.File_AccessRight, DiagnosticCodeSeverity.Error);
            AppLogger.AppLogger.Info("DataLogger Hintergrund-Task sauber beendet.", "DataLogger");
            Enabled = false;
        }
        catch (Exception ex)
        {
       
            AppLogger.AppLogger.Error(ex.ToString(), "DataLogger");
            await Task.Delay(1000);
            Enabled = false;
            return;
        }
    }

    private async Task WriteLineAsync(string line)
    {
        await _semaphore.WaitAsync();
        try
        {
            await CheckFileRolling();
            await _writer.WriteLineAsync(line);
            _currentFileSize += Encoding.UTF8.GetByteCount(line + Environment.NewLine);
        }
        finally {_semaphore.Release(); }        
    }

    private async Task CheckFileRolling()
    {
        if (_currentFileSize >= _maxFileSizeBytes)
        {
            await CreateNewFile();
        }
    }

    private async Task  CreateNewFile()
    {
        Exception exception = null;
        //await _semaphore.WaitAsync();
        try
        {
            _writer?.Flush();
            _writer?.Dispose();

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            _filePath = Path.Combine(_directory, $"{_filePrefix}_{timestamp}.csv");

            _writer = new StreamWriter(_filePath, false, Encoding.UTF8, 65536)
            {
                AutoFlush = true
            };

            _LogLineIndex = 0;
            await _writer.WriteLineAsync(_fileHeader);
            await _writer.WriteLineAsync($"Date: {DateTime.Now:dd.MM.yyyy, HH:mm:ss}");
            await _writer.WriteLineAsync(string.Join(_separator, _headerColumns));

            _currentFileSize = new FileInfo(_filePath).Length;
            NewFileEnrolled?.Invoke(_filePath);
        }
        catch (UnauthorizedAccessException ex)
        {
     
            AppLogger.AppLogger.Error(ex.ToString(), "DataLogger");
            exception = ex;
        }
        catch (IOException ex)
        {
    
            AppLogger.AppLogger.Error(ex.ToString(), "DataLogger");
            exception = ex;
        }
        catch (Exception ex)
        {
       
            AppLogger.AppLogger.Error(ex.ToString(), "DataLogger");
            exception = ex;
        }
        //finally { _semaphore.Release(); }
        if (exception != null) { throw exception; }
    }

    public async Task StopAsync()
    {
        Enabled = false;
        
        // 1. Channel zuerst schließen, damit der Background-Task seine Schleife beendet
        _channel?.Writer.TryComplete();
        await _cts.CancelAsync();
        // 2. Auf den Task warten, OHNE dass das Semaphore blockiert ist!
        if (_backgroundTask != null)
        {
            try { await _backgroundTask; } catch { }
        }
        await _semaphore.WaitAsync();
        try
        {          
            if (_writer != null)
            {
                await _writer.FlushAsync();
                await _writer.DisposeAsync();
                _writer = null;
            }
            
        }
        catch (Exception ex)
        {
            AppLogger.AppLogger.Error(ex.ToString(), "DataLogger");
        }
        finally
        { _semaphore.Release(); }            
        //_cts.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}

