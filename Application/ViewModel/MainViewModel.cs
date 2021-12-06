using System;
using System.Collections.Generic;
using System.Drawing;
using RecognitionComponent;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Media.Imaging;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

namespace ViewModel
{
    public interface IUIServices
    {
        event EventHandler RequerySuggested;
    }
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel(IUIServices svc)
        {
            Images = new();
            ResultsList = new();
            DatabaseList = new();
            GetDatabaseImages();
        }

        private string serverAvailable;
        public string ServerAvailable {
            get => serverAvailable;
            set
            {
                serverAvailable = value;
                RaisePropertyChanged("ServerAvailable");
            }
        }

        private Folder selectedFolder;
        public Folder SelectedFolder
        {
            get => selectedFolder;
            set
            {
                selectedFolder = value;
                RaisePropertyChanged("SelectedFolder");
            }
        }

        public void UpdateFolder(string newFolderPath)
        {
            SelectedFolder = new Folder();
            SelectedFolder.UpdatePath(newFolderPath);
        }

        public ObservableCollection<System.Windows.Controls.Image> Images { get; set; }
        public ObservableCollection<System.Windows.Controls.Image> ResultsList { get; set; }
        public ObservableCollection<System.Windows.Controls.Image> DatabaseList { get; set; }
        public void RecognizeOnServer(CancellationToken token)
        {
            if (token.IsCancellationRequested) return;
            ServerAvailable = "";

            string[] fileNames = DirectoryParser.Parse(SelectedFolder.FolderPath);
            var tasks = new Task[fileNames.Length];
            for (int i = 0; i < fileNames.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew(async pi => {
                    if (token.IsCancellationRequested) return;
                    int idx = (int)pi;

                
                    bool isExists = false;
                    HttpClient client = new();
                    var bitmap = new Bitmap(Image.FromFile(Path.Combine(SelectedFolder.FolderPath, fileNames[idx])));
                    var bytes = ImageToByte(bitmap);

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(bytes);
                    var data = new System.Net.Http.StringContent(json, Encoding.Default, "application/json");
                    HttpResponseMessage response = new();
                    try
                    {
                        response = await client.PostAsync("http://localhost:5000/api/images/recognize", data);
                    }
                    catch (Exception e)
                    {
                        ServerAvailable = "Server is unavailable";
                        return;
                    }
                    
                    if (response.IsSuccessStatusCode && (int)response.StatusCode != 201)
                    {
                        isExists = true;
                    }
                    else if (!response.IsSuccessStatusCode)
                    {
                        return;
                    }
                    string result = await response.Content.ReadAsStringAsync();
                    var results = JsonConvert.DeserializeObject<RecognitionComponent.YoloV4Result[]>(result)
                        .ToList<RecognitionComponent.YoloV4Result>();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var bitmapWithResults = DrawResultsOneImage(bitmap, results);
                        ResultsList.Add(ImageFromBitmap(bitmapWithResults));

                        if (!isExists)
                        {
                            DatabaseList.Add(ImageFromBitmap(bitmapWithResults));
                        }
                    });
                }, i, token);
            }
            Task.WaitAll(tasks);
        }
        public void Recognize(CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            Task task = Task.Factory.StartNew(() =>
            {
                ImageRecognition.ImageRecognize(SelectedFolder.FolderPath, token);
            });

            bool isStarted = false;
            List<RecognitionComponent.YoloV4Result> resultsInFile = new();
            string currentFileName = "";
            while(true)
            {
                
                RecognitionComponent.YoloV4Result result;
                var watch = System.Diagnostics.Stopwatch.StartNew();
                while (!ImageRecognition.resultsQueue.TryDequeue(out result)) 
                {
                    if (isStarted && (watch.ElapsedMilliseconds > 5000)) break;
                }
                isStarted = true;
                watch.Stop();
                
                if (result != null)
                {
                    if (currentFileName.Equals(""))
                    {
                        currentFileName = result.FileName;
                        resultsInFile.Add(result);
                    }
                    else if (currentFileName.Equals(result.FileName))
                    {
                        resultsInFile.Add(result);
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            DrawResults(resultsInFile, currentFileName);
                        });
                        resultsInFile = new();
                        currentFileName = result.FileName;
                        resultsInFile.Add(result);
                    }
                    
                }
                else
                {
                    if (resultsInFile.Count != 0)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            DrawResults(resultsInFile, currentFileName);
                        });
                    }
                    break;
                }
            }
        }

        public List<ImageInfo> GetBitmaps()
        {
            if (SelectedFolder != null)
            {
                return SelectedFolder.GetBitmaps();
            }
            else
            {
                return new List<ImageInfo>();
            }
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        public System.Windows.Controls.Image ImageFromBitmap(Bitmap bitmap)
        {
            return new System.Windows.Controls.Image
            {
                Source = ToBitmapImage(bitmap),
                Width = 300
            };

        }

        public void UpdateImages()
        {
            Images.Clear();
            ResultsList.Clear();
            var bitmaps = GetBitmaps();
            foreach (var bitmap in bitmaps)
            {
                Images.Add(ImageFromBitmap(bitmap.Bitmap));
            }
        }

        public Bitmap DrawResultsOneImage(Bitmap bitmap, List<RecognitionComponent.YoloV4Result> results)
        {
            using (var g = Graphics.FromImage(bitmap))
            {
                foreach (var result in results)
                {
                    var x1 = result.BBox[0];
                    var y1 = result.BBox[1];
                    var x2 = result.BBox[2];
                    var y2 = result.BBox[3];
                    g.DrawRectangle(Pens.Red, x1, y1, x2 - x1, y2 - y1);
                    using (var brushes = new SolidBrush(System.Drawing.Color.FromArgb(50, System.Drawing.Color.Red)))
                    {
                        g.FillRectangle(brushes, x1, y1, x2 - x1, y2 - y1);
                    }

                    g.DrawString(result.Label + " " + result.Confidence.ToString("0.00"),
                    new Font("Arial", 12), System.Drawing.Brushes.Blue, new PointF(x1, y1));
                }
                return bitmap;
            }
        }

        public void DrawResults(List<RecognitionComponent.YoloV4Result> results, string fileName)
        {
            ImageInfo imageInfo = new();
            foreach (var imInfo in GetBitmaps())
            { 
                if (imInfo.FileName.Equals(fileName))
                {
                    imageInfo = imInfo;
                    break;
                }
            }
            using (var db = new ImageContext())
            {
                using (var g = Graphics.FromImage(imageInfo.Bitmap))
                {
                    bool haveRecognizedClasses = false;
                    bool isExists = false;
                    ImageEntity imageEntity = new ImageEntity() { };
                    
                    var currentImage = ImageToByte(imageInfo.Bitmap);
                    int currentHash = ImageEntity.ComputeHashCode(currentImage);

                    var queryHash = db.ImageEntities.Where(entity => entity.HashCode == currentHash);

                    if (queryHash.Any())
                    {
                        foreach (var entity in queryHash)
                        {
                            if (Enumerable.SequenceEqual(entity.Image, currentImage)) isExists = true;
                        }
                    }

                    foreach (var result in results)
                    {
                        if (result.FileName.Equals(imageInfo.FileName))
                        {
                            haveRecognizedClasses = true;
                            var x1 = result.BBox[0];
                            var y1 = result.BBox[1];
                            var x2 = result.BBox[2];
                            var y2 = result.BBox[3];
                            g.DrawRectangle(Pens.Red, x1, y1, x2 - x1, y2 - y1);
                            using (var brushes = new SolidBrush(System.Drawing.Color.FromArgb(50, System.Drawing.Color.Red)))
                            {
                                g.FillRectangle(brushes, x1, y1, x2 - x1, y2 - y1);
                            }

                            g.DrawString(result.Label + " " + result.Confidence.ToString("0.00"),
                                 new Font("Arial", 12), System.Drawing.Brushes.Blue, new PointF(x1, y1));

                            if (!isExists)
                            {
                                BBox box = new BBox() { X1 = result.BBox[0], Y1 = result.BBox[1], 
                                X2 = result.BBox[2], Y2 = result.BBox[3]};
                                ResultEntity resultEntity = new ResultEntity() { Confidence = result.Confidence, 
                                Label = result.Label };
                                box.ResultEntity = resultEntity;
                                resultEntity.BBox = box;
                                imageEntity.Image = currentImage;
                                imageEntity.HashCode = currentHash;
                                imageEntity.Results.Add(resultEntity);
                                db.BBoxes.Add(box);
                                db.ResultEntities.Add(resultEntity);
                            }
                        }
                    }
                    if (haveRecognizedClasses == true)
                    {
                        ResultsList.Add(ImageFromBitmap(imageInfo.Bitmap));
                    }
                    if (!isExists)
                    {
                        db.ImageEntities.Add(imageEntity);
                        db.SaveChanges();
                        DatabaseList.Add(ImageFromBitmap(imageInfo.Bitmap));
                    }   
                }
            }

        }

        public async void GetDatabaseImages()
        {
            ServerAvailable = "";
            DatabaseList.Clear();

            HttpClient client = new HttpClient();
            string resultJson = "";
            try
            {
                resultJson = await client.GetStringAsync("http://localhost:5000/api/images");
            }
            catch (Exception e)
            {
                ServerAvailable = "Server is unavailable";
                return;
            }
            
            var allimages = JsonConvert.DeserializeObject<ImageEntity[]>(resultJson);
            foreach (var imageEntity in allimages)
            {
                var ms = new MemoryStream(imageEntity.Image);
                Bitmap bitmap = new Bitmap(ms);
                string resultEntitiesJson = "";
                try
                {
                    resultEntitiesJson = await client.GetStringAsync(string.Concat("http://localhost:5000/api/images/results/", imageEntity.ImageEntityId));
                }
                catch (Exception e)
                {
                    ServerAvailable = "Server is unavailable";
                    return;
                }
                
                var resultEntities = JsonConvert.DeserializeObject<ResultEntity[]>(resultEntitiesJson);
                var results = new List<RecognitionComponent.YoloV4Result>();
                foreach (var res in resultEntities)
                {
                    string bboxJson = "";
                    try
                    {
                        bboxJson = await client.GetStringAsync(string.Concat("http://localhost:5000/api/images/box/", res.ResultEntityId));
                    }
                    catch (Exception e)
                    {
                        ServerAvailable = "Server is unavailable";
                        return;
                    }
                    
                    BBox box = JsonConvert.DeserializeObject<BBox>(bboxJson);
                    float[] bbox = new float[] { box.X1, box.Y1, box.X2, box.Y2 };
                    RecognitionComponent.YoloV4Result result = new(bbox, res.Label, res.Confidence, "");
                    results.Add(result);
                }
                DrawResultsOnImage(bitmap, results);
            }
        }

        public byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public void DrawResultsOnImage(Bitmap bitmap, List<RecognitionComponent.YoloV4Result> results)
        {
            using (var g = Graphics.FromImage(bitmap))
            {
                foreach (var result in results)
                {
                    var x1 = result.BBox[0];
                    var y1 = result.BBox[1];
                    var x2 = result.BBox[2];
                    var y2 = result.BBox[3];
                    g.DrawRectangle(Pens.Red, x1, y1, x2 - x1, y2 - y1);
                    using (var brushes = new SolidBrush(System.Drawing.Color.FromArgb(50, System.Drawing.Color.Red)))
                    {
                        g.FillRectangle(brushes, x1, y1, x2 - x1, y2 - y1);
                    }

                    g.DrawString(result.Label + " " + result.Confidence.ToString("0.00"),
                    new Font("Arial", 12), System.Drawing.Brushes.Blue, new PointF(x1, y1));
                }
                DatabaseList.Add(ImageFromBitmap(bitmap));
            }
        }

        public async void RemoveFromDatabase()
        {
            ServerAvailable = "";
            HttpClient client = new HttpClient();
            try
            {
                await client.DeleteAsync("http://localhost:5000/api/images/clean");
                DatabaseList.Clear();
            }
            catch (Exception e)
            {
                ServerAvailable = "Server is unavailable";
            }
        }
    }
}
