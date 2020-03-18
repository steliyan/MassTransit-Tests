using System;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;
using MassTransit.Courier;
using MassTransit.Courier.Contracts;
using MassTransit.TestFramework;
using MassTransit.TestFramework.Courier;
using MassTransit.Testing;
using Xunit;

namespace Tests
{
    public class ActivityRetryTests
    {
        private readonly InMemoryTestHarness harness = new InMemoryTestHarness();

        [Fact]
        public async Task ShouldRetry_WhenMessageRetryIsUsed()
        {
            try
            {
                var testActivity = new ActivityTestContext<FirstFaultyActivity, FaultyArguments, FaultyLog>(
                    harness,
                    () => new FirstFaultyActivity(),
                    null,
                    null);
                harness.OnConfigureBus += bus => bus.UseMessageRetry(r => r.Immediate(2));

                await harness.Start();

                var routingSlipCompletedHandler = ConnectPublishHandler<RoutingSlipCompleted>(harness);

                var builder = new RoutingSlipBuilder(Guid.NewGuid());
                builder.AddActivity(testActivity.Name, testActivity.ExecuteUri, new { });
                await harness.Bus.Execute(builder.Build());

                await routingSlipCompletedHandler;
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Fact]
        public async Task ShouldCreateOneSpans_WhenThreeRetriesAreUsedAndActivityFailsOnFirstAttempt()
        {
            try
            {
                var testActivity = new ActivityTestContext<FirstFaultyActivity, FaultyArguments, FaultyLog>(
                    harness,
                    () => new FirstFaultyActivity(),
                    cfg => cfg.UseRetry(r => r.Immediate(2)),
                    null);

                await harness.Start();

                var routingSlipCompletedHandler = ConnectPublishHandler<RoutingSlipCompleted>(harness);

                var builder = new RoutingSlipBuilder(Guid.NewGuid());
                builder.AddActivity(testActivity.Name, testActivity.ExecuteUri, new { });
                await harness.Bus.Execute(builder.Build());

                await routingSlipCompletedHandler;
            }
            finally
            {
                await harness.Stop();
            }
        }

        private Task<ConsumeContext<T>> ConnectPublishHandler<T>(InMemoryTestHarness harness) where T : class
        {
            Task<ConsumeContext<T>> result = null;
            harness.Bus.ConnectReceiveEndpoint(NewId.NextGuid().ToString(), context =>
            {
                result = harness.Handled<T>(context);
            });

            return result;
        }
    }
}
