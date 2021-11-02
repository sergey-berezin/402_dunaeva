using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Drawing;
using RecognitionComponent;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace ViewModel
{
    public interface IUIServices
    {
        string SelectFileSave();
        string SelectFile();
        event EventHandler RequerySuggested;
    }
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel(IUIServices svc)
        {
            
        }
        public static ConcurrentQueue<YoloV4Result> resultsQueueVM = new ConcurrentQueue<YoloV4Result>();

        private bool isSelected;
        private bool isRunning = false;

        private Folder selectedFolder;
        public Folder SelectedFolder
        {
            get => selectedFolder;
            set
            {
                selectedFolder = value;
                isSelected = true;
                RaisePropertyChanged("SelectedFolder");
            }
        }

        public void UpdateFolder(string newFolderPath)
        {
            SelectedFolder = new Folder();
            SelectedFolder.UpdatePath(newFolderPath);
        }


        public List<YoloV4Result> Recognize(CancellationToken token)
        {
            if (token.IsCancellationRequested) return new List<YoloV4Result>();
            isRunning = true;
            var task = Task.Factory.StartNew(() =>
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                ImageRecognition.ImageRecognize(SelectedFolder.FolderPath, token);
            }, token);
            
            List<YoloV4Result> results = new List<YoloV4Result>();
            int length = DirectoryParser.Parse(SelectedFolder.FolderPath).Length;
            Task[] tasks = new Task[6];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew(() => {
                    RecognitionComponent.YoloV4Result result;
                    while (!ImageRecognition.resultsQueue.TryDequeue(out result)) { }
                    results.Add(new YoloV4Result(result));
                    
                });
                
            }
            task.Wait();
            Task.WhenAll(tasks);
            isRunning = false;
            
            return results;
            
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

    }
}
