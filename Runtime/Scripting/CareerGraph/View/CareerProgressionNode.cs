using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEditor.UIElements;
using SimpleGraph;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using System.Linq;
using UnityEditor;

namespace Contracts.Scripting
{
    [NodeMenu("Career Progression")]
    [NodeView(typeof(CareerProgressionNodeModel))]
    public class CareerProgressionNode : SimpleGraphNode
    {
        public const string InputIssuedPortName = "Issued";
        public const string OutputFulfilledPortName = "Fulfilled";
        public const string OutputRejectedPortName = "Rejected";

        private readonly Port inputIssuedPort;
        private readonly Port outputFulfillPort;
        private readonly Port outputRejectPort;

        private readonly PropertyField contractField;

        public CareerProgressionNode() : base()
        {
            title = "Career Progression";
            titleContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.6f));

            // Add an input port for the previous career progression node to issue this progression through.
            inputIssuedPort = SimpleGraphUtils.CreatePort<ContractGraph>(InputIssuedPortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            inputContainer.Add(inputIssuedPort);

            // Add an output port for the next career progression nodes when fulfilled.
            outputFulfillPort = SimpleGraphUtils.CreatePort<ContractGraph>(OutputFulfilledPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            outputContainer.Add(outputFulfillPort);

            // Add an output port for the next career progression nodes when rejected.
            outputRejectPort = SimpleGraphUtils.CreatePort<ContractGraph>(OutputRejectedPortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            outputContainer.Add(outputRejectPort);

            // Add a field to select a contract.
            contractField = new();
            inputContainer.Add(contractField);
        }

        public override SimpleGraphNodeModel CreateDefaultModel()
        {
            return new CareerProgressionNodeModel();
        }

        protected override void RenderModel(SerializedProperty serializedNodeModel)
        {
            contractField.bindingPath = serializedNodeModel
                    .FindPropertyRelative("contract")
                    .propertyPath;
            inputContainer.Bind(serializedNodeModel.serializedObject);
        }
    }
}
