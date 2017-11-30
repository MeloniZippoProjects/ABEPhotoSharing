using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.SessionState;
using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Server.Attributes;
using Grapevine.Shared;
using KPServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HttpStatusCode = Grapevine.Shared.HttpStatusCode;

namespace KPTrustedParty
{
    [RestResource]
    public class KPRestServer
    {
        public static string SessionCookie { get; set; } = "KPABESESSIONID";

        /*
        public static KPDatabase.User UserLogged(IHttpContext context)
        {
            var sessionToken = context.Request.Cookies[SessionCookie]?.Value;

            if (sessionToken != null)
            {
                return KPDatabase.UserLogged(sessionToken);
            }

            return null;
        }
        */

        internal class KPRestResponse
        {
            public string Error { get; set; } = "none";
            public string ErrorDescription { get; set; }
            public string Content { get; set; }
            public string ContentDescription { get; set; }
        }

        public static void LoginNeededMessage(IHttpResponse response)
        {
            var jsonResponse = JsonConvert.SerializeObject(
                new KPRestResponse {
                    Error = "login_needed",
                    ErrorDescription = "You need to login before accessing this resource"
                });
            response.StatusCode = HttpStatusCode.Forbidden;
            response.SendResponse(JsonConvert.SerializeObject(jsonResponse));
        }

        private static void BadRequestResponse(IHttpResponse response)
        {
            var jsonResponse = JsonConvert.SerializeObject(
                new KPRestResponse {
                    Error = "bad_request_format",
                    ErrorDescription = "The request must be a json object with 'username' and 'password' field"
                });
            response.StatusCode = HttpStatusCode.BadRequest;
            response.SendResponse(JsonConvert.SerializeObject(jsonResponse));
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/login")]
        public IHttpContext LoginUser(IHttpContext context)
        {
            var request = context.Request;
            var response = context.Response;
            response.ContentType = ContentType.JSON;
            var responseEncoding = response.ContentEncoding;
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

            var username = (string) jsonRequest["Username"];
            var password = (string) jsonRequest["Password"];

            var token = KPDatabase.LoginUser(username, password);

            var jsonResponse = new KPRestResponse();

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
                response.AppendCookie(new Cookie {
                    Domain = TPServer.Host,
                    Name = SessionCookie,
                    Value = token.TokenString
                });
            }

            response.SendResponse(JsonConvert.SerializeObject(jsonResponse));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/isLogged")]
        public IHttpContext IsLogged(IHttpContext context)
        {
            context.Response.ContentType = ContentType.JSON;

            var jsonResponse = new KPRestResponse();

            if (KPDatabase.UserLogged(context.Request.Cookies[SessionCookie]?.Value) != null)
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

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/getPrivateKey")]
        public IHttpContext GetPrivateKey(IHttpContext context)
        {
            var response = context.Response;
            context.Response.ContentType = ContentType.JSON;
            
            var user = KPDatabase.UserLogged(context.Request.Cookies[SessionCookie]?.Value);
            if (user != null)
            {
                KPRestResponse kpRestResponse;
                if (user.PrivateKey == null)
                {
                    kpRestResponse = new KPRestResponse
                    {
                        Error = "no_private_key",
                        ErrorDescription = "This user has no private key"
                    };
                    response.StatusCode = HttpStatusCode.Ok;
                    response.SendResponse(JsonConvert.SerializeObject(kpRestResponse));
                }
                else
                {
                    kpRestResponse = new KPRestResponse
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

        //todo: probably better to put public key inside shared folder
        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/getPublicKey")]
        public IHttpContext GetPublicKey(IHttpContext context)
        {
            var request = context.Request;
            var response = context.Response;
            context.Response.ContentType = ContentType.JSON;

            var user = KPDatabase.UserLogged(context.Request.Cookies[SessionCookie]?.Value);
            if (user != null)
            {
                var publicKey = TPServer.KpPublicKey;
                var jsonResponse = new KPRestResponse
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
    }
}