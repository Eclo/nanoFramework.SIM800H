////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Current status of HTTP service
    /// </summary>
    public class HttpCurrentStatus
    {

        private HttpAction mode;
        internal HttpAction Mode
        {
            get { return mode; }
        }

        private HttpStatus status;
        internal HttpStatus Status
        {
            get { return status; }
        }

        private int remaining;
        internal int Remaining
        {
            get { return remaining; }
        }

        private int transmitted;
        internal int Transmitted
        {
            get { return transmitted; }
        }

        internal HttpCurrentStatus(HttpAction mode, HttpStatus status, int transmitted, int remaining)
        {
            this.mode = mode;
            this.status = status;
            this.transmitted = transmitted;
            this.remaining = remaining;
        }
    }
}
