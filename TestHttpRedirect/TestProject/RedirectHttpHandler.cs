﻿namespace TestProject
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    public sealed class RedirectHttpHandler : HttpClientHandler
    {
        private const int MaxRedirect = 3;
        
        public Uri RedirectLocation { get; private set; } = default;
        public DateTimeOffset RedirectExpiration { get; private set; } = DateTimeOffset.MinValue;

        public RedirectHttpHandler()
        {
            // The default auto-redirect behavior is to remove any auth header on a redirect.
            // We want to handle ourselves to pass auth header to redirect.
            AllowAutoRedirect = false;

#if DEBUG
            // Ignore any untrusted SSL errors.
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
#endif
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // All requests will pass through here.

            if (DateTimeOffset.Now < this.RedirectExpiration)
            {
                // TODO: MUST BE THREAD SAFE
                request.RequestUri = this.RedirectLocation;
            }

            for (int redirects = 0; ;)
            {
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                if (IsRedirection(response.StatusCode))
                {
                    if (++redirects > MaxRedirect)
                    {
                        throw new Exception("too many redirects");
                    }

                    if (TryGetRedirectVars(response, out Uri redirectUri))
                    {
                        request.RequestUri = this.RedirectLocation = redirectUri;
                    }
                    else
                    {
                        // cannot parse redirect headers. no action.
                        return response;
                    }
                }
                else
                {
                    return response;
                }
            }
        }

        private bool TryGetRedirectVars(HttpResponseMessage httpResponseMessage, out Uri redirectUri)
        {
            var cacheMaxAge = httpResponseMessage?.Headers?.CacheControl?.MaxAge;
            redirectUri = httpResponseMessage?.Headers?.Location;

            if (cacheMaxAge.HasValue && redirectUri != null && redirectUri.IsAbsoluteUri)
            {
                // // If we reach here, we need to update the cache.
                // TODO: NEEDS TO BE THREADSAFE
                this.RedirectLocation = redirectUri;
                this.RedirectExpiration = DateTimeOffset.Now.Add(cacheMaxAge.Value);

                return true;
            }

            return false;
        }

        private static bool IsRedirection(HttpStatusCode statusCode)
        {
            switch ((int)statusCode)
            {
                case StatusCodes.Status307TemporaryRedirect: 
                case StatusCodes.Status308PermanentRedirect:
                    return true;
                default:
                    return false;
            }
        }
    }
}
