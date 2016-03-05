namespace RxComplexFlowWithRetry
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using Annotations;
    using GalaSoft.MvvmLight.Command;

    public class MainViewModel : INotifyPropertyChanged
    {
        private UICallback<Item> failedItem = new UICallback<Item>();

        public MainViewModel()
        {
            // Takes 15 Items to upload
            var itemsObservable = Observable
                .Interval(TimeSpan.FromSeconds(3))
                .Select(id => new Item(string.Format("Item {0}", id + 1)))
                .Take(15)
                .Publish();

            // We observe the incoming items to show them into the view
            itemsObservable
                .ObserveOnDispatcher()
                .Subscribe(item => PendingItems.Add(item));

            // We also observe the items that uploaded successfully (the uploader holds them)
            var uploader = new Uploader(itemsObservable);
            uploader.UploadedItems
               .ObserveOnDispatcher()
               .Subscribe(uploadResults => UploadedItems.Add(uploadResults));

            uploader.Failed
                .ObserveOnDispatcher()
                .Subscribe(
                    failedItem =>
                    {
                        FailedItem = failedItem;
                    });

            //The command that will be invoked when the user "fixes" an item and wants to retry it. It will be available only when there is a item that failed to upload.
            RetryCommand = new RelayCommand(() => Retry(), () => FailedItem.IsReady);

            PendingItems = new ObservableCollection<Item>();
            UploadedItems = new ObservableCollection<UploadResults>();

            itemsObservable.Connect();
        }

        // When and item fails, it will be put inside this property
        public UICallback<Item> FailedItem
        {
            get { return failedItem; }
            set
            {
                failedItem = value;
                OnPropertyChanged();
                RetryCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<Item> PendingItems { get; set; }
        public ObservableCollection<UploadResults> UploadedItems { get; set; }

        public RelayCommand RetryCommand { get; private set; }

        private void Retry()
        {
            FailedItem.Callback(FailedItem.Item);
            FailedItem = UICallback<Item>.Empty;
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