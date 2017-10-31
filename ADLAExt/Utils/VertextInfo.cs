using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;

namespace ADLAExt.Utils
{
    public static class VertextInfo
    {
        public static string GetInfo()
        {
            var info = "";
            var procQuery = "SELECT Name,NumberOfCores FROM Win32_Processor";
            var pObjectSearchersearcher = new ManagementObjectSearcher(procQuery);
            foreach (ManagementObject WniPART in pObjectSearchersearcher.Get())
            {
                info =
                    $"Processor:{WniPART.Properties["Name"].Value};NumberOfCores:{WniPART.Properties["NumberOfCores"].Value};";
            }

            var Query = "SELECT Capacity FROM Win32_PhysicalMemory";
            var searcher = new ManagementObjectSearcher(Query);
            var totalMemorySizeInGb = (from ManagementObject wniPart in searcher.Get()
                                       select Convert.ToUInt64(wniPart.Properties["Capacity"].Value)
                into sizeinByte
                                       select sizeinByte / 1024 / 1024 / 1024).Aggregate<ulong, ulong>(0, (current, sizeinGb) => current + sizeinGb);
            info += $"RAM Size in GB: {totalMemorySizeInGb}";
            return info;
        }

        public static string GetFullInfo()
        {
            var sb = new StringBuilder();
            var myManagementClass = new
                ManagementClass("Win32_OperatingSystem");
            var myManagementCollection =
                myManagementClass.GetInstances();
            var myProperties =
                myManagementClass.Properties;
            var myPropertyResults =
                new Dictionary<string, object>();

            foreach (var obj in myManagementCollection)
            {
                foreach (var myProperty in myProperties)
                {
                    myPropertyResults.Add(myProperty.Name,
                        obj.Properties[myProperty.Name].Value);
                }
            }

            foreach (var myPropertyResult in myPropertyResults)
            {
                var item = $"{myPropertyResult.Key}:{myPropertyResult.Value}";
                sb.AppendLine(item);
            }
            return sb.ToString();
        }

        public static string GetVMInfo()
        {
            var sb = new StringBuilder();
            var myManagementClass = new
                ManagementClass("Win32_PerfRawData_Counters_HyperVDynamicMemoryIntegrationService");
            var myManagementCollection =
                myManagementClass.GetInstances();
            var myProperties =
                myManagementClass.Properties;
            var myPropertyResults =
                new Dictionary<string, object>();

            foreach (var obj in myManagementCollection)
            {
                foreach (var myProperty in myProperties)
                {
                    myPropertyResults.Add(myProperty.Name,
                        obj.Properties[myProperty.Name].Value);
                }
            }

            foreach (var myPropertyResult in myPropertyResults)
            {
                var item = $"{myPropertyResult.Key}:{myPropertyResult.Value}";
                sb.AppendLine(item);
            }
            return sb.ToString();
        }
    }
}