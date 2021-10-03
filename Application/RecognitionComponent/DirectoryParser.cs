using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace RecognitionComponent
{
    class DirectoryParser
    {
        public static string[] Parse(string directoryPath)
        {
            return Directory.GetFiles(directoryPath).Select(f => Path.GetFileName(f)).ToArray();
        }
    }
}
