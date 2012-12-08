/*===================================================================================================================================================

Application Name: SingleDriveSpaceCheck
Application Type: Console Application

Author:           Ryan Irujo
Inception:        09.21.2012
Last Updated:     12.08.2012
 
Descrption:       Designed to work with Icinga/Nagios to parse Drive Space Statistics for a Single Drive residing on a Windows Host.
                  
Changes:          


Syntax:           Single_Drive_Space_Check.exe "<Drive_Letter>" "<Warning_Percent>" "<Critical_Percent>"
 
Example:          Single_Drive_Space_Check.exe "C" "10.00" "5.00"
   
===================================================================================================================================================*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Single_Drive_Check_Console_Application
{
    class Program
    {
        static void Main(string[] args)
        {

            var Hostname = System.Environment.MachineName;

            // Making sure that the command is being run with Argument Values.
            if (args.Length == 0)
            {
                Console.WriteLine("A [Drive Letter], [Warning Percent], and [Critical Percent] Value must be provided!");
                Environment.Exit(2);
            }

            try
            {

                // Declared Argument Values for Disk, Warning Threshold and Critical Threshold.
                String Arg_0 = args[0];
                String Arg_1 = args[1];
                String Arg_2 = args[2];


                // Testing Argument Variables to determine if they are the correct type of value.
                Match DriveLetterCheck = Regex.Match(Arg_0, @"[0-9]", RegexOptions.IgnoreCase);
                Match WarningCheck     = Regex.Match(Arg_1, @"[A-Z]", RegexOptions.IgnoreCase);
                Match CriticalCheck    = Regex.Match(Arg_2, @"[A-Z]", RegexOptions.IgnoreCase);


                // Making sure that the Drive Letter Argument Value is a letter.
                if (DriveLetterCheck.Success)
                {
                    Console.WriteLine("A [Drive Letter] (C,D.E. etc...) must be provided.");
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
                DriveInfo SingleDrive = new System.IO.DriveInfo(args[0]);
                var Warning           = (Convert.ToDouble(args[1]));
                var Critical          = (Convert.ToDouble(args[2]));


                // Making sure that the Drive Being Checked exists and is online and available.
                if (SingleDrive.IsReady != true)
                {
                    Console.WriteLine("The {0} Drive is either not online or does not exist. Login to {1} and verify its availability.", args[0], Hostname);
                    Environment.Exit(2);
                }
                
                // Making sure that the Warning Percent Value is Greater than the Critical Percent Value.
                else if (Warning < Critical)
                {
                    Console.WriteLine("The [Warning Percent] Value must be Greater than the [Critical Percent] Value!");
                    Environment.Exit(3);
                }

                // Making sure that the Warning Percent Value is not equal to the Critical Percent Value.
                else if (Warning == Critical)
                {
                    Console.WriteLine("The [Warning Percent] Value cannot be Equal to the [Critical Percent] Value!");
                    Environment.Exit(3);
                }


                // Gathering Disk Space Statistics and formatting for Output to Icinga/Nagios and PNP4Nagios Reporting.
                var DriveLetter  = SingleDrive.Name;
                var SpaceTotal   = (SingleDrive.TotalSize / 1073741824.00);
                var SpaceUsed    = ((SingleDrive.TotalSize - SingleDrive.TotalFreeSpace) / 1073741824.00);
                var SpaceFree    = (SingleDrive.TotalFreeSpace / 1073741824.00);
                var PercentFree  = ((SpaceFree / SpaceTotal) * 100);
                var WarningPerf  = (SpaceTotal * (Warning * 0.01));
                var CriticalPerf = (SpaceTotal * (Critical * 0.01));



                // Final Results and Performance Data are Returned.
                if (PercentFree < Critical)
                {
                    Console.WriteLine("[{0}]: is CRITICAL! %Free = {1}% Total = {2}GB, Used = {3}GB, Free = {4}GB | 'FreeSpace'={4}GB;{5};{6};0.00;{2};", 
                            DriveLetter, PercentFree.ToString("0.00"), SpaceTotal.ToString("0.00"), SpaceUsed.ToString("0.00"), SpaceFree.ToString("0.00"), WarningPerf.ToString("0.00"), CriticalPerf.ToString("0.00"));
                    Environment.Exit(2);
                }
                else if ((PercentFree > Critical) && (PercentFree < Warning))
                {
                    Console.WriteLine("[{0}]: is LOW! %Free = {1}% Total = {2}GB, Used = {3}GB, Free = {4}GB | 'FreeSpace'={4}GB;{5};{6};0.00;{2};", 
                            DriveLetter, PercentFree.ToString("0.00"), SpaceTotal.ToString("0.00"), SpaceUsed.ToString("0.00"), SpaceFree.ToString("0.00"), WarningPerf.ToString("0.00"), CriticalPerf.ToString("0.00"));
                    Environment.Exit(1);
                }
                else
                {
                    Console.WriteLine("[{0}]: is OK! %Free = {1}% Total = {2}GB, Used = {3}GB, Free = {4}GB | 'FreeSpace'={4}GB;{5};{6};0.00;{2};", 
                            DriveLetter, PercentFree.ToString("0.00"), SpaceTotal.ToString("0.00"), SpaceUsed.ToString("0.00"), SpaceFree.ToString("0.00"), WarningPerf.ToString("0.00"), CriticalPerf.ToString("0.00"));
                    Environment.Exit(0);
                }

            }

            // Catching Exception Errors here due to Missing Variables or Syntax Issues.
            catch (Exception Error)
            {
                if (Error is IndexOutOfRangeException)
                {
                    Console.WriteLine("The [Drive Letter], [Warning Percent], and [Critical Percent] Variables ALL must be present!");
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
