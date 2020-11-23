using System;
using System.Collections.Generic;
using System.Text;

namespace MiniDumpStartupHook
{
    internal static class Settings
    {
        public static string Path { get; private set; }
        public static string[] ExceptionTypes { get; private set; }

        static Settings()
        {
            try
            {
                Path = Environment.GetEnvironmentVariable("MINIDUMP_PATH");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            string exceptionTypes = null;
            try
            {
                exceptionTypes = Environment.GetEnvironmentVariable("MINIDUMP_EXCEPTIONTYPES");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            if (string.IsNullOrWhiteSpace(exceptionTypes))
            {
                exceptionTypes = "BadImageFormatException;TypeLoadException;AccessViolationException;ApplicationException;ExecutionEngineException";
            }
            ExceptionTypes = exceptionTypes.Split(';', StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
