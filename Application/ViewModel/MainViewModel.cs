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
                    if (isStarted && (watch.ElapsedMilliseconds > 3000)) break;
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
                    // смотрим была ли уже такая картинка в бд
                    // если была, флаг в true
                    var currentImage = ImageToByte(imageInfo.Bitmap);
                    var query = db.ImageEntities.Where(entity => entity.Image.Equals(currentImage));
                    if (query.Count() != 0)
                    {
                        isExists = true;
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
                                X2 = result.BBox[3], Y2 = result.BBox[4]};
                                ResultEntity resultEntity = new ResultEntity() { Confidence = result.Confidence, 
                                Label = result.Label };
                                box.ResultEntity = resultEntity;
                                resultEntity.BBox = box;
                                imageEntity.Image = ImageToByte(imageInfo.Bitmap);
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
                    }   
                }
            }

        }

        public void GetDatabaseImages()
        {

        }

        public byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        /*public void DrawResult(RecognitionComponent.YoloV4Result result)
        {
            foreach (var imageInfo in GetBitmaps())
            {
                using (var g = Graphics.FromImage(imageInfo.Bitmap))
                {
                    bool haveRecognizedClasses = false;
                    
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
                        }
                    
                    if (haveRecognizedClasses == true)
                    {
                        ResultsList.Add(ImageFromBitmap(imageInfo.Bitmap));
                    }

                }
            }

        }*/
    }
}
