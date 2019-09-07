using FunctionZero.TreeListItemsSourceZero;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FunctionZero.TreeListItemsSourceZeroTests
{
    [TestClass]
    public class ComprehensiveTests
    {
        Random _rand;

        [TestMethod]
        public void TestBuildTree()
        {
            var root = new TestNode("Root");
            Arrange.BuildTestTree(root, 2, 2);
            Arrange.TestTreeIntegrity(root);
        }

        /// <summary>
        /// Build a test tree
        /// Jumble it up by changing the Parent property of random nodes
        /// Reverse the jumbling by modifying the Children of those nodes
        /// Verify tree integrity along the way
        /// Verify the tree is the same after reversing the jumbling.
        /// </summary>
        [TestMethod]
        public void TestByParentThenChild()
        {
            _rand = new Random(42);
            TerribleUnitTest((from, to) => from.Parent = to, (from, to) => to.Children.Add(from));
        }

        /// <summary>
        /// Build a test tree
        /// Jumble it up by adding random nodes to the Children of other random nodes
        /// Reverse the jumbling by modifying the Parent of those nodes
        /// Verify tree integrity along the way
        /// Verify the tree is the same after reversing the jumbling.
        [TestMethod]
        public void TestByChildrenThenParent()
        {
            _rand = new Random(42);
            TerribleUnitTest((from, to) => to.Children.Add(from), (from, to) => from.Parent = to);
        }

        public void TerribleUnitTest(Action<TestNode, TestNode> method0, Action<TestNode, TestNode> method1)
        {
            var root = new TestNode("Root");
            Arrange.BuildTestTree(root, 4, 4);

            var manager = new TreeItemsSourceManager<TestNode>(true, root, (node) => true, (node) => ((TestNode)node).Children);

            Arrange.ExpandTreeGridNodes(manager);

            // Used to test the tree is correct after we undo every move.
            foreach (TestNode item in TestNode.AllChildren(root))
                item.OriginalParent = item.Parent;

            // Ensure the tree parent/child relationships are correct.
            Arrange.TestTreeIntegrity(root);
            Debug.WriteLine(root.PrintTree());

            // Used to store moves, so we can undo them later.
            Stack<Tuple<TestNode, TestNode>> moves = new Stack<Tuple<TestNode, TestNode>>();

            int nodeCount = Arrange.GetNodeCount(root);
            for (int c = 0; c < nodeCount; c++)
            {
                TestNode child, newParent;
                GetTwoSuitableNodes(root, nodeCount, out child, out newParent);
                Debug.WriteLine($"Moving {child.Name} from {child.Parent.Name} to {newParent.Name}");
                // Store the move, so we can undo it later.
                moves.Push(new Tuple<TestNode, TestNode>(child, child.Parent));
                // Move the child to the new parent.
                method0(child, newParent);

                TestTreeGridAgainstData(manager, root);

                Assert.AreEqual(newParent, child.Parent, "Wrong parent");
                Assert.AreEqual(nodeCount, Arrange.GetNodeCount(root), "Node has gone missing");
            }
            Arrange.TestTreeIntegrity(root);
            //Debug.WriteLine(root.ToString());
            Assert.AreEqual(nodeCount, Arrange.GetNodeCount(root));

            // Undo every move.
            // NOTE: The tree will be back to its starting shape, but the order of node Children may be different.
            while (moves.TryPop(out var move))
            {
                method1(move.Item1, move.Item2);
                TestTreeGridAgainstData(manager, root);
            }

            // Ensure the tree has the same shape we started with, allowing for node Children to be in a different order.
            foreach (TestNode item in TestNode.AllChildren(root))
                Assert.AreEqual(item.OriginalParent, item.Parent);

            //Debug.WriteLine(root.PrintTree());
        }


        int _monsterCount;
        private void TestTreeGridAgainstData(TreeItemsSourceManager<TestNode> manager, TestNode root)
        {
            Arrange.ExpandTreeGridNodes(manager);

            _monsterCount = 0;
            _testTreeGridAgainstData(manager, root);
            Debug.WriteLine($"MONSTER COUNT {_monsterCount}");
        }
        private void _testTreeGridAgainstData(TreeNodeContainer<TestNode> treeNode, TestNode dataNode)
        {
            Assert.AreEqual(treeNode.Children.Count, dataNode.Children.Count, "Child count mismatch");
            _monsterCount++;
            foreach (var treeChildNode in treeNode.Children)
            {
                int dataIndex = dataNode.Children.IndexOf((TestNode)treeChildNode.Data);
                Assert.AreNotEqual(-1, dataIndex, "Tree not contain child.");
                _testTreeGridAgainstData(treeChildNode, dataNode.Children[dataIndex]);
            }
        }

        /// <summary>
        /// Pick a random node and a suitable node we can move it to.
        /// We cannot move a node to itself or to a descendant of itself.
        /// </summary>
        /// <param name="root">The root of the tree</param>
        /// <param name="nodeCount">Number of nodes in the tree</param>
        /// <param name="child">out the node that can be moved</param>
        /// <param name="newParent">out the Parent the node can be moved to</param>
        private void GetTwoSuitableNodes(TestNode root, int nodeCount, out TestNode child, out TestNode newParent)
        {
            do
            {
                int first = _rand.Next(nodeCount);
                int second = _rand.Next(nodeCount);

                child = GetItemAtIndex(root, first);
                newParent = GetItemAtIndex(root, second);
            }
            while (newParent.IsChildOf(child) || child == newParent || child.Parent == newParent);
        }

        private TestNode GetItemAtIndex(TestNode root, int index)
        {
            int nodeCount = 0;
            foreach (TestNode item in TestNode.AllChildren(root))
            {
                if (nodeCount == index)
                    return item;
                nodeCount++;
            }
            throw new InvalidOperationException();
        }

        /// ///////////////////////////////////////////////////////////////
    }
}
