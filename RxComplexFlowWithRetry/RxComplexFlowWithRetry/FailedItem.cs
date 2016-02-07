using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxComplexFlowWithRetry
{
    public class FailedItem
    {
        public Action<Item> FixMe { get; set; }
        public Item Item { get; set; }

        public FailedItem(Item item, Action<Item> fixMe)
        {
            Item = item;
            FixMe = fixMe;
        }
    }
}
