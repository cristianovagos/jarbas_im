using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using HWND = System.IntPtr;

namespace Test1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Show Start Menu
            //ShowStartMenu();

            // Lock Windows Session
            //LockWorkStation();

            // Get all active windows
            GetActiveWindows();

            // Switch focus to other process by name
            //FocusProcess();

            // Desligar PC
            //ShutdownPC();

            // Iniciar Calculadora
            //Process.Start("calc.exe");

            // Iniciar Browser
            //Process.Start("www.ua.pt");

            // Obter percentagem de bateria
            //PowerStatus p = SystemInformation.PowerStatus;
            //int a = (int)(p.BatteryLifePercent * 100);
            //MessageBox.Show("" + a);

            // Obter data e hora
            //MessageBox.Show("" + DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year);
            //MessageBox.Show("" + DateTime.Now.Hour + ":" + DateTime.Now.Minute);

            // Abrir app de meteorologia em Aveiro
            //Process.Start(@"msnweather://forecast?la=40.6442700&lo=-8.6455400");

            // Iniciar Cliente Email
            //Process.Start("mailto:");
            //Process.Start("mailto:cristianovagos@ua.pt,gpatricio@ua.pt?Subject=O Jarbas envia mails&Body=Queremos ter boa nota");
        }

        private static void ShowStartMenu()
        {
            // key down event:
            const byte keyControl = 0x11;
            const byte keyEscape = 0x1B;
            keybd_event(keyControl, 0, 0, UIntPtr.Zero);
            keybd_event(keyEscape, 0, 0, UIntPtr.Zero);

            // key up event:
            const uint KEYEVENTF_KEYUP = 0x02;
            keybd_event(keyControl, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(keyEscape, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static void GetActiveWindows()
        {
            Process[] processlist = Process.GetProcesses();

            foreach (Process process in processlist)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    Console.WriteLine("Process: {0} ID: {1} Window title: {2}", process.ProcessName, process.Id, process.MainWindowTitle);
                    if (process.ProcessName == "explorer")
                    {
                    // Fechar processo
                    //process.Kill();

                        



                    //SetFocus(new HandleRef(null, process.MainWindowHandle));
                    //Console.WriteLine("Process: {0} ID: {1} Window title: {2}", process.ProcessName, process.Id, process.MainWindowTitle);
                    //Console.ReadLine();
                    }
                }
            }
        }

        [DllImport("user32.dll")]
        internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow); //ShowWindow needs an IntPtr

        private static void FocusProcess()
        {
            IntPtr hWnd; //change this to IntPtr
            Process[] processRunning = Process.GetProcesses();
            foreach (Process pr in processRunning)
            {
                Console.WriteLine("Process: {0} ", pr.ProcessName);
                if (pr.ProcessName == "explorer")
                {
                    //hWnd = pr.MainWindowHandle; //use it as IntPtr not int
                    pr.Kill();
                    //ShowWindow(hWnd, 3);
                    //SetForegroundWindow(hWnd); //set to topmost
                }
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr SetFocus(HandleRef hWnd);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags,
           UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern bool LockWorkStation();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        



        

        public static IDictionary<HWND, string> GetOpenWindows()
        {
            HWND shellWindow = GetShellWindow();
            Dictionary<HWND, string> windows = new Dictionary<HWND, string>();

            EnumWindows(delegate (HWND hWnd, int lParam)
            {
                if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;

                StringBuilder builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);

                windows[hWnd] = builder.ToString();
                return true;

            }, 0);

            return windows;
        }

        private delegate bool EnumWindowsProc(HWND hWnd, int lParam);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(HWND hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();



        private static void ShutdownPC()
        {
            var psi = new ProcessStartInfo("shutdown", "/s /t 0");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }

    }
}
