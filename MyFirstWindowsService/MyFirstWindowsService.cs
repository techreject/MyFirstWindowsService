using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel.Design;
using System.Dynamic;
using System.Threading;
// 03/31/23 - Installed System.Threading.ThreadPool through Nuget Package Manager. Added it to references
using System.Timers;

namespace MyFirstWindowsService
{
    public partial class MyFirstWindowsService : ServiceBase
    {
        // 03/31/23 - Created an EventLog variable type to hold my EventLog object. This will be used to write
        // logs to the EventLog for this service
        private EventLog MyFirstWindowsServiceEventLog;
        // 03/31/23 - identifies the next event number to write into the log
        private int eventId = 1;

        // 03/31/23 - My modified the name of the service. This was done from the windows service file and all those files underneath.
        public MyFirstWindowsService()
        {
            InitializeComponent();

            // 03/31/23 - Create an EventLog object
            MyFirstWindowsServiceEventLog = new System.Diagnostics.EventLog();

            // 03/31/23 - Determine if the EventLog named after my service already exists
            if (!System.Diagnostics.EventLog.SourceExists("MyFirstWindowsService"))
            {
                // 03/31/23 - If there is NOT an EventLog named after my Service, create one
                System.Diagnostics.EventLog.CreateEventSource("MyFirstWindowsService", "MyFirstWindowsServiceLog");
            }
            // 03/31/23 - Set the name of where the EventLog entries are coming from and the name of the log
            // In the EventLog service
            MyFirstWindowsServiceEventLog.Source = "MyFirstWindowsService";
            MyFirstWindowsServiceEventLog.Log = "MyFirstWindowsServiceLog";
        }

        // 03/31/2023 - Visual Studio automatically created an empty method definition when you created the project
        // Because a service application is designed to be long-running, it usually polls or monitors the system,
        // which you set up in the OnStart method. The OnStart method must return to the operating system after the
        // service's operation has begun so that the system isn't blocked.
        protected override void OnStart(string[] args)
        {
            // 03/31/2023 - Write an entry to EventViewer stating the service has started
            MyFirstWindowsServiceEventLog.WriteEntry("In OnStart.");
            // 03/31/2023 - Because a service application is designed to be long-running, it usually polls or monitors
            // the system, which you set up in the OnStart method.The OnStart method must return to the operating system
            // after the service's operation has begun so that the system isn't blocked.
            // 03/31/2023 - To set up a simple polling mechanism, use the System.Timers.Timer component. The timer raises
            // an Elapsed event at regular intervals, at which time your service can do its monitoring.You use the Timer
            // component as follows:
            //
            // Set the properties of the Timer component in the MyNewService.OnStart method.
            // Start the timer by calling the Start method.
            // Set up a timer that triggers every minute.
            System.Timers.Timer timer = new System.Timers.Timer();

            // 60 seconds
            timer.Interval = 60000; 
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        protected override void OnStop()
        {
            // 03/31/2023 - Write an entry to EventViewer stating the service has stopped
            MyFirstWindowsServiceEventLog.WriteEntry("In OnStop.");
        }

        // 03/31/23 - Instead of running all your work on the main thread, you can run tasks by using background worker threads.
        //  For more information, see https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.backgroundworker
        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
            MyFirstWindowsServiceEventLog.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);
        }
        protected override void OnContinue()
        {
            // 03/31/2023 - Write an entry to EventViewer
            // https://learn.microsoft.com/en-us/dotnet/api/system.serviceprocess.servicebase.oncontinue
            MyFirstWindowsServiceEventLog.WriteEntry("In OnContinue.");
        }
        protected override void OnShutdown()
        {
            // 03/31/2023 - Write an entry to EventViewer
            // https://learn.microsoft.com/en-us/dotnet/api/system.serviceprocess.servicebase.onshutdown
            MyFirstWindowsServiceEventLog.WriteEntry("In OnShutdown.");
        }

        protected override void OnPause()
        {
            // 03/31/2023 - Write an entry to EventViewer
            // https://learn.microsoft.com/en-us/dotnet/api/system.serviceprocess.servicebase.onpause
            MyFirstWindowsServiceEventLog.WriteEntry("In OnPause.");
        }
    }
}
