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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ViewModel;
using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;

namespace UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel model = new MainViewModel(new WPFUIServices());
        CancellationTokenSource cts = new CancellationTokenSource();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = model;
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = false;
        }

        private void Button_Select_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                model.UpdateFolder(dialog.SelectedPath);
                model.UpdateImages();
            }
            
        }

        private async void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            cts = new CancellationTokenSource();

            await Task.Factory.StartNew(() => {
                try
                {
                    model.RecognizeOnServer(cts.Token);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            });
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

        private void Button_Stop_Click(object sender, RoutedEventArgs e)
        {
            
            cts.Cancel();
        }

        private void Button_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (databaseListBox.Items.Count != 0)
            {
                model.RemoveFromDatabase();
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
    }
}
