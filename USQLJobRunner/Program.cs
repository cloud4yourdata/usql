using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USQLJobRunner.Data;

namespace USQLJobRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var clientId = "<ADF-CLientId>";
            var subscription = "<SubscriptionID>";
            var secretKey = "<ADF-CLient-SecredKey>";
            var adlaAccountName = "adlademos";
            var aldProc = new ADLAProcess(subscription, adlaAccountName);
            aldProc.OnJobExecute += AldProc_OnJobExecute;
            aldProc.Login(clientId, secretKey);

            var usqlScript = @"  DECLARE @startDate DateTime = new DateTime(2017, 11, 20, 05, 00, 00);
                                            DECLARE @endDate DateTime = new DateTime(2017, 11, 21, 07, 00, 00);
                                            DemoPoC.dbo.usp_ComputeSummaryReport
                                            (
                                                @startDate,
                                                @endDate
                                            );";
            aldProc.RunUSqlJob("test", usqlScript, 2);
            Console.ReadKey();
        }

        private static void AldProc_OnJobExecute(object sender, JobExectionProgressEvent e)
        {
            Console.WriteLine($"{ e.JobName} : {e.JobState} - {e.TotalCompilationTime} - {e.TotalPauseTime} - {e.TotalQueuedTime} - {e.TotalRunningTime}");
        }
    }
}
