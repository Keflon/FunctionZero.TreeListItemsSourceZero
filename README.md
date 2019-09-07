# FunctionZero.TreeItemsSourceZero

The purpose of this package is to allow a ListView-type-control to behave like a TreeView and display hierarchical data.
This is particularly useful if you want to adapt a GridView to behave like a TreeView, like this:
  
![alt text](https://raw.githubusercontent.com/Keflon/FunctionZero.TreeGridViewZero/master/SampleTreeGrid.png "TreeGridView")


## Basic Usage

Your nodes must expose some way of getting their children, though the mechanism for that is entirely up to you. 
For example, given a tree of the following nodes:
```csharp
public class MyNode
{
    public ObservableCollection<MyNode> Children{ get; }
    // Make it yours ...
}
```

You simply wrap your root node like this:

```csharp
bool isTreeRootShown = true;        // We want the root node to be visible in our tree
var rootNode = GetRootNode();       // Get your tree data from somewhere

var rootContainer = new TreeItemSourceManager<MyNode>(isTreeRootShown, rootNode, (node) => node.Children);
``` 
The lambda function takes an instance of `MyNode` and must return an IEnumerable<MyNode> containing that node's children. 
If the IEnumerable is an `ObservableCollection`, the library will track changes to the underlying data. 

`rootContainer` is now a wrapper around your rootNode and it exposes:
```csharp
public ReadOnlyObservableCollection<TreeNodeContainer<T>> TreeNodeChildren { get; }
```
You can then bind your ListView ItemsSource to this property, write a suitable ItemTemplate, and your ListView is now a TreeView!

At a minimum, your DataTemplate will want to use the following properties on each TreeNodeContainer:
```csharp
// Bind a checkbox to this to expand or collapse a node
bool IsExpanded;

// Represents the nest level of the current TreeNodeContainer
int Indent;

// This is the node that the container wraps, so represent it as you see fit
MyNode Data;
```
Child nodes are not enumerated until their parent container is expanded. `TreeItemSourceManager` provides
a `NodeChanged` event if you want to manage virtualisation or other custom behaviour.
