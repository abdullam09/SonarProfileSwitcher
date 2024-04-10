using SonarProfileSwitcher.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SonarProfileSwitcher.Services
{
    public class KeyboardLayoutService : IKeyboardLayoutService
    {
        private int _currentLayout = 1033;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hwnd, IntPtr proccess);

        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint thread);

        public async Task<string> CheckKeyboardLayout()
        {
            IntPtr foregroundWindow = GetForegroundWindow();
            uint foregroundProcess = GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
            int keyboardLayout = GetKeyboardLayout(foregroundProcess).ToInt32() & 0xFFFF;
            if (keyboardLayout == 0)
            {
                keyboardLayout = 1033;
            }
            if (_currentLayout != keyboardLayout)
            {
                _currentLayout = keyboardLayout;
            }
           
            return await MapKeyboardLayoutToString(_currentLayout);
        }

        private async Task<string> MapKeyboardLayoutToString(int keyboardLayoutId)
        {
            CultureInfo cultureInfo = new CultureInfo(keyboardLayoutId);
            await Task.CompletedTask;
            return cultureInfo.TwoLetterISOLanguageName;
        }
    }
}
