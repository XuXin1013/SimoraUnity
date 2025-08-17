using UnityEngine.Networking;

namespace SimoraUnity.Http
{
    public class Request
    {
        public string Url;

        public string Method;

        public Request(UnityWebRequest webRequest)
        {
            Url = webRequest.url;
            Method = webRequest.method;
        }
    }
}