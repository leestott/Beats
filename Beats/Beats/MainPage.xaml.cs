using System;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Gpio;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json.Linq;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

namespace Beats
{

    public sealed partial class MainPage : Page
    {
        private const int LED_PIN = 12;
        private GpioPin ledPin;

        public MainPage()
        {
            this.InitializeComponent();

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Devices.GpioController"))
            {
                InitGPIO();
            }

            sendToastNotification("This is a test toast notification");
        }

        private async void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                System.Diagnostics.Debug.WriteLine("There are no GPIO controller on this device.");
                return;
            }

            ledPin = gpio.OpenPin(LED_PIN);

            // Initialize LED to the OFF state by first writing a HIGH value
            ledPin.Write(GpioPinValue.High);
            ledPin.SetDriveMode(GpioPinDriveMode.Output);

            System.Diagnostics.Debug.WriteLine("GPIO pins initialized correctly.");

            while (true)
            {
                int a = await GetHeartRate();

                // Fire notification on heart rate threshold exceeded
                if(a > 70)
                {
                    sendToastNotification("Woahh steady on horsey... you feeling ok?");
                }

                await ChangeLEDSpeed(a);
            }
                
        }

        public async static Task<int> GetHeartRate()
        {
            var http = new HttpClient();
            var url = "http://reasonsapi.azurewebsites.net/api/SentimentData/beats";
            var response = await http.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();

            JToken token = JObject.Parse(result);

            int beat = (int)token.SelectToken("AverageSentiment");

            return beat;

        }

        private async Task ChangeLEDSpeed(int heartRate)
        {
            float secondBeats = (4.0f / heartRate) * 20.0f;
            ledPin.Write(GpioPinValue.Low);
            await Task.Delay(TimeSpan.FromSeconds(secondBeats));
            ledPin.Write(GpioPinValue.High);
            await Task.Delay(TimeSpan.FromSeconds(secondBeats));
        }

        private void sendToastNotification(string msg)
        {
            var xmlDoc = ToastService.CreateToast(msg);
            var notifier = ToastNotificationManager.CreateToastNotifier();
            var toast = new ToastNotification(xmlDoc);
            notifier.Show(toast);
        }

        private void sendTileNotifications()
        {
            // Load the string into an XmlDocument
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("someXML");

            // Then create the tile notification
            var notification = new TileNotification(doc);

            // And send the notification
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
        }

    }
}

