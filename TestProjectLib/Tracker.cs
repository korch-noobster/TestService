using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestProjectLib
{
    public class Tracker
    {

        private readonly FileSystemWatcher _watcher;
        private static readonly object LockObj = new object();
        private bool _enabled = true;
        private readonly DatabaseHelpers _workingWithDb;
        public Tracker()
        {
            _workingWithDb = new DatabaseHelpers();
            _watcher = new FileSystemWatcher("D:\\Values");
            _watcher.Created += Watcher_Created;
        }

        public void Start()
        {
            try
            {
                _watcher.EnableRaisingEvents = true;
                while (_enabled)
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Info(ex.StackTrace);
                throw;
            }
        }
        public void Stop()
        {
            _watcher.EnableRaisingEvents = false;
            _enabled = false;
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            UpdateDbInfo(e.FullPath);
        }
        private void UpdateDbInfo(string filePath)
        {
            Thread.Sleep(180000);
            _workingWithDb.UpdateCurrencies(filePath);
        }
    }
}
