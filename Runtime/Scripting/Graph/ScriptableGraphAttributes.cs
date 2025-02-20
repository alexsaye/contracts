using System;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System.Collections;

namespace Contracts.Scripting.Graph
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class NodeMenuAttribute : Attribute
    {
        public string MenuName { get; }

        public NodeMenuAttribute(string menuName)
        {
            MenuName = menuName;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class NodePresentOnCreationAttribute : Attribute
    {
        public Vector2 Position { get; }

        public NodePresentOnCreationAttribute()
            : this(0, 0) { }

        public NodePresentOnCreationAttribute(float x, float y)
        {
            Position = new Vector2(x, y);
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class NodeContextAttribute : Attribute
    {
        public Type[] Contexts { get; }

        public NodeContextAttribute(params Type[] contexts)
        {
            Contexts = contexts;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class NodeInputAttribute : Attribute
    {
        public Port.Capacity Capacity { get; }

        public NodeInputAttribute(Port.Capacity capacity = Port.Capacity.Single)
        {
            Capacity = capacity;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class NodeOutputAttribute : Attribute
    {
        public Port.Capacity Capacity { get; }

        public NodeOutputAttribute(Port.Capacity capacity = Port.Capacity.Multi)
        {
            Capacity = capacity;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class NodeSlotAttribute : Attribute
    {
        public NodeSlotAttribute()
        {
        }
    }
}
