using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;


namespace RecognitionComponent
{
    public class ImageInfo
    {
        public Bitmap Bitmap { get; set; }
        public string FileName { get; set; }
        public ImageInfo() { }
        public ImageInfo(Bitmap bitmap, string fileName)
        {
            this.Bitmap = bitmap;
            this.FileName = fileName;
        }
    }
}
