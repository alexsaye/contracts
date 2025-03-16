using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

namespace SimpleGraph
{
    [NodeView(typeof(SimpleGraphNodeModel))]
    public abstract class SimpleGraphNode : Node
    {
        public string Guid { get; private set; }

        public SimpleGraphNode()
        {
        }

        public void LoadModel(SerializedProperty serializedNodeModel)
        {
            Guid = serializedNodeModel.FindPropertyRelative(nameof(SimpleGraphNodeModel.Guid)).stringValue;
            SetPosition(serializedNodeModel.FindPropertyRelative(nameof(SimpleGraphNodeModel.Position)).rectValue);
            
            mainContainer.Unbind();
            RenderModel(serializedNodeModel);
            mainContainer.Bind(serializedNodeModel.serializedObject);

            RefreshPorts();
            RefreshExpandedState();
        }

        protected virtual void RenderModel(SerializedProperty serializedNodeModel)
        {
        }

        public abstract SimpleGraphNodeModel CreateDefaultModel();
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class NodeViewAttribute : Attribute
    {
        public Type ModelType { get; }

        public NodeViewAttribute(Type modelType)
        {
            ModelType = modelType;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class NodeContextAttribute : Attribute
    {
        public Type GraphType;

        public NodeContextAttribute(Type graphType)
        {
            GraphType = graphType;
        }
    }


    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class NodeMenuAttribute : Attribute
    {
        public string MenuName { get; }

        public NodeMenuAttribute(string menuName)
        {
            MenuName = menuName;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class NodeCapabilitiesAttribute : Attribute
    {
        public Capabilities Capabilities { get; }

        public NodeCapabilitiesAttribute(Capabilities capabilities)
        {
            Capabilities = capabilities;
        }
    }
}
