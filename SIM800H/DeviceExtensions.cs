////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

using System.Collections;

namespace Eclo.nanoFramework.SIM800H
{
    internal static class DeviceExtensions
    {
        internal static object FindAndRemove(this ArrayList array, object value)
        {
            if (value != null && array.Count > 0)
            {
                int index = array.IndexOf(value);

                if (index > -1)
                {
                    object retValue = array[index];
                    array.RemoveAt(index);
                    return retValue;
                }
            }

            return null;
        }

        internal static int FindItemThatContains(this ArrayList array, string value)
        {
            if (value != null && array.Count > 0)
            {
                for (int i = 0; i < array.Count; i++)
                {
                    if (array[i].GetType() == typeof(string))
                    {
                        if (((string)array[i]).IndexOf(value) > -1)
                        {
                            return i;
                        }
                    }
                }
            }

            return -1;
        }

        internal static string FindAndRemoveItemThatContains(this ArrayList array, string value)
        {
            if (value != null && array.Count > 0)
            {
                for (int i = 0; i < array.Count; i++)
                {
                    if (array[i].GetType() == typeof(string))
                    {
                        if (((string)array[i]).IndexOf(value) > -1)
                        {
                            string retValue = (string)array[i];
                            array.RemoveAt(i);
                            return retValue;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Find a string item that looks like an IP address (has for substrings separated by a '.').
        /// </summary>
        /// <returns>The index of the object that looks like an IP address</returns>
        internal static int FindItemThatLooksIpAddress(this ArrayList array)
        {
            if (array.Count > 0)
            {
                for (int i = 0; i < array.Count; i++)
                {
                    if (array[i].GetType() == typeof(string))
                    {
                        string[] tentativeIp = ((string)array[i]).Split('.');
                        if (tentativeIp.Length == 4)
                        {
                            return i;
                        }
                    }
                }
            }

            return -1;
        }

        internal static int LastIndexOf(this ArrayList array, object value)
        {
            int index = -1;

            if (value != null && array.Count > 0)
            {
                for (int i = 0; i < array.Count; i++)
                {
                    if (array[i].Equals(value))
                    {
                        index = i;
                    }
                }
            }

            return index;
        }
    }
}
