using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts.Scripting
{
    /// <summary>
    /// Manages a career in which contracts are issued through the progression of other contracts.
    /// </summary>
    public class CareerManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The progression graph to use for the career. If an external graph is not provided, an internal graph will be created.")]
        private CareerGraph graph;

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
        /// Raised when there are no more contracts pending progression.
        /// </summary>
        public UnityEvent Retired;

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
            career.Retired += ForwardRetired;

            var progressions = graph.Build(Updated);
            foreach (var progression in progressions)
            {
                career.Issue(progression);
            }
        }

        private void OnDisable()
        {
            career.Issued -= ForwardIssued;
            career.Retired -= ForwardRetired;
            career = null;
        }

        private void Update()
        {
            Updated.Invoke();
        }

        private void ForwardIssued(object sender, IssuedEventArgs e)
        {
            Debug.Log($"Issued");
            Issued.Invoke(e.Contract);
        }

        private void ForwardRetired(object sender, System.EventArgs e)
        {
            Debug.Log("Retired");
            Retired.Invoke();
        }
    }
}