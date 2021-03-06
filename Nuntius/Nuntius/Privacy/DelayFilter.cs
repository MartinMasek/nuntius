﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nuntius.Privacy
{
    /// <summary>
    /// This filter keeps received message and after a period of time sends only the most recent one.
    /// </summary>
    public class DelayFilter : EventSourceBase, IEventPropagator
    {
        private readonly int _delayInMilliseconds;
        private NuntiusMessage _lastMessage;
        private CancellationTokenSource _cancellation;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="delayInMilliseconds">After this period the most recent message will be send.</param>
        public DelayFilter(int delayInMilliseconds)
        {
            _delayInMilliseconds = delayInMilliseconds;
            _cancellation = new CancellationTokenSource();
            MonitorAndSendMessages();
        }

        /// <summary>
        /// Returns a task which represents message processing by the event target.
        /// </summary>
        /// <param name="message">Message to process.</param>
        /// <returns>Task which represents message processing.</returns>
        public Task ProcessMessage(NuntiusMessage message)
        {
            _lastMessage = message;
            return Task.FromResult<object>(null);
        }


        /// <summary>
        /// Callback which is called when no more messages are generated by the event source.
        /// </summary>
        public void EndProcessing()
        {
            _cancellation.Cancel();
            SafelyInvokeEndEvent();
        }

        private Task MonitorAndSendMessages()
        {
            return Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    if (_cancellation.Token.IsCancellationRequested) break;
                    await Task.Delay(_delayInMilliseconds);
                    var msgToSend = Interlocked.Exchange(ref _lastMessage, null);
                    if (msgToSend == null) continue;
                    SafelyInvokeSendEvent(msgToSend);
                }
            });
        }
    }
}
