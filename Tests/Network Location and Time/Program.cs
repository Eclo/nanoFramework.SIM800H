using Eclo.nanoFramework.SIM800H;
using SIM800HSamples;
using System;
using System.Threading;
using Windows.Devices.Gpio;
using Windows.Devices.SerialCommunication;

namespace Network_Location_and_Time
{
    public class Program
    {
        static SerialDevice _serialDevice;
        private const string APNConfigString = "net2.vodafone.pt|vodafone|vodafone";
        private const string mmsApnConfigString = "vas.vodafone.pt|vas|vas";

        public static void Main()
        {
            InitializeSIM800H();

            // loop forever and output available RAM each 5 seconds
            while (true)
            {
                Thread.Sleep(5000);

                // output signal RSSI
                Console.WriteLine("Network signal strength is " + Eclo.nanoFramework.SIM800H.SIM800H.RetrieveSignalStrength().GetSignalStrengthDescription());
            };
        }

        public static void InitializeSIM800H()
        {
            // initialization of the module is very simple 
            // we just need to pass a serial port and an output signal to control the "power key" signal

            // open COM
            _serialDevice = SerialDevice.FromId("COM2");

            // SIM800H signal for "power key"
            GpioPin sim800PowerKey = GpioController.GetDefault().OpenPin(0 * 1 + 10, GpioSharingMode.Exclusive);
            sim800PowerKey.SetDriveMode(GpioPinDriveMode.Output);

            Console.WriteLine("... Configuring SIM800H ...");

            // configure SIM800H device
            Eclo.nanoFramework.SIM800H.SIM800H.Configure(sim800PowerKey, ref _serialDevice);

            // add event handler to be aware of network registration status changes
            Eclo.nanoFramework.SIM800H.SIM800H.GsmNetworkRegistrationChanged += SIM800H_GsmNetworkRegistrationChanged;

            // add event handler to be aware of GPRS network registration status changes
            Eclo.nanoFramework.SIM800H.SIM800H.GprsNetworkRegistrationChanged += SIM800H_GprsNetworkRegistrationChanged;

            // it's wise to set this event handler to get the warning conditions from the module in case of under-voltage, over temperature, etc.
            Eclo.nanoFramework.SIM800H.SIM800H.WarningConditionTriggered += SIM800H_WarningConditionTriggered;

            // because we need Internet connection the access point configuration (APN) is mandatory
            // the configuration depends on what your network operator requires
            // it may be just the access point name or it may require an user and password too
            // AccessPointConfiguration class provides a number of convenient options to create a new APN configuration
            Eclo.nanoFramework.SIM800H.SIM800H.MmsAccessPointConfiguration = AccessPointConfiguration.Parse(APNConfigString);
            Eclo.nanoFramework.SIM800H.SIM800H.AccessPointConfiguration = AccessPointConfiguration.Parse(APNConfigString);

            // async call to power on module 
            // in this example we are implementing the callback in line
            Eclo.nanoFramework.SIM800H.SIM800H.PowerOnAsync((ar) =>
            {
                // check result
                if (((PowerOnAsyncResult)ar).Result == PowerStatus.On)
                {
                    Console.WriteLine("... Power on sequence completed...");
                }
                else
                {
                    // something went wrong...
                    Console.WriteLine("### Power on sequence FAILED ###");
                }
            }
            );

            Console.WriteLine("... Power on sequence started ...");
        }

        private static void SIM800H_GprsNetworkRegistrationChanged(NetworkRegistrationState networkState)
        {
            Console.WriteLine(networkState.GetDescription("GPRS"));

            if (networkState == NetworkRegistrationState.Registered)
            {
                // SIM800H is registered with GPRS network so we can request an Internet connection now

                // add event handler to know when we have an active Internet connection 
                // remove it first so we don't have duplicate calls in case a new successful registration occurs 
                Eclo.nanoFramework.SIM800H.SIM800H.GprsProvider.GprsIpAppsBearerStateChanged -= GprsProvider_GprsIpAppsBearerStateChanged;
                Eclo.nanoFramework.SIM800H.SIM800H.GprsProvider.GprsIpAppsBearerStateChanged += GprsProvider_GprsIpAppsBearerStateChanged;

                Eclo.nanoFramework.SIM800H.SIM800H.GprsProvider.OpenGprsConnectionAsync();

                // async call to GPRS provider to open the GPRS bearer
                // we can set a callback here to get the result of that request and act accordingly
                // or we can manage this in the GprsIpAppsBearerStateChanged event handler that we've already setup during the configuration
                Eclo.nanoFramework.SIM800H.SIM800H.GprsProvider.OpenBearerAsync();
            }
        }

        private static void GprsProvider_GprsIpAppsBearerStateChanged(bool isOpen)
        {
            if (isOpen)
            {
                // launch a new thread to get time and location from network
                new Thread(() =>
                {
                    Thread.Sleep(1000);

                    LocationAndTime lt = Eclo.nanoFramework.SIM800H.SIM800H.GetTimeAndLocation();

                    if (lt.ErrorCode == 0)
                    {
                        // request successful
                        Console.WriteLine("Network time " + lt.DateTime.ToString() + " GMT");
                        Console.WriteLine("Location http://www.bing.com/maps/?v=2&form=LMLTSN&cp=" + lt.Latitude.ToString() + "~" + lt.Longitude.ToString() + "&lvl=17&sty=r&encType=1");
                    }
                    else
                    {
                        // failed to retrieve time and location from network
                        Console.WriteLine("### Failed to retrieve time and location from network. Error code: " + lt.ErrorCode.ToString() + " ###");
                    }

                }).Start();
            }
        }

        private static void SIM800H_WarningConditionTriggered(WarningCondition warningCondition)
        {
            // get friendly string for this warning condition
            Console.WriteLine(SamplesExtensions.GetWarningDescription(warningCondition));
        }

        private static void SIM800H_GsmNetworkRegistrationChanged(NetworkRegistrationState networkState)
        {
            Console.WriteLine(networkState.GetDescription("GSM"));
        }
    }
}
