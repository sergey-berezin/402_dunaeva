using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace RecognitionComponent
{
    class DirectoryParser
    {
        public static string[] parse(string directoryPath)
        {
            List<string> listOfFiles = new List<string>();
            Directory.GetFiles(directoryPath).ToList().ForEach(f => listOfFiles.Add(Path.GetFileName(f)));
            return listOfFiles.ToArray();
        }
    }
}
