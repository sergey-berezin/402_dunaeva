using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecognitionComponent
{
    public class BBox
    {
        public int BBoxId { get; set; }
        public float X1 { get; set; }
        public float Y1 { get; set; }
        public float X2 { get; set; }
        public float Y2 { get; set; }
        public int ResultEntityId { get; set; }
        public ResultEntity ResultEntity { get; set; }
    }
}
