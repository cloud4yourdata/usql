using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USQLJobRunner.Data
{
    public class JobExectionProgressEvent:EventArgs
    {
        public Guid JobId { get; set; }
        public string JobName { get; set; }
        public string JobState { get; set; }
        public TimeSpan? TotalCompilationTime { get; set; }
        public TimeSpan? TotalPauseTime { get; set; }
        public TimeSpan? TotalQueuedTime { get; set; }
        public TimeSpan? TotalRunningTime { get; set; }
    }
}
