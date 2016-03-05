namespace RxComplexFlowWithRetry
{
    using System;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public class Uploader
    {
        private static Random random = new Random((int)DateTime.Now.Ticks);
        private const int UploadFailureProbability = 20;

        public Uploader(IObservable<Item> items)
        {
            UploadedItems = items.Select(item => Upload(item)).Concat();
            Failed = failer;
        }

        private static bool ShouldThrow
        {
            get
            {                
                return random.Next(0, 100) > 100 - UploadFailureProbability;
            }
        }

        public IObservable<UploadResults> UploadedItems { get; private set; }

        public IObservable<UICallback<Item>> Failed
        {
            get;
            private set;
        }
        private Subject<UICallback<Item>> failer = new Subject<UICallback<Item>>();

        private IObservable<UploadResults> Upload(Item item)
        {
            return Observable.Timer(TimeSpan.FromSeconds(4))
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
            failer.OnNext(new UICallback<Item>(item, sub));
            return sub.SelectMany(fixedItem => Upload(fixedItem));
        }
    }
}