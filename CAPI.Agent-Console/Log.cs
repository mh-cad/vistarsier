using System;

namespace CAPI.Agent_Console
{
    internal static class Log
    {
        public static void Write(string logContent)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {logContent}");
        }

        public static void WriteError(string logContent)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [ERROR] {logContent}");
        }

        public static void Exception(Exception ex)
        {
            Write($"Error Message: {ex.Message}");
            Write($"Error Source: {ex.Source}");
            Write($"Error StackTrace: \r\n{ex.StackTrace}");
        }
    }
}