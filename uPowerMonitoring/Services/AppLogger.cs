    using System;

    using System.IO;
    using System.Threading.Channels;
    using System.Threading.Tasks;

    namespace AppLogger;

    public static class AppLogger
    {
        private static string? _logFilePath;
        private static readonly Channel<string> _logChannel = Channel.CreateUnbounded<string>();
        private static Task? _processTask;
        public static void Initialize(string? filePath = null)
        {
            _logFilePath = filePath;
            _processTask = Task.Run(ProcessLogsAsync);
        }

        public static void Info(string msg, string mod) => Write("INFO", mod, msg);
        public static void Error(string msg, string mod) => Write("ERROR", mod, msg);

        private static void Write(string level, string module, string message)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var fullLine = $"[{timestamp}] [{level}] [{module}] : {message}";
            Console.WriteLine(fullLine);
            _logChannel.Writer.TryWrite(fullLine);
        }


        private static async Task ProcessLogsAsync()
        {
            // Dieser Loop läuft ewig im Hintergrund
            await foreach (var line in _logChannel.Reader.ReadAllAsync())
            {
                if (string.IsNullOrEmpty(_logFilePath)) continue;

                try
                {
                    await File.AppendAllTextAsync(_logFilePath, line + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"App Log Error: {ex.Message}");
                    await Task.Delay(100);
                }
            }
        }
    }

