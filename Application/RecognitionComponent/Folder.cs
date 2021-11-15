using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Collections.Specialized;

namespace RecognitionComponent
{
    public class Folder : INotifyPropertyChanged, INotifyCollectionChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;


        private string folderPath;
        public string FolderPath
        {
            get => folderPath;
            set
            {
                folderPath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FolderPath"));
            }
        }

        private List<YoloV4BitmapData> images;
        public List<YoloV4BitmapData> Images
        {
            get => images;
            set
            {
                images = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Images"));
            }
        }

        public void UpdatePath(string newFolderPath)
        {
            FolderPath = newFolderPath;
            SetImages();
        }

        public void SetImages()
        {
            Images = new List<YoloV4BitmapData>();
            var fileNames = DirectoryParser.Parse(FolderPath);
            foreach (string fileName in fileNames)
            {
                var bitmap = new Bitmap(Image.FromFile(Path.Combine(FolderPath, fileName)));
                YoloV4BitmapData yoloV4BitmapData = new YoloV4BitmapData();
                yoloV4BitmapData.Image = bitmap;
                Images.Add(yoloV4BitmapData);
                CollectionChanged?.Invoke(Images, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, yoloV4BitmapData));
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Images"));
        }

        public List<ImageInfo> GetBitmaps()
        {
            var bitmaps = new List<ImageInfo>();
            var fileNames = DirectoryParser.Parse(FolderPath);
            foreach (string fileName in fileNames)
            {
                var bitmap = new Bitmap(Image.FromFile(Path.Combine(FolderPath, fileName)));
                bitmaps.Add(new ImageInfo(bitmap, fileName));
            }
            return bitmaps;
        }
    }
}
