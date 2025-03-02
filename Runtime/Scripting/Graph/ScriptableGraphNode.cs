using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Search;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Contracts.Scripting.Graph
{
    public abstract class ScriptableGraphNode : Node
    {
        public string Guid { get; private set; }

        public ScriptableGraphNode()
        {
        }

        public virtual NodeSaveData Save()
        {
            if (Guid == null)
            {
                Guid = System.Guid.NewGuid().ToString();
            }
            return new NodeSaveData(GetType(), GetPosition(), Guid);
        }

        public virtual void Load(NodeSaveData saveData)
        {
            Guid = saveData.Guid;
        }
    }
}
