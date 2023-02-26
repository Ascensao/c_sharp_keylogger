using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Windows.Forms; //para apagar após testes

namespace Windows_Driver_Foundation
{
    class classExeptionAndErrorManagement
    {
        private static string monitor_file_path = "console.txt";
        public void exeptionHandling(string msg)
        {
            //Console.WriteLine("[ERROR EXEPTION!] " + msg);
            //MessageBox.Show("[ERROR EXEPTION!]: " + msg);
            if(File.Exists(monitor_file_path))
                File.AppendAllText(monitor_file_path,"\n\n[ERROR EXEPTION!] " + msg);
        }

        //Just for debugging and check wdf actions
        public void printConsole(string local, string msg)
        {
            //Console.WriteLine("[" + local + "]: " + msg);
            //MessageBox.Show("[REPORT]: " + msg);
            if(File.Exists(monitor_file_path))
                File.AppendAllText(monitor_file_path, "\n\n[Log] " + msg);
        }

        public bool IsNullOrEmpty(Array array)
        {
            return (array == null || array.Length == 0);
        }
    }
}
