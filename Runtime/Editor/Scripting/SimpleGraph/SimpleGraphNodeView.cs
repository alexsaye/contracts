using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;

namespace SimpleGraph.Editor
{
    public abstract class SimpleGraphNodeView : Node
    {
        public string Guid { get; private set; }

        public SimpleGraphNodeView()
        {
        }

        public void LoadModel(SerializedProperty serializedNodeModel)
        {
            Guid = serializedNodeModel.FindPropertyRelative(nameof(SimpleGraphNodeModel.Guid)).stringValue;
            SetPosition(serializedNodeModel.FindPropertyRelative(nameof(SimpleGraphNodeModel.Position)).rectValue);
            RenderModel(serializedNodeModel);
        }

        protected virtual void RenderModel(SerializedProperty serializedNodeModel)
        {
        }

        public static Port CreatePort<T>(string name, Orientation orientation, Direction direction, Port.Capacity capacity)
        {
            var port = Port.Create<Edge>(orientation, direction, capacity, typeof(T));
            port.name = name;
            port.portName = name;
            return port;
        }
    }

    // TODO: can we do this through a generic parameter in SimpleGraphNodeView instead?
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class SimpleGraphNodeModelAttribute : Attribute
    {
        public Type ModelType { get; }

        public SimpleGraphNodeModelAttribute(Type modelType)
        {
            ModelType = modelType;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class SimpleGraphNodeContextAttribute : Attribute
    {
        public Type GraphType;

        public SimpleGraphNodeContextAttribute(Type graphType)
        {
            GraphType = graphType;
        }
    }


    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class SimpleGraphNodeMenuAttribute : Attribute
    {
        public string MenuName { get; }

        public SimpleGraphNodeMenuAttribute(string menuName)
        {
            MenuName = menuName;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class SimpleGraphNodeCapabilitiesAttribute : Attribute
    {
        public Capabilities Capabilities { get; }

        public SimpleGraphNodeCapabilitiesAttribute(Capabilities capabilities)
        {
            Capabilities = capabilities;
        }
    }
}
