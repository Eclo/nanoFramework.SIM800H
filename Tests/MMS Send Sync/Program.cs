////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

using Eclo.nanoFramework.SIM800H;
using SIM800HSamples;
using System;
using System.Threading;
using Windows.Devices.Gpio;
using Windows.Devices.SerialCommunication;

namespace MMS_Send_Sync
{
    public class Program
    {
        static SerialDevice _serialDevice;

        // Vodafone PT config
        private const string mmsApnConfigString = "vas.vodafone.pt|vas|vas";
        private const string mmsUrl = "mms/servlets/mms";
        private const string mmsProxy = "213.30.27.63";
        private const int mmsPort = 8799;// default


        // mind that the max accepted lenght is 40 chars
        private static string mmsTitle = "MMS testing";
        // mind that the max accepted lenght is 15360 chars
        private static string mmsText = "My message";
        private static string mmsDestination = "<destination phone number>";

        public static void Main()
        {
            InitializeSIM800H();

            // powering on module
            Debug.WriteLine("Power on sequence starting");
            var powerOnResult = Eclo.nanoFramework.SIM800H.SIM800H.PowerOnAsync().End();

            if (powerOnResult == PowerStatus.On)
            {
                Debug.WriteLine("... Power on sequence completed...");
            }
            else
            {
                // something went wrong...
                Debug.WriteLine("### Power on sequence FAILED ###");
            }

            // wait for GPRS network registration
            while (Eclo.nanoFramework.SIM800H.SIM800H.GprsNetworkRegistration != NetworkRegistrationState.Registered)
            {
                Thread.Sleep(1000);
            }

            var connectGprsResult = Eclo.nanoFramework.SIM800H.SIM800H.GprsProvider.OpenGprsConnectionAsync().End();

            if (connectGprsResult == ConnectGprsResult.Open ||
                connectGprsResult == ConnectGprsResult.AlreadyOpen)
            {
                Debug.WriteLine("... GPRS connected ...");
            }
            else
            {
                Debug.WriteLine("### failed to connect to GPRS ###");
            }


            // open GPRS bearer for IP apps
            var openBearerResult = Eclo.nanoFramework.SIM800H.SIM800H.GprsProvider.OpenBearerAsync(BearerProfile.IpAppsBearer).End();

            if (openBearerResult == OpenBearerResult.Open ||
                openBearerResult == OpenBearerResult.AlreadyOpen)
            {
                Debug.WriteLine("... IP apps bearer opened...");
            }
            else
            {
                Debug.WriteLine("### failed to open IP apps bearer ###");
            }


            // open GPRS bearer for MMS
            openBearerResult = Eclo.nanoFramework.SIM800H.SIM800H.GprsProvider.OpenBearerAsync(BearerProfile.MmsBearer).End();

            if (openBearerResult == OpenBearerResult.Open ||
                openBearerResult == OpenBearerResult.AlreadyOpen)
            {
                Debug.WriteLine("... MMS bearer opened...");
            }
            else
            {
                Debug.WriteLine("### failed to open MMS bearer ###");
            }

            // set MMS configuration
            Eclo.nanoFramework.SIM800H.SIM800H.MmsConfiguration = new MmsConfiguration(mmsUrl, mmsProxy, mmsPort);

            var picture = Resources.GetBytes(Resources.BinaryResources.mmsImg);

            // build and send MMS message 1
            MmsMessage msg = new MmsMessage(mmsText + "01", picture, mmsTitle + "01");

            var mmsSendResult = Eclo.nanoFramework.SIM800H.SIM800H.MmsClient.SendMmsMessageAsync(mmsDestination, msg, false).End();

            // check result
            if (mmsSendResult)
            {
                Debug.WriteLine("MMS sent successfully!");
            }
            else
            {
                Debug.WriteLine("### Error sending MMS.");
            }

            // just because....
            Thread.Sleep(5000);

            // power off module
            Debug.WriteLine("Power off module");
            Eclo.nanoFramework.SIM800H.SIM800H.PowerOff();

            // powering on module again
            Debug.WriteLine("Power on sequence starting");
            powerOnResult = Eclo.nanoFramework.SIM800H.SIM800H.PowerOnAsync().End();

            if (powerOnResult == PowerStatus.On)
            {
                Debug.WriteLine("... Power on sequence completed...");

            }
            else
            {
                // something went wrong...
                Debug.WriteLine("### Power on sequence FAILED ###");
            }

            // wait for GPRS network registration
            while (Eclo.nanoFramework.SIM800H.SIM800H.GprsNetworkRegistration != NetworkRegistrationState.Registered)
            {
                Thread.Sleep(1000);
            }


            connectGprsResult = Eclo.nanoFramework.SIM800H.SIM800H.GprsProvider.OpenGprsConnectionAsync().End();
            if (connectGprsResult == ConnectGprsResult.Open ||
                connectGprsResult == ConnectGprsResult.AlreadyOpen)
            {
                Debug.WriteLine("... GPRS connected ...");
            }
            else
            {
                Debug.WriteLine("### failed to connect GPRS ###");
            }

            // open MMS bearer
            openBearerResult = Eclo.nanoFramework.SIM800H.SIM800H.GprsProvider.OpenBearerAsync(BearerProfile.IpAppsBearer).End();

            if (openBearerResult == OpenBearerResult.Open ||
                openBearerResult == OpenBearerResult.AlreadyOpen)
            {
                Debug.WriteLine("... IP apps bearer opened...");
            }
            else
            {
                Debug.WriteLine("### failed to open IP apps bearer ###");
            }


            // open MMS bearer
            openBearerResult = Eclo.nanoFramework.SIM800H.SIM800H.GprsProvider.OpenBearerAsync(BearerProfile.MmsBearer).End();

            if (openBearerResult == OpenBearerResult.Open ||
                openBearerResult == OpenBearerResult.AlreadyOpen)
            {
                Debug.WriteLine("... MMS bearer opened...");
            }
            else
            {
                Debug.WriteLine("### failed to open MMS bearer ###");
            }

            // build and send MMS message 2
            msg = new MmsMessage(mmsText + "02", picture, mmsTitle + "02");

            mmsSendResult = Eclo.nanoFramework.SIM800H.SIM800H.MmsClient.SendMmsMessageAsync(mmsDestination, msg, true).End();

            // check result
            if (mmsSendResult)
            {
                Debug.WriteLine("MMS sent successfully!");
            }
            else
            {
                Debug.WriteLine("### Error sending MMS.");
            }

            // powering off module
            powerOnResult = Eclo.nanoFramework.SIM800H.SIM800H.PowerOnAsync().End();

            // loop forever 
            while (true)
            {
                Thread.Sleep(5000);
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

            Debug.WriteLine("... Configuring SIM800H ...");

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
            Eclo.nanoFramework.SIM800H.SIM800H.MmsAccessPointConfiguration = AccessPointConfiguration.Parse(mmsApnConfigString);
            Eclo.nanoFramework.SIM800H.SIM800H.AccessPointConfiguration = AccessPointConfiguration.Parse(mmsApnConfigString);
        }

        private static void SIM800H_WarningConditionTriggered(WarningCondition warningCondition)
        {
            // get friendly string for this warning condition
            Debug.WriteLine(SamplesExtensions.GetWarningDescription(warningCondition));
        }

        private static void SIM800H_GprsNetworkRegistrationChanged(NetworkRegistrationState networkState)
        {
            Debug.WriteLine(networkState.GetDescription("GPRS"));
        }

        private static void SIM800H_GsmNetworkRegistrationChanged(NetworkRegistrationState networkState)
        {
            Debug.WriteLine(networkState.GetDescription("GSM"));
        }
    }
}
