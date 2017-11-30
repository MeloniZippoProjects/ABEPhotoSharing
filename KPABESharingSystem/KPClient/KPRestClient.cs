﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grapevine.Client;
using Grapevine.Shared;
using Newtonsoft.Json;

namespace KPClient
{
    public class KPRestClient
    {
        internal class KPRestResponse
        {
            public string Error { get; set; } = "none";
            public string ErrorDescription { get; set; }
            public string Content { get; set; }
            public string ContentDescription { get; set; }
        }

        public RestClient RestClient;

        public KPRestClient(string Host, int Port)
        {
            RestClient = new RestClient
            {
                Host = Host,
                Port = Port
            };
        }

        public bool Login(string Username, string Password)
        {
            RestRequest request = new RestRequest
            {
                Payload = JsonConvert.SerializeObject(
                    new
                    {
                        Username = Username,
                        Password = Password
                    }),
                Encoding = Encoding.UTF8,
                HttpMethod = HttpMethod.POST,
                ContentType = ContentType.TXT,
                RequestUri = new Uri("/login")
            };
            var response = RestClient.Execute(request);
            return response.StatusCode == HttpStatusCode.Ok;
        }

        public bool IsLogged
        {
            get
            {
                RestRequest request = new RestRequest
                {
                    RequestUri = new Uri("/isLogged"),
                    HttpMethod = HttpMethod.GET
                };
                var response = RestClient.Execute(request);
                if (response.StatusCode == HttpStatusCode.Ok)
                {
                    var KPResponse = JsonConvert.DeserializeObject<KPRestResponse>(
                        response.GetContent());
                    return KPResponse.Content == "logged_in";
                }
                else
                {
                    return false;
                }
            }
        }

        public byte[] GetPublicKey()
        {
            RestRequest request = new RestRequest
            {
                RequestUri = new Uri("/getPublicKey"),
                HttpMethod = HttpMethod.GET
            };
            var response = RestClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.Ok)
            {
                var jsonResponse = JsonConvert.DeserializeObject<KPRestResponse>(
                    response.GetContent());
                if (jsonResponse.Error == "none")
                    return Convert.FromBase64String(jsonResponse.Content);
            }

            return null;
        }

        public byte[] GetPrivateKey()
        {
            RestRequest request = new RestRequest
            {
                RequestUri = new Uri("/getPrivateKey"),
                HttpMethod = HttpMethod.GET
            };
            var response = RestClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.Ok)
            {
                var jsonResponse = JsonConvert.DeserializeObject<KPRestResponse>(
                    response.GetContent());
                if (jsonResponse.Error == "none")
                    return Convert.FromBase64String(jsonResponse.Content);
            }

            return null;
        }
    }
}
