using System.Threading.Channels;

namespace Autodesk.Api.Controllers.api.v1.Autodesk
{
    public static class ObservableExtensions
    {
        /// <summary>
        /// Converts an IObservable<T> into an IAsyncEnumerable<T> using an unbounded Channel.
        /// </summary>
        public static IAsyncEnumerable<T> ToAsyncEnumerableViaChannel<T>(
            this IObservable<T> source,
            CancellationToken cancellationToken = default)
        {
            var channel = Channel.CreateUnbounded<T>();
            var subscription = source.Subscribe(
                onNext: item =>
                {
                    channel.Writer.TryWrite(item);
                },
                onError: ex =>
                {
                    channel.Writer.TryComplete(ex);
                },
                onCompleted: () =>
                {
                    channel.Writer.TryComplete();
                });

            return ReadAllAsync();

            async IAsyncEnumerable<T> ReadAllAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
            {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cancellationToken);

                try
                {
                    await foreach (var item in channel.Reader.ReadAllAsync(linkedCts.Token))
                    {
                        yield return item;
                    }
                }
                finally
                {
                    subscription.Dispose();
                }
            }
        }
    }
}
