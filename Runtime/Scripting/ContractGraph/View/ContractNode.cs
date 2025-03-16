using SimpleGraph;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Contracts.Scripting
{
    [NodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    [NodeView(typeof(ContractNodeModel))]
    public class ContractNode : SimpleGraphNode
    {
        public const string InputFulfilledPortName = "Fulfilled";
        public const string InputRejectedPortName = "Rejected";

        private readonly Port inputFulfilledPort;
        private readonly Port inputRejectedPort;

        public ContractNode() : base()
        {
            title = "Contract";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.6f, 0.3f));

            // Add an input port for the conditions which fulfil the contract.
            inputFulfilledPort = SimpleGraphUtils.CreatePort<IConditionBuilder>(InputFulfilledPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            inputContainer.Add(inputFulfilledPort);

            // Add an input port for the conditions which reject the contract.
            inputRejectedPort = SimpleGraphUtils.CreatePort<IConditionBuilder>(InputRejectedPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            inputContainer.Add(inputRejectedPort);
        }

        public override SimpleGraphNodeModel CreateDefaultModel()
        {
            return new ContractNodeModel();
        }
    }
}
