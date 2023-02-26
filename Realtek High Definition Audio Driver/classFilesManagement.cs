using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace Windows_Driver_Foundation
{
    class classFilesManagement
    {
        private static classExeptionAndErrorManagement classExeption;

        //AUXILIARY METHODS

        public List<string> searchDocumentByWord(string folderPath, string docExt, string docWordFilter, string recursively)
        {
            classExeption = new classExeptionAndErrorManagement();
            classExeption.printConsole("searchDocumentByWord()", "SearchDocumentByWord: " + folderPath);

            // Guarda os caminhos dos ficheiros encontrados na pesquisa.
            List<string> searchResult = new List<string>();

            try
            {
                if (docWordFilter != string.Empty)
                {
                    if (recursively == "True")
                    {
                        var files = from file in Directory.EnumerateFiles(folderPath, docExt, SearchOption.AllDirectories)
                                    from line in File.ReadLines(file)
                                    where line.Contains(docWordFilter)
                                    select new
                                    {
                                        File = file,
                                        Name = Path.GetFileName(file),
                                        Line = line
                                    };

                        foreach (var fpath in files)
                        {
                            //searchResult.Add($"{f.File}\t{f.Line}\t{f.Name}");
                            searchResult.Add(fpath.File);
                        }
                    }
                    else
                    {
                        var files = from file in Directory.EnumerateFiles(folderPath, docExt, SearchOption.TopDirectoryOnly)
                                    from line in File.ReadLines(file)
                                    where line.Contains(docWordFilter)
                                    select new
                                    {
                                        File = file,
                                        Name = Path.GetFileName(file),
                                        Line = line
                                    };

                        foreach (var fpath in files)
                        {
                            //searchResult.Add($"{f.File}\t{f.Line}\t{f.Name}");
                            searchResult.Add(fpath.File);
                        }
                    }
                    //Console.WriteLine($"{files.Count().ToString()} files found.");
                }
                else
                {
                    if (recursively == "True")
                    {
                        var files = Directory.EnumerateFiles(folderPath, docExt, SearchOption.AllDirectories);
                        foreach (var fpath in files)
                        {
                            searchResult.Add(fpath);
                        }
                    }
                    else
                    {
                        var files = Directory.EnumerateFiles(folderPath, docExt, SearchOption.TopDirectoryOnly);
                        foreach (var fpath in files)
                        {
                            searchResult.Add(fpath);
                        }
                    }
                    //  var files = Directory.EnumerateFiles(folderPath, docExt, SearchOption.TopDirectoryOnly);
                    //Console.WriteLine($"{files.Count().ToString()} files found.");
                }
            }
            catch (UnauthorizedAccessException uAEx)
            {
                classExeption.exeptionHandling(uAEx.Message);
            }
            catch (PathTooLongException pathEx)
            {
                classExeption.exeptionHandling(pathEx.Message);
            }
            catch (Exception e)
            {
                classExeption.exeptionHandling(e.ToString());
            }
            return searchResult;
        }

        public bool copyFile(string sourcePath, string destPath)
        {
            try
            {
                if (File.Exists(sourcePath))
                    if (Directory.Exists(Path.GetDirectoryName(Path.GetFullPath(destPath))))
                        File.Copy(sourcePath, destPath, true);

                return true;
            }
            catch (Exception ex)
            {
                classExeption = new classExeptionAndErrorManagement();
                classExeption.exeptionHandling(ex.ToString());
                return false;
            }
        }

        public bool createDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
                return true;
            }
            catch { return false; }
        }

        public bool ZipDirectory(string path, string name)
        {
            try
            {
                ZipFile.CreateFromDirectory(path, name + ".Zip");
                if (File.Exists(name + ".Zip"))
                {
                    return true;
                }
                else
                    return false;

            }
            catch { return false; }
        }

        public bool deleteDirectory(string path)
        {
            try
            {
                Directory.Delete(path, true);
                return true;
            }
            catch { return false; }
        }

        public string GenerateName(int len)
        {
            Random r = new Random();
            string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
            string[] vowels = { "a", "e", "i", "o", "u", "y" };
            string Name = "";
            Name += consonants[r.Next(consonants.Length)].ToUpper();
            Name += vowels[r.Next(vowels.Length)];
            int b = 2; //b tells how many times a new letter has been added. It's 2 right now because the first two letters are already in the name.
            while (b < len)
            {
                Name += consonants[r.Next(consonants.Length)];
                b++;
                Name += vowels[r.Next(vowels.Length)];
                b++;
            }
            return Name;
        }


    }
}
