using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEditor;
using SimpleGraph.Editor;
using Contracts.Scripting;

namespace Contracts.Editor.Scripting
{
    [SimpleGraphNodeMenu(typeof(CareerGraph), "Contract Progression")]
    [SimpleGraphNodeView(typeof(ProgressionNode))]
    public class ProgressionNodeView : SimpleGraphNodeView
    {
        private readonly Port inputIssuedPort;
        private readonly Port outputFulfillPort;
        private readonly Port outputRejectPort;

        private readonly PropertyField contractField;

        public ProgressionNodeView() : base()
        {
            title = "Progression";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.6f));

            // Add an input port for the previous progression node to issue this progression through.
            inputIssuedPort = CreatePort<ContractGraph>("Issue", Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            inputContainer.Add(inputIssuedPort);

            // Add an output port for the next progression nodes when fulfilled.
            outputFulfillPort = CreatePort<ContractGraph>("Fulfilled", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            outputContainer.Add(outputFulfillPort);

            // Add an output port for the next progression nodes when rejected.
            outputRejectPort = CreatePort<ContractGraph>("Rejected", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            outputContainer.Add(outputRejectPort);

            // Add a field to select a contract.
            contractField = new();
            contractField.label = "";
            extensionContainer.Add(contractField);
        }

        protected override void RenderModel(SerializedProperty serializedNode)
        {
            contractField.bindingPath = serializedNode.FindPropertyRelative(nameof(ProgressionNode.Contract)).propertyPath;
            extensionContainer.Bind(serializedNode.serializedObject);
        }
    }
}
