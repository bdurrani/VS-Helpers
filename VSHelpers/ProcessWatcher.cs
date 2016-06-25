using BD.VSHelpers.WMI.Win32;
using System.Diagnostics;
using System.Management;

namespace BD.VSHelpers
{
    /// <summary>
    /// https://weblogs.asp.net/whaggard/438006
    /// </summary>
    public class ProcessWatcher : ManagementEventWatcher
    {
        public delegate void ProcessEventHandler(Win32_Process process);
        // Process Events
        public event ProcessEventHandler ProcessCreated;
        public event ProcessEventHandler ProcessDeleted;
        public event ProcessEventHandler ProcessModified;

        // WMI WQL process query strings
        // http://www.codeproject.com/Articles/46390/WMI-Query-Language-by-Example
        static readonly string WMI_OPER_EVENT_QUERY = @"SELECT * FROM __InstanceOperationEvent WITHIN 5 WHERE TargetInstance ISA 'Win32_Process'";
        static readonly string WMI_OPER_EVENT_QUERY_WITH_PROC = WMI_OPER_EVENT_QUERY + " and TargetInstance.Name = '{0}'";

        private ProcessWatcher()
        {
            Init(string.Empty);
        }

        /// <summary>
        /// Watch for the process named in the parameter in 5 second interval
        /// </summary>
        /// <param name="processName"></param>
        public ProcessWatcher(string processName)
        {
            Init(processName);
        }

        private void watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            string eventType = e.NewEvent.ClassPath.ClassName;
            Win32_Process proc = new Win32_Process(e.NewEvent["TargetInstance"] as ManagementBaseObject);
            Debug.WriteLine(eventType);
            switch (eventType)
            {
                case "__InstanceCreationEvent":
                    if (ProcessCreated != null)
                    {
                        ProcessCreated(proc);
                    }
                    break;
                case "__InstanceDeletionEvent":
                    if (ProcessDeleted != null)
                    {
                        ProcessDeleted(proc);
                    }
                    break;
                case "__InstanceModificationEvent":
                    if (ProcessModified != null)
                    {
                        ProcessModified(proc);
                    }
                    break;
            }
        }

        private void Init(string processName)
        {
            this.Query.QueryLanguage = "WQL";
            if (string.IsNullOrEmpty(processName))
            {
                this.Query.QueryString = WMI_OPER_EVENT_QUERY;
            }
            else
            {
                this.Query.QueryString = string.Format(WMI_OPER_EVENT_QUERY_WITH_PROC, processName);
            }
            this.EventArrived += new EventArrivedEventHandler(watcher_EventArrived);
        }
    }
}
