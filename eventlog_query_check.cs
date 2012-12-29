/*===================================================================================================================================================

Application Name: eventlog_entry_check.exe
Application Type: Console Application

Author:           Ryan Irujo
Inception:        12.22.2012
Last Updated:     12.28.2012

Descrption:       Designed to work with Windows Vista on up. Still in Progress....

Changes:          


Syntax:           eventlog_entry_check.exe <Log_Name> <Provider_Name> <Event_ID> <Milliseconds>

Example:          eventlog_entry_check.exe Application VSS 8224 1800000

NSC.ini Format:   command[win_tcp_port_check]=X:\Path\To\Plugins\eventlog_entry_check.exe $ARG1$

NRPE Syntax:      ./check_nrpe -H <hostname> -c eventlog_entry_check -a <Log_Name> <Provider_Name> <Event_ID> <Milliseconds>

   
===================================================================================================================================================*/
using System;
using System.Diagnostics.Eventing.Reader;
using System.Security;

namespace EventQuery
{
    class EventQueryExample
    {
        static void Main(string[] args)
        {
            EventQueryExample ex = new EventQueryExample();

            ex.QueryActiveLog(args);
        }


        public void QueryActiveLog(string[] args)
        {
            try
            {
                // Query a specific event log using an XML structured query.
                // Note: You can create your own sample XML Structured queries by simply opening up Windows Event Viewer on a Windows Host running Windows Vista or Later and 
                //       using the 'Create a Custom View...' option. When you use the 'Filter Custom View' option, the raw XML of your filtered choices will be available
                //       which you can then use in the 'queryString' variable below. Additional inforamtion can be found on Ned Pyle's blogpost below:
                //       http://blogs.technet.com/b/askds/archive/2011/09/27/3455548.aspx

                //string queryString = "*[System[Provider[@Name='VSS']] and TimeCreated[timediff(@SystemTime) &lt;= 86400000]]]";
                //string queryString = "*[System[Provider[@Name='Microsoft-Windows-Security-Auditing']] and System/EventID=4672]";
                //string queryString = "*[System[Provider[@Name='Microsoft-Windows-Security-Auditing'] and (EventID=4672) and TimeCreated[timediff(@SystemTime) &lt;= 3600000]]]";

                string queryString = (
                " <QueryList>" +
                "  <Query Id='0' Path='" + args[0] + "'>" +
                "    <Select Path='" + args[0] + "'>*[System[Provider[@Name='" + args[1] + "'] and (EventID=" + args[2] + ") and TimeCreated[timediff(@SystemTime) &lt;=" + args[3] + "]]]</Select>" +
                "  </Query>" +
                " </QueryList>");


                EventLogQuery eventsQuery = new EventLogQuery(args[0], PathType.LogName, queryString);
                EventLogReader logReader  = new EventLogReader(eventsQuery);

                // Display event info
                DisplayEventLogInformation(logReader);

            }
            catch (Exception Error)
            {
                if (Error is IndexOutOfRangeException)
                {
                    Console.WriteLine("The [Log_Name], [Event_ID] and [Provider_Name] Variables must ALL be present!");
                    Environment.Exit(3);
                }
            }
        }


        private void DisplayEventLogInformation(EventLogReader logReader)
        {

            try{

                for (EventRecord eventInstance = logReader.ReadEvent(); null != eventInstance; eventInstance = logReader.ReadEvent())
                {
                    EventLogRecord logRecord = (EventLogRecord)eventInstance;
                        Console.WriteLine("Event Log: {0} Event ID: {1} ProviderName: {2}  Description: {3}",
                            logRecord.ContainerLog, eventInstance.Id, eventInstance.ProviderName, eventInstance.FormatDescription());
                }
            }   
             catch (EventLogException)
                {
                    Console.WriteLine("Unable to read Event Log Entry Description.");
                }
            }
        }
    }


