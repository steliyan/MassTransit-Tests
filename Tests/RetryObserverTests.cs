using System;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;
using MassTransit.Testing;
using Xunit;

namespace Tests
{
    public class RetryObserverTests
    {
        [Fact]
        public async void InMemory()
        {
            var counter = new FaultCounter();
            var harness = new InMemoryTestHarness();

            try
            {
                harness.OnConfigureInMemoryBus += cfg =>
                    cfg.UseRetry(x =>
                    {
                        x.Immediate(1);
                        x.ConnectRetryObserver(new RetryObserver(counter));
                    });

                harness.OnConfigureInMemoryReceiveEndpoint += cfg => harness.Handler<PingMessage>(cfg, ctx => throw new Exception());

                await harness.Start();

                var faultHandlerTask = harness.SubscribeHandler<Fault<PingMessage>>();

                await harness.InputQueueSendEndpoint.Send(new PingMessage(), ctx =>
                {
                    ctx.ResponseAddress = harness.BusAddress;
                    ctx.FaultAddress = harness.BusAddress;
                });

                await faultHandlerTask;

                Assert.Equal(1, counter.Faults);
            }
            finally
            {
                await harness.Stop();
            }
        }

        public class PingMessage
        {
        }
    }
}
