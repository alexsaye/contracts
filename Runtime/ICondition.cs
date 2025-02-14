using System;

namespace Saye.Contracts
{
    /// <summary>
    /// A condition that can be satisfied or dissatisfied.
    /// </summary>
    public interface ICondition
    {
        /// <summary>
        /// Whether the condition is satisfied.
        /// </summary>
        bool Satisfied { get; }

        /// <summary>
        /// Raised when the condition is satisfied.
        /// </summary>
        event EventHandler<ConditionEventArgs> OnSatisfied;

        /// <summary>
        /// Raised when the condition is dissatisfied.
        /// </summary>
        event EventHandler<ConditionEventArgs> OnDissatisfied;

        /// <summary>
        /// Asserts a change in satisfaction, raising the appropriate event.
        /// </summary>
        void Assert();

        /// <summary>
        /// Asserts a change in satisfaction, raising the appropriate event (compatible as an event handler).
        /// </summary>
        void Assert(object sender, EventArgs e);
    }

    public class ConditionEventArgs : EventArgs
    {
        public bool Satisfied { get; }

        public ConditionEventArgs(bool satisfied)
        {
            Satisfied = satisfied;
        }
    }
}