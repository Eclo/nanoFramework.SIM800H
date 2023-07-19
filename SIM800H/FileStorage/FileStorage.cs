////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

using System;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Class with methods to use internal flash storage space.
    /// </summary>
    public class FileStorage : IDisposable
    {
        string driveLetter = string.Empty;

        internal FileStorage()
        {
            // set default drive to 0 (internal storage)
            string atCommand = Prompts.AT + Prompts.FSDRIVE + @"=0";
            AtCommandResult ret = SIM800H.Instance.SendATCommandAndWaitForResponse(atCommand, 2000);
            if (ret.Result != ReturnedState.OK)
            {
                // give it another try
                System.Threading.Thread.Sleep(2000);

                ret = SIM800H.Instance.SendATCommandAndWaitForResponse(atCommand, 2000);
                if (ret.Result != ReturnedState.OK)
                {
                    // fail to set active FS drive
                    // probably should throw an exception
                }
            }

            // save drive letter
            driveLetter = ret.Response.Substring(Prompts.FSDRIVE.Length + 2) + @":\";
        }

        /// <summary>
        /// Read a file from the file storage
        /// </summary>
        /// <param name="fileName">File name including full path.</param>
        /// <returns>File contents or NULL if file is empty or doesn't exist.</returns>
        public string ReadFile(string fileName)
        {
            string atCommand = Prompts.AT + Prompts.FSREAD + driveLetter + fileName + @",0,1024,0";
            AtCommandResult ret = SIM800H.Instance.SendATCommandAndWaitForResponse(atCommand, 4000);
            if (ret.Result == ReturnedState.OK)
            {
                return ret.Response;
            }

            return null;
        }

        /// <summary>
        /// Returns the file size in bytes.
        /// </summary>
        /// <param name="fileName">File name including full path.</param>
        /// <returns>File size in bytes or -1 if size couldn't be read or doesn't exist.</returns>
        public int GetFileSize(string fileName)
        {
            string atCommand = Prompts.AT + Prompts.FSFLSIZE + "=" + driveLetter + fileName;
            AtCommandResult ret = SIM800H.Instance.SendATCommandAndWaitForResponse(atCommand, 2000);
            if (ret.Result == ReturnedState.OK)
            {
                try
                {
                    string fileSize = ret.Response.Substring(Prompts.FSFLSIZE.Length + 2);

                    // parse size
                    return int.Parse(fileSize);
                }
                catch { };
            }

            return -1;
        }
        
        /// <summary>
        /// Returns available storage space.
        /// </summary>
        /// <returns>File available storage space in bytes or -1 if size couldn't be determined.</returns>
        public int GetAvailableStorage()
        {
            string atCommand = Prompts.AT + Prompts.FSMEM;
            AtCommandResult ret = SIM800H.Instance.SendATCommandAndWaitForResponse(atCommand, 2000);
            if (ret.Result == ReturnedState.OK)
            {
                try
                {
                    // reply is in the following format: AT+FSMEM: <local_drive>:<local_size>bytes, ....
                    string[] freeStorageDetails = ret.Response.Substring(Prompts.FSMEM.Length + 2).Split(new char[] { ':' });

                    // parse size (need to remove 'bytes' word from the end)
                    return int.Parse(freeStorageDetails[1].Substring(0, freeStorageDetails[1].Length - 5));
                }
                catch { };
            }

            return -1;
        }

        /// <summary>
        /// Writes content to a storage file.
        /// Content must be plain string without \r \n chars.
        /// </summary>
        /// <param name="fileName">File name including full path.</param>
        /// <param name="content">File content.</param>
        /// <returns>True if file was successfully written. False otherwise.</returns>
        public bool WriteFile(string fileName, string content)
        {
            string atCommand = Prompts.AT + Prompts.FSWRITE + driveLetter + fileName + @",0," + (content.Length + 2) + ",3";
            AtCommandResult ret = SIM800H.Instance.SendATCommandAndWaitForResponse(atCommand, 1000);
            if (ret.Result == ReturnedState.OK && ret.Response == Prompts.SendPrompt)
            {
                // we have a send prompt
                // send file content without wake-up char
                ret = SIM800H.Instance.SendATCommand(content + "\r\n", 1000, false, true);
                if(ret.Result == ReturnedState.OK)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Create an empty file in the file storage
        /// </summary>
        /// <param name="fileName">File name including full path.</param>
        /// <returns>True when file was successfully created. False otherwise.</returns>
        public bool CreateFile(string fileName)
        {
            string atCommand = Prompts.AT + Prompts.FSCREATE + driveLetter + fileName;
            AtCommandResult ret = SIM800H.Instance.SendATCommandAndWaitForResponse(atCommand, 2000);
            if (ret.Result != ReturnedState.OK)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deletes a file from the file storage
        /// </summary>
        /// <param name="fileName">File name including full path.</param>
        /// <returns>True if the file was successfully deleted. False otherwise.</returns>
        public bool DeleteFile(string fileName)
        {
            string atCommand = Prompts.AT + Prompts.FSDEL + driveLetter + fileName;
            AtCommandResult ret = SIM800H.Instance.SendATCommandAndWaitForResponse(atCommand, 2000);
            if (ret.Result != ReturnedState.OK)
            {
                return true;
            }

            return false;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FileStorage() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
