namespace RxComplexFlowWithRetry
{
    using System;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;

    public class Uploader
    {
        private const int UploadFailureProbability = 80;

        public Uploader(IObservable<Item> items)
        {
            UploadedItems = items.SelectMany(item => Upload(item));
            Failed = failer;
        }

        private static bool ShouldThrow
        {
            get
            {
                var random = new Random((int)DateTime.Now.Ticks);
                return random.Next(0, 100) > UploadFailureProbability;
            }
        }

        public IObservable<UploadResults> UploadedItems { get; private set; }

        public IObservable<FailedItem> Failed
        {
            get;
            private set;
        }
        private Subject<FailedItem> failer = new Subject<FailedItem>();

        private IObservable<UploadResults> Upload(Item item)
        {
            return Observable.Timer(TimeSpan.FromSeconds(7))
                .SelectMany(_ =>
                {
                    if (ShouldThrow)
                        return Observable.Throw<UploadResults>(new Exception("Item invalid"));
                    else
                        return Observable.Return(new UploadResults(item));
                }).Catch((Exception ex) => RetryFailedItem(item));
        }


        public IObservable<UploadResults> RetryFailedItem(Item item)
        {
            var sub = new Subject<Item>();
            failer.OnNext(new FailedItem(item, (fixedItem) => { sub.OnNext(fixedItem); sub.OnCompleted(); }));
            return sub.SelectMany(fixedItem => Upload(fixedItem));
        }
    }
}