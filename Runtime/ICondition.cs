using System;

namespace Saye.Contracts
{
    /// <summary>
    /// A condition that can be asserted, bound and unbound.
    /// </summary>
    public interface ICondition : IReadOnlyCondition
    {
        /// <summary>
        /// Asserts a change in satisfaction, raising the appropriate event.
        /// </summary>
        void Assert();

        /// <summary>
        /// Asserts a change in satisfaction, raising the appropriate event (compatible as an event handler).
        /// </summary>
        void Assert(object sender, EventArgs e);

        /// <summary>
        /// Binds the condition to automatic change assertion.
        /// </summary>
        void Bind();

        /// <summary>
        /// Unbinds the condition from automatic change assertion.
        /// </summary>
        void Unbind();
    }

    /// <summary>
    /// A condition that can be satisfied or dissatisfied.
    /// </summary>
    public interface IReadOnlyCondition
    {
        /// <summary>
        /// Whether the condition is satisfied.
        /// </summary>
        bool Satisfied { get; }

        /// <summary>
        /// Raised when the condition is satisfied.
        /// </summary>
        event EventHandler<ConditionStatusEventArgs> OnSatisfied;

        /// <summary>
        /// Raised when the condition is dissatisfied.
        /// </summary>
        event EventHandler<ConditionStatusEventArgs> OnDissatisfied;
    }

    public class ConditionStatusEventArgs : EventArgs
    {
        public bool Satisfied { get; }

        public ConditionStatusEventArgs(bool satisfied)
        {
            Satisfied = satisfied;
        }
    }
}