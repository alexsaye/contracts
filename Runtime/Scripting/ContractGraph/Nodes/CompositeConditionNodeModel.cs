using SimpleGraph;
using System;
using UnityEngine;

namespace Contracts.Scripting
{
    [Serializable]
    public class CompositeConditionNodeModel : SimpleGraphNodeModel
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
