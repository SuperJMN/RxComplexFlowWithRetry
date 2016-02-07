namespace RxComplexFlowWithRetry
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using Annotations;
    using View.ViewModel;

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Uploader uploader;
        private Item failedItem;

        public MainViewModel()
        {
            // Takes 15 Items to upload
            var itemsObservable = Observable
                .Interval(TimeSpan.FromSeconds(5))
                .Select(id => new Item(string.Format("Item {0}", id + 1)))
                .Take(15)
                .Publish();

            // We observe the incoming items to show them into the view
            itemsObservable
                .ObserveOnDispatcher()
                .Subscribe(item => PendingItems.Add(item));

            // We also observe the items that uploaded successfully (the uploader holds them)
            uploader = new Uploader(itemsObservable);
            uploader.UploadedItems
               .ObserveOnDispatcher()
               .Subscribe(uploadResults => UploadedItems.Add(uploadResults));

            uploader.Failed.Subscribe(failedItem =>
            {
                FailedItem = failedItem.Item;
                fixItem = failedItem.FixMe;
            });

            //The command that will be invoked when the user "fixes" an item and wants to retry it. It will be available only when there is a item that failed to upload.
            RetryCommand = new RelayCommand(_ => Retry(), _ => FailedItem != null);

            PendingItems = new ObservableCollection<Item>();
            UploadedItems = new ObservableCollection<UploadResults>();

            itemsObservable.Connect();
        }

        // When and item fails, it will be put inside this property
        public Item FailedItem
        {
            get { return failedItem; }
            set
            {
                failedItem = value;
                OnPropertyChanged();
            }
        }
        public Action<Item> fixItem { get; set; }

        public ObservableCollection<Item> PendingItems { get; set; }
        public ObservableCollection<UploadResults> UploadedItems { get; set; }

        public ICommand RetryCommand { get; private set; }

        private void Retry()
        {
            fixItem(FailedItem);
            FailedItem = null;
            fixItem = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}