/*===================================================================================================================================================

Application Name: memory_usage_check.exe
Application Type: Console Application

Author:           Ryan Irujo
Inception:        12.12.2012
Last Updated:     12.17.2012

Descrption:       Designed to work with Icinga/Nagios to return back Memory Utilization Statistics of a Windows Host.
                  
Changes:          12.17.2012 - [R. Irujo]
                  - Implemented the PerformanceInfo class to return back Memory Statistics using the PERFORMANCE_INFORMATION structure
                    retrieved from the Windows Process Status API (psapi.dll). The PSAPI Code was originally written by Antonio Bakula
                    and was retrieved from the following link: http://stackoverflow.com/questions/10027341/c-sharp-get-used-memory-in.


Syntax:           memory_usage_check.exe <Warning_Percent> <Critical_Percent>
 
Example:          memory_usage_check.exe 80.00 90.00

NSC.ini Format:   command[win_memory_usage_check]=X:\Path\To\Plugins\memory_usage_check.exe $ARG1$ $ARG2$

NRPE Syntax:      ./check_nrpe -H <hostname> -c win_memory_usage_check -a <Warning_Percent> <Critical_Percent>

   
===================================================================================================================================================*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;


namespace memory_usage_check
{

    class memory_usage_check
    {

        static void Main(string[] args)
        {

            // Getting Hostname.
            var Hostname = System.Environment.MachineName;

            // Making sure that the command is being run with Argument Values.
            if (args.Length == 0)
            {
                Console.WriteLine("A [Warning Percent] and a [Critical Percent] Value must be provided!");
                Environment.Exit(3);
            }

            try
            {

                // Declared Argument Values for Warning and Critical Thresholds.
                String Arg_0 = args[0];
                String Arg_1 = args[1];


                // Testing Argument Variables to determine if they are the correct type of value.
                Match WarningCheck  = Regex.Match(Arg_0, @"[A-Z]", RegexOptions.IgnoreCase);
                Match CriticalCheck = Regex.Match(Arg_1, @"[A-Z]", RegexOptions.IgnoreCase);


                // Making sure that the Warning Percent Decimal Value is a number.
                if (WarningCheck.Success)
                {
                    Console.WriteLine("A [Warning Percent] Decimal Value (1.00, 12.00, 90.00 etc...) must be provided!");
                    Environment.Exit(3);
                }

                // Making sure that the Critical Percent Decimal Value is a number.
                if (CriticalCheck.Success)
                {
                    Console.WriteLine("A [Critical Percent] Decimal Value (1.00, 12.00, 90.00 etc...) must be provided!");
                    Environment.Exit(3);
                }


                // Converting all Passed Arguments into a Usable State.
                var Warning  = (Convert.ToDouble(args[0]));
                var Critical = (Convert.ToDouble(args[1]));


                // Making sure that the Warning Percent and Critical Percent Values are not greater than 100%
                if ((Warning > 100.00) || (Critical > 100.00))
                {
                    Console.WriteLine("The [Warning Percent] and [Critical Percent] Values cannot be greater than 100.00!");
                    Environment.Exit(3);
                }

                // Making sure that the Warning Percent Value is Less than the Critical Percent Value.
                if (Warning > Critical)
                {
                    Console.WriteLine("The [Warning Percent] Value must be Less than the [Critical Percent] Value!");
                    Environment.Exit(3);
                }

                // Making sure that the Warning Percent Value is not equal to the Critical Percent Value.
                else if (Warning == Critical)
                {
                    Console.WriteLine("The [Warning Percent] Value cannot be Equal to the [Critical Percent] Value!");
                    Environment.Exit(3);
                }


                // Gathering Memory Statistics.
                Int64 FreeMemoryMB    = PerformanceInfo.GetPhysicalAvailableMemoryInMiB();
                Int64 TotalMemoryMB   = PerformanceInfo.GetTotalMemoryInMiB();
                var UsedMemoryMB      = (double)TotalMemoryMB - (double)FreeMemoryMB;
                var FreeMemoryPercent = ((double)FreeMemoryMB / (double)TotalMemoryMB) * 100;
                var UsedMemoryPercent = 100 - FreeMemoryPercent;


                // Final Results and Performance Data are Returned.
                if (UsedMemoryPercent > Critical)
                {
                    Console.WriteLine("% Used = {0}%, Total = {2}MB, Free = {3}MB, Used = {4}MB - CRITICAL! | 'Memory_Used'={0}%;{5};{6};0.00;100.00; 'Memory_Free'={1}%;;;0.00;100.00;",
                        UsedMemoryPercent.ToString("0.00"), FreeMemoryPercent.ToString("0.00"), TotalMemoryMB.ToString("0.00"), FreeMemoryMB.ToString("0.00"), UsedMemoryMB.ToString("0.00"),
                        Warning.ToString("0.00"), Critical.ToString("0.00"));
                    Environment.Exit(2);
                }

                else if ((UsedMemoryPercent < Critical) && (UsedMemoryPercent > Warning))
                {
                    Console.WriteLine("% Used = {0}%, Total = {2}MB, Free = {3}MB, Used = {4}MB - WARNING! | 'Memory_Used'={0}%;{5};{6};0.00;100.00; 'Memory_Free'={1}%;;;0.00;100.00;",
                        UsedMemoryPercent.ToString("0.00"), FreeMemoryPercent.ToString("0.00"), TotalMemoryMB.ToString("0.00"), FreeMemoryMB.ToString("0.00"), UsedMemoryMB.ToString("0.00"),
                        Warning.ToString("0.00"), Critical.ToString("0.00"));
                    Environment.Exit(1);
                }

                else if (UsedMemoryPercent < Warning)
                {
                    Console.WriteLine("% Used = {0}%, Total = {2}MB, Free = {3}MB, Used = {4}MB - OK! | 'Memory_Used'={0}%;{5};{6};0.00;100.00; 'Memory_Free'={1}%;;;0.00;100.00;",
                        UsedMemoryPercent.ToString("0.00"), FreeMemoryPercent.ToString("0.00"), TotalMemoryMB.ToString("0.00"), FreeMemoryMB.ToString("0.00"), UsedMemoryMB.ToString("0.00"),
                        Warning.ToString("0.00"), Critical.ToString("0.00"));
                    Environment.Exit(0);
                }
            }

            // Catching Exception Errors here due to Missing Variables or Syntax Issues.
            catch (Exception Error)
            {
                if (Error is IndexOutOfRangeException)
                {
                    Console.WriteLine("The [Warning Percent] and [Critical Percent] Variables must BOTH be present!");
                    Environment.Exit(3);
                }

                else if (Error is FormatException)
                {
                    Console.WriteLine("Please check the format of the values you have assigned to your variables.");
                    Environment.Exit(3);
                }
            }
        }
    }


    public static class PerformanceInfo
    {
        // Memory Statistics are being called using the PERFORMANCE_INFORMATION structure using the Windows Process Status API (psapi.dll). 
        // PSAPI Retrieval portion was originally written by Antonio Bakula and posted here: http://stackoverflow.com/questions/10027341/c-sharp-get-used-memory-in

        // psapi.dll import and performance information call.
        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPerformanceInfo([Out] out PerformanceInformation PerformanceInformation, [In] int Size);

        [StructLayout(LayoutKind.Sequential)]
        public struct PerformanceInformation
        {
            public int Size;
            public IntPtr CommitTotal;
            public IntPtr CommitLimit;
            public IntPtr CommitPeak;
            public IntPtr PhysicalTotal;
            public IntPtr PhysicalAvailable;
            public IntPtr SystemCache;
            public IntPtr KernelTotal;
            public IntPtr KernelPaged;
            public IntPtr KernelNonPaged;
            public IntPtr PageSize;
            public int HandlesCount;
            public int ProcessCount;
            public int ThreadCount;
        }


        // Gathering the Amount of Physical Memory Free in Megabytes.
        public static Int64 GetPhysicalAvailableMemoryInMiB()
        {
            PerformanceInformation pi = new PerformanceInformation();
            if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
            {
                return Convert.ToInt64((pi.PhysicalAvailable.ToInt64() * pi.PageSize.ToInt64() / 1048576));
            }
            else
            {
                Console.WriteLine("There was a problem retrieving the Physical Memory Free on the Host.");
                return 2;
            }
        }

        // Gathering the Total Amount of Memory on the Host in Megabtyes.
        public static Int64 GetTotalMemoryInMiB()
        {
            PerformanceInformation pi = new PerformanceInformation();
            if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi)))
            {
                return Convert.ToInt64((pi.PhysicalTotal.ToInt64() * pi.PageSize.ToInt64() / 1048576));
            }
            else
            {
                Console.WriteLine("There was a problem retrieving the Total Amount of Memory on the Host.");
                return 2;
            }
        }
    }
}
