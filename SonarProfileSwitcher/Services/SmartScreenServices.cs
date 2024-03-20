using SonarProfileSwitcher.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarProfileSwitcher.Services
{
    public class SmartScreenServices : ISmartScreenServices
    {
        private readonly IProcessServices _processServices;
        private readonly string exeFilePath = @"F:\Sonar Auto Switch\SmartScreen\TuringSmartScreenTool.exe";
        public SmartScreenServices(IProcessServices processServices)
        {
            _processServices = processServices;
        }

        public async Task Print(string profileName)
        {
            await ChangeScreenOri("p");
            await ClearScreen();
            await ChangeScreenOri("l");
            await ShowTextOnScreen("Active Profile");
            await ShowActiveProfileOnScreen(profileName);
        }

        private async Task ChangeScreenOri(string ori)
        {
            await _processServices.RunExe(exeFilePath, $"orientation -r a -p com3 -m {ori}");
        }

        private async Task ClearScreen()
        {
            await _processServices.RunExe(exeFilePath, "clear -r a -p com3");
        }

        private async Task ShowTextOnScreen(string text)
        {
            await _processServices.RunExe(exeFilePath, $"text -r a -p com3 -t {text.Replace(" ", "_")}_ -x 5 -y 50 -s 40 -b FFFFFF -c AA4AA4 -f \"aero matics display\"");
        }

        private async Task ShowActiveProfileOnScreen(string profileName)
        {
            var modifiedText = profileName.Replace(" ", "_");
            await _processServices.RunExe(exeFilePath, $"text -r a -p com3 -t {modifiedText}_ -x 5 -y 130 -s 25 -b FFFFFF -c 0000FF -f \"aero matics display\"");
        }
    }
}
