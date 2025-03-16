using SimpleGraph;
using SimpleGraph.Editor;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting
{
    [SimpleGraphNodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    [SimpleGraphNodeModel(typeof(CareerStartNodeModel))]
    public class CareerStartNodeView : SimpleGraphNodeView
    {
        public const string OutputHiredPortName = "Hired";

        private readonly Port outputPort;

        public CareerStartNodeView() : base()
        {
            title = "Start";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.6f, 0.3f, 0.3f));

            // Add an output port for the first career progression nodes.
            outputPort = CreatePort<ContractGraph>(OutputHiredPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            outputContainer.Add(outputPort);
        }
    }
}
