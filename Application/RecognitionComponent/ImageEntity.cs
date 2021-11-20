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
    }
}
