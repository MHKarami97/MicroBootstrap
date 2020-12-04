using System;
using System.Threading.Tasks;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using Xunit;

namespace Game.Services.EventProcessor.Tests.Performance
{
    public class PerformanceTests
    {
        //for check how many request per second we can get
        //cd Game.Services.EventProcessor.API  --> dotnet run 
        [Fact]
        public void get_game_event_sources()
        {
            //Arrange
            const string url = "http://localhost:7001";
            const string stepName = "init";
            const int duration = 3;
            const int expectedRps = 100; //100 RPS
            var endpoint = $"{url}/game-event-sources";

            var step = HttpStep.Create(stepName, ctx =>
                Task.FromResult(Http.CreateRequest("GET", endpoint)
                    .WithCheck(response => Task.FromResult(response.IsSuccessStatusCode))));

             //Assert
            var assertions = new[]
            {
                Assertion.ForStep(stepName, s => s.RPS >= expectedRps), //we expected 100 rps but we serve 530 rps and this step will pass
                Assertion.ForStep(stepName, s => s.OkCount >= expectedRps * duration)
            };

            //Act
            var scenario = ScenarioBuilder.CreateScenario("GET game-event-sources", new[] {step})
                .WithConcurrentCopies(1)
                .WithOutWarmUp()
                .WithDuration(TimeSpan.FromSeconds(duration))
                .WithAssertions(assertions);

            NBomberRunner.RegisterScenarios(scenario)
                .RunTest();
        }
    }
}