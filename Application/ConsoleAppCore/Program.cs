using System;
using RecognitionComponent;

namespace ConsoleAppCore
{
    class Program
    {
        static void Main(string[] args)
        {
            var results = ImageRecognition.ImageRecognize(args[0]);
            YoloV4Result res;
            string curFileName = "";
            while (results.TryPop(out res))
            {
                if (!res.FileName.Equals(curFileName))
                {
                    Console.WriteLine(res.FileName);
                    curFileName = res.FileName;
                } 
                
                var x1 = res.BBox[0];
                var y1 = res.BBox[1];
                var x2 = res.BBox[2];
                var y2 = res.BBox[3];
                Console.WriteLine($"    {res.Label} in a rectangle between ({x1:0.0}, {y1:0.0}) and ({x2:0.0}, {y2:0.0}) with probability {res.Confidence.ToString("0.00")}");
                
            }
        }
    }
}
