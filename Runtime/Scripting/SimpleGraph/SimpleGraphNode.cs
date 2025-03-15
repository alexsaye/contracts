using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace SimpleGraph
{
    public abstract class SimpleGraphNode : Node
    {
        public string Guid { get; private set; }

        public SerializedProperty SerializedNodeModel { get; private set; }

        public SimpleGraphNode()
        {
        }

        public void LoadModel(SerializedProperty serializedNodeModel)
        {
            SerializedNodeModel = serializedNodeModel;
            Guid = serializedNodeModel.FindPropertyRelative("guid").stringValue;
            SetPosition(serializedNodeModel.FindPropertyRelative("position").rectValue);
            RenderModel();
            RefreshPorts();
            RefreshExpandedState();
        }

        protected virtual void RenderModel()
        {
        }

        // TODO: I don't like this. Need to review how models are created within the graph behaviour.
        public virtual object GetDefaultValue()
        {
            return null;
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

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class NodeContextAttribute : Attribute
    {
        public Type GraphType { get; }

        public NodeContextAttribute(Type graphType)
        {
            GraphType = graphType;
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
