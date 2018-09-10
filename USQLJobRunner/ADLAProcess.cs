using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.Management.DataLake.Analytics;
using Microsoft.Azure.Management.DataLake.Store;
using Microsoft.Azure.Management.DataLake.Analytics.Models;
using USQLJobRunner.Data;

namespace USQLJobRunner
{
    public delegate void JobExecutionEventHandler(object sender, JobExectionProgressEvent e);
    public class ADLAProcess
    {
        private Uri _adlTokenAudience;
        private string _subscription;
        private string _adlaAccountName;
        private ServiceClientCredentials _serviceClientCredentials;
        private DataLakeAnalyticsCatalogManagementClient _adlaCatalogClient;
        private DataLakeAnalyticsJobManagementClient _adlaJobClient;
        private DataLakeStoreFileSystemManagementClient _adlsFileSystemClient;
        public event JobExecutionEventHandler OnJobExecute;
        public ADLAProcess(string subscription, string adlaAccountName)
        {
            _subscription = subscription;
            _adlaAccountName = adlaAccountName;
            _adlTokenAudience = new Uri(@"https://datalake.azure.net/");
        }

        public void Login(string clientId, string clientSecretKey)
        {
            _serviceClientCredentials = GetCredsServicePrincipalSecretKey(_subscription, _adlTokenAudience, clientId, clientSecretKey);
            _adlaCatalogClient = new DataLakeAnalyticsCatalogManagementClient(_serviceClientCredentials);
            _adlaJobClient = new DataLakeAnalyticsJobManagementClient(_serviceClientCredentials);
            _adlsFileSystemClient = new DataLakeStoreFileSystemManagementClient(_serviceClientCredentials);

        }

        public Guid RunUSqlJob(string jobName, string uSqlScript, int degreeOfParallelism = 5)
        {
            var adlaJob = new JobInformation();
            adlaJob.Type = JobType.USql;
            adlaJob.Name = jobName;
            adlaJob.Properties = new USqlJobProperties();
            adlaJob.Properties.Script = uSqlScript;
            adlaJob.DegreeOfParallelism = degreeOfParallelism;
            USqlJobProperties p;
            var runningJob = _adlaJobClient.Job.Create(_adlaAccountName, Guid.NewGuid(), adlaJob);
            if (OnJobExecute != null)
            {

                while (runningJob.State != JobState.Ended)
                {
                    runningJob = _adlaJobClient.Job.Get(_adlaAccountName, runningJob.JobId.Value);
                    p = (USqlJobProperties)runningJob.Properties;
                    if (p != null)
                    {
                        var e = new JobExectionProgressEvent
                        {
                            JobId = runningJob.JobId.Value,
                            JobName = runningJob.Name,
                            JobState = runningJob.State.Value.ToString(),
                            TotalCompilationTime = p.TotalCompilationTime,
                            TotalPauseTime = p.TotalPauseTime,
                            TotalQueuedTime = p.TotalQueuedTime,
                            TotalRunningTime = p.TotalRunningTime
                        };
                        OnJobExecute(this, e);
                    }
                    Thread.Sleep(1000);
                }
                          }
            return runningJob.JobId.Value;
        }

        private static ServiceClientCredentials GetCredsServicePrincipalSecretKey(string domain, Uri tokenAudience, string clientId, string secretKey)
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            var serviceSettings = ActiveDirectoryServiceSettings.Azure;
            serviceSettings.TokenAudience = tokenAudience;

            var creds = ApplicationTokenProvider.LoginSilentAsync(domain, clientId, secretKey, serviceSettings).GetAwaiter().GetResult();

            return creds;
        }

    }
}
