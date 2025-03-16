using SimpleGraph;
using System;
using UnityEngine;

namespace Contracts.Scripting
{
    [Serializable]
    public class CareerProgressionNode : SimpleGraphNode
    {
        [SerializeReference]
        public ContractGraph Contract;
    }
}
