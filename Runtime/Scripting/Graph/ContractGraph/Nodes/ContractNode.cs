using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Contracts.Scripting.Graph
{
    [NodeContext(typeof(ContractGraph))]
    [NodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    public class ContractNode : ScriptableGraphNode
    {
        private readonly Port fulfillPort;
        private readonly Port rejectPort;

        public ContractNode() : base("Contract", new Color(0.6f, 0.6f, 0.3f))
        {
            // Add an input port for the conditions which fulfil the contract.
            fulfillPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(ConditionNode));
            fulfillPort.name = fulfillPort.portName = "Fulfill";
            inputContainer.Add(fulfillPort);

            // Add an input port for the conditions which reject the contract.
            rejectPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(ConditionNode));
            rejectPort.name = rejectPort.portName = "Reject";
            inputContainer.Add(rejectPort);
        }
    }
}
