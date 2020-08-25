using System;
using System.Threading;
using Eclo.nF.Extensions;
using Windows.Devices.Gpio;
using Windows.Storage.Streams;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// An asynchronous result object returning the result of a Power on sequence execution
    /// </summary>
    public class PowerOnAsyncResult : DeviceAsyncResult
    {
        /// <summary>
        /// Result of power sequence execution
        /// </summary>
        public PowerStatus Result = PowerStatus.Unknown;

        public PowerOnAsyncResult(AsyncCallback asyncCallback = null, object asyncState = null)
            : base(asyncCallback, asyncState)
        {
        }

        /// <summary>
        /// Finishes the asynchronous processing and throws an exception if one was generated
        /// <remarks>Blocks until the asynchronous processing has completed</remarks>
        /// </summary>
        /// <returns>Returns the result of the power on sequence</returns>
        public new PowerStatus End()
        {
            base.End();

            return Result;
        }

        /// <summary>
        /// The method used to perform the asynchronous processing
        /// </summary>
        public override void Process()
        {
            Exception caughtException = null;
            bool wakeUpPromptOK = false, readyPromptOK = false;
            int milisecondsTimeout = 3000;
            const int loopWaitTime = 500;

            // start by checking if configuration was initialized
            if (!SIM800H.Instance._initCompleted)
            {
                Result = PowerStatus.Unknown;
                
                // throw an exception
                throw new InvalidOperationException("Configuration not initialized");
            }

            try
            {
                int sequenceCounter = 0;

                // save module power status before starting power on sequence
                PowerStatus bkpModulePowerStatus = SIM800H._powerStatus;
               
                // set power on sequence flag
                SIM800H._powerStatus = PowerStatus.PowerOnSequenceIsRunning;

//sequence_start:

                // clear response list
                SIM800H.Instance.responseQueue.Clear();

                // clear UART buffer
                SIM800H.Instance._serialDevice.DiscardInBuffer();

#if DEBUG__
                //Console.WriteLine("Starting power on sequence");
#endif

                if (SIM800H.PowerStatus != PowerStatus.On)
                {
                    // clear response list
                    SIM800H.Instance.responseQueue.Clear();

                    // hit power key 
                    // if module is ON it will be off which is good because we'll have it in a know good state
                    // if module is OFF it will power ON
#if DEBUG__
                    Console.WriteLine("Turning module on with power key");
#endif
                    SIM800H.Instance._powerKey.Write(GpioPinValue.High);
                    Thread.Sleep(1100);
                    SIM800H.Instance._powerKey.Write(GpioPinValue.Low);
                    Thread.Sleep(3100);

#if DEBUG__
                    Console.WriteLine("Sending AT to wake module or set auto baud");
#endif

                    // send AT directly to UART so this is not processed as command, waiting for reply, etc
                    SIM800H.Instance._serialDevice.Write(Prompts.AT + "\r");

                    wakePromptWait:
                    // wait "milisecondsTimeout"
                    milisecondsTimeout = 3000;
                    // a) we receive an OK prompt if auto-baud is active
                    // b) we receive an RDY prompt if there is a fixed baud rate and module has waked
                    while (milisecondsTimeout > 0)
                    {
                        // timeout for next iteration
                        milisecondsTimeout = milisecondsTimeout - loopWaitTime;

                        // check buffer for OK
                        lock (SIM800H.Instance.responseQueue)
                        {
                            if (SIM800H.Instance.responseQueue.FindAndRemove(Prompts.OK) != null)
                            {
                                // OK found, we are good to continue

#if DEBUG__
                                Console.WriteLine("OK prompt, module is awake");
#endif

                                wakeUpPromptOK = true;
                                break;
                            }
                        }

                        // check buffer for RDY
                        lock (SIM800H.Instance.responseQueue)
                        {
                            if (SIM800H.Instance.responseQueue.FindAndRemove(Prompts.RDY) != null)
                            {
                                // RDY prompt found, we are good to continue

#if DEBUG__
                                Console.WriteLine("RDY prompt, module is awake");
#endif

                                readyPromptOK = true;
                                break;
                            }
                        }

#if DEBUG__
                        Console.WriteLine("waiting wake up prompt...");
#endif 
                        
                        // sleep
                        Thread.Sleep(loopWaitTime);
                    }

                    if (!wakeUpPromptOK && !readyPromptOK)
                    {
                        // wake up prompt wasn't detected, try to wakeup with power key

#if DEBUG__
                        //Console.WriteLine("Turning module on with power key");
#endif
                        SIM800H.Instance._powerKey.Write(GpioPinValue.High);
                        Thread.Sleep(1100);
                        SIM800H.Instance._powerKey.Write(GpioPinValue.Low);
                        Thread.Sleep(3500);

                        // send AT directly to UART so this is not processed as command, waiting for reply, etc
                        SIM800H.Instance._serialDevice.Write(Prompts.AT + "\r");

                        // add sequence counter
                        sequenceCounter++;


                        // check if we are running the sequence too many times
                        if (sequenceCounter > 4)
                        {
                            // isn't working 
#if DEBUG__
                            Console.WriteLine("*** Power on sequence failed ***");
#endif

                            // restore module power status
                            SIM800H._powerStatus = bkpModulePowerStatus;

                            return;
                        }
                        //else if (sequenceCounter == 3)
                        //{
                        //    // to many retries, maybe module hasn't been configured ever

                        //    //Console.WriteLine("*** Reconfiguring baud rate ***");

                        //    // close serial
                        //    SIM800H._serialLine.Close();

                        //    // set 57600 bps
                        //    SIM800H._serialLine.BaudRate = 57600;

                        //    goto sequence_start;
                        //}
                        else
                        {
                            // after correct power ON we should receive an OK on AT or a RDY 
                            goto wakePromptWait;
                        }
                    }

                    // set echo off, just in case
                    SIM800H.Instance._serialDevice.Write(Prompts.AT + "E0\r\r\n");

                    // this is to enable detailed error msgs
                    //SIM800H._serialLine.Write("AT+CMEE=2\r");
                    //  disable with 
                    //SIM800H._serialLine.Write("AT+CMEE=0\r");

                    // if OK prompt module is in auto-baud mode and we need to configure it
                    if (!SIM800H.Instance.SendConfigurationToDevice())
                    {
#if DEBUG__
                        Console.WriteLine("Failure sending configuration to module.");
#endif
                    }

                    // launch reader thread
                    SIM800H.Instance.readerThread = new Thread(new ThreadStart(SIM800H.Instance.run));
                    SIM800H.Instance.readerThread.Start();

                    // need a tiny pause here to let the reader thread clear the response queue
                    Thread.Sleep(500);

                    // reached here so module must be ON, set flag
                    // have to set the internal to change status from power on sequence
                    SIM800H._powerStatus = PowerStatus.On;
                    // not set the property to fire the event
                    SIM800H.PowerStatus = PowerStatus.On;

#if DEBUG__
                    Console.WriteLine("Module is ON");
#endif
                }

                Result = SIM800H.PowerStatus;
            }
            catch (Exception exception)
            {
                caughtException = exception;
            }
            finally
            {
                Complete(caughtException);
            }
        }
    }
}
