////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

namespace SIM800HSamples
{
    using Eclo.nanoFramework.SIM800H;
    static class SamplesExtensions
    {
        internal static string GetDescription(this NetworkRegistrationState value, string network)
        {
            switch (value)
            {
                case NetworkRegistrationState.Error:
                    return "### " + network + " network registration ERROR ###";

                case NetworkRegistrationState.NotSearching:
                    return "### not searching for " + network + " network ###";

                case NetworkRegistrationState.Registered:
                    return "!!! registered with " + network + " network !!!";

                case NetworkRegistrationState.RegistrationDenied:
                    return "### " + network + " network registration DENIED ###";

                case NetworkRegistrationState.Roaming:
                    return "!!! registered with " + network + " network (roaming) !!!";

                case NetworkRegistrationState.Searching:
                    return "... searching " + network + " network ...";

                case NetworkRegistrationState.Unknown:
                default:
                    return "";
            }
        }

        internal static string GetWarningDescription(this WarningCondition value)
        {
            switch (value)
            {
                case WarningCondition.OverVoltagePowerDown:
                    return "### Over-Voltage Power Down ###";

                case WarningCondition.OverVoltageWarning:
                    return "### Over-Voltage warning ###";

                case WarningCondition.TemperatureWarning:
                    return "### High Temperature warning ###";

                case WarningCondition.UnderVoltagePowerDown:
                    return "### Under-Voltage Power Down ###";

                case WarningCondition.UnderVoltageWarning:
                    return "### Under-Voltage warning ###";

                default:
                    return "";
            }
        }
        
        internal static string GetSignalStrengthDescription(this SignalStrength value)
        {
            switch (value)
            {
                case SignalStrength.Error:
                    return "error";

                case SignalStrength.Strong:
                    return "strong";

                case SignalStrength.Unknown:
                    return "unknown";

                case SignalStrength.VeryStrong:
                    return "very strong";

                case SignalStrength.VeryWeak:
                    return "very weak";

                case SignalStrength.Weak:
                    return "weak";

                default:
                    return "";
            }
        }

        internal static string GetDescription(this PowerStatus value)
        {
            switch (value)
            {
                case PowerStatus.FlightMode:
                    return "flight mode";

                case PowerStatus.Minimum:
                    return "minimum";

                case PowerStatus.Off:
                    return "OFF";

                case PowerStatus.On:
                    return "ON";

                case PowerStatus.PowerOnSequenceIsRunning:
                    return "power on sequence is running";

                case PowerStatus.Unknown:
                    return "unknown";

                default:
                    return "";
            }
        }
    }
}