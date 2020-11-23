using MiniDumpStartupHook;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

internal class StartupHook
{
    static int _miniDumpExecuted = 0;

    public static void Initialize()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("MiniDump is not supported on non windows platform.");
            return;
        }

        AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
    }

    private static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
    {
        if (e.Exception is BadImageFormatException)
        {
            if (Interlocked.CompareExchange(ref _miniDumpExecuted, 1, 0) == 0)
            {
                MiniDump.WriteDump(Process.GetCurrentProcess(), nameof(BadImageFormatException));
            }
        }
        else if (e.Exception is TypeLoadException)
        {
            if (Interlocked.CompareExchange(ref _miniDumpExecuted, 1, 0) == 0)
            {
                MiniDump.WriteDump(Process.GetCurrentProcess(), nameof(TypeLoadException));
            }
        }
    }
}