using SimpleGraph;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    [Serializable]
    public class ConditionNode : SimpleGraphNode
    {
        [SerializeReference]
        public IConditionBuilder Builder;
    }
}
