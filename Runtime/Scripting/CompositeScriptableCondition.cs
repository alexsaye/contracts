using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    [CreateAssetMenu(menuName = "Contracts/Asset/Conditions/Composite")]

    public class CompositeScriptableCondition : ScriptableCondition
    {
        [Serializable]
        public enum CompositeMode
        {
            All,
            Any
        }

        [SerializeField]
        private CompositeMode mode = CompositeMode.All;

        [SerializeField]
        private List<ScriptableCondition> subconditions;

        public override ICondition Build(UnityEvent updated)
        {
            // Build all the subconditions.
            var subconditions = this.subconditions.Select(condition => condition.Build(updated)).ToArray();

            // Create the composite condition from the conditions.
            return mode switch
            {
                CompositeMode.All => Condition.All(subconditions),
                CompositeMode.Any => Condition.Any(subconditions),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
