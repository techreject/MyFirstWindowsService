using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.ComponentModel.Design;
using System.Dynamic;
using System.Threading;
// 03/31/23 - Installed System.Threading.ThreadPool through Nuget Package Manager. Added it to references
using System.Timers;
// 04/13/23 - Allows interactions with the Service Control Manager to set status/states for the service
using System.Runtime.InteropServices;
using System.Runtime;

namespace MyFirstWindowsService
{
    // 04/13/23 - Services report their status to the Service Control Manager so that a user can tell whether a
    // service is functioning correctly. By default, a service that inherits from ServiceBase reports a limited
    // set of status settings, which include SERVICE_STOPPED, SERVICE_PAUSED, and SERVICE_RUNNING. If a service
    // takes a while to start up, it's useful to report a SERVICE_START_PENDING status.
    //
    // You can implement the SERVICE_START_PENDING and SERVICE_STOP_PENDING status settings by adding code that
    // calls the Windows SetServiceStatus function.
    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    // 04/13/23 - The Service Control Manager uses the dwWaitHint and dwCheckpoint members of the SERVICE_STATUS
    // structure to determine how much time to wait for a Windows service to start or shut down. If your OnStart
    // and OnStop methods run long, your service can request more time by calling SetServiceStatus again with an
    // incremented dwCheckPoint value.
    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    };

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

        // 04/13/23 - allows you to declare the SetServiceStatus function by using platform invoke
        // 
        // More on SetServiceStatus at https://learn.microsoft.com/en-us/windows/desktop/api/winsvc/nf-winsvc-setservicestatus
        //
        // More on platform invoke at https://learn.microsoft.com/en-us/dotnet/framework/interop/consuming-unmanaged-dll-functions

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        //04/13/23 - ADd service startup parameters - see https://learn.microsoft.com/en-us/dotnet/framework/windows-services/walkthrough-creating-a-windows-service-application-in-the-component-designer#optional-set-startup-parameters

        // 03/31/2023 - Visual Studio automatically created an empty method definition when you created the project
        // Because a service application is designed to be long-running, it usually polls or monitors the system,
        // which you set up in the OnStart method. The OnStart method must return to the operating system after the
        // service's operation has begun so that the system isn't blocked.
        protected override void OnStart(string[] args)
        {
            // 03/31/2023 - Write an entry to EventViewer stating the service has started
            MyFirstWindowsServiceEventLog.WriteEntry("In OnStart.");

            // 04/13/23 - Update the service state to Start Pending. This gives the time for the
            // service to start and notifies the user of the status of the service as it starts
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            // 04/13/23 - Logging the service start is pending
            MyFirstWindowsServiceEventLog.WriteEntry("Set Service Status to pending...");
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

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

            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            // 04/13/23 - Logging the service start is running
            MyFirstWindowsServiceEventLog.WriteEntry("Set Service Status to running...");
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
            // 03/31/2023 - Write an entry to EventViewer stating the service has stopped
            MyFirstWindowsServiceEventLog.WriteEntry("In OnStop.");

            // Update the service state to Stop Pending. This gives the time for the
            // service to stop and notifies the user of the status of the service as it starts
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            // 04/13/23 - Logging the service is attempting to stop, but taking time
            MyFirstWindowsServiceEventLog.WriteEntry("Set Service Status to pending...");
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            
            // Update the service state to Stopped.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            // 04/13/23 - Logging the service start is stopped
            MyFirstWindowsServiceEventLog.WriteEntry("Set Service Status to stopped...");
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
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
