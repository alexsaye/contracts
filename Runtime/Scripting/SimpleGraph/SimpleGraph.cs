using UnityEngine;

namespace SimpleGraph
{
    public abstract class SimpleGraph : MonoBehaviour
    {
        [SerializeReference]
        protected SimpleGraphModel model;

        /// <summary>
        /// Create a default model for when the behaviour is initialised or reset.
        /// </summary>
        protected abstract SimpleGraphModel CreateDefaultModel();

        protected virtual void OnValidate()
        {
            if (model == null)
            {
                Debug.Log("Validate: Creating default model...");
                model = CreateDefaultModel();
            }
        }

        protected virtual void Reset()
        {
            Debug.Log("Reset: Creating default model...");
            model = CreateDefaultModel();
        }
    }
}