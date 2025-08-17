using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace SimoraUnity.Http
{
    public class Response<T>
    {
        public readonly Request Request;

        public readonly int StatusCode;

        public readonly Dictionary<string, string> Headers;

        public bool Success => _result.Success;

        public T Data => _result.Data;

        public string Message => _result.Message;

        public readonly RequestConfig Config;

        public readonly string Raw;

        private readonly Result<T> _result;

        public Response(Request request, int statusCode, Dictionary<string, string> headers, string rawContent, RequestConfig config)
        {
            Request = request;
            StatusCode = statusCode;
            Headers = headers;
            Raw = rawContent;
            Config = config;

            if (headers.ContainsKey("Content-Type") && headers["Content-Type"].StartsWith("application/json"))
            {
                _result = JsonConvert.DeserializeObject<Result<T>>(Raw);
            }
            else
            {
                _result = null;
            }
        }
    }
}