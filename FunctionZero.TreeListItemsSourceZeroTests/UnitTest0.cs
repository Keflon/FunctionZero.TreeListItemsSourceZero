using FunctionZero.TreeZero.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FunctionZero.TreeListItemsSourceZeroTests
{
    [TestClass]
    public class UnitTest0
    {
        [TestMethod]
        public void TestParentToSelf()
        {
            var root = new TestNode("Root");

            var child0 = new TestNode("0");
            var child1 = new TestNode("1");
            var child2 = new TestNode("2");

            root.Children.Add(child0);
            root.Children.Add(child1);
            root.Children.Add(child2);

            Arrange.VerifyRelationship(root, child0);
            Arrange.VerifyRelationship(root, child1);
            Arrange.VerifyRelationship(root, child2);

            try
            {
                child0.Parent = child0;
                Assert.Fail("Node cannot be its own parent.");
            }
            catch (TreeZeroException ex)
            {
                Assert.AreEqual(ExceptionReason.ParentToSelf, ex.Reason);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Wrong exception:{ex}");
            }
        }
        [TestMethod]
        public void TestChildrenAddSelf()
        {
            var root = new TestNode("Root");

            var child0 = new TestNode("0");
            var child1 = new TestNode("1");
            var child2 = new TestNode("2");

            root.Children.Add(child0);
            root.Children.Add(child1);
            root.Children.Add(child2);

            Arrange.VerifyRelationship(root, child0);
            Arrange.VerifyRelationship(root, child1);
            Arrange.VerifyRelationship(root, child2);

            try
            {
                child0.Children.Add(child0);
                Assert.Fail("Node cannot be its own parent.");
            }
            catch (TreeZeroException ex)
            {
                Assert.AreEqual(ExceptionReason.ParentToSelf, ex.Reason);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Wrong exception:{ex}");
            }
        }

        [TestMethod]
        public void TestParentToDescendant()
        {
            var root = new TestNode("Root");

            var child0 = new TestNode("0");
            var child1 = new TestNode("1");
            var child2 = new TestNode("2");

            root.Children.Add(child0);
            child0.Children.Add(child1);
            child1.Children.Add(child2);

            Arrange.VerifyRelationship(root, child0);
            Arrange.VerifyRelationship(child0, child1);
            Arrange.VerifyRelationship(child1, child2);

            try
            {
                child1.Parent = child2;
                Assert.Fail("Cyclic dependency.");
            }
            catch (TreeZeroException ex)
            {
                Assert.AreEqual(ExceptionReason.ParentToDescendant, ex.Reason);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Wrong exception:{ex}");
            }
        }
        [TestMethod]
        public void TestChildrenAddDescendant()
        {
            var root = new TestNode("Root");

            var child0 = new TestNode("0");
            var child1 = new TestNode("1");
            var child2 = new TestNode("2");

            root.Children.Add(child0);
            child0.Children.Add(child1);
            child1.Children.Add(child2);

            Arrange.VerifyRelationship(root, child0);
            Arrange.VerifyRelationship(child0, child1);
            Arrange.VerifyRelationship(child1, child2);

            try
            {
                child2.Children.Add(child1);
                Assert.Fail("Node cannot be its own parent.");
            }
            catch (TreeZeroException ex)
            {
                Assert.AreEqual(ExceptionReason.ParentToDescendant, ex.Reason);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Wrong exception:{ex}");
            }
        }
    }
}
