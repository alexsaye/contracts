using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Contracts.Scripting.Graph
{
    public abstract class ScriptableGraphNode : Node
    {
        public string Guid { get; private set; }
        public ScriptableObject Asset { get; private set; }
        protected SerializedObject SerializedAsset { get; private set; }

        public ScriptableGraphNode()
        {
        }

        public ScriptableGraphNodeModel Save()
        {
            return new ScriptableGraphNodeModel(GetType(), GetPosition(), Guid, Asset);
        }

        public void Load(ScriptableGraphNodeModel model = null)
        {
            if (model != null)
            {
                Debug.Log($"Loading from model: {model}");
                Guid = model.Guid;
                Asset = model.Asset;
            }
            else
            {
                Debug.Log("Loading default model.");
                Guid = System.Guid.NewGuid().ToString(); // TODO: need to consolidate where guids and default assets are created - I don't think it should be here
            }

            if (Asset == null)
            {
                Asset = CreateDefaultAsset();
            }

            if (Asset != null)
            {
                SerializedAsset = new SerializedObject(Asset);
            }

            SetupAssetElements();
        }

        protected virtual ScriptableObject CreateDefaultAsset()
        {
            return null;
        }

        protected virtual void SetupAssetElements()
        {
        }
    }
}
