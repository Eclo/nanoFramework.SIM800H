using System;
using System.Text;

namespace Eclo.nanoFramework.SIM800H
{   
    /// <summary>
    /// Represents a referenced array of bytes used by byte stream read and write interfaces.
    /// Buffer is the class implementation of this interface.
    /// </summary>
    public interface IBuffer
    {
        /// <summary>
        /// Gets the maximum number of bytes that the buffer can hold.
        /// </summary>
        uint Capacity { get; }

        /// <summary>
        /// Gets or sets the number of bytes currently in use in the buffer.
        /// </summary>
        uint Length { get; set; }
    }
}

