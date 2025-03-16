using SimpleGraph;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    [Serializable]
    public class CompositeConditionNode : SimpleGraphNode
    {
        public enum CompositeMode
        {
            All,
            Any,
        }

        [SerializeField]
        public CompositeMode Mode = CompositeMode.All;
    }
}
