using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Driver_Foundation
{
    class classPathsManagement
    {
        // Este método filtra e limpa o path dado pelas ordens de pesquisas de ficheiros
        public string filterAndCleanDBSearchPath(string folderPath)
        {
            // Guarda nesta string o path depois de limpo.
            string pathCleaned = string.Empty;

            // Se o primeiro carácter for '0' então quer dizer que existe um codigo de pasta, no restante caminho guardado na db.
            if (folderPath[0] == '0')
            {
                string folderCode = "0";
                string bonusPath = string.Empty;

                // Exula o código do resto do path (ex: 020) PS: O simbolo '+' faz a divisão.
                folderCode = GetUntilOrEmpty(folderPath, "+");
                if (folderPath != string.Empty)
                {
                    folderCode = folderCode.Remove(0, 1);   // Remove o primeiro char do folderPath "0" (agora fica ex:20).

                    // Guarda o resto do caminho a seguir ao "+" (ex: \teste)
                    bonusPath = folderPath.Split(new string[] { "+" }, StringSplitOptions.None).Last();

                    pathCleaned = convertCodeToDefaultPaths(folderCode) + bonusPath;
                }
            }
            else
                pathCleaned = folderPath;

            return pathCleaned;
        }

        public static string GetUntilOrEmpty(string text, string stopAt)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }
            return String.Empty;
        }


        // Este método converte os código inseridos nos caminhos em path's reais.
        public string convertCodeToDefaultPaths(string code)
        {
            switch (code)
            {
                case "1":
                    return Environment.GetFolderPath(Environment.SpecialFolder.AdminTools);

                case "2":
                    return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                case "3":
                    return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                case "4":
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

                case "5":
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonAdminTools);

                case "6":
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);

                case "7":
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);

                case "8":
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonMusic);

                case "9":
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonOemLinks);

                case "10":
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures);

                case "11":
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms);

                case "12":
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);

                case "13":
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86);

                case "14":
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);

                case "15":
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);

                case "16":
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonTemplates);

                case "17":
                    return Environment.GetFolderPath(Environment.SpecialFolder.CommonVideos);

                case "18":
                    return Environment.GetFolderPath(Environment.SpecialFolder.Cookies);

                case "19":
                    return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                case "20":
                    return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                case "21":
                    return Environment.GetFolderPath(Environment.SpecialFolder.Favorites);

                case "22":
                    return Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

                case "23":
                    return Environment.GetFolderPath(Environment.SpecialFolder.History);

                case "24":
                    return Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);

                case "25":
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

                case "26":
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                case "27":
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

                case "28":
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                case "29":
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

                case "30":
                    return Environment.GetFolderPath(Environment.SpecialFolder.NetworkShortcuts);

                case "31":
                    return Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                case "32":
                    return Environment.GetFolderPath(Environment.SpecialFolder.PrinterShortcuts);

                case "33":
                    return Environment.GetFolderPath(Environment.SpecialFolder.Programs);

                case "34":
                    return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

                case "35":
                    return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                case "36":
                    return Environment.GetFolderPath(Environment.SpecialFolder.Recent);

                case "37":
                    return Environment.GetFolderPath(Environment.SpecialFolder.Resources);

                case "38":
                    return Environment.GetFolderPath(Environment.SpecialFolder.SendTo);

                case "39":
                    return Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);

                case "40":
                    return Environment.GetFolderPath(Environment.SpecialFolder.Startup);

                case "41":
                    return Environment.GetFolderPath(Environment.SpecialFolder.System);

                case "42":
                    return Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);

                case "43":
                    return Environment.GetFolderPath(Environment.SpecialFolder.Templates);

                case "44":
                    return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                case "45":
                    return Environment.GetFolderPath(Environment.SpecialFolder.Windows);

                default:
                    return string.Empty;
            }
        }
    }
}
