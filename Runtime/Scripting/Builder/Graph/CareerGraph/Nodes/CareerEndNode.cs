using SimpleGraph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting
{
    [NodeContext(typeof(CareerProgressionGraph))]
    [NodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    public class CareerEndNode : SimpleGraphViewNode
    {
        public const string InputRetiredPortName = "Retired";

        private readonly Port inputPort;

        public CareerEndNode() : base()
        {
            title = "End";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.6f, 0.3f));

            // Add an input port for the final career progression nodes.
            inputPort = ObservablePort.Create<Edge>(InputRetiredPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(CareerProgressionBuilder));
            inputContainer.Add(inputPort);
        }
    }
}
