using NUnit.Framework;
using System;

namespace Saye.Contracts.Tests
{
    public class ContractTests
    {
        [Test]
        public void ContractPending()
        {
            var neverendingContract = new Contract(Condition.Never, Condition.Never);

            var invoked = false;
            void handleState(object sender, ObservableStateEventArgs<ContractState> e) => invoked = true;

            neverendingContract.State += handleState;
            Assert.IsFalse(invoked, "Contract should not have emitted a resolved or rejected state.");
        }

        [Test]
        public void ContractResolvedOnObserve()
        {
            var resolvingContract = new Contract(Condition.Always, Condition.Never);

            var state = ContractState.Pending;
            var invocations = 0;
            void handleState(object sender, ObservableStateEventArgs<ContractState> e)
            {
                state = e.State;
                ++invocations;
            }

            resolvingContract.State += handleState;
            Assert.AreEqual(ContractState.Fulfilled, state, "Contract should have emitted a resolved state.");
            Assert.AreEqual(1, invocations, "Contract should have only emitted once.");
        }

        [Test]
        public void ContractRejectedOnObserve()
        {
            var rejectingContract = new Contract(Condition.Never, Condition.Always);

            var state = ContractState.Pending;
            var invocations = 0;
            void handleState(object sender, ObservableStateEventArgs<ContractState> e)
            {
                state = e.State;
                ++invocations;
            }

            rejectingContract.State += handleState;
            Assert.AreEqual(ContractState.Rejected, state, "Contract should have emitted a rejected state.");
            Assert.AreEqual(1, invocations, "Contract should have only emitted once.");
        }

        [Test]
        public void ContractResolvedOnEvent()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();
            var contract = new Contract(condition, Condition.Never);

            var state = ContractState.Pending;
            var invocations = 0;
            void handleState(object sender, ObservableStateEventArgs<ContractState> e)
            {
                state = e.State;
                ++invocations;
            }

            contract.State += handleState;
            Assert.AreEqual(ContractState.Pending, state, "Contract should be pending on subscribe.");

            subject.Satisfy();
            Assert.AreEqual(ContractState.Fulfilled, state, "Contract should have emitted a resolved state.");

            subject.Dissatisfy();
            Assert.AreEqual(ContractState.Fulfilled, state, "Contract should remain resolved when condition is dissatisfied.");

            Assert.AreEqual(1, invocations, "Contract should have only emitted once.");
        }

        [Test]
        public void ContractRejectedOnEvent()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();
            var contract = new Contract(Condition.Never, condition);

            var state = ContractState.Pending;
            var invocations = 0;
            void handleState(object sender, ObservableStateEventArgs<ContractState> e)
            {
                state = e.State;
                ++invocations;
            }

            contract.State += handleState;
            Assert.AreEqual(ContractState.Pending, state, "Contract should be pending on subscribe.");

            subject.Satisfy();
            Assert.AreEqual(ContractState.Rejected, state, "Contract should have emitted a rejected state.");

            subject.Dissatisfy();
            Assert.AreEqual(ContractState.Rejected, state, "Contract should remain rejected when condition is satisfied.");

            Assert.AreEqual(1, invocations, "Contract should have only emitted once.");
        }

        [Test]
        public void ContractRejectedWhenAlsoResolvedOnObserve()
        {
            var schrodingersContract = new Contract(Condition.Always, Condition.Always);

            var state = ContractState.Pending;
            var invocations = 0;
            void handleState(object sender, ObservableStateEventArgs<ContractState> e)
            {
                state = e.State;
                ++invocations;
            }

            schrodingersContract.State += handleState;
            Assert.AreEqual(ContractState.Rejected, state, "Contract should have emitted a rejected state.");
            Assert.AreEqual(1, invocations, "Contract should have only emitted once.");
        }

        [Test]
        public void ContractRejectedWhenAlsoResolvedOnEvent()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();
            var schrodingersContract = new Contract(condition, condition);

            var state = ContractState.Pending;
            var invocations = 0;
            void handleState(object sender, ObservableStateEventArgs<ContractState> e)
            {
                state = e.State;
                ++invocations;
            }

            schrodingersContract.State += handleState;
            Assert.AreEqual(ContractState.Pending, state, "Contract should be pending on subscribe.");

            subject.Satisfy();
            Assert.AreEqual(ContractState.Rejected, state, "Contract should have emitted a rejected state.");

            subject.Dissatisfy();
            Assert.AreEqual(ContractState.Rejected, state, "Contract should remain rejected when condition is satisfied.");

            Assert.AreEqual(1, invocations, "Contract should have only emitted once.");
        }
    }
}
