using System.ServiceProcess;
using System.Threading;
using TestProjectLib;

namespace WriteService
{
    public partial class WriteFile : ServiceBase
    {
        private Tracker tracker;
        public WriteFile()
        {
            InitializeComponent();
            CanStop = true;
            CanPauseAndContinue = true;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            tracker = new Tracker();
            Thread loggerThread = new Thread(tracker.Start);
            loggerThread.Start();
        }

        protected override void OnStop()
        {

            tracker.Stop();
            Thread.Sleep(1000);
        }
    }
}
