using SimpleGraph;
using SimpleGraph.Editor;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting
{
    [SimpleGraphNodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    [SimpleGraphNodeModel(typeof(CareerEndNodeModel))]
    public class CareerEndNodeView : SimpleGraphNodeView
    {
        public const string InputRetiredPortName = "Retired";

        private readonly Port inputPort;

        public CareerEndNodeView() : base()
        {
            title = "End";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.6f, 0.3f));

            // Add an input port for the final career progression nodes.
            inputPort = CreatePort<ContractGraph>(InputRetiredPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputPort);
        }
    }
}
