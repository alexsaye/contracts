using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Saye.Contracts.Scripting
{
    /// <summary>
    /// Manages a career in which contracts are issued through the progression of other contracts.
    /// </summary>
    public class CareerManager : MonoBehaviour
    {
        [Tooltip("The initial scripted contracts to issue when this issuer is enabled.")]
        [SerializeField]
        private List<ScriptableContract> initialContracts;

        /// <summary>
        /// The managed career.
        /// </summary>
        private ICareer career;

        /// <summary>
        /// The contracts currently pending progression.
        /// </summary>
        public IEnumerable<IContract> Pending => career.Pending;

        /// <summary>
        /// Raised when a contract is issued.
        /// </summary>
        public UnityEvent<IContract> Issued;

        /// <summary>
        /// Raised when the issuer updates, for updating any conditions which require per-frame assertion.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private UnityEvent Updated;

        private void OnEnable()
        {
            career = new Career();
            career.Issued += ForwardIssued;

            // Build and issue all initial contracts.
            var progressions = initialContracts.Select(scriptedContract => scriptedContract.Build(Updated));
            foreach (var progression in progressions)
            {
                career.Issue(progression);
            }
        }

        private void OnDisable()
        {
            career.Issued -= ForwardIssued;
            career = null;
        }

        private void Update()
        {
            Updated.Invoke();
        }

        private void ForwardIssued(object sender, IssuedEventArgs e)
        {
            Debug.Log($"Issued contract: {e.Contract}");
            Issued.Invoke(e.Contract);
        }
    }
}