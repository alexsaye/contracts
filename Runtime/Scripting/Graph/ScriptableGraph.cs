using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

namespace Contracts.Scripting.Graph
{
    public abstract class ScriptableGraph : ScriptableObject
    {
        [HideInInspector]
        public List<NodeSaveData> Nodes = new();

        [HideInInspector]
        public List<EdgeSaveData> Edges = new();

        private void Awake()
        {
            EnsureNodesPresentOnCreation();
        }

        private void EnsureNodesPresentOnCreation()
        {
            var nodeTypesPresentOnCreation = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.GetCustomAttribute<NodeContextAttribute>()?.Contexts.Contains(GetType()) ?? true)
                .Select(type => (type, type.GetCustomAttribute<NodePresentOnCreationAttribute>()))
                .Where(tuple => tuple.Item2 != null)
                .ToList();

            foreach (var (nodeType, attribute) in nodeTypesPresentOnCreation)
            {
                var nodePresent = Nodes.Any(node => node.Type == nodeType.AssemblyQualifiedName);
                if (nodePresent)
                    continue;

                Nodes.Add(new NodeSaveData(nodeType.AssemblyQualifiedName, attribute.Position));
            }
        }
    }
}
