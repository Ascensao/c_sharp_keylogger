using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Windows_Driver_Foundation
{
    class classProcessDownloadFiles
    {
        private static classMySQLDatabase classMysql;
        private static classFTPManagement ftpManagement;
        private static classExeptionAndErrorManagement classExeption;

        public void execute_downloadsFiles_protocol(int uid)
        {
            classExeption = new classExeptionAndErrorManagement();
            classMysql = new classMySQLDatabase();

            classMysql.openDatabaseConnection();

            string[] ftpLogin = classMysql.read_tblFTPServers(uid);

            if (!classExeption.IsNullOrEmpty(ftpLogin))
            {
            
                List<int> pending_downloads = classMysql.getIds_OfDownloadsFiles(uid);

                if (pending_downloads.Any())
                {
                    //Calling FTP
                    ftpManagement = new classFTPManagement(ftpLogin[1], ftpLogin[2], ftpLogin[3]);
                    classMysql.insert_server_log(Convert.ToInt32(ftpLogin[0]), uid, 1); //Status = FTP class acquired
                    foreach (int id_selected in pending_downloads)
                    {
                        string[] row_fields = classMysql.read_tblDownloadFilesFields(id_selected);

                        /* tbl_downloads_files GUIDE
                         * row_fields[0] = id
                         * row_fields[1] = client_id
                         * row_fields[2] = server_path
                         * row_fields[3] = local_path
                         * row_fields[4] = execute_order ( 0 = no execute, 1 = execute, 2 = execute and shutdown WDF)
                         * row_fields[5] = status_date 
                         * row_fields[6] = order_status ( 0 = none, 1 = pending, 2 = readed, 3 = downloaded, 4 = executed, 5 = download fail 6 = app execution fail) */

                        classExeption.printConsole("execute_downloadFiles_protocol()", "Id Selected: " + id_selected.ToString() + "     Server File: " + row_fields[3]);

                        classMysql.update_DownloadStatus(id_selected, 2); //Download Lido
                        classExeption.printConsole("execute_downloadFiles_protocol()", "oder_satus updated to readed in DB.");

                        if (ftpManagement.download(row_fields[2], row_fields[3]))
                        {         
                            if (File.Exists(row_fields[3]))
                            {
                                classMysql.insert_server_log(Convert.ToInt32(ftpLogin[0]), uid, 4);
                                classMysql.update_DownloadStatus(id_selected, 3);
                                classExeption.printConsole("execute_downloadFiles_protocol()", "Ficheiro transferido: " + row_fields[3]);

                                if ((row_fields[4] == "1") || row_fields[4] == "2")
                                {
                                    try
                                    {
                                        System.Diagnostics.Process.Start(row_fields[3]);
                                        classMysql.update_DownloadStatus(id_selected, 4);
                                        if (row_fields[4] == "2") 
                                            Environment.Exit(0);
                                    }
                                    catch (Exception ex)
                                    {
                                        classMysql.update_DownloadStatus(id_selected, 6); //execução falhou
                                        classExeption.exeptionHandling(ex.ToString());
                                    }
                                }
                            }
                            else
                            {
                                classMysql.update_DownloadStatus(id_selected, 5); //download falhou
                                classMysql.insert_server_log(Convert.ToInt32(ftpLogin[0]), uid, 40);
                            }
                        }
                        else
                        {
                            classMysql.update_DownloadStatus(id_selected, 5); //download falhou
                            classMysql.insert_server_log(Convert.ToInt32(ftpLogin[0]), uid, 40);
                        }
                    }
                }else
                    classExeption.printConsole("execute_downloadFiles_protocol()", "Não existe ficheiro para Download");
            }
            else
                classExeption.printConsole("execute_downloadFiles_protocol()", "No FTP Credentials: Download Files");

            classMysql.closeDatabaseConnection();
        }


    }
}
