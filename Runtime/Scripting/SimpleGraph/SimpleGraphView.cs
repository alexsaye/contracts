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
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(SimpleGraphViewNode)));

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

        public SimpleGraphView()
        {
            this.StretchToParentSize();

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

            //styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>($"Packages/com.contracts/Runtime/Scripting/Graph/USS/ScriptableGraphView.uss"));

            var grid = new GridBackground();
            grid.name = "Grid";
            Add(grid);
            grid.SendToBack();

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent geometryChangedEvent)
        {
            contentViewContainer.transform.position = new Vector3(layout.width / 2, layout.height / 2, 0);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            Vector2 localMousePosition = contentViewContainer.WorldToLocal(evt.mousePosition);
            foreach (var (menuName, nodeType) in availableMenuNodes)
            {
                evt.menu.AppendAction(menuName, (action) =>
                {
                    var node = CreateNode(nodeType, new Rect(localMousePosition, Vector2.zero));
                    node.Load();
                    node.RefreshPorts();
                    node.RefreshExpandedState();
                    AddElement(node);
                });
            }
            evt.menu.AppendSeparator();

            base.BuildContextualMenu(evt);
        }

        public void Contextualise(Type type)
        {
            allMenuNodes.TryGetValue(type, out var menuNodes);
            if (menuNodes == null)
            {
                throw new ArgumentException($"No node contexts found for type {type}");
            }
            availableMenuNodes = menuNodes;
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

        private SimpleGraphViewNode CreateNode(Type type, Rect position)
        {
            var node = (SimpleGraphViewNode)Activator.CreateInstance(type);
            node.SetPosition(position);
            var capabilitiesAttribute = type.GetCustomAttribute<NodeCapabilitiesAttribute>();
            if (capabilitiesAttribute != null)
            {
                node.capabilities = capabilitiesAttribute.Capabilities;
            }
            return node;
        }

        private SimpleGraphViewNode LoadNode(ScriptableGraphNodeModel model)
        {
            var nodeType = Type.GetType(model.Type);
            var node = CreateNode(nodeType, model.Position);
            node.Load(model);
            node.RefreshPorts();
            node.RefreshExpandedState();
            return node;
        }

        private Edge LoadEdge(ScriptableGraphEdgeModel model)
        {
            var outputNode = graphElements
                .Where((element) => element is SimpleGraphViewNode)
                .Select((element) => element as SimpleGraphViewNode)
                .First((node) => node.Guid == model.OutputNodeGuid);
            var outputPort = outputNode.outputContainer.Q<Port>(model.OutputPortName);

            var inputNode = graphElements
                .Where((element) => element is SimpleGraphViewNode)
                .Select((element) => element as SimpleGraphViewNode)
                .First((node) => node.Guid == model.InputNodeGuid);
            var inputPort = inputNode.inputContainer.Q<Port>(model.InputPortName);

            var edge = new Edge
            {
                input = inputPort,
                output = outputPort
            };

            inputPort.Connect(edge);
            outputPort.Connect(edge);

            return edge;
        }

        public void Load(SimpleGraphModel model)
        {
            var elementsToRemove = graphElements.ToArray();
            foreach (var element in elementsToRemove)
            {
                RemoveElement(element);
            }

            foreach (var nodeModel in model.Nodes)
            {
                var node = LoadNode(nodeModel);
                AddElement(node);
            }

            foreach (var edgeModel in model.Edges)
            {
                var edge = LoadEdge(edgeModel);
                AddElement(edge);
            }
        }

        public SimpleGraphModel Save()
        {
            var nodes = new List<ScriptableGraphNodeModel>();
            var edges = new List<ScriptableGraphEdgeModel>();
            foreach (var element in graphElements)
            {
                if (element is SimpleGraphViewNode node)
                {
                    var nodeModel = node.Save();
                    nodes.Add(nodeModel);
                    Debug.Log($"Saved node: {nodeModel}");
                }
                else if (element is Edge edge)
                {
                    var outputNode = edge.output.node as SimpleGraphViewNode;
                    var inputNode = edge.input.node as SimpleGraphViewNode;
                    var edgeModel = new ScriptableGraphEdgeModel(
                        outputNode.Guid,
                        inputNode.Guid,
                        edge.output.portName,
                        edge.input.portName);
                    edges.Add(edgeModel);
                    Debug.Log($"Saved edge: {edgeModel}");
                }
            }
            return new SimpleGraphModel(nodes, edges);
        }
    }
}
