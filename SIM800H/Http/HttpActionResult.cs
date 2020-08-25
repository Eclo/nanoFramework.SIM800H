namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Class with the result of an <see cref="HttpAction"/> request.
    /// </summary>
    public class HttpActionResult
    {
        public HttpAction Action { get; private set; }
        public int StatusCode { get; private set; }
        public int DataLenght { get; private set; }

        public HttpActionResult(HttpAction action, int statusCode, int dataLenght)
        {
            this.Action = action;
            this.StatusCode = statusCode;
            this.DataLenght = dataLenght;
        }
    }
}
