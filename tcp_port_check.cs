/*===================================================================================================================================================

Application Name: tcp_port_check.exe
Application Type: Console Application

Author:           Ryan Irujo
Inception:        12.20.2012
Last Updated:     12.20.2012

Descrption:       Designed to work with Icinga/Nagios to return back whether a TCP Port on a Windows Host is Listening
                  or Unavailable.The check is done based upon the Numeric Value that is passed to the Port Number Variable.
                  
                  The Plugin has been designed to use the .NET 2.0 Framework for backwards comaptibility with older 
                  versions of Windows.

Changes:          


Syntax:           tcp_port_check.exe <Port_Number>

Example:          tcp_port_check.exe 443

NSC.ini Format:   command[win_tcp_port_check]=X:\Path\To\Plugins\tcp_port_check.exe $ARG1$

NRPE Syntax:      ./check_nrpe -H <hostname> -c win_tcp_port_check -a <Service_Name>

   
===================================================================================================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;


namespace tcp_port_check
{
    class tcp_port_check
    {
        static void Main(string[] args)
        {
            // Getting Hostname.
            var Hostname = System.Environment.MachineName;

            // Making sure that the command is being run with Argument Values.
            if (args.Length == 0)
            {
                Console.WriteLine("A [Port_Number] Value must be provided!");
                Environment.Exit(3);
            }

            try
            {
                // Declared Argument Value for Port Number.
                String Arg_0 = args[0];

                // Testing Argument Variables to determine if they are the correct type of value.
                Match PortNumber = Regex.Match(Arg_0, @"[A-Z!<,>@#$%\&*()_+\-=\\/\,.?\{}]", RegexOptions.IgnoreCase);


                // Making sure that the Argument Value is Numeric.
                if (PortNumber.Success)
                {
                    Console.WriteLine("A Numeric [Port_Number] Value (80, 443, 22, etc...) must be provided!");
                    Environment.Exit(3);
                }

                // Making sure the Argument Value is equal to or below 65535.
                if (Convert.ToDouble(args[0]) > (double)65535)
                {
                    Console.WriteLine("The Numeric [Port_Number] Value must be below 65535!");
                    Environment.Exit(3);
                }


                // Retrieving the current network connectivity of the Windows Host.
                IPGlobalProperties IPs = IPGlobalProperties.GetIPGlobalProperties();

                // Retrieving all current TCP Ports in a Listening State.
                IPEndPoint[] OpenPorts = IPs.GetActiveTcpListeners();

                // Sorting through the OpenPorts Array to determine if the Port being queried is in a Listening State.
                var Entry = Array.Find(OpenPorts, TCPPort => TCPPort.Port.Equals(Convert.ToInt32(args[0])));


                // Final Results and Performance Data are Returned.
                if (Entry == null)
                {
                    Console.WriteLine("Port [{0}] is Unavailable on {1}! - CRITICAL! | 'Port_State'=0.0;;;0.0;10.0;", args[0], Hostname);
                    Environment.Exit(2);
                }

                if (Entry.Port == Convert.ToInt32(args[0]))
                {
                    Console.WriteLine("{0} is Listening on Port [{1}]. - OK! | 'Port_State'=1.0;;;0.0;10.0;", Hostname, args[0]);
                    Environment.Exit(0);
                }
            }

            // Catching Exception Errors here due to Missing Variables or Syntax Issues.
            catch (Exception Error)
            {
                if (Error is NullReferenceException)
                {
                    Console.WriteLine("Please check the format of the value you have assigned for the [Port_Number] Variable.");
                    Environment.Exit(3);
                }
            }
        }
    }
}
