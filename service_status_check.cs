/*===================================================================================================================================================

Application Name: service_status_check.exe
Application Type: Console Application

Author:           Ryan Irujo
Inception:        12.19.2012
Last Updated:     12.19.2012

Descrption:       Designed to work with Icinga/Nagios to return back the State of a particular Service running on a 
                  Windows Host. The check is done based upon the Service Name of the Service NOT the Display Name.
                  The Service Name Variable is not case-sensitive. 
                  
                  The Plugin has been designed to use the .NET 2.0 Framework for backwards comaptibility with older 
                  versions of Windows.

Changes:          


Syntax:           service_status_check.exe <Service_Name>

Example:          service_status_check.exe W3SVC

NSC.ini Format:   command[win_service_status_check]=X:\Path\To\Plugins\service_status_check.exe $ARG1$

NRPE Syntax:      ./check_nrpe -H <hostname> -c win_service_status_check -a <Service_Name>

   
===================================================================================================================================================*/

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;

namespace service_status_check
{
    class service_status_check
    {
        static void Main(string[] args)
        {
            // Getting Hostname.
            var Hostname = System.Environment.MachineName;

            // Making sure that the command is being run with Argument Values.
            if (args.Length == 0)
            {
                Console.WriteLine("A [Service_Name] Value must be provided!");
                Environment.Exit(3);
            }

            try
            {

                // Calling ServiceController Class to retrieve the Service passed from the Argument Variable.
                ServiceController ServiceCheck = new ServiceController(args[0]);

                // Retriving the Status of the Service being checked and then converting it to a String Type.
                var ServiceStatus = ServiceCheck.Status.ToString();


                // RegEx Matching on the Returned Status State of the Service.
                Match StatusRunning  = Regex.Match(ServiceStatus, @"\b(Running)\b", RegexOptions.IgnoreCase);
                Match StatusStopped  = Regex.Match(ServiceStatus, @"\b(Stopped)\b", RegexOptions.IgnoreCase);
                Match StatusPaused   = Regex.Match(ServiceStatus, @"\b(Paused)\b", RegexOptions.IgnoreCase);
                Match StatusStopping = Regex.Match(ServiceStatus, @"\b(Stopping)\b", RegexOptions.IgnoreCase);
                Match StatusStarting = Regex.Match(ServiceStatus, @"\b(Starting)\b", RegexOptions.IgnoreCase);


                // Checking to see which of the RegEx Matching entries is Successful and then returning the result.
                if (StatusRunning.Success)
                {
                    Console.WriteLine("{0} is {1}. | 'Service_State'=4;;;0.0;5.0;", ServiceCheck.DisplayName, ServiceCheck.Status);
                    Environment.Exit(0);
                }

                else if (StatusStopped.Success)
                {
                    Console.WriteLine("{0} is {1}. | 'Service_State'=2;;;0.0;5.0;", ServiceCheck.DisplayName, ServiceCheck.Status);
                    Environment.Exit(2);
                }

                else if (StatusPaused.Success)
                {
                    Console.WriteLine("{0} is {1}. | 'Service_State'=1;;;0.0;5.0;", ServiceCheck.DisplayName, ServiceCheck.Status);
                    Environment.Exit(2);
                }

                else if (StatusStopping.Success)
                {
                    Console.WriteLine("{0} is {1}. | 'Service_State'=3;;;0.0;5.0;", ServiceCheck.DisplayName, ServiceCheck.Status);
                    Environment.Exit(2);
                }

                else if (StatusStarting.Success)
                {
                    Console.WriteLine("{0} is {1}. | 'Service_State'=3;;;0.0;5.0;", ServiceCheck.DisplayName, ServiceCheck.Status);
                    Environment.Exit(2);
                }
            }

            // Catching Exception Errors here due to Missing Variables or Syntax Issues.
            catch (Exception Error)
            {
                if (Error is IndexOutOfRangeException)
                {
                    Console.WriteLine("The [Service_Name] Variable must be present!");
                    Environment.Exit(3);
                }

                else if (Error is FormatException)
                {
                    Console.WriteLine("Please check the format of the values you have assigned to your variables.");
                    Environment.Exit(3);
                }

                else if (Error is InvalidOperationException)
                {
                    Console.WriteLine("The Service Name [{0}] was not found on {1}!", args[0], Hostname);
                    Environment.Exit(3);
                }
            }
        }
    }
}
