using System;
using Grapevine.Client;
using Grapevine.Shared;
using KPServices;
using Newtonsoft.Json;

namespace KPClient
{
    public class KpRestClient
    {
        internal class KpRestResponse
        {
            public string Error { get; set; } = "none";
            public string ErrorDescription { get; set; }
            public string Content { get; set; }
            public string ContentDescription { get; set; }
        }

        public RestClient RestClient;
        public Uri RequestUri;

        public KpRestClient(string host, int port, bool useHttps)
        {
            RestClient = new RestClient
            {
                Scheme = useHttps ? UriScheme.Https : UriScheme.Http,
                Host = host,
                Port = port
            };
            UriBuilder ub = new UriBuilder()
            {
                Host = host
            };
            RequestUri = ub.Uri;
        }

        public bool Login(string username, string password)
        {
            RestRequest request = new RestRequest("/login")
            {
                Payload = JsonConvert.SerializeObject(
                    new
                    {
                        Username = username,
                        Password = password
                    }),
                HttpMethod = HttpMethod.POST,
                ContentType = ContentType.JSON
            };
            IRestResponse response = RestClient.Execute(request);
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
                    IRestResponse response = RestClient.Execute(request);
                    if (response.StatusCode == HttpStatusCode.Ok)
                    {
                        KpRestResponse kpResponse = JsonConvert.DeserializeObject<KpRestResponse>(
                            response.GetContent());
                        return kpResponse.Content == "logged_in";
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
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
            IRestResponse response = RestClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.Ok)
            {
                KpRestResponse jsonResponse = JsonConvert.DeserializeObject<KpRestResponse>(
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
            IRestResponse response = RestClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.Ok)
            {
                KpRestResponse jsonResponse = JsonConvert.DeserializeObject<KpRestResponse>(
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
            IRestResponse response = RestClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.Ok)
            {
                KpRestResponse jsonResponse = JsonConvert.DeserializeObject<KpRestResponse>(
                    response.GetContent());
                if (jsonResponse.Error == "none")
                    return Convert.FromBase64String(jsonResponse.Content);
            }

            return null;
        }
    }
}