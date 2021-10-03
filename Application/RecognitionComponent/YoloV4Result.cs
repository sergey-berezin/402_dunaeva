using System;
using System.Collections.Generic;
using System.Text;

namespace RecognitionComponent
{
    public class YoloV4Result
    {
        /// <summary>
        /// x1, y1, x2, y2 in page coordinates.
        /// <para>left, top, right, bottom.</para>
        /// </summary>
        public float[] BBox { get; }

        /// <summary>
        /// The Bbox category.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Confidence level.
        /// </summary>
        public float Confidence { get; }

        public string FileName { get; }

        public YoloV4Result(float[] bbox, string label, float confidence, string fileName)
        {
            BBox = bbox;
            Label = label;
            Confidence = confidence;
            FileName = fileName;
        }
    }
}
