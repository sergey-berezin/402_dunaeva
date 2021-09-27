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

        static async Task<IReadOnlyList<YoloV4Result>> processImageAsync(string imageName, string imageFolder, PredictionEngine<YoloV4BitmapData, YoloV4Prediction> predictionEngine)
        {
            return await Task.Factory.StartNew(() => {
                using (var bitmap = new Bitmap(Image.FromFile(Path.Combine(imageFolder, imageName))))
                {
                    var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
                    var results = predict.GetResults(classesNames, 0.3f, 0.7f);
                    return results;
                }
            });
        }
        public static void imageRecognition(string imageFolder)
        {
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

            string[] fileNames = DirectoryParser.parse(imageFolder);
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
            Task.WaitAll(tasks);

            Task[] tasksResult = new Task[bitmaps.Count];
            int j = 0;
            foreach (var bm in bitmaps)
            {
                var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bm.Value });

                tasksResult[j] = Task.Factory.StartNew(() => {
                    var results = predict.GetResults(classesNames, 0.3f, 0.7f);
                    Console.WriteLine(bm.Key);
                    
                    foreach (var res in results)
                    {
                        var x1 = res.BBox[0];
                        var y1 = res.BBox[1];
                        var x2 = res.BBox[2];
                        var y2 = res.BBox[3];
                        Console.WriteLine($"    {res.Label} in a rectangle between ({x1:0.0}, {y1:0.0}) and ({x2:0.0}, {y2:0.0}) with probability {res.Confidence.ToString("0.00")}");
                    }
                });
                j++;    
            }
            Task.WaitAll(tasksResult);

            sw.Stop();
            Console.WriteLine($"Done in {sw.ElapsedMilliseconds}ms.");
        }
    }
}
