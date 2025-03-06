using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Contracts.Scripting.Graph
{
    public abstract class ScriptableGraph : ScriptableObject
    {
        private ScriptableGraphModel model;
        public ScriptableGraphModel Model
        {
            get => model;
            internal set
            {
                // Remove all previous model assets from this graph.
                if (model != null)
                {
                    foreach (var nodeModel in model.Nodes)
                    {
                        if (nodeModel.Asset != null)
                        {
                            AssetDatabase.RemoveObjectFromAsset(nodeModel.Asset);
                        }
                    }
                }

                // Cache the new model.
                model = value;

                // Add all model assets to this graph.
                if (model != null)
                {
                    foreach (var nodeModel in model.Nodes)
                    {
                        if (nodeModel.Asset != null)
                        {
                            AssetDatabase.AddObjectToAsset(nodeModel.Asset, this);
                        }
                    }
                }
            }
        }
    }
}
