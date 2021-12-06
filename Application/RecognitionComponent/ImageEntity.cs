using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RecognitionComponent
{
    public class ImageEntity
    {
        public int ImageEntityId { get; set; }
        public byte[] Image { get; set; }
        public List<ResultEntity> Results { get; } = new List<ResultEntity>();
        public int HashCode { get; set; }

        public static int ComputeHashCode(byte[] arr)
        {
            int hash = 0;
            if (arr == null) return 0;
            foreach (var b in arr)
            {
                hash = (hash + b.GetHashCode()) % int.MaxValue; 
            }
            return hash;
        }

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
    }
}
