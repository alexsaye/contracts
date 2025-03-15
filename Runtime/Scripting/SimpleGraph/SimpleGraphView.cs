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
        /// <summary>
        /// A cache of all nodes that can be created through the contextual menu for each graph type.
        /// </summary>
        private static Dictionary<Type, Dictionary<string, Type>> allMenuNodes = CacheAllMenuNodes();
        private static Dictionary<Type, Dictionary<string, Type>> CacheAllMenuNodes()
        {
            // Get all the node types.
            var nodeTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(SimpleGraphNode)));

            // Build the menu nodes from all nodes that have a menu attribute.
            var allMenuNodes = new Dictionary<Type, Dictionary<string, Type>>();
            foreach (var nodeType in nodeTypes)
            {
                var menuAttribute = nodeType.GetCustomAttribute<NodeMenuAttribute>();
                if (menuAttribute == null)
                {
                    continue;
                }

                // Populate the menu nodes for each graph type referenced by the node's context attributes.
                var contextAttributes = nodeType.GetCustomAttributes<NodeContextAttribute>();
                foreach (var contextAttribute in contextAttributes)
                {
                    if (!allMenuNodes.TryGetValue(contextAttribute.GraphType, out var graphMenuNodes))
                    {
                        graphMenuNodes = new Dictionary<string, Type>();
                        allMenuNodes[contextAttribute.GraphType] = graphMenuNodes;
                    }
                    graphMenuNodes[menuAttribute.MenuName] = nodeType;
                }
            }

            return allMenuNodes;
        }

        private Dictionary<string, Type> availableMenuNodes;
        private SerializedObject serializedGraph;
        private SerializedProperty serializedNodes;
        private SerializedProperty serializedEdges;

        public SimpleGraphView(SerializedObject serializedGraph)
        {
            this.serializedGraph = serializedGraph;

            // Contextualise the available menu nodes based on the type of the graph.
            var graphType = serializedGraph.targetObject.GetType();
            allMenuNodes.TryGetValue(graphType, out availableMenuNodes);
            if (availableMenuNodes == null)
            {
                throw new ArgumentException($"No node contexts found for graph type {graphType}");
            }

            // Pull the model from the serialized graph.
            var serializedModel = serializedGraph.FindProperty("model");

            serializedNodes = serializedModel.FindPropertyRelative("nodes");
            var nodesEnumerator = serializedNodes.GetEnumerator();
            while (nodesEnumerator.MoveNext())
            {
                AddNode((SerializedProperty)nodesEnumerator.Current);
            }

            serializedEdges = serializedModel.FindPropertyRelative("edges");
            var edgesEnumerator = serializedEdges.GetEnumerator();
            while (edgesEnumerator.MoveNext())
            {
                AddEdge((SerializedProperty)edgesEnumerator.Current);
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

            serializedGraph.ApplyModifiedProperties();
            return change;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var mousePosition = new Rect(contentViewContainer.WorldToLocal(evt.mousePosition), Vector2.zero);
            foreach (var (menuName, nodeType) in availableMenuNodes)
            {
                evt.menu.AppendAction(menuName, (action) =>
                {
                    // Create a node of the chosen type at the mouse position.
                    var node = CreateNode(nodeType);

                    // Save the node to the model at the mouse position.
                    var serializedNodeModel = AddNodeToModel(node, mousePosition);
                    serializedGraph.ApplyModifiedProperties();

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
            var type = Type.GetType(serializedEdgeModel.FindPropertyRelative("type").stringValue);
            var node = CreateNode(type);
            node.LoadModel(serializedEdgeModel);
            return node;
        }


        private SerializedProperty AddNodeToModel(SimpleGraphNode node, Rect position)
        {
            var nodeModel = new SimpleGraphNodeModel(node.GetType(), position, node.GetDefaultValue());

            var index = serializedNodes.arraySize;
            serializedNodes.InsertArrayElementAtIndex(index);
            
            var serializedNodeModel = serializedNodes.GetArrayElementAtIndex(index);
            serializedNodeModel.FindPropertyRelative("guid").stringValue = nodeModel.Guid;
            serializedNodeModel.FindPropertyRelative("position").rectValue = nodeModel.Position;
            serializedNodeModel.FindPropertyRelative("type").stringValue = nodeModel.Type;
            serializedNodeModel.FindPropertyRelative("value").managedReferenceValue = nodeModel.Value;

            Debug.Log($"Added node {nodeModel.Guid} of type {nodeModel.Type} at position {nodeModel.Position} with value {nodeModel.Value}.");
            return serializedNodeModel;
        }

        private void DeleteNodeFromModel(SimpleGraphNode node)
        {
            var enumerator = serializedNodes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var serializedNode = (SerializedProperty)enumerator.Current;
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
            var enumerator = serializedNodes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var serializedNode = (SerializedProperty)enumerator.Current;
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

            var enumerator = serializedEdges.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var serializedEdge = (SerializedProperty)enumerator.Current;
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
