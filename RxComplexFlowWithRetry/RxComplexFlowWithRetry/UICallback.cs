using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxComplexFlowWithRetry
{
    public class UICallback<T>
    {
        /// <summary>
        /// Callback to return the {T} back to the model. Can only be called once.
        /// </summary>
        public Action<T> Callback { get; set; }
        public T Item { get; set; }

        /// <summary>
        /// Flag to indicate the callback can be used.
        /// </summary>
        public Boolean IsReady { get; private set; }

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
                    callbackObs.OnNext(it); callbackObs.OnCompleted();
                }
                IsReady = false;
            };
            IsReady = true;
        }
    }
}
