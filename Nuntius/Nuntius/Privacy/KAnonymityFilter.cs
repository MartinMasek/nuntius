﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuntius.Privacy
{
    /// <summary>
    /// Represents a filter which distributes incoming messages to sets. The set decides what to do 
    /// with the message and outputs a result.
    /// </summary>
    public class KAnonymityFilter : EventSourceBase, IEventPropagator
    {
        private readonly Dictionary<int, IKAnonymitySet> _sets = new Dictionary<int, IKAnonymitySet>();
        private readonly Func<NuntiusMessage, int> _chooseSet;

        /// <summary>
        /// Creates a new instance which does not check its listeners task exceptions.
        /// </summary>
        /// <param name="sets">Sets used by k-set algorithm.</param>
        /// <param name="chooseSet">This function have to choose a proper set from the sets
        /// based on passed <see cref="NuntiusMessage"/>.</param>
        public KAnonymityFilter(IKAnonymitySet[] sets, Func<NuntiusMessage, int> chooseSet)
            : this(sets, chooseSet, EventSourceCallbackMonitoringOptions.NotCheckTaskException)
        { }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="sets">Sets used by k-set algorithm.</param>
        /// <param name="chooseSet">This function have to choose a proper set from the sets
        /// based on passed <see cref="NuntiusMessage"/>.</param>
        /// <param name="options">Configuration regarding checking of tasks returned by <see cref="EventSourceBase.Send"/> event
        /// handlers.</param>
        public KAnonymityFilter(IKAnonymitySet[] sets, Func<NuntiusMessage, int> chooseSet,
            EventSourceCallbackMonitoringOptions options) : base(options)
        {
            if (sets == null || sets.Length == 0) throw new ArgumentException($"Parameter {nameof(sets)} must be array of length at least 1.");
            foreach (var set in sets)
            {
                if (set == null) throw new ArgumentNullException($"One set from {nameof(sets)} was null.");
                if (_sets.ContainsKey(set.Id)) throw new ArgumentException($"There was a set with the same id {set.Id} as a different set.");
                _sets.Add(set.Id, set);
            }

            if (chooseSet == null) throw new ArgumentNullException($"{nameof(chooseSet)} function was null.");
            _chooseSet = chooseSet;
        }

        /// <summary>
        /// Returns a task which represents message processing by the event target.
        /// </summary>
        /// <param name="message">Message to process.</param>
        /// <returns>Task which represents message processing.</returns>
        public Task ProcessMessage(NuntiusMessage message)
        {
            return Task.Factory.StartNew(() =>
            {
                var setId = _chooseSet(message);
                IKAnonymitySet set;
                if (!_sets.TryGetValue(setId, out set))
                {
                    throw new KeyNotFoundException(
                        $"Element for message {message} returned set id {setId} which was not present between sets.");
                }
                var result = set.OfferMessage(message);
                if (result != null) SafelyInvokeSendEvent(result);
            });

        }

        /// <summary>
        /// Callback which is called when no more messages are generated by the event source.
        /// </summary>
        public void EndProcessing()
        {
            SafelyInvokeEndEvent();
        }
    }
}
