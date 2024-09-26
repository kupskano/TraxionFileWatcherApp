using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace TraxionFileWatcher
{
    public partial class Service1 : ServiceBase
    {
        private FileSystemWatcher _fileWatcher;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            string folder1 = @"C:\Folder1";
            string folder2 = @"C:\Folder2";

            try
            {
                if (!Directory.Exists(folder1))
                    Directory.CreateDirectory(folder1);

                if (!Directory.Exists(folder2))
                    Directory.CreateDirectory(folder2);

                StartFileWatcher(folder1, folder2);
                LogEvent("FileMoverService started successfully.");
            }
            catch (Exception ex)
            {
                LogEvent($"Error starting service: {ex.Message}");
            }
        }

        protected override void OnStop()
        {
            try
            {
                if (_fileWatcher != null)
                {
                    _fileWatcher.EnableRaisingEvents = false;
                    _fileWatcher.Dispose();
                }
                LogEvent("FileMoverService stopped successfully.");
            }
            catch (Exception ex)
            {
                LogEvent($"Error stopping service: {ex.Message}");
            }
        }

        private void StartFileWatcher(string sourcePath, string destinationPath)
        {
            try
            {
                _fileWatcher = new FileSystemWatcher(sourcePath)
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                    Filter = "*.*"
                };

                _fileWatcher.Created += (sender, e) => OnFileCreated(e, destinationPath);
                _fileWatcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                LogEvent($"Error initializing FileSystemWatcher: {ex.Message}");
            }
        }

        private void OnFileCreated(FileSystemEventArgs e, string destinationPath)
        {
            try
            {
                string sourceFile = e.FullPath;
                string destinationFile = Path.Combine(destinationPath, e.Name);

                System.Threading.Thread.Sleep(1000);

                File.Move(sourceFile, destinationFile);
                LogEvent($"Moved file: {e.Name} to {destinationPath}");
            }
            catch (IOException ioEx)
            {
                LogEvent($"IO error moving file: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                LogEvent($"Access error moving file: {uaEx.Message}");
            }
            catch (Exception ex)
            {
                LogEvent($"Error moving file: {ex.Message}");
            }
        }

        private void LogEvent(string message)
        {
            if (!EventLog.SourceExists("FileMoverService"))
                EventLog.CreateEventSource("FileMoverService", "Application");

            EventLog.WriteEntry("FileMoverService", message, EventLogEntryType.Information);

            Logger.Info(message);
        }

    }
}
