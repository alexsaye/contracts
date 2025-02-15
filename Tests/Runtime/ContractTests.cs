using NUnit.Framework;

namespace Saye.Contracts.Tests
{
    public class ContractTests
    {
        [Test]
        public void ContractFulfilledOnConstruction()
        {
            var fulfilledContract = new Contract(Condition.Always, Condition.Never);
            Assert.AreEqual(ContractState.Fulfilled, fulfilledContract.CurrentState, "Is fulfilled on construction.");

            var state = ContractState.Pending;
            void handleState(object sender, StateEventArgs<ContractState> e) => state = e.CurrentState;

            fulfilledContract.State += handleState;
            Assert.AreEqual(ContractState.Fulfilled, state, "Is fulfilled on subscribe.");
        }

        [Test]
        public void ContractRejectedOnConstruction()
        {
            var rejectedContract = new Contract(Condition.Never, Condition.Always);
            Assert.AreEqual(ContractState.Rejected, rejectedContract.CurrentState, "Is rejected on construction.");

            var state = ContractState.Pending;
            void handleState(object sender, StateEventArgs<ContractState> e) => state = e.CurrentState;

            rejectedContract.State += handleState;
            Assert.AreEqual(ContractState.Rejected, state, "Is rejected on subscribe.");
        }

        [Test]
        public void ContractRejectedWhenAlsoFulfilledOnConstruction()
        {
            var schrodingersContract = new Contract(Condition.Always, Condition.Always);

            var state = ContractState.Pending;
            void handleState(object sender, StateEventArgs<ContractState> e) => state = e.CurrentState;
            Assert.AreEqual(ContractState.Rejected, schrodingersContract.CurrentState, "Is rejected on construction.");

            schrodingersContract.State += handleState;
            Assert.AreEqual(ContractState.Rejected, state, "Is rejected on subscribe.");
        }

        [Test]
        public void ContractFulfilledOnEvent()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();
            var contract = new Contract(condition, Condition.Never);
            Assert.AreEqual(ContractState.Pending, contract.CurrentState, "Is pending on construction.");

            var state = ContractState.Fulfilled;
            void handleState(object sender, StateEventArgs<ContractState> e) => state = e.CurrentState;

            contract.State += handleState;
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
            Assert.AreEqual(ContractState.Pending, contract.CurrentState, "Is pending on construction.");

            var state = ContractState.Pending;
            void handleState(object sender, StateEventArgs<ContractState> e) => state = e.CurrentState;

            contract.State += handleState;
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
            Assert.AreEqual(ContractState.Pending, schrodingersContract.CurrentState, "Is pending on construction.");

            var state = ContractState.Pending;
            void handleState(object sender, StateEventArgs<ContractState> e) => state = e.CurrentState;

            schrodingersContract.State += handleState;
            Assert.AreEqual(ContractState.Pending, state, "Is pending on subscribe.");

            subject.Satisfy();
            Assert.AreEqual(ContractState.Rejected, state, "Is rejected when condition is satisfied.");

            subject.Dissatisfy();
            Assert.AreEqual(ContractState.Rejected, state, "Is still rejected when condition is dissatisfied.");
        }
    }
}
