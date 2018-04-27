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

        FileSystemWatcher watcher;
        private static readonly object LockObj = new object();
        bool enabled = true;
        private readonly DatabaseHelpers workingWithDb;
        public Tracker()
        {
            workingWithDb = new DatabaseHelpers();
            watcher = new FileSystemWatcher("D:\\Values");
            watcher.Created += Watcher_Created;
            Logger.Log.Info("hello");

        }

        public void Start()
        {
            try
            {
                Logger.Log.Info("hello2");
                watcher.EnableRaisingEvents = true;
                while (enabled)
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
            Logger.Log.Info("hello3");
            watcher.EnableRaisingEvents = false;
            enabled = false;
        }

        // создание файлов
        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            Logger.Log.Info(e.FullPath);
            UpdateDbInfo(e.FullPath);
        }
        private void UpdateDbInfo(string filePath)
        {
            Logger.Log.Info("it alive");
            Thread.Sleep(180000);
            workingWithDb.UpdateCurrencies(filePath);
        }
    }
}
