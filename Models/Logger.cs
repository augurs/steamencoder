using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncoderApp.Models
{
    public static class Logger
    {
        private static readonly string logFilePath = "app_log.txt";

        public static void Log(string message)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }

        public static void LogError(Exception ex)
        {
            Log($"[ERROR] {ex.Message}\n{ex.StackTrace}");
        }
    }
}
