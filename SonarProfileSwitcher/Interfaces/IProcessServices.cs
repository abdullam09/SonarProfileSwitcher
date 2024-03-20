using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarProfileSwitcher.Interfaces
{
    public interface IProcessServices
    {
        bool exeFileExists(string exeFile);
        Task RunExe(string exeFilePath, string args);
    }
}
