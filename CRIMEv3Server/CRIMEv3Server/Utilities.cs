using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace BREACHv3Server
{
    partial class Program
    {

        static Char[] KeySpace = new Char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
        static int NumberOfRequests = 0;
        static int PADDING_SIZE = 40; // THIS NUMBER MAKES A DIFFERENCE.
        static int PADDING_SIZE_ADJUSTMENT = 0;

        static int guessedTokens = 0;
        static int failedTokens = 0;

        static int lastFrameRead = 0;

        public static void WriteConsoleInfo(string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor; // restore color
        }

        public static void WriteConsoleSuccess(string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor; // restore color
        }

        public static void WriteConsoleError(string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor; // restore color
        }

        public static void WriteConsoleWarning(string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor; // restore color
        }
    }
}
