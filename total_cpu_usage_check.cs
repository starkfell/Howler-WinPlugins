/*===================================================================================================================================================

Application Name: total_cpu_usage_check.exe
Application Type: Console Application

Author:           Ryan Irujo
Inception:        12.10.2012
Last Updated:     12.10.2012

Descrption:       Designed to work with Icinga/Nagios to return back the Total % CPU Usage on a Host.
                  
Changes:          


Syntax:           total_cpu_usage_check.exe <Warning_Percent> <Critical_Percent>
 
Example:          total_cpu_usage_check.exe "80.00" "90.00"

NSC.ini Format:   command[win_total_cpu_usage_check]=X:\Path\To\Plugins\total_cpu_usage_check.exe $ARG1$ $ARG2$

NRPE Syntax:      ./check_nrpe -H <hostname> -c win_total_cpu_usage_check -a <Warning_Percent> <Critical_Percent>

   
===================================================================================================================================================*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace total_cpu_usage_check
{
    class Program
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

                // Declared Argument Values for Disk, Warning Threshold and Critical Threshold.
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


                var cpuCounter = new PerformanceCounter("Processor","% Processor Time","_Total",true);

                var Sample_1 = cpuCounter.NextSample(); System.Threading.Thread.Sleep(1000);
                var Sample_2 = cpuCounter.NextSample();

                var TotalCPUPercent = ((1 - ((double)(Sample_2.RawValue - Sample_1.RawValue) / (double)(Sample_2.TimeStamp100nSec - Sample_1.TimeStamp100nSec))) * 100);


                // Final Results and Performance Data are Returned.
                if (TotalCPUPercent > Critical)
                {
                    Console.WriteLine("[_Total]: % CPU Usage = {0}% - CRITICAL | '[_Total]'={0}%;{1};{2};0.00;100.00;", TotalCPUPercent.ToString("0.00"), Warning.ToString("0.00"), Critical.ToString("0.00"));
                    Environment.Exit(2);
                }

                else if ((TotalCPUPercent < Critical) && (TotalCPUPercent > Warning))
                {
                    Console.WriteLine("[_Total]: % CPU Usage = {0}% - WARNING | '[_Total]'={0}%;{1};{2};0.00;100.00;", TotalCPUPercent.ToString("0.00"), Warning.ToString("0.00"), Critical.ToString("0.00"));
                    Environment.Exit(1);
                }

                else if (TotalCPUPercent < Warning)
                {
                    Console.WriteLine("[_Total]: % CPU Usage = {0}% - OK | '[_Total]'={0}%;{1};{2};0.00;100.00;", TotalCPUPercent.ToString("0.00"), Warning.ToString("0.00"), Critical.ToString("0.00"));
                    Environment.Exit(0);
                }
            }

            //// Catching Exception Errors here due to Missing Variables or Syntax Issues.
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
}
