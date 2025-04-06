using Contracts.Scripting;
using SimpleGraph.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Editor.Scripting
{
    [SimpleGraphNodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    [SimpleGraphNodeView(typeof(ContractNode))]
    public class ContractNodeView : SimpleGraphNodeView
    {
        private readonly Port inputFulfillingPort;
        private readonly Port inputRejectingPort;

        public ContractNodeView() : base()
        {
            title = "Contract";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.6f, 0.3f));

            // Add an input port for the conditions which fulfil the contract.
            inputFulfillingPort = CreatePort<IConditionBuilder>("Fulfil", Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            inputContainer.Add(inputFulfillingPort);

            // Add an input port for the conditions which reject the contract.
            inputRejectingPort = CreatePort<IConditionBuilder>("Reject", Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            inputContainer.Add(inputRejectingPort);
        }
    }
}
