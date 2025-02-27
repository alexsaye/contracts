using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Contracts.Scripting.Graph
{
    [NodeContext(typeof(ContractGraph))]
    [NodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    public class ContractNode : ScriptableGraphNode
    {
        [NodeInput(Port.Capacity.Single)]
        public bool Fulfill;

        [NodeInput(Port.Capacity.Single)]
        public bool Reject;

        public ContractNode() : base("Contract", new Color(0.6f, 0.6f, 0.3f))
        {
        }
    }
}
