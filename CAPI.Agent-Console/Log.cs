using System;

namespace CAPI.Agent_Console
{
    internal static class Log
    {
        public static void Write(string logContent)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {logContent}");
        }

        public static void Exception(Exception ex)
        {
            Log.Write($"Exception Message: {ex.Message}");
            Log.Write($"Exception Source: {ex.Source}");
            Log.Write($"Exception StackTrace: \r\n{ex.StackTrace}");
        }
    }
}