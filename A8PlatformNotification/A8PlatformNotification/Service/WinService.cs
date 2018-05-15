using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using A8PlatformNotification.Common;

namespace A8PlatformNotification.Service
{
    class WinService
    {

        public ILog Log { get; private set; }

        private ProcessNotification _processNotification;
        private Task _task; 
        public WinService(ILog logger)
        {

            // IocModule.cs needs to be updated in case new paramteres are added to this constructor

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            Log = logger;
            _processNotification = new ProcessNotification(Log);

        }

        public bool Start(HostControl hostControl)
        {

            Log.Info($"{nameof(Service.WinService)} Start command received.");
            //Task.Factory.StartNew(() => { _processNotification.StartProcess(); });
                _task = new Task(new Action(_processNotification.StartProcess));
                _task.Start();
            Log.Info($"{nameof(Service.WinService)} Started successfully.");
            return true;

        }

        public bool Stop(HostControl hostControl)
        {

            Log.Trace($"{nameof(Service.WinService)} Stop command received.");
            
            _processNotification.ContinueProcessing = false;
            try
            {
                _task.Dispose();
            }
            catch (Exception ignore)
            {
            }
            return true;

        }

        public bool Pause(HostControl hostControl)
        {

            Log.Trace($"{nameof(Service.WinService)} Pause command received.");

            //TODO: Implement your service start routine.
            return true;

        }

        public bool Continue(HostControl hostControl)
        {

            Log.Trace($"{nameof(Service.WinService)} Continue command received.");

            //TODO: Implement your service stop routine.
            return true;

        }

        public bool Shutdown(HostControl hostControl)
        {

            Log.Trace($"{nameof(Service.WinService)} Shutdown command received.");

            _processNotification.ContinueProcessing = false;
            _processNotification = null;
            return true;

        }

    }
}
