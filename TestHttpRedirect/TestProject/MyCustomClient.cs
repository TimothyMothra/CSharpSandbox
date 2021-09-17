namespace TestProject
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class MyCustomClient
    {
        public readonly Uri uri;
        public readonly HttpClient httpClient;
        private readonly RedirectHttpHandler handler;

        public MyCustomClient(string url, RedirectHttpHandler httpMessageHandler = null)
        {
            this.uri = new Uri(url);

            this.handler = httpMessageHandler;

            this.httpClient = (httpMessageHandler == null)
                ? new HttpClient()
                : new HttpClient(httpMessageHandler);
        }

        public async Task<string> GetAsync()
        {
            var result = await this.httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, this.uri));

            return await result.Content.ReadAsStringAsync();
        }
    }
}
