using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grapevine.Client;
using Grapevine.Shared;
using KPServices;
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
        public Uri RequestUri;

        public KPRestClient(string Host, int Port, bool UseHTTPS)
        { 
            RestClient = new RestClient
            {
                Scheme = UseHTTPS ? UriScheme.Https : UriScheme.Http,
                Host = Host,
                Port = Port              
            };
            UriBuilder ub = new UriBuilder()
            {
                Host = Host
            };
            RequestUri = ub.Uri;
        }

        public bool Login(string Username, string Password)
        {
            RestRequest request = new RestRequest("/login")
            {
                Payload = JsonConvert.SerializeObject(
                    new
                    {
                        Username = Username,
                        Password = Password
                    }),
                HttpMethod = HttpMethod.POST,
                ContentType = ContentType.JSON
            };
            var response = RestClient.Execute(request);
            return response.StatusCode == HttpStatusCode.Ok;
        }

        public bool IsLogged
        {
            get
            {
                try
                {
                    RestRequest request = new RestRequest("/isLogged")
                    {
                        HttpMethod = HttpMethod.GET,
                        RequestUri = RequestUri
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
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public Universe GetUniverse()
        {
            RestRequest request = new RestRequest("/universe")
            {
                HttpMethod = HttpMethod.GET,
                RequestUri = RequestUri
            };
            var response = RestClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.Ok)
            {
                var jsonResponse = JsonConvert.DeserializeObject<KPRestResponse>(
                    response.GetContent());
                if (jsonResponse.Error == "none")
                    return Universe.FromString(jsonResponse.Content);
            }

            return null;
        }

        public byte[] GetPublicKey()
        {
            RestRequest request = new RestRequest("/publicKey")
            {
                HttpMethod = HttpMethod.GET,
                RequestUri = RequestUri
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
            RestRequest request = new RestRequest("/privateKey")
            {
                HttpMethod = HttpMethod.GET,
                RequestUri = RequestUri
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
