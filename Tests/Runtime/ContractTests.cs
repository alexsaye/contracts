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
            EventHandler<ContractEventArgs> statusHandler = (object sender, ContractEventArgs e) => invoked = true;

            neverendingContract.OnResolved += statusHandler;
            neverendingContract.OnRejected += statusHandler;
            Assert.IsFalse(invoked, "Contract should not have emitted a resolved or rejected status.");
        }

        [Test]
        public void ContractResolvedOnSubscribe()
        {
            var resolvingContract = new Contract(Condition.Always, Condition.Never);

            var status = ContractStatus.Pending;
            var invocations = 0;
            EventHandler<ContractEventArgs> statusHandler = (object sender, ContractEventArgs e) =>
            {
                status = e.Status;
                ++invocations;
            };

            resolvingContract.OnResolved += statusHandler;
            resolvingContract.OnRejected += statusHandler;
            Assert.AreEqual(ContractStatus.Resolved, status, "Contract should have emitted a resolved status.");
            Assert.AreEqual(1, invocations, "Contract should have only emitted once.");
        }

        [Test]
        public void ContractRejectedOnSubscribe()
        {
            var rejectingContract = new Contract(Condition.Never, Condition.Always);

            var status = ContractStatus.Pending;
            var invocations = 0;
            EventHandler<ContractEventArgs> statusHandler = (object sender, ContractEventArgs e) =>
            {
                status = e.Status;
                ++invocations;
            };

            rejectingContract.OnResolved += statusHandler;
            rejectingContract.OnRejected += statusHandler;
            Assert.AreEqual(ContractStatus.Rejected, status, "Contract should have emitted a rejected status.");
            Assert.AreEqual(1, invocations, "Contract should have only emitted once.");
        }

        [Test]
        public void ContractResolvedOnEvent()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();
            var contract = new Contract(condition, Condition.Never);

            var status = ContractStatus.Pending;
            var invocations = 0;
            EventHandler<ContractEventArgs> statusHandler = (object sender, ContractEventArgs e) =>
            {
                status = e.Status;
                ++invocations;
            };

            contract.OnResolved += statusHandler;
            contract.OnRejected += statusHandler;
            Assert.AreEqual(ContractStatus.Pending, status, "Contract should be pending on subscribe.");

            subject.Satisfy();
            Assert.AreEqual(ContractStatus.Resolved, status, "Contract should have emitted a resolved status.");

            subject.Dissatisfy();
            Assert.AreEqual(ContractStatus.Resolved, status, "Contract should remain resolved when condition is dissatisfied.");

            Assert.AreEqual(1, invocations, "Contract should have only emitted once.");
        }

        [Test]
        public void ContractRejectedOnEvent()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();
            var contract = new Contract(Condition.Never, condition);

            var status = ContractStatus.Pending;
            var invocations = 0;
            EventHandler<ContractEventArgs> statusHandler = (object sender, ContractEventArgs e) =>
            {
                status = e.Status;
                ++invocations;
            };

            contract.OnResolved += statusHandler;
            contract.OnRejected += statusHandler;
            Assert.AreEqual(ContractStatus.Pending, status, "Contract should be pending on subscribe.");

            subject.Satisfy();
            Assert.AreEqual(ContractStatus.Rejected, status, "Contract should have emitted a rejected status.");

            subject.Dissatisfy();
            Assert.AreEqual(ContractStatus.Rejected, status, "Contract should remain rejected when condition is satisfied.");

            Assert.AreEqual(1, invocations, "Contract should have only emitted once.");
        }

        [Test]
        public void ContractRejectedWhenAlsoResolvedOnSubscribe()
        {
            var schrodingersContract = new Contract(Condition.Always, Condition.Always);

            var status = ContractStatus.Pending;
            var invocations = 0;
            EventHandler<ContractEventArgs> statusHandler = (object sender, ContractEventArgs e) =>
            {
                status = e.Status;
                ++invocations;
            };

            schrodingersContract.OnResolved += statusHandler;
            schrodingersContract.OnRejected += statusHandler;
            Assert.AreEqual(ContractStatus.Rejected, status, "Contract should have emitted a rejected status.");
            Assert.AreEqual(1, invocations, "Contract should have only emitted once.");
        }

        [Test]
        public void ContractRejectedWhenAlsoResolvedOnEvent()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();
            var schrodingersContract = new Contract(condition, condition);

            var status = ContractStatus.Pending;
            var invocations = 0;
            EventHandler<ContractEventArgs> statusHandler = (object sender, ContractEventArgs e) =>
            {
                status = e.Status;
                ++invocations;
            };

            schrodingersContract.OnResolved += statusHandler;
            schrodingersContract.OnRejected += statusHandler;
            Assert.AreEqual(ContractStatus.Pending, status, "Contract should be pending on subscribe.");

            subject.Satisfy();
            Assert.AreEqual(ContractStatus.Rejected, status, "Contract should have emitted a rejected status.");

            subject.Dissatisfy();
            Assert.AreEqual(ContractStatus.Rejected, status, "Contract should remain rejected when condition is satisfied.");

            Assert.AreEqual(1, invocations, "Contract should have only emitted once.");
        }
    }
}
