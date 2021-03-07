using System;

namespace MegaMod
{
    static class ConsoleTools
    {
        public static void Info(object message)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine("[MegaMod INFO] " + message);
            System.Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Error(object message)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("[MegaMod ERROR] " + message);
            System.Console.ForegroundColor = ConsoleColor.White;
        }
    }
}