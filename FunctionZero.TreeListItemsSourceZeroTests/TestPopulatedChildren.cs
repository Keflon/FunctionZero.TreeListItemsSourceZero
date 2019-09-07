using FunctionZero.TreeListItemsSourceZero;
using FunctionZero.TreeZero.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace FunctionZero.TreeListItemsSourceZeroTests
{
    [TestClass]
    public class TestPopulatedChildren
    {
        [TestMethod]
        public void TestNewChildren()
        {
            ObservableCollection<TestNode> childNodes = new ObservableCollection<TestNode>();
            childNodes.Add(new TestNode("0"));
            childNodes.Add(new TestNode("1"));
            childNodes.Add(new TestNode("2"));

            var root = new TestNode(childNodes, "Root");

            Arrange.VerifyRelationship(root, childNodes[0]);
            Arrange.VerifyRelationship(root, childNodes[1]);
            Arrange.VerifyRelationship(root, childNodes[2]);

            var manager = new TreeItemsSourceManager<TestNode>(true, root, (node) => true, (node) => ((TestNode)node).Children);
            //manager.IsVisible = true;
            manager.IsExpanded = true;

            Debug.WriteLine(manager);
        }


        [TestMethod]
        public void TestNewChildrenFail()
        {
            ObservableCollection<TestNode> childNodes = new ObservableCollection<TestNode>();
            childNodes.Add(new TestNode("0"));
            childNodes.Add(new TestNode("1"));
            childNodes.Add(new TestNode("2"));

            var root = new TestNode(childNodes, "Root");

            try
            {
                var root2 = new TestNode(childNodes, "Root");
                Assert.Fail("Added same node to two roots");
            }
            catch (TreeZeroException ex)
            {
                Assert.AreEqual(ExceptionReason.CollectionItemAlreadyParented, ex.Reason);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Wrong exception:{ex}");
            }
        }

        /// ///////////////////////////////////////////////////////////////
        /// 
        [TestMethod]
        public void TestParentToDescendant()
        {
            var root = new TestNode("Root");

            var manager = new TreeItemsSourceManager<TestNode>(true, root, (node) => true, (node) => ((TestNode)node).Children);

            //manager.IsVisible = true;
            manager.IsExpanded = true;
            var child0 = new TestNode("0");
            var child1 = new TestNode("1");
            var child2 = new TestNode("2");

            root.Children.Add(child0);
            child0.Children.Add(child1);
            child1.Children.Add(child2);

            Arrange.VerifyRelationship(root, child0);
            Arrange.VerifyRelationship(child0, child1);
            Arrange.VerifyRelationship(child1, child2);

            //manager.IsVisible = true;
            manager.IsExpanded = true;

            Debug.WriteLine(manager);

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
