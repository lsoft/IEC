using System;
using System.Runtime.CompilerServices;

namespace IEC.Common.Other
{
    /// <summary>
    /// IEC логгер
    /// </summary>
    public interface IIECLogger
    {
        void LogMessage(
            string message,
            [CallerMemberName] string callerClassName = ""
            );

        void LogHandledException(
            string errorMessage,
            Exception excp,
            [CallerMemberName] string callerClassName = ""
            );
    }
}