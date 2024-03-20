using SonarProfileSwitcher.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarProfileSwitcher.Services
{
    public class ProcessServices : IProcessServices
    {
        public bool exeFileExists(string exeFile)
        {
            var process = Process.GetProcessesByName(exeFile);
            return process.Length > 0;
        }

        public Task RunExe(string exeFilePath, string args)
        {
            if (string.IsNullOrEmpty(exeFilePath))
            {
                throw new ArgumentNullException(nameof(exeFilePath), "EXE path cannot be null or empty.");
            }

            Process process = new Process();
            process.StartInfo.FileName = exeFilePath;
            process.StartInfo.CreateNoWindow = true; // Suppress console window
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; // Hide the process window
            process.StartInfo.Arguments = args;
            process.Start();
            Task.Delay(500).Wait();
            return Task.CompletedTask;
        }
    }
}
