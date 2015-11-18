using System;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Gpio;
using Windows.UI.Core;

namespace Beats_UWP
{

    public sealed partial class MainPage : Page
    {
        private const int LED_PIN = 12;
        private GpioPin ledPin;
        private GpioPinValue ledPinValue = GpioPinValue.High;

        public MainPage()
        {
            this.InitializeComponent();

            if (Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.Devices.DevicesLowLevelContract", 1))
            {
                InitGPIO();
            }
        }

        private void InitGPIO()
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

            ledPinValue = (ledPinValue == GpioPinValue.Low) ?
            GpioPinValue.High : GpioPinValue.Low;
            ledPin.Write(ledPinValue);
        }

    }
}
