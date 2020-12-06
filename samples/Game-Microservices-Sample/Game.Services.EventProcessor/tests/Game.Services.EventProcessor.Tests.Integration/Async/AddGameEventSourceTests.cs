using System;
using System.Threading.Tasks;
using Game.Services.EventProcessor.API;
using Game.Services.EventProcessor.Core.Entities;
using Game.Services.EventProcessor.Core.Messages.Commands;
using Game.Services.EventProcessor.Tests.Shared.Factories;
using Game.Services.EventProcessor.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace Game.Services.EventProcessor.Tests.Integration.Async
{
    //test our microservice integrates with both mongodb and rabbitmq
    //async test with involve message broker
    public class AddGameEventSourceTests : IClassFixture<GameApplicationFactory<Startup>>,
        IClassFixture<MongoDbFixture<GameEventSource, Guid>>, IDisposable
    {
        private Task Act(AddGameEventSource command) => _rabbitMqFixture.PublishAsync(command, Exchange);

        [Fact] //problem with rabbitmq queue  //http://localhost:15672
        public async Task add_game_event_source_command_should_add_document_with_given_id_to_database()
        {
            var command = new AddGameEventSource(Guid.NewGuid(), 10, true, Guid.NewGuid());

            //we have 2 scenario here:

            //1.Subscribe to the message and wait for this message (Task.Delay(500)) and then publish our message and
            //in the next line we should subscribe and get this data. the issue is here because we have eventually
            //consistency we need some trigger to know when can I actually look to the database and get data. we need
            //some mechanism for this like bellow but it is slow

            // await Act(command);
            // await Task.Delay(5000)

            //2.after publishing integration event we can subscribe to integration event. event means something already happened,
            //here GameEventSourceAdded integration event so once we get this event we go to mongodb look for data and get it 
            //we can do this with special method inside our rabbitmq fixture.

            var tcs = _rabbitMqFixture.SubscribeAndGet<AddGameEventSource, GameEventSource>(Exchange,
                _mongoDbFixture.GetAsync, command.Id); //use task completion source (tcs)

            await Act(command);

            var document = await tcs.Task;

            document.ShouldNotBeNull();
            document.Id.ShouldBe(command.Id);
            document.IsWin.ShouldBe(command.IsWin);
            document.Score.ShouldBe(command.Score);
        }

        #region Arrange

        private const string Exchange = "game-event-sources";
        private readonly MongoDbFixture<GameEventSource, Guid> _mongoDbFixture;
        private readonly RabbitMqFixture _rabbitMqFixture;

        public AddGameEventSourceTests(GameApplicationFactory<Startup> factory,
            MongoDbFixture<GameEventSource, Guid> mongoDbFixture)
        {
            _rabbitMqFixture = new RabbitMqFixture();
            _mongoDbFixture = mongoDbFixture;
            _mongoDbFixture.CollectionName = Exchange;
            factory.Server.AllowSynchronousIO = true;
        }

        public void Dispose()
        {
            _mongoDbFixture.Dispose();
        }

        #endregion
    }
}