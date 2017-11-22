using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.SessionState;
using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Server.Attributes;
using Grapevine.Shared;
using HttpStatusCode = Grapevine.Shared.HttpStatusCode;

namespace KPTrustedParty
{
    [RestResource]
    public class KPRestServer
    {
        public string SessionCookie { get; set; } = "KPABESESSIONID";

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/login")]
        public IHttpContext LoginUser(IHttpContext context)
        {
            var request = context.Request;
            var response = context.Response;
            response.ContentType = ContentType.TXT;
            var responseEncoding = response.ContentEncoding;
            byte[] payloadBytes = responseEncoding.GetBytes(request.Payload);
            string payload = Encoding.UTF8.GetString(Encoding.Convert(responseEncoding, Encoding.UTF8, payloadBytes));

            Regex usernamePassword = new Regex(@"username=(?<Username>\w+);password=(?<Password>.+)");
            var match = usernamePassword.Match(payload);
            var groups = match.Groups;

            if (groups.Count != 3)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.SendResponse("The request format must be \"username=<username>;password=<password>\"");
            }

            var token = KPDatabase.LoginUser(groups["Username"].Value, groups["Password"].Value);

            if (token == null)
            {
                response.StatusCode = HttpStatusCode.Forbidden;
                response.SendResponse("This combination of user and password does not exist");
            }
            else
            {
                response.StatusCode = HttpStatusCode.Ok;
                response.AppendCookie(new Cookie {
                    Domain = TPServer.Host,
                    Name = "KPABESESSIONID",
                    Value = token.TokenString
                });
                response.SendResponse("Login OK!");
            }

            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/isLogged")]
        public IHttpContext IsLogged(IHttpContext context)
        {
            context.Response.ContentType = ContentType.TXT;
            var sessionToken = context.Request.Cookies[SessionCookie];
            if (sessionToken?.Value != null && KPDatabase.UserLogged(sessionToken.Value))
            {
                context.Response.StatusCode = HttpStatusCode.Ok;
                context.Response.SendResponse("You are logged in");
                return context;
            }

            context.Response.StatusCode = HttpStatusCode.Forbidden;
            context.Response.SendResponse("You are not logged in");
            return context;
        }
    }

    
}