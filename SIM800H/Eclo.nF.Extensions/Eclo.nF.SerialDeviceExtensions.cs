////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

using System;
using System.IO;
using System.IO.Ports;

namespace Eclo.nF.Extensions
{
    internal static class SerialDeviceExtensions
    {
        /// <summary>
        /// Writes a string to the serial port.
        /// </summary>
        /// <param name="serialDevice">Serial port to write string to</param>
        /// <param name="text">string to write</param>
        public static void Write(this SerialPort serialDevice, string text)
        {
            if (text != null)
            {
                // setup data writer for Serial Device output stream
                StreamWriter outputDataWriter = new StreamWriter(serialDevice.BaseStream);
                // write string to Serial Device output stream using data writer
                // (this doesn't send any data, just writes to the stream)
                ////var bytesWritten = outputDataWriter.Write(text);
                outputDataWriter.Write(text);
                // calling the 'Store' method on the data writer actually sends the data
                ////var bw1 = outputDataWriter.Store();
            }
        }
        public static void WriteBytes(this SerialPort serialDevice, byte[] buffer, int offset, int count)
        {
            if (buffer != null)
            {
                byte[] tmp = new byte[count];
                Array.Copy(buffer, offset, tmp, 0, count);
                // setup data writer for Serial Device output stream
                StreamWriter outputDataWriter = new StreamWriter(serialDevice.BaseStream);
                // write string to Serial Device output stream using data writer
                // (this doesn't send any data, just writes to the stream)
                outputDataWriter.Write(tmp);
                // calling the 'Store' method on the data writer actually sends the data
                ////var bw1 = outputDataWriter.Store();
            }
        }

        ////public static byte ReadByte(this SerialPort serialDevice, StreamReader inputDataReader)
        ////{
        ////    // read one bytes from the Serial Device input stream
        ////    var byteRead = inputDataReader.Load(1);
        ////    return inputDataReader.ReadByte();
        ////}

        ////public static int ReadChars(this SerialPort serialDevice, StreamReader inputDataReader, ref char[] buffer, int offset, int count)
        ////{
        ////    byte[] tempBuffer = new byte[count];

        ////    // load count bytes to be read
        ////    var bytesRead = inputDataReader.ReadBlock(buffer, offset, tempBuffer.Length);

        ////    // copy temp buffer content to buffer arg, casting to char
        ////    foreach (byte b in tempBuffer)
        ////    {
        ////        buffer[offset++] = (char)b;
        ////    }

        ////    // release buffer memory
        ////    tempBuffer = null;

        ////    return (int)bytesRead;
        ////}

        public static void DiscardInBuffer(this SerialPort serialDevice)
        {
            serialDevice.ReadExisting();
            ////using (var inputDataReader = new StreamReader(serialDevice.BaseStream))
            ////{
            ////    var bytesRead = inputDataReader.ReadToEnd(); //// Load(serialDevice.BytesToRead);
            ////    ////if(bytesRead > 0)
            ////    ////    inputDataReader.Read(bytesRead);
            ////}
        }
    }
}