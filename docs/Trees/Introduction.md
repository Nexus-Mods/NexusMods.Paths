# Introduction

!!! tip "NexusMods.Paths has an extensible system for creating high performance, efficient tree structures."

## How to Use

!!! info "How to create a tree based on the dynamic tree system."

### Creating Your Type

First, create a type to represent a node in the tree structure.

```csharp
public struct TreeNode { }
```

Then implement one of the following interfaces:

| Interface                   | Description                      |
|-----------------------------|----------------------------------|
| `IHaveBoxedChildren`        | Stores `Children` as array.      |
| `IHaveBoxedChildrenWithKey` | Stores `Children` as dictionary. |

Example:

```csharp
public struct TreeNode : IHaveBoxedChildrenWithKey<RelativePath, TreeNode>
{
    public Dictionary<RelativePath, ChildrenWithKeyBox<RelativePath, TreeNode>> Children { get; }
}
```

This provides you access to methods which are implemented via interface `IHaveBoxedChildrenWithKey<TPath, TSelf>`.

```csharp
// _item is TreeNode
public int CountChildren() => _item.CountChildren<TestTree, RelativePath>();

public void EnumerateChildren()
{
    foreach (var child in _item.EnumerateChildren<TestTree, RelativePath>())
    {
        // Process child
    }
}
```

!!! note "These methods have 0 overhead compared to a manual human implementation."

### Using in Practice

When using the type, you may choose to either box the root, or not box it.

=== "Box the Root"

    ```csharp
    // Box the Root (Code is cleaner if you put the box type on the left!!)
    ChildBox<TestTree> root = new TestTree(new ChildBox<TestTree>[] { child1, child2 });
    root.CountChildren();

    // Note: Refer to interface, e.g. IHaveBoxedChildren to determine box type.
    ```

=== "Don't box the Root"

    ```csharp
    // Don't box the Root.
    var root = new TestTree(new ChildBox<TestTree>[] { child1, child2 });
    root.CountChildren<TestTree, RelativePath>();
    ```

*If multiple generic types are involved* and you are not boxing the root, you might need to specify the generic types manually on method calls.  

In those situations you may wish to re-export them in the base type for usage convenience.  

```csharp
public struct TreeNode : IHaveBoxedChildrenWithKey<RelativePath, TreeNode>
{
    public Dictionary<RelativePath, ChildrenWithKeyBox<RelativePath, TreeNode>> Children { get; }

    /// <inheritdoc cref="IHaveBoxedChildrenWithKeyExtensions.CountChildren{TSelf,TKey}"/>
    public int CountChildren() => this.CountChildren<TreeNode, RelativePath>();

    /// <inheritdoc cref="IHaveBoxedChildrenWithKeyExtensions.EnumerateChildren{TSelf,TKey}"/>
    public int EnumerateChildren() => this.CountChildren<TreeNode, RelativePath>();
}
```

!!! tip "Not Boxing the Root is generally better for Performance"

### Adding Functionality 

!!! info "In order to add functionality to your tree, simply implement a combination of the interfaces below."

| Interface                   | Description                                                                          |
|-----------------------------|--------------------------------------------------------------------------------------|
| `IHaveParent`               | Provides access to the parent of the current node.                                   |
| `IHaveAFileOrDirectory`     | Provides access to file specific info for a given files.                             |
| `IHaveDepthInformation`     | Provides access to depth of a given node, with depth 0 representing root.            |
| `IHavePathSegment`          | Contains a string which represents an individual segment (e.g. directory) of a path. |

Available Methods:

| Method                 | Description                                             | Required Traits         |
|------------------------|---------------------------------------------------------|-------------------------|
| `IsLeaf`               | Returns true if the node has no children.               |                         |
| `EnumerateChildren`    | Enumerates (`IEnumerator`) over children of this node.  |                         |
| `CountChildren`        | Counts the total number of child nodes under this node. |                         |
| `CountFiles`           | Counts files under this node (directory).               | `IHaveAFileOrDirectory` |
| `CountDirectories`     | Counts directories under this node (directory).         | `IHaveAFileOrDirectory` | 
| `EnumerateSiblings`[1] | Enumerates (`IEnumerator`) over siblings of this node.  | `IHaveParent`           |
| `GetSiblingCount`      | Returns the number of siblings this node has.           | `IHaveParent`           |
| `GetSiblings`[1]       | Returns all siblings of this node.                      | `IHaveParent`           |

[1] Siblings are determined on Value equality *when called from internal boxed struct*. This means, when called from struct, if all fields are the same on two nodes, they may be (incorrectly) assumed as same node.

!!! note "All methods require the `IHaveBoxedChildren` or `IHaveBoxedChildrenWithKey` interface to be implemented, thus they are omitted from the table."

# Benchmarks

!!! note "The interface implemented methods have no overhead compared to manually implemented methods."

| Method                          |     Mean |    Error |   StdDev | Code Size |
|---------------------------------|---------:|---------:|---------:|----------:|
| CountChildren_ViaInterface      | 20.90 ms | 0.398 ms | 0.372 ms |     215 B |
| CountChildren_Manual            | 21.25 ms | 0.076 ms | 0.064 ms |     229 B |
| CountChildrenDepth_ViaInterface | 20.91 ms | 0.090 ms | 0.084 ms |     212 B |
| CountChildrenDepth_Manual       | 20.90 ms | 0.075 ms | 0.066 ms |     206 B |
