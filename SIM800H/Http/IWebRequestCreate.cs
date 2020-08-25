namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// The interface for creating <see cref="Eclo.nanoFramework.SIM800H.WebRequest"/> class
    /// objects.
    /// </summary>
    internal interface IWebRequestCreate
    {
        /// <summary>
        /// Creates an instance of a class derived from
        /// <itemref>WebRequest</itemref>.
        /// </summary>
        /// <param name="uri">The URI for initialization of the class that is
        /// derived from <itemref>WebRequest</itemref>.</param>
        /// <returns>
        /// An instance of the class that is derived from
        /// <itemref>WebRequest</itemref>.
        /// </returns>
        WebRequest Create(Uri uri);

    } // interface IWebRequestCreate
}
