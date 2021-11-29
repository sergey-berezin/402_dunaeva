using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecognitionComponent
{
    public class ResultEntity
    {
        public int ResultEntityId { get; set; }
        public BBox BBox { get; set; }
        public string Label { get; set; }
        public float Confidence { get; set; }
        public int ImageEntityId { get; set; }
        public ImageEntity ImageEntity { get; set; } 
    }
}
