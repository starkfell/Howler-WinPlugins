/*===================================================================================================================================================

Application Name: single_cpu_usage_check.exe
Application Type: Console Application

Author:           Ryan Irujo
Inception:        12.12.2012
Last Updated:     12.12.2012

Descrption:       Designed to work with Icinga/Nagios to return back the % Processor Time of a specific Processor Instance on a Host.
                  
Changes:          


Syntax:           single_cpu_usage_check.exe <Processor_Instance> <Warning_Percent> <Critical_Percent>
 
Example:          single_cpu_usage_check.exe 0 80.00 90.00

NSC.ini Format:   command[win_single_cpu_usage_check]=X:\Path\To\Plugins\single_cpu_usage_check.exe $ARG1$ $ARG2$ $ARG3$

NRPE Syntax:      ./check_nrpe -H <hostname> -c win_single_cpu_usage_check -a <Processor_Instance> <Warning_Percent> <Critical_Percent>

   
===================================================================================================================================================*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace single_cpu_usage_check
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
                Console.WriteLine("A [Processor_Instance], [Warning Percent] and a [Critical Percent] Value must be provided!");
                Environment.Exit(3);
            }

            try
            {

                // Declared Argument Values for Disk, Warning Threshold and Critical Threshold.
                String Arg_0 = args[0];
                String Arg_1 = args[1];
                String Arg_2 = args[2];


                // Testing Argument Variables to determine if they are the correct type of value.
                Match ProcessorInstanceCheck = Regex.Match(Arg_0, @"[A-Z]", RegexOptions.IgnoreCase);
                Match WarningCheck           = Regex.Match(Arg_1, @"[A-Z]", RegexOptions.IgnoreCase);
                Match CriticalCheck          = Regex.Match(Arg_2, @"[A-Z]", RegexOptions.IgnoreCase);

                // Making sure that the Warning Percent Decimal Value is a number.
                if (ProcessorInstanceCheck.Success)
                {
                    Console.WriteLine("A [Proceesor_Instance] Numeric Value (0,1,2,5 etc...) must be provided!");
                    Environment.Exit(3);
                }

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
                var Warning      = (Convert.ToDouble(args[1]));
                var Critical     = (Convert.ToDouble(args[2]));


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
                
                // Checking to see if the Processor Instance exists.
                if (!System.Diagnostics.PerformanceCounterCategory.InstanceExists(args[0], "Processor"))
                {
                    Console.WriteLine("Processor Instance [{0}] does not exist on {1}!", args[0], Hostname);
                    Environment.Exit(3);
                }


                // Processor Instance is queried.
                var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", args[0], true);


                // Calculating the % Processor Time of the chosen Processor Instance.
                var Sample_1 = cpuCounter.NextSample(); System.Threading.Thread.Sleep(1000);
                var Sample_2 = cpuCounter.NextSample();

                var ProcessorTime = ((1 - ((double)(Sample_2.RawValue - Sample_1.RawValue) / (double)(Sample_2.TimeStamp100nSec - Sample_1.TimeStamp100nSec))) * 100);


                // Final Results and Performance Data are Returned.
                if (ProcessorTime > Critical)
                {
                    Console.WriteLine("[{0}]: % Processor Time = {1}% - CRITICAL | '[{0}]'={1}%;{2};{3};0.00;100.00;", args[0], ProcessorTime.ToString("0.00"), Warning.ToString("0.00"), Critical.ToString("0.00"));
                    Environment.Exit(2);
                }

                else if ((ProcessorTime < Critical) && (ProcessorTime > Warning))
                {
                    Console.WriteLine("[{0}]: % Processor Time = {1}% - WARNING | '[{0}]'={1}%;{2};{3};0.00;100.00;", args[0], ProcessorTime.ToString("0.00"), Warning.ToString("0.00"), Critical.ToString("0.00"));
                    Environment.Exit(1);
                }

                else if (ProcessorTime < Warning)
                {
                    Console.WriteLine("[{0}]: % Processor Time = {1}% - OK | '[{0}]'={1}%;{2};{3};0.00;100.00;", args[0], ProcessorTime.ToString("0.00"), Warning.ToString("0.00"), Critical.ToString("0.00"));
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
