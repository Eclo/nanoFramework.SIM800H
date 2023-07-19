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

namespace SMS_Receive
{
    public class Program
    {
        static SerialDevice _serialDevice;

        public static void Main()
        {
            InitializeSIM800H();

            // loop forever and output available RAM each 5 seconds
            while (true)
            {
                Thread.Sleep(5000);

                // output signal RSSI
                Debug.WriteLine("Network signal strength is " + Eclo.nanoFramework.SIM800H.SIM800H.RetrieveSignalStrength().GetSignalStrengthDescription());
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

            // set event handler for SMS ready 
            Eclo.nanoFramework.SIM800H.SIM800H.SmsReady += SIM800H_SmsReady;

            // add event handler to be aware of network registration status changes
            Eclo.nanoFramework.SIM800H.SIM800H.GsmNetworkRegistrationChanged += SIM800H_GsmNetworkRegistrationChanged;

            // it's wise to set this event handler to get the warning conditions from the module in case of under-voltage, over temperature, etc.
            Eclo.nanoFramework.SIM800H.SIM800H.WarningConditionTriggered += SIM800H_WarningConditionTriggered;

            // async call to power on module 
            // in this example we are implementing the callback in line
            Eclo.nanoFramework.SIM800H.SIM800H.PowerOnAsync((ar) =>
            {
                // check result
                if (((PowerOnAsyncResult)ar).Result == PowerStatus.On)
                {
                    Debug.WriteLine("... Power on sequence completed...");
                }
                else
                {
                    // something went wrong...
                    Debug.WriteLine("### Power on sequence FAILED ###");
                }
            }
            );

            Debug.WriteLine("... Power on sequence started ...");
        }

        private static void SIM800H_SmsReady()
        {
            Debug.WriteLine("... SIM800H SMS engine is ready ...");

            // setup event handler to be notified when a new SMS arrives
            // because we may receive this event more than once (module wake-up, reboot, intermittent registration in network, etc.
            // it's advisable to always remove the handler before adding it this way we'll be sure that only one event handle is setup

            Eclo.nanoFramework.SIM800H.SIM800H.SmsProvider.SmsReceived -= SmsProvider_SmsReceived;
            Eclo.nanoFramework.SIM800H.SIM800H.SmsProvider.SmsReceived += SmsProvider_SmsReceived;
        }

        private static void SmsProvider_SmsReceived(byte messageIndex)
        {
            // the handler receives the index of the message received
            // now we need to actually read the message
            // as an optional argument we can delete the message from the memory after being read
            var message = Eclo.nanoFramework.SIM800H.SIM800H.SmsProvider.ReadTextMessage(messageIndex, true);

            Debug.WriteLine("******************************************************");
            Debug.WriteLine("Message from " + message.TelephoneNumber);
            Debug.WriteLine("Received @ " + message.Timestamp);
            Debug.WriteLine("«" + message.Text + "»");
            Debug.WriteLine("******************************************************");
        }

        private static void SIM800H_WarningConditionTriggered(WarningCondition warningCondition)
        {
            // get friendly string for this warning condition
            Debug.WriteLine(SamplesExtensions.GetWarningDescription(warningCondition));
        }

        private static void SIM800H_GsmNetworkRegistrationChanged(NetworkRegistrationState networkState)
        {
            Debug.WriteLine(networkState.GetDescription("GSM"));
        }
    }
}
