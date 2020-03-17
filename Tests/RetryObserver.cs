using System;
using System.Threading.Tasks;
using GreenPipes;

namespace Tests
{
    public class RetryObserver : IRetryObserver
    {
        private readonly FaultCounter counter;

        public RetryObserver(FaultCounter counter)
        {
            this.counter = counter;
        }

        async Task IRetryObserver.PostCreate<T>(RetryPolicyContext<T> context)
        {
            var span = new Span("0");
            context.Context.GetOrAddPayload(() => span);
            Console.WriteLine("PostCreate: ", span.Id);
        }

        async Task IRetryObserver.PreRetry<T>(RetryContext<T> context)
        {
            var span = new Span(context.RetryAttempt.ToString());
            context.Context.AddOrUpdatePayload(() => span, _ => span);
            Console.WriteLine("PreRetry: ", span);
        }

        async Task IRetryObserver.PostFault<T>(RetryContext<T> context)
        {
            if (context.Context.TryGetPayload(out Span span))
            {
                this.counter.Increment();
                Console.WriteLine("PostFault:", span.Id);
            }
        }

        async Task IRetryObserver.RetryComplete<T>(RetryContext<T> context)
        {
            if (context.Context.TryGetPayload(out Span span))
            {
                Console.WriteLine("RetryComplete:", span.Id);
            }
        }

        async Task IRetryObserver.RetryFault<T>(RetryContext<T> context)
        {
            if (context.Context.TryGetPayload(out Span span))
            {
                Console.WriteLine("RetryFault:", span.Id);
            }
        }
    }
}
