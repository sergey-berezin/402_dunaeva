using System;
using RecognitionComponent;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ConsoleAppCore
{
    class Program
    {
        static void Main(string[] args)
        {
            
                ImageRecognition.ImageRecognize(args[0]);
        }
    }
}
