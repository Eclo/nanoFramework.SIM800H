namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Result of the execution of an AT command.
    /// </summary>
    public class AtCommandResult
    {
        private ReturnedState _result;
        public ReturnedState Result
        {
            get { return _result; }
            private set { _result = value; }
        }

        private string _response = string.Empty;
        public string Response
        {
            get { return _response; }
            private set { _response = value; }
        }

        public AtCommandResult(ReturnedState state)
        {
            _result = state;
        }

        public AtCommandResult(ReturnedState state, string response)
        {
            _result = state;
            _response = response;
        }
    }

}
