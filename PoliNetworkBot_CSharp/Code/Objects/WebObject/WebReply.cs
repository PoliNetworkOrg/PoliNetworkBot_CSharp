using System.Net;

namespace PoliNetworkBot_CSharp.Code.Objects.WebObject
{
    internal class WebReply
    {
        private readonly string data;
        private readonly HttpStatusCode statusCode;

        public WebReply(string data, HttpStatusCode statusCode)
        {
            this.data = data;
            this.statusCode = statusCode;
        }

        internal bool IsValid()
        {
            switch (statusCode)
            {
                case HttpStatusCode.Continue:
                    break;

                case HttpStatusCode.SwitchingProtocols:
                    break;

                case HttpStatusCode.Processing:
                    break;

                case HttpStatusCode.EarlyHints:
                    break;

                case HttpStatusCode.OK:
                    return true;
                    break;

                case HttpStatusCode.Created:
                    break;

                case HttpStatusCode.Accepted:
                    break;

                case HttpStatusCode.NonAuthoritativeInformation:
                    break;

                case HttpStatusCode.NoContent:
                    break;

                case HttpStatusCode.ResetContent:
                    break;

                case HttpStatusCode.PartialContent:
                    break;

                case HttpStatusCode.MultiStatus:
                    break;

                case HttpStatusCode.AlreadyReported:
                    break;

                case HttpStatusCode.IMUsed:
                    break;

                case HttpStatusCode.Ambiguous:
                    break;

                case HttpStatusCode.Moved:
                    break;

                case HttpStatusCode.Found:
                    break;

                case HttpStatusCode.RedirectMethod:
                    break;

                case HttpStatusCode.NotModified:
                    break;

                case HttpStatusCode.UseProxy:
                    break;

                case HttpStatusCode.Unused:
                    break;

                case HttpStatusCode.RedirectKeepVerb:
                    break;

                case HttpStatusCode.PermanentRedirect:
                    break;

                case HttpStatusCode.BadRequest:
                    break;

                case HttpStatusCode.Unauthorized:
                    break;

                case HttpStatusCode.PaymentRequired:
                    break;

                case HttpStatusCode.Forbidden:
                    break;

                case HttpStatusCode.NotFound:
                    break;

                case HttpStatusCode.MethodNotAllowed:
                    break;

                case HttpStatusCode.NotAcceptable:
                    break;

                case HttpStatusCode.ProxyAuthenticationRequired:
                    break;

                case HttpStatusCode.RequestTimeout:
                    break;

                case HttpStatusCode.Conflict:
                    break;

                case HttpStatusCode.Gone:
                    break;

                case HttpStatusCode.LengthRequired:
                    break;

                case HttpStatusCode.PreconditionFailed:
                    break;

                case HttpStatusCode.RequestEntityTooLarge:
                    break;

                case HttpStatusCode.RequestUriTooLong:
                    break;

                case HttpStatusCode.UnsupportedMediaType:
                    break;

                case HttpStatusCode.RequestedRangeNotSatisfiable:
                    break;

                case HttpStatusCode.ExpectationFailed:
                    break;

                case HttpStatusCode.MisdirectedRequest:
                    break;

                case HttpStatusCode.UnprocessableEntity:
                    break;

                case HttpStatusCode.Locked:
                    break;

                case HttpStatusCode.FailedDependency:
                    break;

                case HttpStatusCode.UpgradeRequired:
                    break;

                case HttpStatusCode.PreconditionRequired:
                    break;

                case HttpStatusCode.TooManyRequests:
                    break;

                case HttpStatusCode.RequestHeaderFieldsTooLarge:
                    break;

                case HttpStatusCode.UnavailableForLegalReasons:
                    break;

                case HttpStatusCode.InternalServerError:
                    break;

                case HttpStatusCode.NotImplemented:
                    break;

                case HttpStatusCode.BadGateway:
                    break;

                case HttpStatusCode.ServiceUnavailable:
                    break;

                case HttpStatusCode.GatewayTimeout:
                    break;

                case HttpStatusCode.HttpVersionNotSupported:
                    break;

                case HttpStatusCode.VariantAlsoNegotiates:
                    break;

                case HttpStatusCode.InsufficientStorage:
                    break;

                case HttpStatusCode.LoopDetected:
                    break;

                case HttpStatusCode.NotExtended:
                    break;

                case HttpStatusCode.NetworkAuthenticationRequired:
                    break;
            }

            return false;
        }

        internal string GetData()
        {
            return data;
        }
    }
}