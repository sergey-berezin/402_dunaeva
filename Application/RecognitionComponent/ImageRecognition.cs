using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML;
using Microsoft.ML.OnnxRuntime;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;
using System.Collections.Concurrent;


namespace RecognitionComponent
{
    public class ImageRecognition
    {
        const string modelPath = @"C:\Users\Nastya\source\repos\MachineLearning\yolov4.onnx";

        static readonly string[] classesNames = new string[] { "person", "bicycle", "car", "motorbike", "aeroplane", "bus", "train", "truck", "boat", "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "sofa", "pottedplant", "bed", "diningtable", "toilet", "tvmonitor", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush" };

        public static ConcurrentStack<YoloV4Result> ImageRecognize(string imageFolder)
        {
            var yoloV4Results = new ConcurrentStack<YoloV4Result>();
            MLContext mlContext = new MLContext();

            var pipeline = mlContext.Transforms.ResizeImages(inputColumnName: "bitmap", outputColumnName: "input_1:0", imageWidth: 416, imageHeight: 416, resizing: ResizingKind.IsoPad)
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input_1:0", scaleImage: 1f / 255f, interleavePixelColors: true))
                .Append(mlContext.Transforms.ApplyOnnxModel(
                    shapeDictionary: new Dictionary<string, int[]>()
                    {
                        { "input_1:0", new[] { 1, 416, 416, 3 } },
                        { "Identity:0", new[] { 1, 52, 52, 3, 85 } },
                        { "Identity_1:0", new[] { 1, 26, 26, 3, 85 } },
                        { "Identity_2:0", new[] { 1, 13, 13, 3, 85 } },
                    },
                    inputColumnNames: new[]
                    {
                        "input_1:0"
                    },
                    outputColumnNames: new[]
                    {
                        "Identity:0",
                        "Identity_1:0",
                        "Identity_2:0"
                    },
                    modelFile: modelPath, recursionLimit: 100));

            var model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));

            var predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);

            var sw = new Stopwatch();
            sw.Start();

            string[] fileNames = DirectoryParser.Parse(imageFolder);
            var tasks = new Task<Bitmap>[fileNames.Length];
            
            ConcurrentDictionary<string, Bitmap> bitmaps = new ConcurrentDictionary<string, Bitmap>();

            for (int i = 0; i < fileNames.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew<Bitmap>(pi =>
                {
                    int idx = (int)pi;  
                    var bitmap = new Bitmap(Image.FromFile(Path.Combine(imageFolder, fileNames[idx])));
                    return bitmap;
                }, i);
                bitmaps.GetOrAdd(fileNames[i], tasks[i].Result);
                
            }
            
            Task[] tasksResult = new Task[bitmaps.Count];
            int j = 0;
            foreach (var bm in bitmaps)
            {
                var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bm.Value });

                tasksResult[j] = Task.Factory.StartNew(() => {
                    var results = predict.GetResults(classesNames, bm.Key , 0.3f, 0.7f);
                    
                    foreach (var res in results)
                    {
                        yoloV4Results.Push(res);
                    }
                });
                j++;    
            }
            Task.WaitAll(tasksResult);

            sw.Stop();
            Console.WriteLine($"Done in {sw.ElapsedMilliseconds}ms.");
            return yoloV4Results;
        }
    }
}
