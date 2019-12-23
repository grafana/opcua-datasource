using System;
using System.Collections.Generic;
using System.Text;

namespace MicrosoftOpcUa.Client.Utility
{
    public static class Screen
    {
        public static void Log(
            string text, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss,fff")}]"
                + text);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
