using SimpleGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Contracts.Scripting
{
    [Serializable]
    public class CompositeConditionNode : SimpleGraphNode, IConditionBuilder
    {
        public enum CompositeMode
        {
            All,
            Any,
        }

        [SerializeField]
        public CompositeMode Mode = CompositeMode.All;

        public IEnumerable<IConditionBuilder> Subconditions { get; set; }

        public ICondition Build()
        {
            Debug.Log($"Building {this}...");
            var subconditions = Subconditions
                .Select(builder => builder.Build())
                .ToList();
            return Mode switch
            {
                CompositeMode.All => Condition.All(subconditions),
                CompositeMode.Any => Condition.Any(subconditions),
                _ => throw new InvalidOperationException($"Unknown composite mode: {Mode}"),
            };
        }
    }
}
