namespace TestProject
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UnitTest1
    {
        private const string helloString = "Hello World!";

        private const string Localurl1 = "http://localhost:1111";
        private const string Localurl2 = "http://localhost:2222";

        [TestMethod]
        public async Task DefaultUseCase()
        {
            int counter1 = 0;
            int counter2 = 0;

            using var localServer1 = new LocalInProcHttpServer(Localurl1)
            {
                ServerLogic = async (httpContext) =>
                {
                    counter1++;
                    httpContext.Response.StatusCode = StatusCodes.Status308PermanentRedirect;//.Status307TemporaryRedirect;
                    httpContext.Response.Headers.Add("Location", Localurl2);
                    await httpContext.Response.WriteAsync("redirect");
                },
            };

            using var localServer2 = new LocalInProcHttpServer(Localurl2)
            {
                ServerLogic = async (httpContext) =>
                {
                    counter2++;
                    await httpContext.Response.WriteAsync(helloString);
                },
            };

            var client = new MyCustomClient(url: Localurl1);
            
            var testStr1 = await client.GetAsync();
            Assert.AreEqual(helloString, testStr1);
            Assert.AreEqual(1, counter1);
            Assert.AreEqual(1, counter2);

            var testStr2 = await client.GetAsync();
            Assert.AreEqual(helloString, testStr2);
            Assert.AreEqual(2, counter1);
            Assert.AreEqual(2, counter2);
        }

        [TestMethod]
        public async Task VerifyRedirectHandler()
        {
            int counter1 = 0;
            int counter2 = 0;

            using var localServer1 = new LocalInProcHttpServer(Localurl1)
            {
                ServerLogic = async (httpContext) =>
                {
                    counter1++;
                    httpContext.Response.StatusCode = StatusCodes.Status308PermanentRedirect;//.Status307TemporaryRedirect;
                    httpContext.Response.Headers.Add("Location", Localurl2);

                    // https://docs.microsoft.com/en-us/aspnet/core/performance/caching/middleware?view=aspnetcore-5.0
                    // https://docs.microsoft.com/en-us/dotnet/api/system.net.http.headers.cachecontrolheadervalue?view=net-5.0
                    httpContext.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromDays(1),
                    };

                    await httpContext.Response.WriteAsync("redirect");
                },
            };

            using var localServer2 = new LocalInProcHttpServer(Localurl2)
            {
                ServerLogic = async (httpContext) =>
                {
                    counter2++;
                    await httpContext.Response.WriteAsync(helloString);
                },
            };

            var client = new MyCustomClient(url: Localurl1, new RedirectHttpHandler());

            var testStr1 = await client.GetAsync();
            Assert.AreEqual(helloString, testStr1);
            Assert.AreEqual(1, counter1);
            Assert.AreEqual(1, counter2);

            var testStr2 = await client.GetAsync();
            Assert.AreEqual(helloString, testStr2);
            Assert.AreEqual(1, counter1, "redirect should be cached");
            Assert.AreEqual(2, counter2);
        }


        [TestMethod]
        public async Task VerifyRedirectCache()
        {
            int counter1 = 0;
            int counter2 = 0;

            int cacheSeconds = 5;

            using var localServer1 = new LocalInProcHttpServer(Localurl1)
            {
                ServerLogic = async (httpContext) =>
                {
                    counter1++;
                    httpContext.Response.StatusCode = StatusCodes.Status308PermanentRedirect;//.Status307TemporaryRedirect;
                    httpContext.Response.Headers.Add("Location", Localurl2);

                    // https://docs.microsoft.com/en-us/aspnet/core/performance/caching/middleware?view=aspnetcore-5.0
                    // https://docs.microsoft.com/en-us/dotnet/api/system.net.http.headers.cachecontrolheadervalue?view=net-5.0
                    httpContext.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromSeconds(cacheSeconds),
                    };

                    await httpContext.Response.WriteAsync("redirect");
                },
            };

            using var localServer2 = new LocalInProcHttpServer(Localurl2)
            {
                ServerLogic = async (httpContext) =>
                {
                    counter2++;
                    await httpContext.Response.WriteAsync(helloString);
                },
            };

            var client = new MyCustomClient(url: Localurl1, new RedirectHttpHandler());

            var testStr1 = await client.GetAsync();
            Assert.AreEqual(helloString, testStr1);
            Assert.AreEqual(1, counter1);
            Assert.AreEqual(1, counter2);

            var testStr2 = await client.GetAsync();
            Assert.AreEqual(helloString, testStr2);
            Assert.AreEqual(1, counter1, "redirect should be cached");
            Assert.AreEqual(2, counter2);

            // wait for cache to expire
            await Task.Delay(TimeSpan.FromSeconds(2 * cacheSeconds));

            var testStr3 = await client.GetAsync();
            Assert.AreEqual(helloString, testStr3);
            Assert.AreEqual(2, counter1);
            Assert.AreEqual(3, counter2);
        }
    }
}
