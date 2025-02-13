using NUnit.Framework;

namespace Saye.Contracts.Tests
{
    public class ContractTests
    {
        [Test]
        public void ContractPendingOnConstruction()
        {
            var resolvingContract = new Contract(Condition.Always, Condition.Never);
            Assert.AreEqual(ContractStatus.Pending, resolvingContract.Status);

            var rejectingContract = new Contract(Condition.Never, Condition.Always);
            Assert.AreEqual(ContractStatus.Pending, rejectingContract.Status);
        }

        [Test]
        public void ContractPendingOnBind()
        {
            var neverendingContract = new Contract(Condition.Never, Condition.Never);
            neverendingContract.Bind();
            Assert.AreEqual(ContractStatus.Pending, neverendingContract.Status, "Neverending contract should remain pending when bound.");
            neverendingContract.Unbind();
            Assert.AreEqual(ContractStatus.Pending, neverendingContract.Status, "Neverending contract should remain pending when unbound.");
            neverendingContract.Bind();
            Assert.AreEqual(ContractStatus.Pending, neverendingContract.Status, "Neverending contract should remain pending when rebound.");
        }

        [Test]
        public void ContractResolvedOnBind()
        {
            var resolvingContract = new Contract(Condition.Always, Condition.Never);
            resolvingContract.Bind();
            Assert.AreEqual(ContractStatus.Resolved, resolvingContract.Status, "Resolving contract should be resolved when bound.");
            resolvingContract.Unbind();
            Assert.AreEqual(ContractStatus.Resolved, resolvingContract.Status, "Resolving contract should remain resolved when unbound.");
            resolvingContract.Bind();
            Assert.AreEqual(ContractStatus.Resolved, resolvingContract.Status, "Resolving contract should remain resolved when rebound.");
        }

        [Test]
        public void ContractRejectedOnBind() {
            var rejectingContract = new Contract(Condition.Never, Condition.Always);
            rejectingContract.Bind();
            Assert.AreEqual(ContractStatus.Rejected, rejectingContract.Status, "Rejecting contract should be rejected when bound.");
            rejectingContract.Unbind();
            Assert.AreEqual(ContractStatus.Rejected, rejectingContract.Status, "Rejecting contract should remain rejected when unbound.");
            rejectingContract.Bind();
            Assert.AreEqual(ContractStatus.Rejected, rejectingContract.Status, "Rejecting contract should remain rejected when rebound.");
        }

        [Test]
        public void ContractResolvedOrRejectedOnBoundCondition()
        {
            var subject = new MockSubject(false);
            var condition = subject.AsCondition();

            var unboundResolvingContract = new Contract(condition, Condition.Never);
            unboundResolvingContract.Bind();
            unboundResolvingContract.Unbind();
            var unboundRejectingContract = new Contract(Condition.Never, condition);
            unboundRejectingContract.Bind();
            unboundRejectingContract.Unbind();

            var boundResolvingContract = new Contract(condition, Condition.Never);
            boundResolvingContract.Bind();

            var boundRejectingContract = new Contract(Condition.Never, condition);
            boundRejectingContract.Bind();

            Assert.AreEqual(ContractStatus.Pending, unboundResolvingContract.Status, "Unbound resolving contract should be pending when condition is unsatisfied.");
            Assert.AreEqual(ContractStatus.Pending, unboundRejectingContract.Status, "Unbound rejecting contract should be pending when condition is unsatisfied.");
            Assert.AreEqual(ContractStatus.Pending, boundResolvingContract.Status, "Bound resolving contract should remain pending when condition is unsatisfied.");
            Assert.AreEqual(ContractStatus.Pending, boundRejectingContract.Status, "Bound rejecting contract should remain pending when condition is unsatisfied.");

            subject.Satisfy();
            Assert.AreEqual(ContractStatus.Pending, unboundResolvingContract.Status, "Unbound resolving contract should remain pending when condition is satisfied.");
            Assert.AreEqual(ContractStatus.Pending, unboundRejectingContract.Status, "Unbound rejecting contract should remain pending when condition is satisfied.");
            Assert.AreEqual(ContractStatus.Resolved, boundResolvingContract.Status, "Bound resolving contract should be resolved when condition is satisfied.");
            Assert.AreEqual(ContractStatus.Rejected, boundRejectingContract.Status, "Bound rejecting contract should be rejected when condition is satisfied.");

            subject.Dissatisfy();
            Assert.AreEqual(ContractStatus.Pending, unboundResolvingContract.Status, "Unbound resolving contract should remain pending when condition is dissatisfied.");
            Assert.AreEqual(ContractStatus.Pending, unboundRejectingContract.Status, "Unbound rejecting contract should remain pending when condition is dissatisfied.");
            Assert.AreEqual(ContractStatus.Resolved, boundResolvingContract.Status, "Bound resolving contract should remain resolved when condition is dissatisfied.");
            Assert.AreEqual(ContractStatus.Rejected, boundRejectingContract.Status, "Bound rejecting contract should remain rejected when condition is dissatisfied.");
        }
    }
}
