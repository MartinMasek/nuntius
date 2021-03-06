﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuntius
{
    /// <summary>
    /// Represents an ability to consume <see cref="NuntiusMessage"/>.
    /// </summary>
    public interface IEventTarget
    {
        /// <summary>
        /// Returns a task which represents message processing by the event target.
        /// </summary>
        /// <param name="message">Message to process.</param>
        /// <returns>Task which represents message processing.</returns>
        Task ProcessMessage(NuntiusMessage message);

        /// <summary>
        /// Callback which is called when no more messages are generated by the event source.
        /// </summary>
        void EndProcessing();
    }
}
