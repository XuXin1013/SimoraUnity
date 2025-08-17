using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using RSG;
using UnityEngine;
using UnityEngine.Networking;

namespace SimoraUnity.Http
{
    public class Client : MonoBehaviour
    {
        [HideInInspector]
        public string baseUrl;
        [HideInInspector]
        public bool enableRequestLogs;

        public Promise<Response<T>> Get<T>(string url, RequestConfig config = null)
        {
            config = config ?? CreateConfig();
            config.Url = url;
            config.Method = "get";
            var req = CreateRequest(config);

            return Do<T>(req, config);
        }

        public Promise<Response<T>> Post<T>(string url, dynamic data, RequestConfig config = null)
        {
            config = config ?? CreateConfig();
            config.Url = url;
            config.Method = "post";
            config.Data = data;
            var req = CreateRequest(config);

            return Do<T>(req, config);
        }

        public Promise<Response<T>> Post<T>(string url, Dictionary<string, string> data, RequestConfig config = null)
        {
            config = config ?? CreateConfig();
            config.Url = url;
            config.Method = "post";
            config.Data = data;
            var req = CreateRequest(config);

            return Do<T>(req, config);
        }

        public Promise<Response<T>> Do<T>(UnityWebRequest req)
        {
            var promise = new Promise<Response<T>>((resolve, reject) =>
            {
                StartCoroutine(SendRequest(req, resolve, reject));
            });

            return promise;
        }

        private Promise<Response<T>> Do<T>(UnityWebRequest req, RequestConfig config)
        {
            var promise = new Promise<Response<T>>((resolve, reject) =>
            {
                StartCoroutine(SendRequest(req, resolve, reject, config));
            });

            return promise;
        }

        private UnityWebRequest CreateRequest(RequestConfig config)
        {
            UnityWebRequest req;
            switch (config.Method)
            {
                case "get":
                    req = UnityWebRequest.Get(config.AbsoluteUrl);
                    break;
                case "post":
                    req = new UnityWebRequest(config.AbsoluteUrl, "POST");
                    req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(config.Data)));
                    req.uploadHandler.contentType = "application/json";
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.SetRequestHeader("Accept", "application/json");
                    req.SetRequestHeader("Content-Type", "application/json");
                    break;
                default:
                    req = new UnityWebRequest(config.AbsoluteUrl, config.Method);
                    break;
            }
            req.redirectLimit = 0;

            return req;
        }

        private IEnumerator SendRequest<T>(UnityWebRequest req, Action<Response<T>> resolve, Action<Exception> reject, RequestConfig config = null)
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success || req.result == UnityWebRequest.Result.ProtocolError)
            {
                var resp = new Response<T>(new Request(req), (int)req.responseCode, req.GetResponseHeaders(), req.downloadHandler.text, config);
                resolve(resp);
            }
            else
            {
                reject(new Exception(req.error));
            }
        }

        private RequestConfig CreateConfig()
        {
            var config = new RequestConfig();
            config.BaseUrl = baseUrl;
            return config;
        }
    }
}