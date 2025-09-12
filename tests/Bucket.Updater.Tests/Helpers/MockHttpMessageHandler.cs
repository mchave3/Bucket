using System.Net;
using System.Text;
using Moq.Protected;

namespace Bucket.Updater.Tests.Helpers
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, (HttpStatusCode statusCode, string content)> _responses;
        private readonly Dictionary<string, Func<HttpRequestMessage, HttpResponseMessage>> _customHandlers;

        public MockHttpMessageHandler()
        {
            _responses = new Dictionary<string, (HttpStatusCode, string)>();
            _customHandlers = new Dictionary<string, Func<HttpRequestMessage, HttpResponseMessage>>();
        }

        public MockHttpMessageHandler SetupGet(string url, HttpStatusCode statusCode, string content)
        {
            _responses[url] = (statusCode, content);
            return this;
        }

        public MockHttpMessageHandler SetupCustomHandler(string url, Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _customHandlers[url] = handler;
            return this;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var url = request.RequestUri?.ToString() ?? string.Empty;

            // Check for custom handlers first
            if (_customHandlers.TryGetValue(url, out var customHandler))
            {
                return Task.FromResult(customHandler(request));
            }

            // Check for simple responses
            if (_responses.TryGetValue(url, out var response))
            {
                return Task.FromResult(new HttpResponseMessage(response.statusCode)
                {
                    Content = new StringContent(response.content, Encoding.UTF8, "application/json")
                });
            }

            // Default 404 for unmatched requests
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        public static MockHttpMessageHandler CreateWithJsonResponse(string url, string jsonContent, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new MockHttpMessageHandler().SetupGet(url, statusCode, jsonContent);
        }

        public static MockHttpMessageHandler CreateWithStreamResponse(string url, byte[] content, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var handler = new MockHttpMessageHandler();
            handler.SetupCustomHandler(url, _ => new HttpResponseMessage(statusCode)
            {
                Content = new ByteArrayContent(content)
            });
            return handler;
        }

        public static MockHttpMessageHandler CreateWithException(string url, Exception exception)
        {
            var handler = new MockHttpMessageHandler();
            handler.SetupCustomHandler(url, _ => throw exception);
            return handler;
        }
    }
}