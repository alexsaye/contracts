﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimpleGraph.Editor
{
    public class SimpleGraphView : GraphView
    {
        private static readonly Dictionary<Type, Type> modelToViewMapping;

        static SimpleGraphView()
        {
            modelToViewMapping = new Dictionary<Type, Type>();

            // Find all node types in the current domain.
            var nodeViewTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(SimpleGraphNodeView)));

            foreach (var nodeViewType in nodeViewTypes)
            {
                var modelAttribute = nodeViewType.GetCustomAttribute<SimpleGraphNodeViewAttribute>();
                if (modelAttribute == null)
                {
                    Debug.LogWarning($"Node view {nodeViewType} does not have a {nameof(SimpleGraphNodeViewAttribute)} and will be ignored.");
                    continue;
                }

                modelToViewMapping.Add(modelAttribute.ModelType, nodeViewType);
            }
        }

        private readonly SerializedProperty serializedGraphModel;

        public SimpleGraphView(SerializedProperty serializedGraphModel)
        {
            this.serializedGraphModel = serializedGraphModel;

            var serializedNodes = serializedGraphModel.FindPropertyRelative(nameof(SimpleGraphModel.Nodes));
            var serializedNodesEnumerator = serializedNodes.GetEnumerator();
            while (serializedNodesEnumerator.MoveNext())
            {
                AddNodeView((SerializedProperty)serializedNodesEnumerator.Current);
            }

            var serializedEdges = serializedGraphModel.FindPropertyRelative(nameof(SimpleGraphModel.Edges));
            var serializedEdgesEnumerator = serializedEdges.GetEnumerator();
            while (serializedEdgesEnumerator.MoveNext())
            {
                AddEdgeView((SerializedProperty)serializedEdgesEnumerator.Current);
            }

            this.StretchToParentSize();
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
            var grid = new GridBackground();
            grid.name = "Grid";
            Add(grid);
            grid.SendToBack();
            //styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>($"Packages/com.contracts/Runtime/Scripting/Graph/USS/ScriptableGraphView.uss"));

            graphViewChanged = OnGraphViewChanged;
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent change)
        {
            contentViewContainer.transform.position = new Vector3(layout.width / 2, layout.height / 2, 0);
        }

        /// <summary>
        /// Action handler triggered when the graph view changes, except not when a new node is added (thanks Unity).
        /// </summary>
        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            // Delete from the model any nodes or edges that have been removed from the view.
            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is SimpleGraphNodeView nodeView)
                    {
                        DeleteNodeModel(nodeView);
                    }
                    else if (element is Edge edgeView)
                    {
                        DeleteEdgeModel(edgeView);
                    }
                }
            }

            // Update in the model any nodes that have been moved in the view.
            if (change.movedElements != null)
            {
                foreach (var element in change.movedElements)
                {
                    if (element is SimpleGraphNodeView nodeView)
                    {
                        MoveNodeModel(nodeView);
                    }
                }
            }

            // Add to the model any edges that have been added to the view.
            if (change.edgesToCreate != null)
            {
                foreach (var edgeView in change.edgesToCreate)
                {
                    var outputNodeView = edgeView.output.node as SimpleGraphNodeView;
                    var inputNodeView = edgeView.input.node as SimpleGraphNodeView;
                    var edgeModel = new SimpleGraphEdge(outputNodeView.Guid, inputNodeView.Guid, edgeView.output.portName, edgeView.input.portName);
                    AddEdgeModel(edgeModel);
                }
            }

            serializedGraphModel.serializedObject.ApplyModifiedProperties();
            return change;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port || startPort.node == port.node)
                    return;

                if (startPort.portType == port.portType)
                {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent populate)
        {
            var graphType = serializedGraphModel.serializedObject.targetObject.GetType();
            var mousePosition = new Rect(contentViewContainer.WorldToLocal(populate.mousePosition), Vector2.zero);
            foreach (var (modelType, viewType) in modelToViewMapping)
            {
                var menuAttribute = viewType.GetCustomAttribute<SimpleGraphNodeMenuAttribute>();
                if (menuAttribute == null || menuAttribute.GraphType != graphType)
                {
                    continue;
                }

                populate.menu.AppendAction(menuAttribute.MenuName, (action) =>
                {
                    // Create the selected node view and its associated model.
                    var node = Activator.CreateInstance(modelType) as SimpleGraphNode;

                    // Set the node's position to the mouse position.
                    node.Position = mousePosition;

                    // Add the node model to the graph model and apply the new node change.
                    var serializedNode = AddNodeModel(node);
                    serializedGraphModel.serializedObject.ApplyModifiedProperties();

                    // Add the node view to the graph view.
                    AddNodeView(serializedNode);
                });
            }
            populate.menu.AppendSeparator();

            base.BuildContextualMenu(populate);
        }

        private SimpleGraphNodeView AddNodeView(SerializedProperty serializedNode)
        {
            // Create the associated view for the model type.
            var modelType = serializedNode.managedReferenceValue.GetType();
            var viewType = modelToViewMapping[modelType];
            var nodeView = Activator.CreateInstance(viewType) as SimpleGraphNodeView;

            // Load the model into the view and add it to the graph.
            nodeView.LoadModel(serializedNode);
            AddElement(nodeView);
            return nodeView;
        }

        private SerializedProperty AddNodeModel(SimpleGraphNode node)
        {
            var serializedNodes = serializedGraphModel.FindPropertyRelative(nameof(SimpleGraphModel.Nodes));

            // Insert a new node (this will be a managed reference of null).
            var index = serializedNodes.arraySize;
            serializedNodes.InsertArrayElementAtIndex(index);

            // Create a model from the node and assign it to the managed reference.
            var serializedNode = serializedNodes.GetArrayElementAtIndex(index);
            serializedNode.managedReferenceValue = node;
            return serializedNode;
        }

        private void DeleteNodeModel(SimpleGraphNodeView nodeView)
        {
            var serializedNodes = serializedGraphModel.FindPropertyRelative(nameof(SimpleGraphModel.Nodes));
            var serializedNodesEnumerator = serializedNodes.GetEnumerator();
            while (serializedNodesEnumerator.MoveNext())
            {
                var serializedNode = (SerializedProperty)serializedNodesEnumerator.Current;
                var node = serializedNode.managedReferenceValue as SimpleGraphNode;
                if (node.Guid == nodeView.Guid)
                {
                    if (!serializedNode.DeleteCommand())
                    {
                        throw new InvalidOperationException("Failed to delete node from model.");
                    }
                    return;
                }
            }
            throw new InvalidOperationException("Node not found in model.");
        }

        private void MoveNodeModel(SimpleGraphNodeView nodeView)
        {
            var serializedNodes = serializedGraphModel.FindPropertyRelative(nameof(SimpleGraphModel.Nodes));
            var serializedNodesEnumerator = serializedNodes.GetEnumerator();
            while (serializedNodesEnumerator.MoveNext())
            {
                var serializedNode = (SerializedProperty)serializedNodesEnumerator.Current;
                var node = serializedNode.managedReferenceValue as SimpleGraphNode;
                if (node.Guid == nodeView.Guid)
                {
                    serializedNode.FindPropertyRelative(nameof(SimpleGraphNode.Position)).rectValue = nodeView.GetPosition();
                    return;
                }
            }
            throw new InvalidOperationException("Node not found in model.");
        }

        private Edge AddEdgeView(SerializedProperty serializedEdge)
        {
            var outputNodeGuid = serializedEdge.FindPropertyRelative("outputNodeGuid").stringValue;
            var outputNodeView = graphElements
                .Where((element) => element is SimpleGraphNodeView)
                .Select((element) => element as SimpleGraphNodeView)
                .First((nodeView) => nodeView.Guid == outputNodeGuid);

            var inputNodeGuid = serializedEdge.FindPropertyRelative("inputNodeGuid").stringValue;
            var inputNodeView = graphElements
                .Where((element) => element is SimpleGraphNodeView)
                .Select((element) => element as SimpleGraphNodeView)
                .First((nodeView) => nodeView.Guid == inputNodeGuid);

            var outputPortName = serializedEdge.FindPropertyRelative("outputPortName").stringValue;
            var outputPort = outputNodeView.outputContainer.Q<Port>(outputPortName);

            var inputPortName = serializedEdge.FindPropertyRelative("inputPortName").stringValue;
            var inputPort = inputNodeView.inputContainer.Q<Port>(inputPortName);

            var edgeView = new Edge
            {
                input = inputPort,
                output = outputPort
            };

            inputPort.Connect(edgeView);
            outputPort.Connect(edgeView);
            AddElement(edgeView);
            return edgeView;
        }

        private SerializedProperty AddEdgeModel(SimpleGraphEdge edgeModel)
        {
            var serializedEdges = serializedGraphModel.FindPropertyRelative(nameof(SimpleGraphModel.Edges));
            var index = serializedEdges.arraySize;
            ++serializedEdges.arraySize;

            // TODO: more strings - not good!
            var serializedEdge = serializedEdges.GetArrayElementAtIndex(index);
            serializedEdge.FindPropertyRelative("outputNodeGuid").stringValue = edgeModel.OutputNodeGuid;
            serializedEdge.FindPropertyRelative("inputNodeGuid").stringValue = edgeModel.InputNodeGuid;
            serializedEdge.FindPropertyRelative("outputPortName").stringValue = edgeModel.OutputPortName;
            serializedEdge.FindPropertyRelative("inputPortName").stringValue = edgeModel.InputPortName;
            return serializedEdge;
        }

        private void DeleteEdgeModel(Edge edgeView)
        {
            var outputNodeView = edgeView.output.node as SimpleGraphNodeView;
            var inputNodeView = edgeView.input.node as SimpleGraphNodeView;

            var serializedEdges = serializedGraphModel.FindPropertyRelative(nameof(SimpleGraphModel.Edges));
            var serializedEdgesEnumerator = serializedEdges.GetEnumerator();
            while (serializedEdgesEnumerator.MoveNext())
            {
                var serializedEdgeView = (SerializedProperty)serializedEdgesEnumerator.Current;
                // TODO: more strings - also not good! (maybe the edges should be stored in the nodes themselves?)
                if (serializedEdgeView.FindPropertyRelative("outputNodeGuid").stringValue == outputNodeView.Guid &&
                    serializedEdgeView.FindPropertyRelative("inputNodeGuid").stringValue == inputNodeView.Guid &&
                    serializedEdgeView.FindPropertyRelative("outputPortName").stringValue == edgeView.output.portName &&
                    serializedEdgeView.FindPropertyRelative("inputPortName").stringValue == edgeView.input.portName)
                {
                    if (!serializedEdgeView.DeleteCommand())
                    {
                        throw new InvalidOperationException("Failed to delete edge from model.");
                    }
                    return;
                }
            }
            throw new InvalidOperationException("Edge not found in model.");
        }
    }
}
