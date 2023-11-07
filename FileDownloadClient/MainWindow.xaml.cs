using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.AspNetCore.SignalR.Client;

namespace FileDownloadClient
{
    public partial class MainWindow : Window
    {
        #region FIELDS

        private const string HubName = "SignatureHub"; 
        private const string BroadcastingEvent = "ReceiveMessage"; 

        #endregion

        #region C-TOR

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region EVENTS

        private void BtnConnectClicked(object sender, RoutedEventArgs e)
        {
            var url = this.txtUrl.Text;

            var hubConnection = new HubConnectionBuilder()
                .WithUrl($"{url}{HubName}")
                .Build();

            hubConnection.On<string>(BroadcastingEvent, filePath =>
            {
                if (filePath == null) return;

                using (var client = new WebClient())
                {
                    var file = client.DownloadData($"{filePath}");
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.StreamSource = new System.IO.MemoryStream(file);
                    img.EndInit();
                    img.Freeze();

                    ExecuteOrInvoke(() => { SetImage(img); });
                }
            });

            Task.Run(async () =>
                {
                    try
                    {
                        await hubConnection.StartAsync();
                    }
                    catch (Exception exception)
                    {
                        Debugger.Break();
                    }
                })
                .ContinueWith(t =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        this.txtUrl.IsEnabled = false;
                        this.btnConnect.IsEnabled = false;
                        this.tbOutput.Text = "Connection established ...";
                    });
                });
        }

        #endregion

        #region HELPERS

        private void SetImage(BitmapImage img)
        {
            this.imgImage.Source = img;
        }

        private void ExecuteOrInvoke(Action action)
        {
            if (CheckAccess()) action();
            else this.Dispatcher.BeginInvoke(action);
        }

        #endregion
    }
}
