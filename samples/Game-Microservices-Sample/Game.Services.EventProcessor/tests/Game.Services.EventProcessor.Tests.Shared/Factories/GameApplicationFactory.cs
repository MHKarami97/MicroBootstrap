using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Game.Services.EventProcessor.Tests.Shared.Factories
{
    public class GameApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
    {
        protected override IWebHostBuilder CreateWebHostBuilder() => base.CreateWebHostBuilder().UseEnvironment("tests");
    }
}