using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    [Serializable]
    public class CompositeConditionBuilder : ConditionBuilder
    {
        [Serializable]
        public enum CompositeMode
        {
            All,
            Any,
            // Count?
        }

        [SerializeField]
        private CompositeMode mode = CompositeMode.All;

        [SerializeField]
        private List<ConditionBuilder> subconditions;

        public CompositeConditionBuilder() : this(CompositeMode.All, new List<ConditionBuilder>())
        {
        }

        public CompositeConditionBuilder(CompositeMode mode, List<ConditionBuilder> subconditions)
        {
            this.mode = mode;
            this.subconditions = subconditions;
        }

        public override ICondition Build(UnityEvent updated)
        {
            var subconditions = this.subconditions
                .Select(condition => condition.Build(updated))
                .ToArray();

            var careerProgression = mode switch
            {
                CompositeMode.All => Condition.All(subconditions),
                CompositeMode.Any => Condition.Any(subconditions),
                _ => throw new NotImplementedException(),
            };

            return careerProgression;
        }
    }
}
