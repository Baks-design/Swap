using R3;

namespace SwapChains.Runtime.Utilities.Helpers
{
    public static class CustomObservables
    {
        public static Observable<bool> Latch(Observable<Unit> tick, Observable<Unit> latchTrue, bool initialValue)
        {
            // Create a custom Observable, whose behavior is determined by our calls to the provided 'observable'
            return Observable.Create<bool>(observer =>
            {
                // Our state value.
                var value = initialValue;
                // Create an inner subscription to latch:
                // Whenever latch fires, store true.
                var latchSub = latchTrue.Subscribe(_ => value = true);
                // Create an inner subscription to tick:
                var tickSub = tick.Subscribe(
                    // Whenever tick fires, send the current value and reset state.
                    _ =>
                    {
                        observer.OnNext(value);
                        value = false;
                    },
                    observer.OnErrorResume, // pass through tick's errors (if any)
                    observer.OnCompleted); // complete when tick completes
                // If we're disposed, dispose inner subscriptions too.
                return Disposable.Create(() =>
                {
                    latchSub.Dispose();
                    tickSub.Dispose();
                });
            });
        }

        public static Observable<T> SelectRandom<T>(this Observable<Unit> eventObs, T[] items)
        {
            // Edge-cases:
            var n = items.Length;
            if (n is 0)
                return Observable.Empty<T>(); // No items!
            else if (n is 1)
                return eventObs.Select(_ => items[0]);  // Only one item!

            var myItems = (T[])items.Clone();
            return Observable.Create<T>(observer =>
            {
                var sub = eventObs.Subscribe(_ =>
                {
                    // Select any item after the first.
                    var i = UnityEngine.Random.Range(1, n);
                    var value = myItems[i];

                    // Swap with value at index 0 to avoid selecting an item twice in a row.
                    var temp = myItems[0];
                    myItems[0] = value;
                    myItems[i] = temp;
                    
                    // Finally emit the selected value.
                    observer.OnNext(value);
                },
                observer.OnErrorResume,
                observer.OnCompleted);
                return Disposable.Create(() => sub.Dispose());
            });
        }
    }
}
