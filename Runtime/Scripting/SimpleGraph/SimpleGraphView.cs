using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimpleGraph
{
    public class SimpleGraphView : GraphView
    {
        private static readonly Type[] allNodeTypes;
        private static readonly Dictionary<Type, Dictionary<string, Type>> contextualisedNodeTypes;

        static SimpleGraphView()
        {
            // Find all node types in the current domain.
            allNodeTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(SimpleGraphNode)))
                .ToArray();

            // Build the menu nodes from all nodes that have a menu attribute.
            contextualisedNodeTypes = new Dictionary<Type, Dictionary<string, Type>>();
            foreach (var nodeType in allNodeTypes)
            {
                // Populate the menu nodes for each graph type referenced by the node's context attributes.
                var menuAttributes = nodeType.GetCustomAttributes<NodeMenuAttribute>();
                foreach (var menuAttribute in menuAttributes)
                {
                    if (!contextualisedNodeTypes.TryGetValue(menuAttribute.GraphType, out var graphMenuNodes))
                    {
                        graphMenuNodes = new Dictionary<string, Type>();
                        contextualisedNodeTypes[menuAttribute.GraphType] = graphMenuNodes;
                    }
                    graphMenuNodes[menuAttribute.MenuName] = nodeType;
                }
            }
        }

        private Dictionary<string, Type> menuNodeTypes;
        private SerializedProperty serializedGraphModel;

        public SimpleGraphView(SerializedProperty serializedGraphModel, Type contextualType)
        {
            contextualisedNodeTypes.TryGetValue(contextualType, out menuNodeTypes);
            if (menuNodeTypes == null)
            {
                throw new ArgumentException($"No node contexts found for type {contextualType}");
            }

            this.serializedGraphModel = serializedGraphModel;

            var serializedNodes = serializedGraphModel.FindPropertyRelative(nameof(SimpleGraphModel.Nodes));
            var serializedNodesEnumerator = serializedNodes.GetEnumerator();
            while (serializedNodesEnumerator.MoveNext())
            {
                AddNode((SerializedProperty)serializedNodesEnumerator.Current);
            }

            var serializedEdges = serializedGraphModel.FindPropertyRelative(nameof(SimpleGraphModel.Edges));
            var serializedEdgesEnumerator = serializedEdges.GetEnumerator();
            while (serializedEdgesEnumerator.MoveNext())
            {
                AddEdge((SerializedProperty)serializedEdgesEnumerator.Current);
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

        public bool IsShowingModel(SerializedProperty serializedGraphModel)
        {
            return this.serializedGraphModel == serializedGraphModel;
        }

        private void OnGeometryChanged(GeometryChangedEvent geometryChangedEvent)
        {
            contentViewContainer.transform.position = new Vector3(layout.width / 2, layout.height / 2, 0);
        }

        /// <summary>
        /// Action handler triggered when the graph view changes, except not when a new node is added (thanks Unity).
        /// </summary>
        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            Debug.Log($"Graph view changed: {change}");

            // Delete from the model any nodes or edges that have been removed from the view.
            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is SimpleGraphNode node)
                    {
                        DeleteNodeFromModel(node);
                    }
                    else if (element is Edge edge)
                    {
                        DeleteEdgeFromModel(edge);
                    }
                }
            }

            // Update in the model any nodes that have been moved in the view.
            if (change.movedElements != null)
            {
                foreach (var element in change.movedElements)
                {
                    if (element is SimpleGraphNode node)
                    {
                        UpdateNodeInModel(node);
                    }
                }
            }

            // Add to the model any edges that have been added to the view.
            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    AddEdgeToModel(edge);
                }
            }

            serializedGraphModel.serializedObject.ApplyModifiedProperties();
            return change;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var mousePosition = new Rect(contentViewContainer.WorldToLocal(evt.mousePosition), Vector2.zero);
            foreach (var (menuName, nodeType) in menuNodeTypes)
            {
                evt.menu.AppendAction(menuName, (action) =>
                {
                    // Create a node of the chosen type at the mouse position.
                    var node = CreateNode(nodeType);

                    // Save the node to the model at the mouse position.
                    var serializedNodeModel = AddNodeToModel(node, mousePosition);
                    serializedGraphModel.serializedObject.ApplyModifiedProperties();

                    // Load the model into the node.
                    node.LoadModel(serializedNodeModel);
                });
            }
            evt.menu.AppendSeparator();

            base.BuildContextualMenu(evt);
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

        private SimpleGraphNode CreateNode(Type type)
        {
            var viewNode = (SimpleGraphNode)Activator.CreateInstance(type);
            AddElement(viewNode);
            return viewNode;
        }

        private SimpleGraphNode AddNode(SerializedProperty serializedEdgeModel)
        {
            // Get the type of the model.
            var modelType = serializedEdgeModel.managedReferenceFullTypename.GetType();

            // Find in the assembly any node types which extend SimpleGraphNode<T> where T is the type of the model.
            var nodeTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(SimpleGraphNode)))
                .ToList();
            
            if (nodeTypes.Count == 0)
            {
                throw new UnityException($"No node types found for model type {modelType}.");
            }

            if (nodeTypes.Count > 1)
            {
                throw new UnityException($"Multiple node types found for model type {modelType}: {string.Join(", ", nodeTypes.Select((type) => type.Name))}.");
            }

            var nodeType = nodeTypes.First();
            var node = CreateNode(nodeType);
            node.LoadModel(serializedEdgeModel);
            return node;
        }


        private SerializedProperty AddNodeToModel(SimpleGraphNode node, Rect position)
        {
            var serializedNodes = serializedGraphModel.FindPropertyRelative(nameof(SimpleGraphModel.Nodes));

            // Insert a new node (this will be a managed reference of null).
            var index = serializedNodes.arraySize;
            serializedNodes.InsertArrayElementAtIndex(index);

            // Create a model from the node and assign it to the managed reference.
            var nodeModel = node.CreateDefaultModel();
            var serializedNodeModel = serializedNodes.GetArrayElementAtIndex(index);
            serializedNodeModel.managedReferenceValue = nodeModel;

            Debug.Log($"Added node {nodeModel}.");
            return serializedNodeModel;
        }

        private void DeleteNodeFromModel(SimpleGraphNode node)
        {
            var serializedNodes = serializedGraphModel.FindPropertyRelative(nameof(SimpleGraphModel.Nodes));
            var serializedNodesEnumerator = serializedNodes.GetEnumerator();
            while (serializedNodesEnumerator.MoveNext())
            {
                var serializedNode = (SerializedProperty)serializedNodesEnumerator.Current;
                if (serializedNode.FindPropertyRelative("guid").stringValue == node.Guid)
                {
                    if (!serializedNode.DeleteCommand())
                    {
                        throw new InvalidOperationException("Failed to delete node from model.");
                    }
                    Debug.Log($"Deleted node {node.Guid}.");
                    return;
                }
            }
            throw new InvalidOperationException("Node not found in model.");
        }

        private void UpdateNodeInModel(SimpleGraphNode node)
        {
            var serializedNodes = serializedGraphModel.FindPropertyRelative(nameof(SimpleGraphModel.Nodes));
            var serializedNodesEnumerator = serializedNodes.GetEnumerator();
            while (serializedNodesEnumerator.MoveNext())
            {
                var serializedNode = (SerializedProperty)serializedNodesEnumerator.Current;
                if (serializedNode.FindPropertyRelative("guid").stringValue == node.Guid)
                {
                    serializedNode.FindPropertyRelative("position").rectValue = node.GetPosition();
                    Debug.Log($"Updated node {node.Guid} to position {node.GetPosition()}.");
                    return;
                }
            }
            throw new InvalidOperationException("Node not found in model.");
        }

        private Edge AddEdge(SerializedProperty serializedEdgeModel)
        {
            var outputNodeGuid = serializedEdgeModel.FindPropertyRelative("outputNodeGuid").stringValue;
            var outputNode = graphElements
                .Where((element) => element is SimpleGraphNode)
                .Select((element) => element as SimpleGraphNode)
                .First((node) => node.Guid == outputNodeGuid);

            var inputNodeGuid = serializedEdgeModel.FindPropertyRelative("inputNodeGuid").stringValue;
            var inputNode = graphElements
                .Where((element) => element is SimpleGraphNode)
                .Select((element) => element as SimpleGraphNode)
                .First((node) => node.Guid == inputNodeGuid);

            var outputPortName = serializedEdgeModel.FindPropertyRelative("outputPortName").stringValue;
            var outputPort = outputNode.outputContainer.Q<Port>(outputPortName);

            var inputPortName = serializedEdgeModel.FindPropertyRelative("inputPortName").stringValue;
            var inputPort = inputNode.inputContainer.Q<Port>(inputPortName);

            var edge = new Edge
            {
                input = inputPort,
                output = outputPort
            };

            inputPort.Connect(edge);
            outputPort.Connect(edge);
            AddElement(edge);
            return edge;
        }

        private SerializedProperty AddEdgeToModel(Edge edge)
        {
            var outputNode = edge.output.node as SimpleGraphNode;
            var inputNode = edge.input.node as SimpleGraphNode;
            var edgeModel = new SimpleGraphEdgeModel(outputNode.Guid, inputNode.Guid, edge.output.portName, edge.input.portName);

            var serializedEdges = serializedGraphModel.FindPropertyRelative(nameof(SimpleGraphModel.Edges));
            var index = serializedEdges.arraySize;
            ++serializedEdges.arraySize;

            var serializedEdge = serializedEdges.GetArrayElementAtIndex(index);
            serializedEdge.FindPropertyRelative("outputNodeGuid").stringValue = edgeModel.OutputNodeGuid;
            serializedEdge.FindPropertyRelative("inputNodeGuid").stringValue = edgeModel.InputNodeGuid;
            serializedEdge.FindPropertyRelative("outputPortName").stringValue = edgeModel.OutputPortName;
            serializedEdge.FindPropertyRelative("inputPortName").stringValue = edgeModel.InputPortName;

            Debug.Log($"Added edge from the output port {edgeModel.OutputPortName} of node {edgeModel.OutputNodeGuid} to the input port {edgeModel.InputPortName} of node {edgeModel.InputNodeGuid}.");
            return serializedEdge;
        }

        private void DeleteEdgeFromModel(Edge edge)
        {
            var outputNode = edge.output.node as SimpleGraphNode;
            var inputNode = edge.input.node as SimpleGraphNode;

            var serializedEdges = serializedGraphModel.FindPropertyRelative(nameof(SimpleGraphModel.Edges));
            var serializedEdgesEnumerator = serializedEdges.GetEnumerator();
            while (serializedEdgesEnumerator.MoveNext())
            {
                var serializedEdge = (SerializedProperty)serializedEdgesEnumerator.Current;
                if (serializedEdge.FindPropertyRelative("outputNodeGuid").stringValue == outputNode.Guid &&
                    serializedEdge.FindPropertyRelative("inputNodeGuid").stringValue == inputNode.Guid &&
                    serializedEdge.FindPropertyRelative("outputPortName").stringValue == edge.output.portName &&
                    serializedEdge.FindPropertyRelative("inputPortName").stringValue == edge.input.portName)
                {
                    if (!serializedEdge.DeleteCommand())
                    {
                        throw new InvalidOperationException("Failed to delete edge from model.");
                    }
                    Debug.Log($"Deleted edge from the output port {edge.output.name} of node {outputNode.Guid} to the input port {edge.input.name} of node {inputNode.Guid}.");
                    return;
                }
            }
            throw new InvalidOperationException("Edge not found in model.");
        }
    }
}
