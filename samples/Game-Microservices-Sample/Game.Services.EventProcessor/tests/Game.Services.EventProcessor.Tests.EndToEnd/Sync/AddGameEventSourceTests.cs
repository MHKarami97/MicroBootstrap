using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Game.Services.EventProcessor.API;
using Game.Services.EventProcessor.Core.Entities;
using Game.Services.EventProcessor.Core.Messages.Commands;
using Game.Services.EventProcessor.Tests.Shared.Factories;
using Game.Services.EventProcessor.Tests.Shared.Fixtures;
using Newtonsoft.Json;
using Shouldly;

namespace Game.Services.EventProcessor.Tests.EndToEnd.Sync
{
    public class AddGameEventSourceTests : IClassFixture<GameApplicationFactory<Program>>,
        IClassFixture<MongoDbFixture<GameEventSource, Guid>>, IDisposable
    {
        private Task<HttpResponseMessage> Act(AddGameEventSource command)
            => _httpClient.PostAsJsonAsync("game-event-sources", command); //send request to our web api

        [Fact]
        public async Task add_game_event_source_endpoint_should_return_http_status_code_created()
        {
            var command = new AddGameEventSource(Guid.NewGuid(), 10, true);

            var response = await Act(command);

            response.ShouldNotBeNull();
            response.StatusCode.ShouldBe(HttpStatusCode.Created);
            response.Headers.Location.ShouldNotBeNull();
            response.Headers.Location.ToString().ShouldBe($"game-event-sources/{command.Id}");
        }
        
        [Fact]
        public async Task add_game_event_source_endpoint_should_add_document_with_given_id_to_database()
        {
            var command = new AddGameEventSource(Guid.NewGuid(), 10, true);
        
            await Act(command);
        
            var document = await _mongoDbFixture.GetAsync(command.Id);
        
            document.ShouldNotBeNull();
            document.Id.ShouldBe(command.Id);
            document.IsWin.ShouldBe(command.IsWin);
            document.Score.ShouldBe(command.Score);
        }

         [Fact]
         public async Task add_game_event_source_endpoint_should_return_location_header_with_correct_id()
         {
             var command = new AddGameEventSource(Guid.NewGuid(), 10, true);
         
             var response = await Act(command);
         
             var locationHeader = response.Headers.FirstOrDefault(h => h.Key == "Location").Value.First();
         
             locationHeader.ShouldNotBeNull();
             locationHeader.ShouldBe($"game-event-sources/{command.Id}");
         }
        

        #region Arrange

        private readonly HttpClient _httpClient;
        private readonly MongoDbFixture<GameEventSource, Guid> _mongoDbFixture;

        public AddGameEventSourceTests(GameApplicationFactory<Program> factory,
            MongoDbFixture<GameEventSource, Guid> mongoDbFixture)
        {
            _mongoDbFixture = mongoDbFixture;
            _mongoDbFixture.CollectionName = "gameEventSources";
            factory.Server.AllowSynchronousIO = true;
            _httpClient = factory.CreateClient();
        }

        public void Dispose()
        {
            _mongoDbFixture.Dispose(); //cleanup mongodb after each test run 
        }

        #endregion
    }
}