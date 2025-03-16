using SimpleGraph;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting
{
    [NodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    [NodeView(typeof(CareerStartNodeModel))]
    public class CareerStartNode : SimpleGraphNode
    {
        public const string OutputHiredPortName = "Hired";

        private readonly Port outputPort;

        public CareerStartNode() : base()
        {
            title = "Start";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.6f, 0.3f, 0.3f));

            // Add an output port for the first career progression nodes.
            outputPort = SimpleGraphUtils.CreatePort<ContractGraph>(OutputHiredPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            outputContainer.Add(outputPort);
        }

        public override SimpleGraphNodeModel CreateDefaultModel()
        {
            return new CareerStartNodeModel();
        }
    }
}
