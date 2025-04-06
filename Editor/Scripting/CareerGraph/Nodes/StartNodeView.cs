using Contracts.Scripting;
using SimpleGraph.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Editor.Scripting
{
    [SimpleGraphNodeCapabilities(~Capabilities.Deletable & ~Capabilities.Copiable & ~Capabilities.Resizable)]
    [SimpleGraphNodeView(typeof(StartNode))]
    public class StartNodeView : SimpleGraphNodeView
    {
        private readonly Port outputPort;

        public StartNodeView() : base()
        {
            title = "Start";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.6f, 0.3f, 0.3f));

            // Add an output port for the first career progression nodes.
            outputPort = CreatePort<ContractGraph>("Hired", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            outputContainer.Add(outputPort);
        }
    }
}
