using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEditor.UIElements;
using SimpleGraph;

namespace Contracts.Scripting
{
    [NodeMenu("Career Progression")]
    [NodeContext(typeof(CareerProgressionGraph))]
    public class CareerProgressionNode : SimpleGraphNode
    {
        public const string InputIssuedPortName = "Issued";
        public const string OutputFulfilledPortName = "Fulfilled";
        public const string OutputRejectedPortName = "Rejected";

        private readonly PropertyField contractField;
        private readonly Port inputIssuedPort;
        private readonly Port outputFulfillPort;
        private readonly Port outputRejectPort;

        public CareerProgressionNode() : base()
        {
            title = "Career Progression";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.6f));

            // Add an input port for the previous career progression node to issue this progression through.
            inputIssuedPort = SimpleGraphUtils.CreatePort<CareerProgressionBuilder>(InputIssuedPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            inputContainer.Add(inputIssuedPort);

            // Add an output port for the next career progression nodes when fulfilled.
            outputFulfillPort = SimpleGraphUtils.CreatePort<CareerProgressionBuilder>(OutputFulfilledPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            outputContainer.Add(outputFulfillPort);

            // Add an output port for the next career progression nodes when rejected.
            outputRejectPort = SimpleGraphUtils.CreatePort<CareerProgressionBuilder>(OutputRejectedPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            outputContainer.Add(outputRejectPort);

            // Add a field to select a contract.
            contractField = new();
            inputContainer.Add(contractField);
        }

        protected override void RenderModel()
        {
            contractField.bindingPath = SerializedNodeModel
                    .FindPropertyRelative("value")
                    .FindPropertyRelative("contract")
                    .propertyPath;
            inputContainer.Bind(SerializedNodeModel.serializedObject);
        }

        public override object GetDefaultValue()
        {
            return new CareerProgressionBuilder();
        }
    }
}
