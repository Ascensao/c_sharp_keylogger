using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Permissions;
using Microsoft.Win32;
using System.Net;
using System.Net.NetworkInformation;

namespace Windows_Driver_Foundation
{
    class classMachineInformation
    {
        public string getMachineName()
        {
            try
            {
                return Environment.MachineName;
            }
            catch { return " "; }
        }

        public string getMachineArq()
        {
            try
            {
                bool is64bit = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"));
                if (is64bit)
                    return "64";
                else
                    return "32";
            }
            catch { return " "; }
        }

        public string getUsername()
        {
            try
            {
                return System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1];
            }
            catch { return " "; }
        }

        public string getUsernameHomeFolder()
        {
            try
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            catch { return " "; }
        }

        public string getWindowsVersion()
        {
            try
            {
                return (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows NT\CurrentVersion", "ProductName", null);
            }
            catch { return " "; }
        }

        public string getWindowsReleaseNumber()
        {
            try
            {
                return Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", "").ToString();
            }
            catch { return " "; }
        }

        public string getDefaultBrowser()
        {
            string progId;
            try
            {
                const string userChoice = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice";
                using (RegistryKey userChoiceKey = Registry.CurrentUser.OpenSubKey(userChoice))
                {
                    if (userChoiceKey == null)
                    {
                        progId = "userchoicekey = null";
                    }
                    else
                    {
                        object progIdValue = userChoiceKey.GetValue("Progid");
                        if (progIdValue == null)
                        {
                            progId = "progid = null";
                        }
                        else
                        {
                            progId = progIdValue.ToString();
                            switch (progId)
                            {
                                case "IE.HTTP":
                                    progId = "Internet Explorer";
                                    break;
                                case "FirefoxURL":
                                    progId = "Firefox";
                                    break;
                                case "ChromeHTML":
                                    progId = "Chrome";
                                    break;
                                case "OperaStable":
                                    progId = "Opera";
                                    break;
                                case "SafariHTML":
                                    progId = "Safari";
                                    break;
                                case "AppXq0fevzme2pys62n3e0fbqa7peapykr8v":
                                    progId = "Edge";
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            catch { progId = " "; }
            return progId;
        }

        public string getMacAddress()
        {
            /// <summary>
            /// Finds the MAC address of the NIC with maximum speed.
            /// </summary>
            /// <returns>The MAC address.</returns>
            //const int MIN_MAC_ADDR_LENGTH = 12;
            string macAddress = string.Empty;
            //long maxSpeed = -1;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                macAddress += "\nFound MAC Address: " + nic.GetPhysicalAddress() +
                "\nType: " + nic.NetworkInterfaceType;
            }

            //se fosse cmd batch script era: msconfig /all   para mostrar mac address
            return macAddress;
        }


    }
}
