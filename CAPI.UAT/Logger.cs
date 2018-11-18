using System;

namespace CAPI.UAT
{
    public static class Logger
    {
        public static void Write(string text, bool line = true, TextType textType = TextType.Content,
                                 bool bright = false, sbyte gapTop = 0, sbyte indentation = 9)
        {
            var color = Console.ForegroundColor;

            switch (textType)
            {
                case TextType.Content:
                    Console.ForegroundColor = bright ? ConsoleColor.Gray : ConsoleColor.DarkGray;
                    break;
                case TextType.Success:
                    Console.ForegroundColor = bright ? ConsoleColor.Green : ConsoleColor.DarkGreen;
                    break;
                case TextType.Fail:
                    Console.ForegroundColor = bright ? ConsoleColor.Red : ConsoleColor.DarkRed;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(textType), textType, null);
            }

            for (var i = 0; i < gapTop; i++) Console.WriteLine("");

            if (line)
                Console.WriteLine($"{new string(' ', indentation)}{text}");
            else Console.Write($"{new string(' ', indentation)}{text}");

            Console.ForegroundColor = color;
        }

        public enum TextType
        {
            Content,
            Success,
            Fail
        }
    }
}
