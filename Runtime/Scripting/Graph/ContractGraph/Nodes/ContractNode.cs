using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Contracts.Scripting.Graph
{
    [NodeContext(typeof(ContractGraph))]
    [NodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    public class ContractNode : ScriptableGraphNode
    {
        private readonly Port fulfilPort;
        private readonly Port rejectPort;

        public ContractNode() : base("Contract", new Color(0.6f, 0.6f, 0.3f))
        {
            // Add an input port for the conditions which fulfil the contract.
            fulfilPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(ConditionNode));
            fulfilPort.name = fulfilPort.portName = "Fulfil";
            inputContainer.Add(fulfilPort);

            // Add an input port for the conditions which reject the contract.
            rejectPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(ConditionNode));
            rejectPort.name = rejectPort.portName = "Reject";
            inputContainer.Add(rejectPort);
        }
    }
}
