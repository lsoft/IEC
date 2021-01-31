using System;
using IEC.Common.Other;

namespace IEC.TestConsole
{
    public class ConsoleLogger : IIECLogger
    {
        /// <inheritdoc />
        public void LogMessage(
            string message,
            string callerClassName = ""
            )
        {
            Console.WriteLine($"MESSAGE from {callerClassName}: {message}");
        }

        /// <inheritdoc />
        public void LogHandledException(
            string errorMessage,
            Exception excp,
            string callerClassName = ""
            )
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR from {callerClassName}");
            Console.WriteLine(errorMessage);
            Console.WriteLine(excp.Message);
            Console.WriteLine(excp.StackTrace);
            Console.ResetColor();
        }
    }
}