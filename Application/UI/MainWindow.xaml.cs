using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging; // не добавляется в ViewModel
using System.Windows.Navigation;
using System.Windows.Shapes;
using ViewModel;
using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging; // не добавляется в ViewModel
using System.IO;
using System.Threading;
//using System.Drawing;

namespace UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel model = new MainViewModel(new WPFUIServices());
        CancellationTokenSource cts = new CancellationTokenSource();
        //bool isCancelled = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = model;
        }

        private void Button_Select_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                model.UpdateFolder(dialog.SelectedPath);
            }
            //isCancelled = false;
            UpdateImages();
        }

        private async void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            cts = new CancellationTokenSource();
            var results = await Task.Factory.StartNew(() => {
                
                try
                {
                    return model.Recognize(cts.Token);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    return new List<YoloV4Result>();
                }
            });
            DrawResults(results);
            
        }

        private void Button_Stop_Click(object sender, RoutedEventArgs e)
        {
            //isCancelled = true;
            cts.Cancel();
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

        public void UpdateImages()
        {
            resultsListBox.Items.Clear();
            imagesListBox.Items.Clear();
            var bitmaps = model.GetBitmaps();
            foreach (var bitmap in bitmaps)
            {
                imagesListBox.Items.Add(ImageFromBitmap(bitmap.Bitmap));
                
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

        public void DrawResults(List<YoloV4Result> results)
        {
            foreach (var imageInfo in model.GetBitmaps())
            {
                using (var g = Graphics.FromImage(imageInfo.Bitmap))
                {
                    bool haveRecognizedClasses = false;
                    foreach (var res in results)
                    {
                        if (res.FileName.Equals(imageInfo.FileName))
                        {
                            haveRecognizedClasses = true;
                            var x1 = res.BBox[0];
                            var y1 = res.BBox[1];
                            var x2 = res.BBox[2];
                            var y2 = res.BBox[3];
                            g.DrawRectangle(Pens.Red, x1, y1, x2 - x1, y2 - y1);
                            using (var brushes = new SolidBrush(System.Drawing.Color.FromArgb(50, System.Drawing.Color.Red)))
                            {
                                g.FillRectangle(brushes, x1, y1, x2 - x1, y2 - y1);
                            }

                            g.DrawString(res.Label + " " + res.Confidence.ToString("0.00"),
                                 new Font("Arial", 12), System.Drawing.Brushes.Blue, new PointF(x1, y1));
                        }
                    }
                    if (haveRecognizedClasses == true)
                    {
                        resultsListBox.Items.Add(ImageFromBitmap(imageInfo.Bitmap));
                    }
                    
                }
            }
            
        }
    }

    public class WPFUIServices : IUIServices
    {
        public event EventHandler RequerySuggested
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public string SelectFileSave()
        {
            MessageBoxResult result = MessageBox.Show("Изменения будут потеряны. Сохранить изменения?", "", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.Filter = "Text documents (.txt)|*.txt";
                    dlg.CreatePrompt = true;
                    dlg.OverwritePrompt = true;
                    if (dlg.ShowDialog() == true)
                    {
                        string filename = dlg.FileName;
                        return filename;
                    }
                    return "No filename";
                case MessageBoxResult.No:
                    return "No filename";
                default:
                    return "No filename";
            }
        }
        public string SelectFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Text documents (.txt)|*.txt";
            try
            {
                if (dlg.ShowDialog() == true)
                {
                    string filename = dlg.FileName;
                    return filename;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return "Error";
            }
            return "Error";
        }
    }
}
