using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System;

namespace Eclo.nF.Extensions
{
    internal static class SerialDeviceExtensions
    {
        /// <summary>
        /// Writes a string to the serial port.
        /// </summary>
        /// <param name="serialDevice">Serial port to write string to</param>
        /// <param name="text">string to write</param>
        public static void Write(this SerialDevice serialDevice, string text)
        {
            if (text != null)
            {
                // setup data writer for Serial Device output stream
                DataWriter outputDataWriter = new DataWriter(serialDevice.OutputStream);
                // write string to Serial Device output stream using data writer
                // (this doesn't send any data, just writes to the stream)
                var bytesWritten = outputDataWriter.WriteString(text);
                // calling the 'Store' method on the data writer actually sends the data
                var bw1 = outputDataWriter.Store();
            }
        }
        public static void WriteBytes(this SerialDevice serialDevice, byte[] buffer, int offset, int count)
        {
            if (buffer != null)
            {
                byte[] tmp = new byte[count];
                Array.Copy(buffer, offset, tmp, 0, count);
                // setup data writer for Serial Device output stream
                DataWriter outputDataWriter = new DataWriter(serialDevice.OutputStream);
                // write string to Serial Device output stream using data writer
                // (this doesn't send any data, just writes to the stream)
                outputDataWriter.WriteBytes(tmp);
                // calling the 'Store' method on the data writer actually sends the data
                var bw1 = outputDataWriter.Store();
            }
        }

        public static byte ReadByte(this SerialDevice serialDevice, DataReader inputDataReader)
        {
            // read one bytes from the Serial Device input stream
            var byteRead = inputDataReader.Load(1);
            return inputDataReader.ReadByte();
        }

        public static int ReadChars(this SerialDevice serialDevice, DataReader inputDataReader, ref char[] buffer, int offset, int count)
        {
            byte[] tempBuffer = new byte[count];

            // load count bytes to be read
            var bytesRead = inputDataReader.Load((uint)count);
            inputDataReader.ReadBytes(tempBuffer);

            // copy temp buffer content to buffer arg, casting to char
            foreach (byte b in tempBuffer)
            {
                buffer[offset++] = (char)b;
            }

            // release buffer memory
            tempBuffer = null;

            return (int)bytesRead;
        }

        public static void DiscardInBuffer(this SerialDevice serialDevice)
        {
            using (var inputDataReader = new DataReader(serialDevice.InputStream))
            {
                var bytesRead = inputDataReader.Load(serialDevice.BytesToRead);
                if(bytesRead > 0)
                    inputDataReader.ReadString(bytesRead);
            }
        }
    }
}