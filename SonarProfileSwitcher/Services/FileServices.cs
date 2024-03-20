using SonarProfileSwitcher.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace SonarProfileSwitcher.Services
{
    public class FileServices : IFileServices
    {
        public string ReadFile(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            throw new FileNotFoundException();
        }
    }
}
