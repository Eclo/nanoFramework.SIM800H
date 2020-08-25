namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Phone Funcionality 
    /// </summary>
    public enum PhoneFuncionality
    {
        /// <summary>
        /// Minimum funcionality: minimum current consumption, RF and SIM are off
        /// </summary>
        Minimum,
        /// <summary>
        /// Full funcionality (default)
        /// </summary>
        Full,
        /// <summary>
        /// Flight mode: disable tx and rx RF circuits
        /// </summary>
        FligthMode
    }
}
