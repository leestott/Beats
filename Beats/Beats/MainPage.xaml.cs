using System;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Gpio;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json.Linq;

namespace Beats
{

    public sealed partial class MainPage : Page
    {
        private const int LED_PIN = 12;
        private GpioPin ledPin;

        public MainPage()
        {
            this.InitializeComponent();

            if (Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.Devices.DevicesLowLevelContract", 1))
            {
                InitGPIO();
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
                int a = await GetHeartRate();
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

    }
}

