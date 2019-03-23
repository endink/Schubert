using Microsoft.Net.Http.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using HttpRequestHeader = System.Net.Http.Headers.HttpRequestHeaders;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace Schubert.Framework.Http
{
    public class RestClient : IDisposable
    {
        private readonly HttpClient _client;
        private Version _requestedApiVersion;
        private static readonly TimeSpan InfiniteTimeout = TimeSpan.FromMilliseconds(System.Threading.Timeout.Infinite);
        private const string UserAgent = "schubert-rest";

        public RestClient(RestClientConfiguration configuration, Version requestedApiVersion = null)
        {
            Configuration = configuration;
            _requestedApiVersion = requestedApiVersion;
            JsonSerializer = new HttpJsonSerializer();

            ManagedHandler handler;
            var uri = Configuration.EndpointBaseUri;
            switch (uri.Scheme.ToLowerInvariant())
            {
                case "http":
                    var builder = new UriBuilder(uri)
                    {
                        Scheme = configuration.Credentials.IsTlsCredentials() ? "https" : "http"
                    };
                    uri = builder.Uri;
                    handler = new ManagedHandler();
                    break;

                case "https":
                    handler = new ManagedHandler();
                    break;

                case "unix":
                    var pipeString = uri.LocalPath;
                    handler = new ManagedHandler(async (string host, int port, CancellationToken cancellationToken) =>
                    {
                        var sock = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                        await sock.ConnectAsync(new UnixDomainSocketEndPoint(pipeString));
                        return sock;
                    });
                    uri = new UriBuilder("http", uri.Segments.Last()).Uri;
                    break;
                default:
                    throw new Exception($"Unsupported url scheme '{configuration.EndpointBaseUri.Scheme}'");
            }

            this.EndpointBaseUri = uri;

            _client = new HttpClient(Configuration.Credentials.GetHandler(handler), true);
            _client.Timeout = InfiniteTimeout;
            _client.Timeout = Configuration.Timeout;
        }

        public Uri EndpointBaseUri { get; }

        public HttpJsonSerializer JsonSerializer { get; }

        public RestClientConfiguration Configuration { get; }

        public async Task<T> PutJsonAsync<T>(
            string path,
            String queryString = null,
            object body = null,
            Action<HttpRequestHeader> headersConfigure = null,
            TimeSpan? timeout = null,
            CancellationToken? token = null,
            IEnumerable<RestResponseErrorHandlingDelegate> errorHandlers = null)
        {
            var content = await this.ExecuteJsonAsync<T>(HttpMethod.Put, path, queryString, body, headersConfigure, timeout, token, errorHandlers);
            return content;
        }

        public async Task<T> DeleteJsonAsync<T>(
            string path,
            String queryString = null,
            object body = null,
            Action<HttpRequestHeader> headersConfigure = null,
            TimeSpan? timeout = null,
            CancellationToken? token = null,
            IEnumerable<RestResponseErrorHandlingDelegate> errorHandlers = null)
        {
            var content = await this.ExecuteJsonAsync<T>(HttpMethod.Delete, path, queryString, body, headersConfigure, timeout, token, errorHandlers);
            return content;
        }

        public async Task<T> PostJsonAsync<T>(
            string path,
            String queryString = null,
            object body = null,
            Action<HttpRequestHeader> headersConfigure = null,
            TimeSpan? timeout = null,
            CancellationToken? token = null,
            IEnumerable<RestResponseErrorHandlingDelegate> errorHandlers = null)
        {
            var content = await this.ExecuteJsonAsync<T>(HttpMethod.Post, path, queryString, body, headersConfigure, timeout, token, errorHandlers);
            return content;
        }

        public async Task<T> GetJsonAsync<T>(
            string path,
            String queryString = null,
            object body = null,
            Action<HttpRequestHeader> headersConfigure = null,
            TimeSpan? timeout = null,
            CancellationToken? token = null,
            IEnumerable<RestResponseErrorHandlingDelegate> errorHandlers = null)
        {
            var content = await this.ExecuteJsonAsync<T>(HttpMethod.Get, path, queryString, body, headersConfigure, timeout, token, errorHandlers);
            return content;
        }

        private async Task<T> ExecuteJsonAsync<T>(
            HttpMethod method,
            string path,
            String queryString = null,
            object body = null,
            Action<HttpRequestHeader> headersConfigure = null,
            TimeSpan? timeout = null,
            CancellationToken? token = null,
            IEnumerable<RestResponseErrorHandlingDelegate> errorHandlers = null)
        {
            var content = body == null ? null : new JsonRequestContent(body, this.JsonSerializer);
            var resp = await this.MakeRequestJsonAsync<T>(method, path, queryString, content, headersConfigure, timeout, token, errorHandlers);
            return resp;
        }

        public async Task<T> MakeRequestJsonAsync<T>(
            HttpMethod method,
            string path,
            String queryString = null,
            IRequestContent body = null,
            Action<HttpRequestHeader> headersConfigure = null,
            TimeSpan? timeout = null,
            CancellationToken? token = null,
            IEnumerable<RestResponseErrorHandlingDelegate> errorHandlers = null)
        {
            var response =  await this.MakeRequestAsync(method, path, queryString, body, headersConfigure, timeout, token, errorHandlers);
            return this.JsonSerializer.DeserializeObject<T>(response.Body);
        }

        public async Task<RestResponse> MakeRequestAsync(
            HttpMethod method,
            string path,
            String queryString = null,
            IRequestContent body = null,
            Action<HttpRequestHeader> headersConfigure = null,
            TimeSpan? timeout = null,
            CancellationToken? token = null,
            IEnumerable<RestResponseErrorHandlingDelegate> errorHandlers = null)
        {
            var response = await this.MakeRequestCoreAsync(method, path, queryString, body, timeout, token, HttpCompletionOption.ResponseContentRead, headersConfigure).ConfigureAwait(false);
            using (response)
            {
                var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                HandleIfErrorResponse(response.StatusCode, responseBody, errorHandlers);

                return new RestResponse(response.StatusCode, responseBody, response.Headers);
            }
        }

        public async Task<Stream> MakeRequestForStreamAsync(
            HttpMethod method,
            string path,
            String queryString = null,
            IRequestContent body = null,
            Action<HttpRequestHeader> headersConfigure = null,
            TimeSpan? timeout = null,
            CancellationToken? token = null,
             IEnumerable<RestResponseErrorHandlingDelegate> errorHandlers = null)
        {
            var response = await this.MakeRequestCoreAsync(method, path, queryString, body, timeout, token, HttpCompletionOption.ResponseHeadersRead, headersConfigure).ConfigureAwait(false);

            HandleIfErrorResponse(response.StatusCode, null, errorHandlers);

            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        public async Task<RestStreamedResponse> MakeRequestForStreamedResponseAsync(
            HttpMethod method,
            string path,
            String queryString = null,
            CancellationToken? cancellationToken = null,
            IEnumerable<RestResponseErrorHandlingDelegate> errorHandlers = null)
        {
            var response = await MakeRequestCoreAsync(method, path, queryString, null, InfiniteTimeout, cancellationToken, HttpCompletionOption.ResponseHeadersRead, null);

            HandleIfErrorResponse(response.StatusCode, null, errorHandlers);

            var body = await response.Content.ReadAsStreamAsync();

            return new RestStreamedResponse(response, body);
        }

        public async Task<WriteClosableStream> MakeRequestForHijackedStreamAsync(
           HttpMethod method,
           string path,
           String queryString = null,
           IRequestContent body = null,
           Action<HttpRequestHeader> headersConfigure = null,
           TimeSpan? timeout = null,
           CancellationToken? cancellationToken = null,
           IEnumerable<RestResponseErrorHandlingDelegate> errorHandlers = null)
        {
            var response = await MakeRequestCoreAsync(method, path, queryString, body, timeout, cancellationToken, HttpCompletionOption.ResponseHeadersRead, headersConfigure).ConfigureAwait(false);

            HandleIfErrorResponse(response.StatusCode, null, errorHandlers);

            var content = response.Content as HttpConnectionResponseContent;
            if (content == null)
            {
                throw new NotSupportedException("message handler does not support hijacked streams");
            }

            return content.HijackStream();
        }


        private async Task<HttpResponseMessage> MakeRequestCoreAsync(
           HttpMethod method,
           string path,
           String queryString = null,
           IRequestContent data = null,
           TimeSpan? timeout = null,
           CancellationToken? cancellationToken = null,
           HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
           Action<HttpRequestHeader> headersConfigure = null)
        {
            var request = PrepareRequest(method, path, queryString, headersConfigure, data);

            CancellationToken token;
            if (timeout != InfiniteTimeout)
            {
                token = cancellationToken ?? CancellationToken.None;
                var timeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                timeoutTokenSource.CancelAfter(timeout ?? this.Configuration.Timeout);
                token = timeoutTokenSource.Token;
            }

            try
            {
                var result = await _client.SendAsync(request, completionOption, token);
                return result;
            }
            catch (OperationCanceledException cex)
            {
                String body = null;
                if (data != null)
                {
                    body = data.GetContent()?.ReadAsStringAsync()?.GetAwaiter().GetResult();
                }
                throw new SchubertRestTimeoutException($@"在 {(timeout ?? this.Configuration.Timeout).TotalSeconds.ToString()} 秒内没有收到服务端的 HTTP 响应（uri: {request.RequestUri?.ToString().IfNullOrWhiteSpace("null")}, data:{body.IfNullOrWhiteSpace("null")}）调用的响应。", cex);
            }
            catch (Exception ex)
            {
                String body = null;
                if (data != null)
                {
                    body = data.GetContent()?.ReadAsStringAsync()?.GetAwaiter().GetResult();
                }
                throw new SchubertRestException(HttpStatusCode.BadRequest, $@"发送 HTTP 请求生错误（uri: {request.RequestUri?.ToString().IfNullOrWhiteSpace("null")}, data:{body.IfNullOrWhiteSpace("null")}）发生错误。", ex);
            }
        }

        private void HandleIfErrorResponse(HttpStatusCode statusCode, string responseBody, IEnumerable<RestResponseErrorHandlingDelegate> handlers)
        {
            // If no customer handlers just default the response.
            if (handlers != null)
            {
                foreach (var handler in handlers)
                {
                    handler(statusCode, responseBody);
                }
            }

            // No custom handler was fired. Default the response for generic success/failures.
            if (statusCode < HttpStatusCode.OK || statusCode >= HttpStatusCode.BadRequest)
            {
                throw new SchubertRestException(statusCode, responseBody);
            }
        }

        protected HttpRequestMessage PrepareRequest(HttpMethod method, string path, String queryString,Action<HttpRequestHeader> headerConfigure, IRequestContent data)
        {
            if (string.IsNullOrEmpty("path"))
            {
                throw new ArgumentNullException(nameof(path));
            }

            var request = new HttpRequestMessage(method, RestUtility.BuildUri(this.EndpointBaseUri, this._requestedApiVersion, path, queryString));

            request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);
            foreach (var header in this.Configuration.DefaultHeaders)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            headerConfigure?.Invoke(request.Headers);

            if (data != null)
            {
                var requestContent = data.GetContent(); // make the call only once.
                request.Content = requestContent;
            }

            return request;
        }

        public virtual void Dispose()
        {
            _client.Dispose();
            Configuration.Dispose();
        }
    }

    public delegate void RestResponseErrorHandlingDelegate(HttpStatusCode statusCode, string responseBody);

}
