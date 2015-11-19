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
using Windows.UI.Xaml;
using System.Collections.Generic;

namespace Beats
{

    public sealed partial class MainPage : Page
    {
        private const int LED_PIN = 12;
        private GpioPin ledPin;
        private int bpm = 70;


        public MainPage()
        {
            this.InitializeComponent();

            /*
                Using IsApiContractPresent (below) does not seem to work on the RPi (it crashes). And when using the IsTypePresent, 
                when running the app on the PC it still runs the GPIO method, even though it is not a GPIO device.

                 Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.Devices.Gpio.GpioController", 1)

                Uncomment or comment sections of the code below to get this to work. This is not ideal but I will update the code when
                I find a solution.

            */


            /* For the above reason, uncomment this if you are running on the RPi, and comment this out if running on anything else */
            //if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Devices.Gpio.GpioController"))
            //{
            //    InitGPIO();
            //}

            /* Uncomment this if running on PC/Mobile, comment this out if running on RPi */
            //InitApp();             
        }

        private async void InitApp()
        {
            /* JS websites/webapps do not update when run through webview which is currently an issue - you just get a static page */
            UIwebview.Navigate(new Uri("http://beats.azurewebsites.net/"));
            
            bpm = await GetHeartRate();

            TileService.SetBadgeCountOnTile(bpm);
            UpdatePrimaryTile(new PrimaryTile());

            while (true)
            {
                bpm = await GetHeartRate();

                // Fire notification on heart rate threshold exceeded
                if (bpm > 80)
                {
                    sendToastNotification("Woahh steady on horsey... you feeling ok?");
                    PrimaryTile tile = new PrimaryTile();
                    tile.message = "You're pumped!";
                    UpdatePrimaryTile(tile);
                }
                else if (bpm < 50)
                {
                    sendToastNotification("Wakey Wakey you Sloth!");
                    PrimaryTile tile = new PrimaryTile();
                    tile.message = "You may be dying?!";
                    UpdatePrimaryTile(tile);

                    TileService.SetBadgeCountOnTile(bpm);
                }
            }
           
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
                bpm = await GetHeartRate();
                await ChangeLEDSpeed(bpm);
            }
                
        }

        public async static Task<int> GetHeartRate()
        {
            var http = new HttpClient();
            var url = "http://beatsapi.azurewebsites.net/api/SentimentData/beats";
            var response = await http.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();

            JToken token = JObject.Parse(result);

            int beat = (int)token.SelectToken("Heart");

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

        private void UpdatePrimaryTile(PrimaryTile tile)
        {
            var xmlDoc = TileService.CreateTiles(tile);

            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            var notification = new TileNotification(xmlDoc);
            updater.Update(notification);
        }

        private void test(object sender, RoutedEventArgs e)
        {
            
        }
    }
}

