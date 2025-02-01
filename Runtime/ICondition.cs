using System;

namespace Saye.Contracts
{
    /// <summary>
    /// A condition that is satisfied or dissatisfied when asserted.
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
        event EventHandler OnSatisfied;

        /// <summary>
        /// Raised when the condition is dissatisfied.
        /// </summary>
        event EventHandler OnDissatisfied;

        /// <summary>
        /// Asserts satisfaction, raising the appropriate event.
        /// </summary>
        void Assert();

        /// <summary>
        /// Asserts a change in satisfaction, raising the appropriate event.
        /// </summary>
        void AssertChanged();

        /// <summary>
        /// Binds the condition to automatic change assertion.
        /// </summary>
        void Bind();

        /// <summary>
        /// Unbinds the condition from automatic change assertion.
        /// </summary>
        void Unbind();
    }
}
