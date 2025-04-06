using NUnit.Framework;

namespace Contracts.Tests
{
    public class ContractTests
    {
        [Test]
        public void ContractFulfilledOnConstruction()
        {
            var fulfilledContract = new Contract(Condition.Always, Condition.Never);
            Assert.AreEqual(ContractState.Fulfilled, fulfilledContract.State, "Is fulfilled on construction.");

            var state = ContractState.Pending;
            void handleState(object sender, StateUpdatedEventArgs<ContractState> e) => state = e.State;

            fulfilledContract.StateUpdated += handleState;
            Assert.AreEqual(ContractState.Fulfilled, state, "Is fulfilled on subscribe.");
        }

        [Test]
        public void ContractRejectedOnConstruction()
        {
            var rejectedContract = new Contract(Condition.Never, Condition.Always);
            Assert.AreEqual(ContractState.Rejected, rejectedContract.State, "Is rejected on construction.");

            var state = ContractState.Pending;
            void handleState(object sender, StateUpdatedEventArgs<ContractState> e) => state = e.State;

            rejectedContract.StateUpdated += handleState;
            Assert.AreEqual(ContractState.Rejected, state, "Is rejected on subscribe.");
        }

        [Test]
        public void ContractRejectedWhenAlsoFulfilledOnConstruction()
        {
            var schrodingersContract = new Contract(Condition.Always, Condition.Always);
            Assert.AreEqual(ContractState.Rejected, schrodingersContract.State, "Is rejected on construction.");

            var state = ContractState.Pending;
            void handleState(object sender, StateUpdatedEventArgs<ContractState> e) => state = e.State;

            schrodingersContract.StateUpdated += handleState;
            Assert.AreEqual(ContractState.Rejected, state, "Is rejected on subscribe.");
        }

        [Test]
        public void ContractFulfilledOnEvent()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();
            var contract = new Contract(condition, Condition.Never);
            Assert.AreEqual(ContractState.Pending, contract.State, "Is pending on construction.");

            var state = ContractState.Fulfilled;
            void handleState(object sender, StateUpdatedEventArgs<ContractState> e) => state = e.State;

            contract.StateUpdated += handleState;
            Assert.AreEqual(ContractState.Pending, state, "Is pending on subscribe.");

            subject.Satisfy();
            Assert.AreEqual(ContractState.Fulfilled, state, "Is fulfilled when condition is satisfied.");

            subject.Dissatisfy();
            Assert.AreEqual(ContractState.Fulfilled, state, "Is still fulfilled when condition is dissatisfied.");
        }

        [Test]
        public void ContractRejectedOnEvent()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();

            var contract = new Contract(Condition.Never, condition);
            Assert.AreEqual(ContractState.Pending, contract.State, "Is pending on construction.");

            var state = ContractState.Pending;
            void handleState(object sender, StateUpdatedEventArgs<ContractState> e) => state = e.State;

            contract.StateUpdated += handleState;
            Assert.AreEqual(ContractState.Pending, state, "Is pending on subscribe.");

            subject.Satisfy();
            Assert.AreEqual(ContractState.Rejected, state, "Is rejected when condition is satisfied.");

            subject.Dissatisfy();
            Assert.AreEqual(ContractState.Rejected, state, "Is still rejected when condition is dissatisfied.");
        }

        [Test]
        public void ContractRejectedWhenAlsoFulfilledOnEvent()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();
            var schrodingersContract = new Contract(condition, condition);
            Assert.AreEqual(ContractState.Pending, schrodingersContract.State, "Is pending on construction.");

            var state = ContractState.Pending;
            void handleState(object sender, StateUpdatedEventArgs<ContractState> e) => state = e.State;

            schrodingersContract.StateUpdated += handleState;
            Assert.AreEqual(ContractState.Pending, state, "Is pending on subscribe.");

            subject.Satisfy();
            Assert.AreEqual(ContractState.Rejected, state, "Is rejected when condition is satisfied.");

            subject.Dissatisfy();
            Assert.AreEqual(ContractState.Rejected, state, "Is still rejected when condition is dissatisfied.");
        }
    }
}
