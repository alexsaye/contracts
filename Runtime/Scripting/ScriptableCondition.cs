using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Saye.Contracts.Scripting
{
    public abstract class ScriptableCondition : ScriptableObject
    {
        /// <summary>
        /// Create a condition based on a given assertion, with binding managed by Unity events.
        /// </summary>
        public static ICondition When(Func<bool> assert, IEnumerable<UnityEvent> bind)
        {
            return Condition.When(
                assert,
                bind: condition =>
                {
                    foreach (var e in bind)
                    {
                        e.AddListener(condition.Assert);
                    }
                },
                unbind: condition =>
                {
                    foreach (var e in bind)
                    {
                        e.RemoveListener(condition.Assert);
                    }
                }
            );
        }

        /// <summary>
        /// Create a condition based on a given assertion, with binding managed by Unity events.
        /// </summary>
        public static ICondition When(Func<bool> assert, params UnityEvent[] bind)
        {
            return When(assert, (IEnumerable<UnityEvent>)bind);
        }

        public abstract ICondition Build(UnityEvent update);
    }
}
