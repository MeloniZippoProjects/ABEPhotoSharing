using System;
using System.Net;
using System.Text;
using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Server.Attributes;
using Grapevine.Shared;
using KPTrustedParty.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HttpStatusCode = Grapevine.Shared.HttpStatusCode;

namespace KPTrustedParty
{
    [RestResource]
    public class KpRestServer
    {
        public static string SessionCookie { get; set; } = "KPABESESSIONID";

        internal class KpRestResponse
        {
            public string Error { get; set; } = "none";
            public string ErrorDescription { get; set; }
            public string Content { get; set; }
            public string ContentDescription { get; set; }
        }

        public static void LoginNeededMessage(IHttpResponse response)
        {
            string jsonResponse = JsonConvert.SerializeObject(
                new KpRestResponse
                {
                    Error = "login_needed",
                    ErrorDescription = "You need to login before accessing this resource"
                });
            response.StatusCode = HttpStatusCode.Forbidden;
            response.SendResponse(JsonConvert.SerializeObject(jsonResponse));
        }

        private static void BadRequestResponse(IHttpResponse response)
        {
            string jsonResponse = JsonConvert.SerializeObject(
                new KpRestResponse
                {
                    Error = "bad_request_format",
                    ErrorDescription = "The request must be a json object with 'username' and 'password' field"
                });
            response.StatusCode = HttpStatusCode.BadRequest;
            response.SendResponse(JsonConvert.SerializeObject(jsonResponse));
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/login")]
        public IHttpContext LoginUser(IHttpContext context)
        {
            IHttpRequest request = context.Request;
            IHttpResponse response = context.Response;
            response.ContentType = ContentType.JSON;
            Encoding responseEncoding = response.ContentEncoding;
            byte[] payloadBytes = responseEncoding.GetBytes(request.Payload);
            string payload = Encoding.UTF8.GetString(Encoding.Convert(responseEncoding, Encoding.UTF8, payloadBytes));

            JObject jsonRequest;
            try
            {
                jsonRequest = JObject.Parse(payload);
            }
            catch (JsonReaderException)
            {
                BadRequestResponse(response);
                return context;
            }

            if (jsonRequest["Username"] == null || jsonRequest["Password"] == null)
            {
                BadRequestResponse(response);
                return context;
            }

            string username = (string) jsonRequest["Username"];
            string password = (string) jsonRequest["Password"];

            KpDatabase.Token token = KpDatabase.LoginUser(username, password);

            KpRestResponse jsonResponse = new KpRestResponse();

            if (token == null)
            {
                jsonResponse.Error = "wrong_credentials";
                jsonResponse.ErrorDescription = "This combination of user and password does not exist";
                response.StatusCode = HttpStatusCode.Forbidden;
            }
            else
            {
                jsonResponse.Error = "none";
                jsonResponse.ErrorDescription = "login_ok";
                response.StatusCode = HttpStatusCode.Ok;
                response.AppendCookie(new Cookie
                {
                    Domain = request.Headers["Host"],
                    Path = "/",
                    Name = SessionCookie,
                    Value = token.TokenString,
                    Expires = token.ExpirationDateTime
                });
            }

            response.SendResponse(JsonConvert.SerializeObject(jsonResponse));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/isLogged")]
        public IHttpContext IsLogged(IHttpContext context)
        {
            context.Response.ContentType = ContentType.JSON;

            KpRestResponse jsonResponse = new KpRestResponse();

            if (KpDatabase.UserLogged(context.Request.Cookies[SessionCookie]?.Value) != null)
            {
                jsonResponse.Content = "logged_in";
                context.Response.StatusCode = HttpStatusCode.Ok;
                context.Response.SendResponse(JsonConvert.SerializeObject(jsonResponse));
                return context;
            }

            jsonResponse.Content = "not_logged";
            context.Response.StatusCode = HttpStatusCode.Ok;
            context.Response.SendResponse(JsonConvert.SerializeObject(jsonResponse));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/universe")]
        public IHttpContext GetUniverse(IHttpContext context)
        {
            IHttpResponse response = context.Response;
            context.Response.ContentType = ContentType.JSON;

            KpDatabase.User user = KpDatabase.UserLogged(context.Request.Cookies[SessionCookie]?.Value);
            if (user != null)
            {
                KpRestResponse jsonResponse = new KpRestResponse
                {
                    Error = "none",
                    Content = TpServer.Universe.ToString(),
                    ContentDescription = "Universe"
                };
                response.StatusCode = HttpStatusCode.Ok;
                response.SendResponse(JsonConvert.SerializeObject(jsonResponse));
                return context;
            }

            LoginNeededMessage(response);
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/publicKey")]
        public IHttpContext GetPublicKey(IHttpContext context)
        {
            IHttpResponse response = context.Response;
            context.Response.ContentType = ContentType.JSON;

            KpDatabase.User user = KpDatabase.UserLogged(context.Request.Cookies[SessionCookie]?.Value);
            if (user != null)
            {
                byte[] publicKey = TpServer.KpPublicKey;
                KpRestResponse jsonResponse = new KpRestResponse
                {
                    Error = "none",
                    Content = Convert.ToBase64String(publicKey),
                    ContentDescription = "Public Key"
                };
                response.StatusCode = HttpStatusCode.Ok;
                response.SendResponse(JsonConvert.SerializeObject(jsonResponse));
                return context;
            }

            LoginNeededMessage(response);
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/privateKey")]
        public IHttpContext GetPrivateKey(IHttpContext context)
        {
            IHttpResponse response = context.Response;
            context.Response.ContentType = ContentType.JSON;

            KpDatabase.User user = KpDatabase.UserLogged(context.Request.Cookies[SessionCookie]?.Value);
            if (user != null)
            {
                KpRestResponse kpRestResponse;
                if (user.PrivateKey == null)
                {
                    kpRestResponse = new KpRestResponse
                    {
                        Error = "no_private_key",
                        ErrorDescription = "This user has no private key"
                    };
                    response.StatusCode = HttpStatusCode.Ok;
                    response.SendResponse(JsonConvert.SerializeObject(kpRestResponse));
                }
                else
                {
                    kpRestResponse = new KpRestResponse
                    {
                        Error = "none",
                        Content = Convert.ToBase64String(user.PrivateKey),
                        ContentDescription = "User Private Key"
                    };
                    response.StatusCode = HttpStatusCode.Ok;
                    response.SendResponse(JsonConvert.SerializeObject(kpRestResponse));
                }
                return context;
            }
            else
            {
                LoginNeededMessage(response);
                return context;
            }
        }
    }
}