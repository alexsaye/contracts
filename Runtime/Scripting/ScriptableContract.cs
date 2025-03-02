using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    [CreateAssetMenu(fileName = "New Contract", menuName = "Contracts/Asset/Contract")]
    public class ScriptableContract : ScriptableObject
    {
        [SerializeField]
        private ScriptableCondition fulfilling;

        [SerializeField]
        private ScriptableCondition rejecting;

        public virtual IContract Build(UnityEvent updated)
        {
            Debug.Log($"Building contract {name}");

            // Build the conditions so they can start evaluating their assertations.
            var fulfilling = this.fulfilling != null ? this.fulfilling.Build(updated) : Condition.Never;
            var rejecting = this.rejecting != null ? this.rejecting.Build(updated) : Condition.Never;

            return new Contract(fulfilling, rejecting);
        }
    }
}
