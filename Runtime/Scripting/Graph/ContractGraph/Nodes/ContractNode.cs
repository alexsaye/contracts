using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting.Graph
{
    [NodeContext(typeof(ContractGraph))]
    [NodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    public class ContractNode : ScriptableGraphNode
    {
        private readonly ObservablePort fulfillPort;
        private readonly ObservablePort rejectPort;

        public ContractNode() : base()
        {
            title = "Contract";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.6f, 0.3f));

            // Add an input port for the conditions which fulfil the contract.
            fulfillPort = ObservablePort.Create<Edge>("Fulfill", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(ScriptableCondition));
            inputContainer.Add(fulfillPort);

            // Add an input port for the conditions which reject the contract.
            rejectPort = ObservablePort.Create<Edge>("Reject", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(ScriptableCondition));
            inputContainer.Add(rejectPort);
        }
    }
}
