using System;

namespace RxComplexFlowWithRetry
{
    public class UICallback<T>
    {
        public static readonly UICallback<T> Empty = new UICallback<T>();
        
        /// <summary>
        /// Callback to return the {T} back to the model. Can only be called once.
        /// </summary>
        public Action<T> Callback { get; private set; }
        public T Item { get; set; }

        /// <summary>
        /// Flag to indicate the callback can be used.
        /// </summary>
        public bool IsReady { get; private set; }

        public UICallback()
        {
            IsReady = false;
        }

        public UICallback(T item, IObserver<T> callbackObs)
        {
            Item = item;
            Callback = (it) =>
            {
                if (IsReady)
                {
                    callbackObs.OnNext(it);
                    callbackObs.OnCompleted();
                }

                IsReady = false;
            };
            IsReady = true;
        }
    }
}
