using FunctionZero.TreeListItemsSourceZero;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace FunctionZero.TreeListItemsSourceZeroTests
{
    public static class Arrange
    {
        public static string VerifyRelationship(TestNode parent, TestNode child)
        {
            string reason = string.Empty;

            if (child.Parent != parent)
            {
                reason += "Child not pointing to parent";
            }
            int count = CountChildReferences(parent, child);
            if (count == 0)
            {
                reason += "Child not contained by parent";
            }
            if (count > 1)
            {
                reason += $"Child contained by parent {count} times";
            }

            return reason;
        }

        public static int GetNodeCount(TestNode root)
        {
            int nodeCount = 0;
            foreach (TestNode item in TestNode.AllChildren(root))
                nodeCount++;
            return nodeCount;
        }

        public static int CountChildReferences(TestNode parent, TestNode child)
        {
            int count = 0;
            foreach (var item in parent.Children)
            {
                if (item == child)
                    count++;
            }
            return count;
        }

        private static int _createdIndex;
        public static void BuildTestTree(TestNode root, int depth, int childNodeCount)
        {
            _createdIndex = 0;
            _BuildTestTree(root, depth, childNodeCount);
        }
        private static void _BuildTestTree(TestNode root, int depth, int childNodeCount)
        {
            for (int c = 0; c < childNodeCount; c++)
            {
                TestNode newNode = new TestNode($"Index:{_createdIndex++}");
                root.Children.Add(newNode);
                if (depth > 0)
                    _BuildTestTree(newNode, depth - 1, childNodeCount);
            }
        }

        public static void TestTreeIntegrity(TestNode root)
        {
            foreach (TestNode parent in TestNode.AllChildren(root))
            {
                foreach (var child in parent.Children)
                {
                    Debug.WriteLine($"Testing:{parent.Name} against {child.Name}");
                    string result = VerifyRelationship(parent, child);
                    Assert.AreEqual(string.Empty, result, $"Item relationship is not correct for Parent:{parent}, Child:{child}, reason:{result}");
                }
            }
        }

        public static void ExpandTreeGridNodes(TreeNodeContainer<TestNode> rootTreeNode)
        {
            //rootTreeNode.IsVisible = true;
            rootTreeNode.IsExpanded = true;
            foreach (var treeNode in rootTreeNode.Children)
                ExpandTreeGridNodes(treeNode);
        }
    }
}
