using SimpleGraph;
using System;
using UnityEngine;

namespace Contracts.Scripting
{
    [Serializable]
    public class ConditionNode : SimpleGraphNode, IConditionBuilder
    {
        [SerializeReference]
        public IConditionBuilder Builder;

        public virtual ICondition Build()
        {
            Debug.Log($"Building {this}...");
            if (Builder == null)
            {
                Debug.LogWarning("Redundant condition node has no condition builder and will always be satisfied.");
                return Condition.Always;
            }
            return Builder.Build();
        }

        public override string ToString()
        {
            return $"{base.ToString()} ({Builder})";
        }
    }
}
