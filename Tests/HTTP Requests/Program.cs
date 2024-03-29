﻿////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

using Eclo.nanoFramework.SIM800H;
using SIM800HSamples;
using System;
using System.Text;
using System.Threading;
using Windows.Devices.Gpio;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace HTTP_Requests
{
    public class Program
    {
        static SerialDevice _serialDevice;

        private const string APNConfigString = "net2.vodafone.pt|vodafone|vodafone";
        private const string thingsSpeakApiKey = "G4AVDUPU27ZC899X";

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

            // add event handler to be aware of network registration status changes
            Eclo.nanoFramework.SIM800H.SIM800H.GsmNetworkRegistrationChanged += SIM800H_GsmNetworkRegistrationChanged;

            // add event handler to be aware of GPRS network registration status changes
            Eclo.nanoFramework.SIM800H.SIM800H.GprsNetworkRegistrationChanged += SIM800H_GprsNetworkRegistrationChanged;

            // it's wise to set this event handler to get the warning conditions from the module in case of under-voltage, over temperature, etc.
            Eclo.nanoFramework.SIM800H.SIM800H.WarningConditionTriggered += SIM800H_WarningConditionTriggered;

            Eclo.nanoFramework.SIM800H.SIM800H.PowerStatusChanged += SIM800H_PowerStatusChanged;

            // because we need Internet connection the access point configuration (APN) is mandatory
            // the configuration depends on what your network operator requires
            // it may be just the access point name or it may require an user and password too
            // AccessPointConfiguration class provides a number of convenient options to create a new APN configuration
            Eclo.nanoFramework.SIM800H.SIM800H.AccessPointConfiguration = AccessPointConfiguration.Parse(APNConfigString);

            // async call to power on module 
            // in this example we are setting up a callback on a separate method
            Eclo.nanoFramework.SIM800H.SIM800H.PowerOnAsync(PowerOnCompleted);

            Debug.WriteLine("... Power on sequence started ...");
        }

        static void SIM800H_PowerStatusChanged(PowerStatus powerStatus)
        {
            Debug.WriteLine("Power status is: " + powerStatus.GetDescription());
        }

        private static void PowerOnCompleted(IAsyncResult result)
        {
            // check result
            if (((PowerOnAsyncResult)result).Result == PowerStatus.On)
            {
                Debug.WriteLine("... Power on sequence completed...");
            }
            else
            {
                // something went wrong...
                Debug.WriteLine("### Power on sequence FAILED ###");
            }
        }

        private static void SIM800H_GsmNetworkRegistrationChanged(NetworkRegistrationState networkState)
        {
            Debug.WriteLine(networkState.GetDescription("GSM"));
        }

        private static void SIM800H_GprsNetworkRegistrationChanged(NetworkRegistrationState networkState)
        {
            Debug.WriteLine(networkState.GetDescription("GPRS"));

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
                Eclo.nanoFramework.SIM800H.SIM800H.GprsProvider.OpenBearerAsync(BearerProfile.IpAppsBearer);
            }
        }

        private static void GprsProvider_GprsIpAppsBearerStateChanged(bool isOpen)
        {
            if (isOpen)
            {
                // launch a new thread to download weather data
                new Thread(() =>
                {
                    Thread.Sleep(1000);

                    UploadDataToChannel();
                }).Start();
            }
        }

        private static void SIM800H_WarningConditionTriggered(WarningCondition warningCondition)
        {
            // get friendly string for this warning condition
            Debug.WriteLine(SamplesExtensions.GetWarningDescription(warningCondition));
        }

        static void UploadDataToChannel()
        {
            // download weather data for Lisbon (Portugal) from Open Weather Data
            Debug.WriteLine("... uploading data to channel ...");


            ///////////////////////////////////////////////////////
            //// option 1: using .NETMF API like and data on URL //
            ///////////////////////////////////////////////////////

            byte[] receivedBody = new byte[500];

            // data to upload to channel
            double newData1 = 15.16;
            double newData2 = 11.12;

            // create HTTTP web request with URI
            using (var webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://api.thingspeak.com/update?key=" + thingsSpeakApiKey + "&headers=false&field1=" + newData1.ToString("N2") + "&field2 = " + newData2.ToString("N2"))))
            {
                // set HTTP method for the request
                webRequest.Method = "POST";

                // perform the request and get the response
                using (var res = webRequest.GetResponse() as HttpWebResponse)
                {
                    Debug.WriteLine("******************************************************");
                    Debug.WriteLine(res.BodyData);
                    Debug.WriteLine("******************************************************");
                }
            }

            //// use a string builder to make body data printable
            //StringBuilder sb = new StringBuilder();
            //foreach (byte b in receivedBody)
            //{
            //    sb.Append((char)b);
            //}

            // need to wait 15 seconds to send new data to the same channel (ThingSpeak usage policy)
            Thread.Sleep(15000);
            

            ///////////////////////////////////////////////////////////////////
            // option 2: using the HTTP client of the driver and data on URL //
            ///////////////////////////////////////////////////////////////////
            /*
                        //newData1 = 18.19;
                        //newData2 = 17.18;

                        using (var webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://njbuch.pythonanywhere.com/trap/api-v1/event/")))
                        {
                            // set HTTP method for the request
                            webRequest.Method = "POST";
                            webRequest.Headers.Add("Authorization", "Basic dGVzdDpwbGFkZGVy");
                            webRequest.ContentType = "application/json";

                            webRequest.Data = "{\"eventtype\": \"AllOK\", \"eventtime\": \"2016-11-01 20:41\", \"trap\": \"1111\" }";


                            // perform the HTTP request asynchronously and set a callback handler to print the response
                            Eclo.nanoFramework.SIM800H.SIM800H.HttpClient.PerformHttpWebRequestAsync(webRequest, true, false, true, 5000, (ar) =>
                            {
                                // get the response
                                var response = ((HttpWebRequestAsyncResult)ar).HttpResponse;

                                // check if the HTTP request was successful
                                if (response.RequestSuccessful)
                                {
                                    Debug.WriteLine("******************************************************");

                                    // grab body data as a string directly from the response
                                    Debug.WriteLine(response.BodyData);

                                    Debug.WriteLine("******************************************************");
                                }
                            });
                        }


                        // need to wait 15 seconds to send new data to the same channel (ThingSpeak usage policy)
                        Thread.Sleep(15000);
            */

            ////////////////////////////////////////////////////////////////////////////////////////////////
            //// option 3: using the HTTP client of the driver, data in body and authentication in header //
            ////////////////////////////////////////////////////////////////////////////////////////////////
            /*
                        double newData1 = 28.29;
                        double newData2 = 18.19;

                        using (var webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://api.thingspeak.com/update?&headers=false")))
                        {
                            // set HTTP method for the request
                            webRequest.Method = "POST";

                            // send authentication in a header 
                            // check ThingsSpeak API documentation
                            webRequest.Headers.Add("THINGSPEAKAPIKEY", thingsSpeakApiKey);

                            // request data that will be send on the request body
                            // there are two options to add request data
                            // 1) in a stream using webRequest.GetRequestStream() for .NETMF API like usage
                            // 2) as a string by setting the webRequest.Data field
                            // for this example we'll be using option 2) which is the most straightforward

                            webRequest.Data = "field1=" + newData1.ToString("N2");
                            webRequest.Data += "\r\n" + "field2=" + newData2.ToString("N2");

                            // perform the HTTP request asynchronously and set a callback handler to print the response
                            Eclo.nanoFramework.SIM800H.SIM800H.HttpClient.PerformHttpWebRequestAsync(webRequest, true, false, true, 5000, (ar) =>
                            {
                                // get the response
                                var response = ((HttpWebRequestAsyncResult)ar).HttpResponse;

                                // check if the HTTP request was successful
                                if (response.RequestSuccessful)
                                {
                                    Debug.WriteLine("******************************************************");

                                    // grab body data as a string directly from the response
                                    Debug.WriteLine(response.BodyData);

                                    Debug.WriteLine("******************************************************");
                                }
                            });
                        }
            */
            ///////////////////////////////////////////////////////////////////
            // option 4: testing the multipart/form-data ********* DOESN'T WORK WITH THIS SERVER *********** //
            ///////////////////////////////////////////////////////////////////
            /*
            //using (var webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://httpbin.org/post")))
            using (var webRequest = (HttpWebRequest)WebRequest.Create(new Uri("http://ptsv2.com/t/em8wl-1539684710/post")))
            {
                string boundary = DateTime.UtcNow.Ticks.ToString("");
                string boundarystart = "--" + boundary + "\r\n";
                string boundarytrailer = "--" + boundary + "--\r\n";

                // set HTTP method for the request
                webRequest.Method = "POST";
                webRequest.ContentType = "multipart/form-data; boundary=" + boundary;
                webRequest.Headers.Add("Authorization", "Basic dGVzdDplY2xvZWNsbw==");

                // CR LF separator
                // boundary
                webRequest.GetRequestStream().Write(UTF8Encoding.UTF8.GetBytes(boundarystart));


                // form part form part
                string dataFormPart = "Content-Disposition: form-data; name=\"eventtype\"\r\n\r\nAllOK\r\n";
                webRequest.GetRequestStream().Write(UTF8Encoding.UTF8.GetBytes(dataFormPart));

                webRequest.GetRequestStream().Write(UTF8Encoding.UTF8.GetBytes(boundarystart));

                // file header form part
                string fileHeaderFormPart = "Content-Disposition: form-data; name=\"image\"; filename=\"fileName.jpg\"\r\nContent-Type: image/jpg\r\n\r\n";
                webRequest.GetRequestStream().Write(UTF8Encoding.UTF8.GetBytes(fileHeaderFormPart));

                //// form part form part
                //string dataFormPart = "Content-Disposition: form-data; name='fname'\r\n\r\nJose\r\n";
                //webRequest.GetRequestStream().Write(UTF8Encoding.UTF8.GetBytes(dataFormPart), 0, dataFormPart.Length);

                //webRequest.GetRequestStream().Write(UTF8Encoding.UTF8.GetBytes(boundarystart), 0, boundarystart.Length);

                // file content
                //                webRequest.GetRequestStream().Write(Resources.GetBytes(Resources.BinaryResources.test_img_1), 0, Resources.GetBytes(Resources.BinaryResources.mmsImg).Length);
                //webRequest.GetRequestStream().Write(UTF8Encoding.UTF8.GetBytes("HELLO WORLD"), 0, 11);
                // CR LF separator
                webRequest.GetRequestStream().Write(UTF8Encoding.UTF8.GetBytes("\r\n"));

                // boundary trailer
                webRequest.GetRequestStream().Write(UTF8Encoding.UTF8.GetBytes(boundarytrailer));

                // perform the request and get the response
                using (var res = webRequest.GetResponse() as HttpWebResponse)
                {
                    Debug.WriteLine("STATUS FROM SERVER:" + res.StatusCode.ToString());
                    // read body data from response stream
                    using (var stream = res.GetResponseStream())
                    {
                        IBuffer receivedBody = new ByteBuffer((uint)stream.Length);
                        stream.Read(receivedBody, (uint)stream.Length, InputStreamOptions.Partial);
                        string respString = new string(Encoding.UTF8.GetChars(((ByteBuffer)receivedBody).Data));
                        Debug.WriteLine("RETURN FROM SERVER:" + respString);
                        stream.Dispose();

                    }
                }
            }
            */
            /*
                        // Lorenzo Maiorfi test call

                        string TEST_URL = "https://iot.mqtt.it:1880/test-sim800h";
                        AutoResetEvent are = new AutoResetEvent(false);
                        bool outcome = false;

                        var numpar = 2;

                        using (var webRequest = WebRequest.Create(new Uri(TEST_URL + "?par=" + numpar)) as HttpWebRequest)
                        {
                            webRequest.Method = "GET";

                            // perform the HTTP request asynchronously and set a callback handler to print the response
                            Eclo.nanoFramework.SIM800H.SIM800H.HttpClient.PerformHttpWebRequestAsync(webRequest, true, true, false, 5000, (ar) =>
                            {
                                // get the response
                                var response = (ar as HttpWebRequestAsyncResult).HttpResponse;

                                var h = response.Headers.AllKeys;

                                var h1 = response.Headers["etag"];

                                // check if the HTTP request was successful
                                if (response.RequestSuccessful)
                                {
                                    Debug.WriteLine("*********************** BODY ************************");

                                    // grab body data as a string directly from the response
                                    Debug.WriteLine(response.BodyData);

                                    Debug.WriteLine("******************************************************");

                                    outcome = true;
                                    are.Set();
                                }
                                else
                                {
                                    outcome = false;
                                    are.Set();
                                }
                            });
                        }
            */
        }
    }
}
