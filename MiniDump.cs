using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace MiniDumpStartupHook
{
    internal static class MiniDump
    {
        public static void WriteDump(Process proc, string name, string outputFolder = null)
        {
            try
            {
                var dumpFileName = $"{proc.ProcessName}-{proc.Id}-{name}-{DateTime.UtcNow.Ticks}.dmp";

                if (string.IsNullOrWhiteSpace(outputFolder))
                {
                    try
                    {
                        outputFolder = Environment.GetEnvironmentVariable("MINIDUMP_PATH");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

                if (!string.IsNullOrWhiteSpace(outputFolder))
                {
                    dumpFileName = Path.Combine(outputFolder, dumpFileName);
                }

                Console.WriteLine("Writing dump to: {0}", dumpFileName);

                if (File.Exists(dumpFileName))
                {
                    File.Delete(dumpFileName);
                }

                var hFile = NativeMethods.CreateFile(
                  dumpFileName,
                  NativeMethods.EFileAccess.GenericWrite,
                  NativeMethods.EFileShare.None,
                  lpSecurityAttributes: IntPtr.Zero,
                  dwCreationDisposition: NativeMethods.ECreationDisposition.CreateAlways,
                  dwFlagsAndAttributes: NativeMethods.EFileAttributes.Normal,
                  hTemplateFile: IntPtr.Zero);

                if (hFile == NativeMethods.INVALID_HANDLE_VALUE)
                {
                    var hr = Marshal.GetHRForLastWin32Error();
                    var ex = Marshal.GetExceptionForHR(hr);
                    Console.WriteLine(ex);
                    return;
                }

                NativeMethods._MINIDUMP_TYPE dumpType = NativeMethods._MINIDUMP_TYPE.MiniDumpWithFullMemory;
                NativeMethods.MINIDUMP_EXCEPTION_INFORMATION exceptInfo = default;

                if (!Is32BitProcess(proc) && IntPtr.Size == 4)
                {
                    Console.WriteLine("Error: Can't create 32 bit dump of 64 bit process");
                    return;
                }

                var result = NativeMethods.MiniDumpWriteDump(
                          proc.Handle,
                          proc.Id,
                          hFile,
                          dumpType,
                          ref exceptInfo,
                          UserStreamParam: IntPtr.Zero,
                          CallbackParam: IntPtr.Zero);
                if (result == false)
                {
                    var hr = Marshal.GetHRForLastWin32Error();
                    var ex = Marshal.GetExceptionForHR(hr);
                    Console.WriteLine(ex);
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static bool Is32BitProcess(Process proc)
        {
            bool fIs32bit = false;

            // if we're runing on 32bit, default to true
            if (IntPtr.Size == 4)
            {
                fIs32bit = true;
            }

            bool fIsRunningUnderWow64 = false;

            // if machine is 32 bit then all procs are 32 bit
            if (NativeMethods.IsWow64Process(NativeMethods.GetCurrentProcess(), out fIsRunningUnderWow64) && fIsRunningUnderWow64)
            {
                // current OS is 64 bit
                if (NativeMethods.IsWow64Process(proc.Handle, out fIsRunningUnderWow64) && fIsRunningUnderWow64)
                {
                    fIs32bit = true;
                }
                else
                {
                    fIs32bit = false;
                }
            }

            return fIs32bit;
        }
    }
}
