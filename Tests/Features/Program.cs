////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

using Eclo.nanoFramework.SIM800H;
using SIM800HSamples;
using System;
using System.IO.Ports;
using System.Threading;
using System.Device.Gpio;
using System.Diagnostics;

namespace Features
{
    public class Program
    {
        static SerialPort _serialDevice;

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
            _serialDevice = new SerialPort("COM2");

            // SIM800H signal for "power key"
            GpioPin sim800PowerKey = new GpioController().OpenPin(0 * 1 + 10, PinMode.Output);            

            Debug.WriteLine("... Configuring SIM800H ...");

            // configure SIM800H device
            Eclo.nanoFramework.SIM800H.SIM800H.Configure(sim800PowerKey, ref _serialDevice);

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
