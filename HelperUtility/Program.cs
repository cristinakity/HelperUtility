using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Configuration;
using System.Xml;
using System.Reflection;
using HelperUtility.Helpers;

namespace HelperUtility
{
    static class Program
    {
        const uint VK_MENU = 18;//virtual key code of Alt key
        const uint VK_SNAPSHOT = 44;//virtual key code of Snapshot key
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        public static Keys startKey = Keys.P;
        public static Keys stopKey = Keys.O;
        public static int seconds = 10;
        public static int mouseSteps = 100;
        private static bool ctrlPressed;
        private static bool ctrlAltPressed;
        private static bool isStarted;
        const string startConfigKey = "startKey";
        const string stopConfigKey = "stopKey";
        const string secondsConfigKey = "secondsKey";
        const string mouseStepsConfigKey = "mouseStepsConfigKey";
        private static bool isPositive = true;
        //Timer
        static System.Timers.Timer aTimer;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            GetAppConfigKeys();
            _hookID = SetHook(_proc);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new HelperUtility(startKey, stopKey, seconds));
            UnhookWindowsHookEx(_hookID);
        }
        
        static void aTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("Click..");
            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftDown);
            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp);

            MouseOperations.MousePoint point = MouseOperations.GetCursorPosition();
            point.X = point.X +(isPositive ? mouseSteps : -1*mouseSteps);
            //point.Y = point.Y +(isPositive ? mouseSteps : -mouseSteps);
            isPositive = !isPositive;
            MouseOperations.SetCursorPosition(point);
        }

        private static void GetAppConfigKeys()
        {
            string startKeyValue =ConfigurationManager.AppSettings[startConfigKey];
            string stopKeyValue =ConfigurationManager.AppSettings[stopConfigKey];
            string secondsKeyValue =ConfigurationManager.AppSettings[secondsConfigKey];
            string mouseStepsKeyValue = ConfigurationManager.AppSettings[mouseStepsConfigKey];
            startKey = (Keys)Enum.Parse(typeof(Keys), startKeyValue, true);
            stopKey = (Keys)Enum.Parse(typeof(Keys), stopKeyValue, true);
            Int32.TryParse(secondsKeyValue,out seconds);
            seconds = seconds == 0 ? 10 : seconds;
            Int32.TryParse(mouseStepsKeyValue, out mouseSteps);
            mouseSteps = mouseSteps == 0 ? 100 : mouseSteps;
        }

        public static void SetAppConfigKeys()
        {
            ChangeValueByKey(startConfigKey, startKey.ToString());
            ChangeValueByKey(stopConfigKey, stopKey.ToString());
            ChangeValueByKey(secondsConfigKey, seconds.ToString());
        }

        public static void ChangeValueByKey(string key, string value)
        {
            Configuration configuration = ConfigurationManager.
            OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save();
            ConfigurationManager.RefreshSection("appSettings");
        }


        public static void Start(bool startChild)
        {
            if (!isStarted)
            {
                Console.WriteLine("STARTED...");
                var interval = (seconds == 0 ? 1 : seconds) * 1000;
                aTimer = new System.Timers.Timer(interval);
                aTimer.Elapsed += aTimer_Elapsed;
                aTimer.Interval = interval;
                aTimer.Enabled = true;
                aTimer.Start();
                isStarted = true;

                if (startChild)
                {
                    StartChild();
                }

            }
        }

        private static void StartChild()
        {
            var from = Application.OpenForms?[0];
            if (from != null)
            {
                var hu = (from as HelperUtility);

                hu?.Start();
            }
        }

        public static void Stop(bool stopChild)
        {
            if (isStarted)
            {
                Console.WriteLine("STOPED");
                aTimer.Stop();
                isStarted = false;
                if (stopChild)
                {
                    StopChild();
                }
            }
        }
        private static void StopChild()
        {
            var from = Application.OpenForms?[0];
            if (from != null)
            {
                var hu = (from as HelperUtility);

                hu?.Stop();
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                var keyPressed = KeyInterop.KeyFromVirtualKey(vkCode);

                if (vkCode == (int)Keys.LControlKey || vkCode == (int)Keys.RControlKey ) //162 is Left Ctrl, 163 is Right Ctrl
                {
                    ctrlPressed = true;
                    //Console.WriteLine("ctrlPressed");
                }
                else if ((vkCode == (int)Keys.LMenu || vkCode == (int)Keys.RMenu) && ctrlPressed)
                {
                    ctrlPressed = false;
                    ctrlAltPressed = true;
                    //Console.WriteLine("ctrlAltPressed");
                }
                else if (ctrlAltPressed ) 
                {                     
                    if ((Keys)vkCode == stopKey && isStarted)
                    {
                        Console.WriteLine(stopKey + " STOPED");
                        Stop(true);
                        isStarted = false;

                    }
                    
                    if((Keys)vkCode == startKey  && !isStarted)
                    {
                        Console.WriteLine(startKey + " STARTED...");
                        Start(true);
                        isStarted = true;
                    }
                    
                    ctrlPressed = false;
                    ctrlAltPressed = false;
                    //Console.WriteLine("Bingo!");
                }
                else
                {
                    ctrlPressed = false;
                    ctrlAltPressed = false;
                }
                

                //Console.WriteLine((Keys)vkCode);
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
