using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace SimpleGraph
{
    public abstract class SimpleGraphViewNode : Node
    {
        public string Guid { get; private set; }

        public SimpleGraphViewNode()
        {
        }

        public virtual ScriptableGraphNodeModel Save()
        {
            return new ScriptableGraphNodeModel(Guid, GetType(), GetPosition());
        }

        public virtual void Load(ScriptableGraphNodeModel model = null)
        {
            if (model != null)
            {
                Debug.Log($"Loading from model: {model}");
                Guid = model.Guid;
            }
            else
            {
                Debug.Log("Loading default model.");
                Guid = System.Guid.NewGuid().ToString(); // TODO: need to consolidate where guids and default assets are created - I don't think it should be here
            }
        }
    }

    public abstract class SimpleGraphViewNode<T> : SimpleGraphViewNode where T : UnityEngine.Object
    {
        private T objectReference;
        public T ObjectReference
        {
            get => objectReference;
            protected set
            {
                objectReference = value;
                if (value != null)
                {
                    SerializedObject = new SerializedObject(ObjectReference);
                    RenderObjectReference();
                }
            }
        }

        protected SerializedObject SerializedObject { get; private set; }

        public override ScriptableGraphNodeModel Save()
        {
            var save = base.Save();
            save.ObjectReference = ObjectReference;
            return save;
        }

        public override void Load(ScriptableGraphNodeModel model = null)
        {
            base.Load(model);
            if (model != null && model.ObjectReference != null)
            {
                ObjectReference = (T)model.ObjectReference;
            }
            else
            {
                ObjectReference = CreateObjectReference();
            }
        }

        protected virtual T CreateObjectReference()
        {
            return null;
        }

        /// <summary>
        /// Render display related to the object reference.
        /// </summary>
        protected abstract void RenderObjectReference();
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
