using SimpleGraph;
using SimpleGraph.Editor;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Contracts.Scripting
{
    [SimpleGraphNodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    [SimpleGraphNodeView(typeof(ContractNode))]
    public class ContractNodeView : SimpleGraphNodeView
    {
        public const string InputFulfilledPortName = "Fulfilled";
        public const string InputRejectedPortName = "Rejected";

        private readonly Port inputFulfilledPort;
        private readonly Port inputRejectedPort;

        public ContractNodeView() : base()
        {
            title = "Contract";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.6f, 0.3f));

            // Add an input port for the conditions which fulfil the contract.
            inputFulfilledPort = CreatePort<IConditionBuilder>(InputFulfilledPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            inputContainer.Add(inputFulfilledPort);

            // Add an input port for the conditions which reject the contract.
            inputRejectedPort = CreatePort<IConditionBuilder>(InputRejectedPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            inputContainer.Add(inputRejectedPort);
        }
    }
}
