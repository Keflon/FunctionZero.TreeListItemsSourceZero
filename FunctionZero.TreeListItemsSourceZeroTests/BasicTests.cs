using FunctionZero.TreeListItemsSourceZero;
using FunctionZero.TreeZero.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace FunctionZero.TreeListItemsSourceZeroTests
{
    [TestClass]
    public class BasicTests
    {


        [TestMethod]
        public void TestBasicTreeByChildren()
        {
            var root = new TestNode("Root");

            var manager = new TreeItemsSourceManager<TestNode>(true, root, (node)=>true, (node) => (node).Children);

            var child0 = new TestNode("0");
            var child1 = new TestNode("1");
            var child2 = new TestNode("2");

            root.Children.Add(child0);
            child0.Children.Add(child1);
            child1.Children.Add(child2);

            Arrange.VerifyRelationship(root, child0);
            Arrange.VerifyRelationship(child0, child1);
            Arrange.VerifyRelationship(child1, child2);

            Arrange.ExpandTreeGridNodes(manager);

            //child1.Parent = child0;

            try
            {
                root.Children.Add(child0);
            }
            catch(TreeZeroException tzex)
            {
                Assert.AreEqual(ExceptionReason.ChildAddedToSameParent, tzex.Reason);
            }
            catch
            {
                Assert.Fail("Expected ExceptionReason.ChildAddedToSameParent");
            }
        }

        [TestMethod]
        public void TestBasicTreeByParent()
        {
            var root = new TestNode("Root");

            var child0 = new TestNode("0");
            var child1 = new TestNode("1");
            var child2 = new TestNode("2");


            child0.Parent = root;
            child1.Parent = root;
            child2.Parent = root;
            Arrange.VerifyRelationship(root, child0);
            Arrange.VerifyRelationship(root, child1);
            Arrange.VerifyRelationship(root, child2);

            Assert.AreEqual(Arrange.GetNodeCount(root), 4);

            TestNode s = root;

            Debug.WriteLine(s);
        }

        [TestMethod]
        public void TestNestChild()
        {
            var root = new TestNode("Root");

            var child0 = new TestNode("0");
            var child1 = new TestNode("1");
            var child2 = new TestNode("2");

            child0.Parent = root;
            child1.Parent = root;
            child2.Parent = child1;

            Arrange.VerifyRelationship(root, child0);
            Arrange.VerifyRelationship(root, child1);
            Arrange.VerifyRelationship(child1, child2);

            Assert.AreEqual(Arrange.GetNodeCount(root), 4);
        }

        [TestMethod]
        public void TestReparentChild()
        {
            var root = new TestNode("Root");

            var child0 = new TestNode("0");
            var child1 = new TestNode("1");
            var child2 = new TestNode("2");

            child0.Parent = root;
            child1.Parent = root;
            child2.Parent = child1;

            child0.Parent = child2;

            Arrange.VerifyRelationship(child2, child0);
            Arrange.VerifyRelationship(root, child1);
            Arrange.VerifyRelationship(child1, child2);

            Assert.AreEqual(Arrange.GetNodeCount(root), 4);
        }


        /// ///////////////////////////////////////////////////////////////


        [TestMethod]
        public void TestRemoveChild()
        {
            var root = new TestNode("Root");

            var child0 = new TestNode("0");
            var child1 = new TestNode("1");
            var child2 = new TestNode("2");

            child0.Parent = root;
            child1.Parent = root;
            child2.Parent = root;

            Arrange.VerifyRelationship(root, child0);

            Arrange.VerifyRelationship(root, child1);
            root.Children.Remove(child1);
            Assert.AreEqual(0, Arrange.CountChildReferences(root, child1));
            Assert.AreEqual(null, child1.Parent);

            Arrange.VerifyRelationship(root, child2);

            Assert.AreEqual(Arrange.GetNodeCount(root), 3);
        }
    }
}
