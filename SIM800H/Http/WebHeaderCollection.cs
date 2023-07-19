////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

using System.Collections;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Contains protocol headers associated with a request or response.
    /// Manages name-value pairs for HTTP headers.
    /// </summary>
    /// <remarks>
    /// This class includes additional methods, including HTTP parsing of a
    /// collection into a buffer that can be sent.
    /// <para>
    /// Headers are validated when attempting to add them.
    /// </para>
    /// Implementation follows .NETMF System.Net.WebHeaderCollection
    /// </remarks>
    public class WebHeaderCollection
    {
        internal Hashtable _headers;

        ///// <summary>
        ///// Data and constants.
        ///// </summary>
        //private const int ApproxAveHeaderLineSize = 30;
        //private static readonly HeaderInfoTable HInfo = new HeaderInfoTable();

        ///// <summary>
        ///// Array list of headers and values
        ///// </summary>
        //private HeaderValueCollection head_val_coll = new HeaderValueCollection();

        ///// <summary>
        ///// true if this object is created for internal use, in this case
        ///// we turn on checking when adding special headers.
        ///// </summary>
        //private bool m_IsHttpWebHeaderObject = false;

        ///// <summary>
        ///// Adds header name/value pair to collection. Does not check if
        ///// multiple values are allowed.
        ///// </summary>
        ///// <param name="headerName">Name in header </param>
        ///// <param name="headerValue">Value in header </param>
        //internal void AddWithoutValidate(string headerName, string headerValue)
        //{
        //    headerName = CheckBadChars(headerName, false);
        //    headerValue = CheckBadChars(headerValue, true);

        //    head_val_coll.Add(headerName, headerValue);
        //}

        ///// <summary>
        ///// Adds header name/value pair to collection.
        ///// If multi value allowed for this header name - adds new one
        ///// If multi value is not allowed, replace the old value with new one
        ///// </summary>
        ///// <param name="name">Name in header </param>
        ///// <param name="value">Value in header </param>
        //internal void SetAddVerified(string name, string value)
        //{
        //    if (HInfo[name].AllowMultiValues)
        //    {
        //        head_val_coll.Add(name, value);
        //    }
        //    else
        //    {
        //        head_val_coll.Set(name, value);
        //    }
        //}

        //// The below 3 methods are for fast headers manipulation, bypassing all
        //// the checks.

        ///// <summary>
        ///// Just internal fast add.
        ///// </summary>
        ///// <param name="headerName"></param>
        ///// <param name="headerValue"></param>
        //internal void AddInternal(string headerName, string headerValue)
        //{
        //    head_val_coll.Add(headerName, headerValue);
        //}

        ///// <summary>
        ///// Internal fast channge
        ///// </summary>
        ///// <param name="name"></param>
        ///// <param name="value"></param>
        //internal void ChangeInternal(string name, string value)
        //{
        //    head_val_coll.Set(name, value);
        //}

        ///// <summary>
        ///// Internal remove of header.
        ///// </summary>
        ///// <param name="name"></param>
        //internal void RemoveInternal(string name)
        //{
        //    head_val_coll.RemoveHeader(name);
        //}

        ///// <summary>
        ///// Changes to new value. Check for illegal characters first.
        ///// </summary>
        ///// <param name="name"></param>
        ///// <param name="value"></param>
        //internal void CheckUpdate(string name, string value)
        //{
        //    value = CheckBadChars(value, true);
        //    ChangeInternal(name, value);
        //}

        ///// <summary>
        ///// Throws an error if invalid chars are found in the header name or
        ///// value.
        ///// </summary>
        ///// <param name="name">The header name or header value string to
        ///// check.</param>
        ///// <param name="isHeaderValue">Whether the name parameter is a header
        ///// name or a header value.</param>
        ///// <returns></returns>
        //internal static string CheckBadChars(string name, bool isHeaderValue)
        //{

        //    if (name == null || name.Length == 0)
        //    {
        //        // empty name is invlaid
        //        if (!isHeaderValue)
        //        {
        //            throw new ArgumentException();
        //        }

        //        // empty value is OK
        //        return string.Empty;
        //    }

        //    if (isHeaderValue)
        //    {
        //        // VALUE check
        //        // Trim spaces from both ends
        //        name = name.Trim();

        //        // First, check for correctly formed multi-line value
        //        // Second, check for absence of CTL characters
        //        bool crlf = false;
        //        for (int i = 0; i < name.Length; ++i)
        //        {
        //            char c = name[i];
        //            if (c == 127 || (c < ' ' && !(c == '\t' || c == '\r' || c == '\n')))
        //            {
        //                throw new ArgumentException();
        //            }

        //            if (crlf)
        //            {
        //                if (!(c == ' ' || c == '\t'))
        //                {
        //                    throw new ArgumentException();
        //                }

        //                crlf = false;
        //            }
        //            else
        //            {
        //                if (c == '\n')
        //                {
        //                    crlf = true;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // NAME check
        //        // First, check for absence of separators and spaces
        //        if (name.IndexOfAny(ValidationHelper.InvalidParamChars) != -1)
        //        {
        //            throw new ArgumentException();
        //        }

        //        // Second, check for non CTL ASCII-7 characters (32-126)
        //        if (ContainsNonAsciiChars(name))
        //        {
        //            throw new ArgumentException();
        //        }
        //    }

        //    return name;
        //}

        //internal static bool IsValidToken(string token)
        //{
        //    return (token.Length > 0)
        //        && (token.IndexOfAny(ValidationHelper.InvalidParamChars) == -1)
        //        && !ContainsNonAsciiChars(token);
        //}

        //internal static bool ContainsNonAsciiChars(string token)
        //{
        //    for (int i = 0; i < token.Length; ++i)
        //    {
        //        if ((token[i] < 0x20) || (token[i] > 0x7e))
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        ///// <summary>
        ///// Throws an exception if the user passed in a reserved string as the
        ///// header name.
        ///// </summary>
        ///// <param name="headerName"></param>
        //internal void ThrowOnRestrictedHeader(string headerName)
        //{
        //    if (m_IsHttpWebHeaderObject && HInfo[headerName].IsRestricted)
        //    {
        //        throw new ArgumentException("Cannot update restricted header: " + headerName);
        //    }
        //}

        //// Our Public METHOD set, most are inherited from NameValueCollection,
        //// not all methods from NameValueCollection are listed, even though
        //// usable.
        ////
        //// This includes:
        //// Add(name, value)
        //// Add(header)
        //// this[name] {set, get}
        //// Remove(name), returns bool
        //// Remove(name), returns void
        //// Set(name, value)
        //// ToString()
        ////
        //// SplitValue(name, value)
        //// ToByteArray()
        //// ParseHeaders(char [], ...)
        //// ParseHeaders(byte [], ...)

        /// <summary>
        /// Inserts a header with the specified name and value into the
        /// collection.
        /// </summary>
        /// <param name="name">The name of the header that is being added to the
        /// collection.</param>
        /// <param name="value">The content of the header that is being added
        /// (its header-value).  If a header with the specified name already
        /// exists, this value is concatenated onto the existing header.</param>
        /// <remarks>
        /// If a header with the specified name already exists, the header that
        /// is being added is concatenated onto the existing header.
        /// <para>
        /// Throws an exception if the specified header name is the name of a
        /// special header.
        /// </para>
        /// </remarks>
        public void Add(string name, string value)
        {
            _headers.Add(name.ToLower(), value);
        }

        ///// <summary>
        ///// Inserts a new header into the collection.
        ///// </summary>
        ///// <param name="header">A header name/value pair, in the format
        ///// "myHeaderName:myValue".</param>
        ///// <remarks>
        ///// This method expects a string with the format "myName:myValue", and
        ///// parses the two parts out.
        ///// <para>
        ///// If a header with the specified name already exists, the header that
        ///// is being added is concatenated onto the existing header.
        ///// </para>
        ///// <para>
        ///// Throws an exception if the specified header name is the name of a
        ///// special header.
        ///// </para>
        ///// </remarks>
        //public void Add(string header)
        //{
        //    // Special headers are listed in the RestrictedHeaders object.

        //    if (ValidationHelper.IsBlankString(header))
        //    {
        //        throw new ArgumentNullException();
        //    }

        //    int colpos = header.IndexOf(':');

        //    // check for badly formed header passed in
        //    if (colpos < 0)
        //    {
        //        throw new ArgumentException();
        //    }

        //    string name = header.Substring(0, colpos);
        //    string value = header.Substring(colpos + 1);

        //    name = CheckBadChars(name, false);
        //    ThrowOnRestrictedHeader(name);
        //    value = CheckBadChars(value, true);

        //    head_val_coll.Add(name, value);
        //}

        ///// <summary>
        ///// Sets the specified header to the specified value.
        ///// </summary>
        ///// <param name="name">The header to set.</param>
        ///// <param name="value">The content of the header to set.</param>
        ///// <remarks>
        ///// Includes validation.
        ///// Throws an exception if the specified header name is the name of a
        ///// special header.
        ///// </remarks>
        //public void Set(String name, String value)
        //{
        //    // Special headers are listed in the RestrictedHeaders object.

        //    if (ValidationHelper.IsBlankString(name))
        //    {
        //        throw new ArgumentNullException("name");
        //    }

        //    name = CheckBadChars(name, false);
        //    ThrowOnRestrictedHeader(name);
        //    value = CheckBadChars(value, true);

        //    head_val_coll.Set(name, value);
        //}

        /// <summary>
        /// Removes the specified header from the collection.
        /// </summary>
        /// <param name="name">The name of the header to remove.</param>
        /// <remarks>
        /// Throws an exception if the specified header name is the name of a
        /// special header.
        /// </remarks>
        public void Remove(string name)
        {
            _headers.Remove(name.ToLower());
        }

        /// <summary>
        /// Returns the values for the specified header name.
        /// </summary>
        /// <param name="header">The name of the header.</param>
        /// <returns>An array of parsed string objects.</returns>
        /// <remarks>
        /// Takes a header name and returns a string array representing
        /// the individual values for that header.  For example, if the headers
        /// contain the following line:
        /// <code>
        /// Accept: text/plain, text/html
        /// </code>
        /// then <c>GetValues("Accept")</c> returns an array of
        /// two strings: "text/plain" and "text/html".
        /// </remarks>
        public string[] GetValues(string header)
        {
            if (_headers[header.ToLower()] != null)
            {
                return ((string)_headers[header.ToLower()]).Split(';');
            }

            return null;
        }

        ///// <summary>
        ///// Generates a string representation of the headers, that is ready to
        ///// be sent except for it being in String format.
        ///// </summary>
        ///// <returns>A string representation of the headers.</returns>
        ///// <remarks>
        ///// The format looks like the following:
        ///// <code>
        ///// Header-Name: Header-Value\r\n
        ///// Header-Name2: Header-Value2\r\n
        ///// ...
        ///// Header-NameN: Header-ValueN\r\n
        ///// \r\n
        ///// </code>
        ///// </remarks>
        //public override string ToString()
        //{
        //    // Iterates on all headers and add them line by line in form:
        //    // header: value
        //    string retString = "";
        //    for (int i = 0; i < head_val_coll.Count; i++)
        //    {
        //        // Try to be most efficient by calling Concat.
        //        // There is no Concat with 5 arguments.
        //        retString = String.Concat(retString, ((HeaderValuePair)head_val_coll[i]).headerAsKey, ": ", ((HeaderValuePair)head_val_coll[i]).value);
        //        retString = String.Concat(retString, "\r\n");
        //    }

        //    // Adds extra line return at the end of headers.
        //    retString = String.Concat(retString, "\r\n");

        //    // Return concatinated headers and
        //    return retString;
        //}

        ///// <summary>
        ///// Generates a byte array representation of the headers, that is ready
        ///// to be sent.
        ///// </summary>
        ///// <returns>An array of bytes.</returns>
        ///// <remarks>
        ///// This method serializes the headers into a byte array that can be
        ///// sent over the network.  The format looks like:
        ///// <code>
        ///// Header-Name1: Header-Value1\r\n
        ///// Header-Name2: Header-Value2\r\n
        ///// ...
        ///// Header-NameN: Header-ValueN\r\n
        ///// \r\n
        ///// </code>
        ///// </remarks>
        //public byte[] ToByteArray()
        //{
        //    // Performance Note:  We aren't doing a single copy/covert run,
        //    // because (according to Demitry), it's cheaper to copy the headers
        //    // twice than to call the UNICODE-to-ANSI conversion code many
        //    // times.  (The code before used to know the size of the output.)

        //    // Make sure the buffer is big enough.

        //    string tempStr = ToString();

        //    // Use the String of headers, convert to Char Array, then convert to
        //    // Bytes, serializing finally into the buffer, along the way.
        //    byte[] buffer = Encoding.UTF8.GetBytes(tempStr);

        //    return buffer;
        //}

        ///// <summary>
        ///// Tests whether the specified HTTP header can be set.
        ///// </summary>
        ///// <param name="headerName">Name for the header.</param>
        ///// <returns></returns>
        ///// <remarks>
        ///// Throws an exception if the header name is blank, contains illegal
        ///// characters, or contains characters that are reserved by the HTTP
        ///// protocol.
        ///// </remarks>
        //public static bool IsRestricted(string headerName)
        //{
        //    if (ValidationHelper.IsBlankString(headerName))
        //    {
        //        throw new ArgumentNullException("headerName");
        //    }

        //    return HInfo[CheckBadChars(headerName, false)].IsRestricted;
        //}

        /// <summary>
        /// Creates an empty collection of WEB headers.
        /// </summary>
        public WebHeaderCollection()
        {
            _headers = new Hashtable();
        }

        ///// <summary>
        ///// Private constructor, called internally.
        ///// </summary>
        ///// <param name="internalCreate">Whether this is an HTTP headers
        ///// object.</param>
        //internal WebHeaderCollection(bool internalCreate)
        //{
        //    m_IsHttpWebHeaderObject = internalCreate;
        //}

        ///// <summary>
        ///// Calculates the number of bytes needed to store the headers.
        ///// </summary>
        ///// <returns></returns>
        //internal int byteLength()
        //{
        //    int ret = 0;
        //    // Runs for all collection and adds length of header and value
        //    // strings
        //    for (int i = 0; i < head_val_coll.Count; i++)
        //    {
        //        ret += ((HeaderValuePair)head_val_coll[i]).headerAsKey.Length;
        //        ret += 2; //for the ": "
        //        ret += ((HeaderValuePair)head_val_coll[i]).value.Length;
        //        ret += 2; //for the "\r\n"
        //    }

        //    ret += 2; //for the final "\r\n"

        //    return ret;
        //}

        /// <summary>
        /// Returns the string value for the header.
        /// </summary>
        /// <param name="header">The name of the header.</param>
        /// <value>A string containing the value. If no value is present,
        /// returns <itemref>null</itemref>.</value>
        public string this[string header]
        {
            get
            {
                // If header is not present, then pair is null. Return null
                // string
                if (_headers[header] == null)
                {
                    return null;
                }

                // Pair was found. Return the value string of it
                return _headers[header] as string;
            }
        }

        /// <summary>
        /// Gets the number of headers in the collection.
        /// </summary>
        /// <value>An <b>Int32</b> indicating the number of headers in a
        /// request.</value>
        public int Count
        {
            get
            {
                return _headers.Count;
            }
        }

        /// <summary>
        /// Gets all header names (keys) in the collection.
        /// </summary>
        /// <value>An array of type <b>String</b> containing all header names in
        /// a Web request.</value>
        public string[] AllKeys
        {
            get
            {
                ArrayList tempCollection = new ArrayList();
                foreach (string header in _headers.Keys)
                //for (int i = 0; i < _headers.Count; i++)
                {
                    tempCollection.Add(header);
                }

                string[] stringArray = new string[tempCollection.Count];
                return (string[])tempCollection.ToArray(typeof(string));
            }
        }

        ///// <summary>
        ///// Copies the headers into the byte array starting at bytes[offset].
        ///// If the byte array is too small to hold the data, an
        ///// ArgumentException is thrown.
        ///// </summary>
        ///// <param name="bytes">The array to copy.</param>
        ///// <param name="offset">The offset to the beginning of the data to be
        ///// copied into the WEB Headers collection.</param>
        ///// <returns>How many bytes were copied.</returns>
        //internal int copyTo(byte[] bytes, int offset)
        //{
        //    // Create array representing the headers
        //    byte[] headersBytes = ToByteArray();
        //    // Copy to destination
        //    headersBytes.CopyTo(bytes, offset);
        //    // Return count of bytes copied.
        //    return headersBytes.Length;
        //}
    }
}
