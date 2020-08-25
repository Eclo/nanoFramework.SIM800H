namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Warning conditions
    /// </summary>
    public enum WarningCondition
    {
        /// <summary>
        ///  Under Voltage Power Down
        /// </summary>
        UnderVoltagePowerDown,
        /// <summary>
        /// Under Voltage warning
        /// </summary>
        UnderVoltageWarning,
        /// <summary>
        ///  Over Voltage Power Down
        /// </summary>
        OverVoltagePowerDown,
        /// <summary>
        /// Over Voltage warning
        /// </summary>
        OverVoltageWarning,
        /// <summary>
        /// Module temperature is not normal
        /// </summary>
        TemperatureWarning
    }
}
