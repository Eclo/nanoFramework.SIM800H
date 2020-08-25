using Eclo.nanoFramework.SIM800H;
using SIM800HSamples;
using System;
using System.Threading;
using Windows.Devices.Gpio;
using Windows.Devices.SerialCommunication;

namespace SMS_batch_sending
{
    public class Program
    {
        static SerialDevice _serialDevice;

        private static string smsDestination = "<destination phone number>";

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

            // set event handler for SMS ready 
            Eclo.nanoFramework.SIM800H.SIM800H.SmsReady += SIM800H_SmsReady; ;

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

        private static void SIM800H_SmsReady()
        {
            Console.WriteLine("... SIM800H SMS engine is ready ...");

            // launch a new thread to send 5 SMSes with 5 seconds interval
            int smsToSend = 1;
            int intervalBettweenSms = 5;

            new Thread(() =>
            {
                Thread.Sleep(1000);

                int sendCount = 0;

                while (sendCount < smsToSend)
                {
                    // send SMS asynchronously 
                    // set a callback to check the outcome 
                    // replace the White House switch board number bellow with a valid mobile number 
                    // the number must ALWAYS include the country code and international prefix
                    Eclo.nanoFramework.SIM800H.SIM800H.SmsProvider.SendTextMessageAsync(smsDestination, "Test message " + sendCount + " from SIM800H module", (ar) =>
                    {
                        if (((SendTextMessageAsyncResult)ar).Reference == -1)
                        {
                            // something went wrong...
                            Console.WriteLine("### FAILED sending SMS " + sendCount + " ###");
                        }
                        else
                        {
                            Console.WriteLine("... SMS " + sendCount + " sent ...");
                        }
                    }
                    );

                    // add counter
                    sendCount++;

                    // sleep for 30 seconds
                    Thread.Sleep(intervalBettweenSms * 1000);
                }

            }).Start();
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
