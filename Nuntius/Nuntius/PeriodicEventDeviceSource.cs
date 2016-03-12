﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuntius
{
    /// <summary>
    /// Represents base implementation of <see cref="IDeviceSourceEndpoint"/> which periodically sends
    /// a message.
    /// </summary>
    public abstract class PeriodicEventDeviceSource : BaseDeviceSourceEndpoint
    {
        private readonly int _intervalInMiliseconds;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="intervalInMiliseconds">This is an interval in milliseconds before sending a message again. 
        /// It must be at least 10 ms.</param>
        protected PeriodicEventDeviceSource(int intervalInMiliseconds)
        {
            if (intervalInMiliseconds < 10)
                throw new ArgumentException($"{nameof(intervalInMiliseconds)} must be greater or equal 10.");
            _intervalInMiliseconds = intervalInMiliseconds;
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(_intervalInMiliseconds);
                SendMessage(GetNextMessage());
            });
        }

        /// <summary>
        /// Function which is periodically called and its message is send.
        /// </summary>
        /// <returns></returns>
        protected abstract NuntiusMessage GetNextMessage();
    }
}
