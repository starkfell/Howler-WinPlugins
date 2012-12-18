/*===================================================================================================================================================

Application Name: system_uptime_check.exe
Application Type: Console Application

Author:           Ryan Irujo
Inception:        12.18.2012
Last Updated:     12.18.2012

Descrption:       Designed to work with Icinga/Nagios to return back the System Uptime of a Windows Host. The returned
                  values are calculated in Minutes that are in decimal format giving the ability to set thresholds
                  based on Minutes & Seconds. 

                  Example(s): 300 = 5 Minutes. 600.10 = 10 Minutes 10 Seconds. 1200.30 = 20 Minutes, 30 Seconds.

                  Additionally, the Plugin has been designed to use the .NET 2.0 Framework for backwards comaptibility
                  with older versions of Windows.

Changes:          


Syntax:           system_uptime_check.exe <Warning_Value> <Critical_Value>
 
Example:          system_uptime_check.exe 600.00 300.00

NSC.ini Format:   command[win_system_uptime_check]=X:\Path\To\Plugins\system_uptime_check.exe $ARG1$ $ARG2$

NRPE Syntax:      ./check_nrpe -H <hostname> -c win_system_uptime_check -a <Warning_Value> <Critical_Value>

   
===================================================================================================================================================*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;

namespace system_uptime_check
{
    class system_uptime_check
    {
        static void Main(string[] args)
        {

            // Getting Hostname.
            var Hostname = System.Environment.MachineName;

            // Making sure that the command is being run with Argument Values.
            if (args.Length == 0)
            {
                Console.WriteLine("Numeric [Warning] and [Critical] Values (in Minutes) must be provided!");
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


                // Making sure that the Warning Value is Numeric.
                if (WarningCheck.Success)
                {
                    Console.WriteLine("A Numeric [Warning] Value (in Minutes: 50, 100, 300, etc...) must be provided!");
                    Environment.Exit(3);
                }

                // Making sure that the Critical Value is Numeric.
                if (CriticalCheck.Success)
                {
                    Console.WriteLine("A Numeric [Critical] Value (in Minutes: 50, 100, 300, etc...) must be provided!");
                    Environment.Exit(3);
                }


                // Converting all Passed Arguments into a Usable State.
                var Warning  = (Convert.ToDouble(args[0]));
                var Critical = (Convert.ToDouble(args[1]));


                // Making sure that the Warning and Critical Numeric Values are not less than 0.00.
                if ((Warning < 0.00) || (Critical < 0.00))
                {
                    Console.WriteLine("Numeric [Warning] and [Critical] Values cannot be less than 0.00!");
                    Environment.Exit(3);
                }

                // Making sure that the Warning Value is Greater Than the Critical Value.
                if (Warning < Critical)
                {
                    Console.WriteLine("The [Warning] Value must be Greater Than the [Critical] Value!");
                    Environment.Exit(3);
                }

                // Making sure that the Warning Value is not equal to the Critical Value.
                else if (Warning == Critical)
                {
                    Console.WriteLine("The [Warning] Value cannot be Equal to the [Critical] Value!");
                    Environment.Exit(3);
                }


                ManagementObject MgmtQuery  = new ManagementObject(@"\\.\root\cimv2:Win32_OperatingSystem=@");
                DateTime LastStartUp        = ManagementDateTimeConverter.ToDateTime(MgmtQuery["LastBootUpTime"].ToString());
                var UpTime                  = DateTime.Now.ToUniversalTime() - LastStartUp.ToUniversalTime();
                var SysUpTime               = Convert.ToDouble(UpTime.TotalMinutes.ToString("0.00"));


                // Final Results and Performance Data are Returned.
                if (SysUpTime < Critical)
                {
                    Console.WriteLine("System UpTime = {0} Minutes. {1} Appears to have recovered from Shutdown or has been Restarted. | 'SysUpTime'={0};{2};{3};0.00;1000000000000.00;",
                        UpTime.TotalMinutes.ToString("0.00"), Hostname, Warning.ToString("0.00"), Critical.ToString("0.00"));
                    Environment.Exit(2);
                }

                else if ((SysUpTime > Critical) && (SysUpTime < Warning))
                {
                    Console.WriteLine("System UpTime = {0} Minutes. {1} Appears to have been recently Restarted.| 'SysUpTime'={0};{2};{3};0.00;1000000000000.00;",
                        UpTime.TotalMinutes.ToString("0.00"), Hostname, Warning.ToString("0.00"), Critical.ToString("0.00"));
                    Environment.Exit(1);
                }

                else if (SysUpTime > Warning)
                {
                    Console.WriteLine("System UpTime = {0} Minutes. {1} is OK! | 'SysUpTime'={0};{2};{3};0.00;1000000000000.00;",
                        UpTime.TotalMinutes.ToString("0.00"), Hostname, Warning.ToString("0.00"), Critical.ToString("0.00"));
                    Environment.Exit(0);
                }
            }

            // Catching Exception Errors here due to Missing Variables or Syntax Issues.
            catch (Exception Error)
            {
                if (Error is IndexOutOfRangeException)
                {
                    Console.WriteLine("The [Warning] and [Critical] Variables must BOTH be present!");
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
