using System;
using System.Collections.Generic;
using System.IO;
using SeratoCrateWrapper;

namespace SeratoCrateWrapperExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please enter the path to the folder you want to add: ");

            var folderPath = Console.ReadLine();

            if (Directory.Exists(folderPath))
            {
                //Standard path where Serato searches for .crate files, place it here so it will automatically be discovered the next time you start Serato
                var seratoPath = Path.GetPathRoot(folderPath) + "_Serato_" + Path.DirectorySeparatorChar + "Subcrates";

                if (!Directory.Exists(seratoPath))
                {
                    Directory.CreateDirectory(seratoPath);
                }

                //Create an empty crate
                var crate = SeratoCrate.CreateNew();

                
                //Get all filePaths of .mp3 files in the target folder
                foreach (var filePath in Directory.GetFiles(folderPath, "*.mp3", SearchOption.AllDirectories))
                {
                    //Serato only needs the path relative to the drive root, since every drive has its own _Serato_ folder.
                    var songLocation = filePath.Substring(filePath.IndexOf(Path.VolumeSeparatorChar) + 1).Replace(@"\\", @"\").Replace(@"\", "/");

                    while (songLocation.StartsWith("/"))
                    {
                        songLocation = songLocation.Substring(1);
                    }

                    //Add the file to the crate
                    crate.Songs.Add(songLocation);
                }

                var crateName = Path.GetDirectoryName(folderPath);
                crateName = crateName.Substring(crateName.LastIndexOf(Path.DirectorySeparatorChar)).Replace(@"\", "");
                //Write the crate, for nesting etc look at the XML comments in the sourcecode.
                crate.WriteFile(seratoPath + Path.DirectorySeparatorChar + crateName + ".crate");
            }
            else
            {
                Console.WriteLine("This path does not exist.");
            }
        }
    }
}
