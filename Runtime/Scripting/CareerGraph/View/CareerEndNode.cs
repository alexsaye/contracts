using SimpleGraph;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting
{
    [NodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    [NodeView(typeof(CareerEndNodeModel))]
    public class CareerEndNode : SimpleGraphNode
    {
        public const string InputRetiredPortName = "Retired";

        private readonly Port inputPort;

        public CareerEndNode() : base()
        {
            title = "End";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.6f, 0.3f));

            // Add an input port for the final career progression nodes.
            inputPort = SimpleGraphUtils.CreatePort<ContractGraph>(InputRetiredPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputPort);
        }

        public override SimpleGraphNodeModel CreateDefaultModel()
        {
            return new CareerEndNodeModel();
        }
    }
}
