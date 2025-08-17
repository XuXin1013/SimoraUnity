using System.Collections.Generic;

namespace SimoraUnity.Http
{
    public class RequestConfig
    {
        public string Url;

        public string Method = "get";

        public string BaseUrl = "";

        public Dictionary<string, string> Headers;

        public Dictionary<string, string> Params;

        public dynamic Data;

        public string AbsoluteUrl => BaseUrl.Length > 0 ? BaseUrl + Url : Url;
    }
}