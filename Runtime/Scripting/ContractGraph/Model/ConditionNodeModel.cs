using SimpleGraph;
using System;
using UnityEngine;

namespace Contracts.Scripting
{
    [Serializable]
    public class ConditionNodeModel : SimpleGraphNodeModel
    {
        [SerializeReference]
        public IConditionBuilder Builder;
    }
}
