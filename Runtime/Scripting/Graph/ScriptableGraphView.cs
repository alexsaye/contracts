using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Contracts.Scripting.Graph
{
    public class ScriptableGraphView : GraphView
    {
        public ScriptableGraph Graph { get; private set; }
        private static Dictionary<Type, List<NodeAttributeComposition>> cachedNodeTypes = new();

        public ScriptableGraphView(ScriptableGraph graph)
        {
            this.StretchToParentSize();

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>($"Packages/com.saye.contracts/Runtime/Scripting/Graph/USS/ScriptableGraphView.uss"));

            var grid = new GridBackground();
            grid.name = "Grid";
            Add(grid);
            grid.SendToBack();

            Graph = graph;

            foreach (var nodeSave in Graph.Nodes)
            {
                var node = LoadNode(nodeSave);
                AddElement(node);
            }

            foreach (var edgeSave in Graph.Edges)
            {
                var edge = LoadEdge(edgeSave);
                AddElement(edge);
            }

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent geometryChangedEvent)
        {
            contentViewContainer.transform.position = new Vector3(layout.width / 2, layout.height / 2, 0);
        }

        private void EnsureCachedNodeTypes()
        {
            var graphType = Graph.GetType();
            if (cachedNodeTypes.ContainsKey(graphType))
                return;

            var nodeTypes = new List<NodeAttributeComposition>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    var menuAttribute = type.GetCustomAttribute<NodeMenuAttribute>();
                    if (menuAttribute == null)
                    {
                        continue;
                    }

                    var contextAttribute = type.GetCustomAttribute<NodeContextAttribute>();
                    if (contextAttribute != null && !contextAttribute.Contexts.Contains(Graph.GetType()))
                    {
                        continue;
                    }

                    nodeTypes.Add(new NodeAttributeComposition()
                    {
                        Type = type,
                        Menu = menuAttribute,
                        Context = contextAttribute,
                    });
                }
            }
            cachedNodeTypes.Add(graphType, nodeTypes);
        }

        private new void BuildContextualMenu(ContextualMenuPopulateEvent populateEvent)
        {
            EnsureCachedNodeTypes();

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(populateEvent.mousePosition);

            foreach (var composition in cachedNodeTypes[Graph.GetType()])
            {
                populateEvent.menu.AppendAction(composition.Menu.MenuName, (action) => {
                    var node = CreateNode(composition.Type, new Rect(localMousePosition, Vector2.zero));
                    AddElement(node);
                });
            }
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

        private ScriptableGraphNode CreateNode(Type type, Rect position)
        {
            var node = (ScriptableGraphNode)Activator.CreateInstance(type);
            node.SetPosition(position);

            var capabilitiesAttribute = type.GetCustomAttribute<NodeCapabilitiesAttribute>();
            if (capabilitiesAttribute != null)
            {
                node.capabilities = capabilitiesAttribute.Capabilities;
            }

            node.RefreshPorts();
            node.RefreshExpandedState();
            return node;
        }

        private ScriptableGraphNode LoadNode(NodeSaveData nodeSave)
        {
            var nodeType = Type.GetType(nodeSave.Type);
            var node = CreateNode(nodeType, nodeSave.Position);
            node.Load(nodeSave);
            node.RefreshPorts();
            node.RefreshExpandedState();
            return node;
        }

        private Edge LoadEdge(EdgeSaveData edgeSave)
        {
            var outputNode = graphElements.Where((element) => element is ScriptableGraphNode).Select((element) => element as ScriptableGraphNode).First((node) => node.Guid == edgeSave.OutputNodeGuid);
            var outputPort = outputNode.outputContainer.Q<Port>(edgeSave.OutputPortName);

            var inputNode = graphElements.Where((element) => element is ScriptableGraphNode).Select((element) => element as ScriptableGraphNode).First((node) => node.Guid == edgeSave.InputNodeGuid);
            var inputPort = inputNode.inputContainer.Q<Port>(edgeSave.InputPortName);

            var edge = new Edge
            {
                input = inputPort,
                output = outputPort
            };

            inputPort.Connect(edge);
            outputPort.Connect(edge);

            return edge;
        }

        public void SaveGraph()
        {
            Graph.Nodes.Clear();
            Graph.Edges.Clear();

            foreach (var element in graphElements)
            {
                if (element is ScriptableGraphNode node)
                {
                    Graph.Nodes.Add(node.Save());
                }
                else if (element is Edge edge)
                {
                    var outputNode = edge.output.node as ScriptableGraphNode;
                    var inputNode = edge.input.node as ScriptableGraphNode;
                    var edgeSave = new EdgeSaveData(
                        outputNode.Guid,
                        edge.output.portName,
                        inputNode.Guid,
                        edge.input.portName);
                    Graph.Edges.Add(edgeSave);
                }
            }
        }
    }

    class NodeAttributeComposition
    {
        public Type Type;
        public NodeMenuAttribute Menu;
        public NodeContextAttribute Context;
    }
}
