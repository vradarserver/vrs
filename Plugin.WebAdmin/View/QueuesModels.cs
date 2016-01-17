using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface;

namespace VirtualRadar.Plugin.WebAdmin.View.Queues
{
    public class ViewModel
    {
        public QueueModel[] Queues { get; private set; }

        public ViewModel(IQueue[] queues)
        {
            Queues = queues.OrderBy(r => r.Name).Select(r => new QueueModel(r)).ToArray();
        }
    }

    public class QueueModel
    {
        public string Name { get; private set; }

        public int CountQueuedItems { get; private set; }

        public int PeakQueuedItems { get; private set; }

        public QueueModel(IQueue queue)
        {
            Name =              queue.Name;
            CountQueuedItems =  queue.CountQueuedItems;
            PeakQueuedItems =   queue.PeakQueuedItems;
        }
    }
}
