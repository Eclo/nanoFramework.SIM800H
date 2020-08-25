namespace Eclo.nanoFramework.SIM800H
{
    static internal class Prompts
    {
        internal const string AT = "AT";

        internal const string CREG = "+CREG:";
        internal const string CGREG = "+CGREG:";
        internal const string CMTI = "+CMTI:";
        internal const string CMGS_PROMPT = "+CMGS:";
        internal const string CMGS = "+CMGS";
        internal const string CDS = "+CDS:";
        internal const string CSQ = "+CSQ";
        internal const string COPS = "COPS?";
        internal const string CIFSR = "+CIFSR";
        internal const string CSMINS_PROMPT = "+CSMINS:";
        internal const string CSMINS = "+CSMINS";
        internal const string CIICR = "+CIICR";
        internal const string CGATT = "+CGATT";
        internal const string CIPSTATUS = "+CIPSTATUS";
        internal const string CMTE = "+CMTE:";
        internal const string CFUN_PROMPT = "+CFUN:";
        internal const string CFUN_1_PROMPT = "+CFUN=1";
        internal const string CFUN = "+CFUN";
        internal const string CPOWD = "+CPOWD=1";
        internal const string CNTP = "+CNTP";
        internal const string CGMM = "+CGMM";
        internal const string CGMR = "+CGMR";
        internal const string CMGD = "+CMGD";
        internal const string CMGDA = "+CMGDA";
        internal const string SAPBR = "+SAPBR";
        internal const string HTTPACTION_PROMPT = "+HTTPACTION:";
        internal const string NORMAL_POWER_DOWN = "NORMAL POWER DOWN";
        internal const string Call_Ready = "Call Ready";
        internal const string SMS_Ready = "SMS Ready";
        internal const string PDP_DEACT = "+PDP: DEACT";
        internal const string SAPBR_DEACT = "+SAPBR 1: DEACT";
        internal const string UNDER_VOLTAGE_WARNING = "UNDER-VOLTAGE WARNNING";// WARNNING with two 'NN' to match module prompt
        internal const string OVER_VOLTAGE_WARNING = "OVER-VOLTAGE WARNNING";// WARNNING with two 'NN' to match module prompt
        internal const string UNDER_VOLTAGE_POWER_DOWN = "UNDER-VOLTAGE POWER DOWN";
        internal const string OVER_VOLTAGE_POWER_DOWN = "OVER-VOLTAGE POWER DOWN";
        internal const string CONNECT_OK_MUX = ", CONNECT OK";
        internal const string CONNECT_FAIL_MUX = ", CONNECT FAIL";
        internal const string CLOSE_OK_MUX = ", CLOSE OK";
        internal const string CLOSED_MUX = ", CLOSED";
        internal const string CPIN_NOT_READY = "+CPIN: NOT READY";
        internal const string CPIN_READY = "+CPIN: READY";
        internal const string CPIN_NOT_INSERTED = "+CPIN: NOT INSERTED";
        internal const string CPIN = "+CPIN?";
        internal const string CIPCLOSE = "+CIPCLOSE";
        internal const string CIPSSL = "+CIPSSL";
        internal const string CIPSTART = "+CIPSTART";
        internal const string CIPSEND = "+CIPSEND";
        internal const string CSMP = "+CSMP";
        internal const string IPR = "+IPR";
        internal const string GSN = "+GSN";
        internal const string RDY = "RDY";
        internal const string OK = "OK";
        internal const string ERROR = "ERROR";
        internal const string CMEERROR = "+CME ERROR: ";
        internal const string DATA_ACCEPT = "DATA ACCEPT:";
        internal const string SEND_FAIL = "SEND FAIL";
        internal const string RECEIVE = "+RECEIVE,";
        internal const string CPMS = "+CPMS";
        internal const string CMGR = "+CMGR";
        internal const string CSTT = "+CSTT";
        internal const string CCLK = "+CCLK?";
        internal const string CBC = "+CBC";
        internal const string CIPGSMLOC = "+CIPGSMLOC";
        internal const string HTTPHEAD = "+HTTPHEAD";
        internal const string HTTPINIT = "+HTTPINIT";
        internal const string HTTPTERM = "+HTTPTERM";
        internal const string HTTPSCONT = "+HTTPSCONT";
        internal const string HTTPSTATUS = "+HTTPSTATUS?";
        internal const string HTTPPARA = "+HTTPPARA=";
        internal const string HTTPACTION = "+HTTPACTION=";
        internal const string HTTPSSL = "+HTTPSSL=";
        internal const string HTTPREAD = "+HTTPREAD";
        internal const string HTTPDATA = "+HTTPDATA=";

        #region File storage prompts

        internal const string FSDRIVE = "+FSDRIVE";
        internal const string FSREAD = "+FSREAD=";
        internal const string FSCREATE = "+FSCREATE=";
        internal const string FSWRITE = "+FSWRITE=";
        internal const string FSDEL = "+FSDEL=";
        internal const string FSFLSIZE = "+FSFLSIZE";
        internal const string FSMEM = "+FSMEM";

        #endregion

        #region MMS Client prompts

        internal const string CMMS = "+CMMS";
        internal const string CMMSPROTO = "+CMMSPROTO";
        internal const string CMMSSENDCFG = "+CMMSSENDCFG";
        internal const string CMMSDOWN = "+CMMSDOWN";
        internal const string CONNECT = "CONNECT";
        internal const string CMMSRECP = "+CMMSRECP";
        internal const string CMMSSEND = "+CMMSSEND";
        internal const string CMMSEDIT = "+CMMSEDIT";
        internal const string CMMSINIT = "+CMMSINIT";
        internal const string CMMSVIEW = "+CMMSVIEW";
        internal const string CMMSTERM = "+CMMSTERM";
        internal const string CMMSTIMEOUT = "+CMMSTIMEOUT";
        internal const string CMMSSCONT = "+CMMSSCONT";

        #endregion

        internal const string SendPrompt = "> ";
        internal const string DonwloadPrompt = "DOWNLOAD";
    }
}
