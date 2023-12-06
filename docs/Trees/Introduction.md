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

| Interface                   | Description                                  |
|-----------------------------|----------------------------------------------|
| `IHaveBoxedChildren`        | Stores `Children` as array.                  |
| `IHaveObservableChildren`   | Stores `Children` as `ObservableCollection`. |
| `IHaveBoxedChildrenWithKey` | Stores `Children` as dictionary.             |

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

Then consider adding a constructor. Use a 'Create' method to do so, and return a boxed item.

```csharp
public struct TreeNode : IHaveBoxedChildrenWithKey<RelativePath, TreeNode>
{
    public Dictionary<RelativePath, ChildrenWithKeyBox<RelativePath, TreeNode>> Children { get; }

    // Create a Constructor.
    public static KeyedBox<int, TestTree> Create(Dictionary<int, KeyedBox<int, TestTree>>? children = null)
        => (KeyedBox<int, TestTree>) new TestTree()
        {
            Children = children ?? new Dictionary<int, KeyedBox<int, TestTree>>()
        };
}
```

#### Micro Optimization

!!! tip "Not Boxing the Root"

    Depending on your use case, you can choose to not box the root, and store unboxed `TestTree` directly in a class.
    This saves a pointer dereference.

!!! warning

    Some methods (they are deliberately marked 'Obsolete') may also cause a box/heap allocation if called from unboxed item.

If you don't box the root, you might sometimes need to specify the generic types manually on method calls (no auto inference).

You can work around this by exporting helper methods for convenience (if desired).

```csharp
public struct TreeNode : IHaveBoxedChildrenWithKey<RelativePath, TreeNode>
{
    // .. other code

    // [Optional] Re-export methods for convenience if you are working with non-boxed root.

    /// <inheritdoc cref="IHaveBoxedChildrenWithKeyExtensions.CountChildren{TSelf,TKey}"/>
    public int CountChildren() => this.CountChildren<TreeNode, RelativePath>();

    /// <inheritdoc cref="IHaveBoxedChildrenWithKeyExtensions.EnumerateChildren{TSelf,TKey}"/>
    public int EnumerateChildren() => this.CountChildren<TreeNode, RelativePath>();
}
```

### Adding Functionality

!!! info "In order to add functionality to your tree, simply implement a combination of the interfaces below."

| Interface               | Description                                                                          |
|-------------------------|--------------------------------------------------------------------------------------|
| `IHaveParent`           | Provides access to the parent of the current node.                                   |
| `IHaveAFileOrDirectory` | Provides access to file specific info for a given files.                             |
| `IHaveDepthInformation` | Provides access to depth of a given node, with depth 0 representing root.            |
| `IHavePathSegment`      | Contains a string which represents an individual segment (e.g. directory) of a path. |
| `IHaveKey`              | Contains a getter for a 'key'. Can be used for building dictionaries.                |
| `IHaveValue`            | Contains an internal 'value'. Useful for dictionaries & further extensions.          |

Available Methods:

| Method                       | Description                                                                   | Required Traits                   |
|------------------------------|-------------------------------------------------------------------------------|-----------------------------------|
| `CountChildren`              | Counts the total number of child nodes under this node.                       |                                   |
| `CountDirectories`           | Counts directories under this node (directory).                               | `IHaveAFileOrDirectory`           |
| `CountFiles`                 | Counts files under this node (directory).                                     | `IHaveAFileOrDirectory`           |
| `CountLeaves`                | Returns number of leaf nodes in this tree.                                    |                                   |
| `EnumerateChildrenBfs`       | Enumerates children of this node Breadth First.                               |                                   |
| `EnumerateChildrenDfs`       | Enumerates children of this node using Depth First.                           |                                   |
| `EnumerateKeysBfs`           | Enumerates child keys of this node Breadth First.                             | `IHaveKey`                        |
| `EnumerateKeysDfs`           | Enumerates child keys of this node using Depth First.                         | `IHaveKey`                        |
| `EnumerateSiblings`[1]       | Enumerates (`IEnumerator`) over siblings of this node.                        | `IHaveParent`                     |
| `EnumerateValuesBfs`         | Enumerates child values of this node Breadth First.                           | `IHaveValue`                      |
| `EnumerateValuesDfs`         | Enumerates child values of this node using Depth First.                       | `IHaveValue`                      |
| `FindSubPathsByKeyUpward`    | Finds all nodes whose sub-path matches Span of keys (optimized version).      | `IHaveKey`, `IHaveParent`         |
| `FindSubPathsByKey`[2]       | Finds all nodes whose sub-path matches Span of keys.                          | `IHaveKey`                        |
| `FindByKey`[2]               | Finds a given node in a tree using a Span of keys.                            | `IHaveKey`                        |
| `FindByKeyUpward`            | Verifies the path to the node against a Span of keys (inverse FindByKey).     | `IHaveKey`, `IHaveParent`         |
| `FindRootByKeyUpward`        | Verifies the path to the node against a Span of keys (optimized FindByKey).   | `IHaveKey`, `IHaveParent`         |
| `FindByPath`[2]              | Finds a given node in a tree using a relative path.                           | `IHavePathSegment`                |
| `GetChildrenRecursive`       | Retrieves all children of this node (flattened).                              |                                   |
| `GetChildrenRecursiveUnsafe` | Retrieves all children of this node (no bound checks).                        |                                   |
| `GetKeys`                    | Retrieves all keys of the children of this node.                              | `IHaveKey`                        |
| `GetKeysUnsafe`              | Retrieves all keys of the children of this node (no bound checks).            | `IHaveKey`                        |
| `GetKeyValues`               | Retrieves all key-value pairs of the children of this node.                   | `IHaveKey`, `IHaveValue`          |
| `GetKeyValuesUnsafe`         | Retrieves all key-value pairs of the children of this node (no bound checks). | `IHaveKey`, `IHaveValue`          |
| `GetLeaves`                  | Retrieves all leaves of this tree.                                            |                                   |
| `GetLeavesUnsafe`            | Retrieves all leaves of this tree (no bound checks).                          |                                   |
| `GetSiblingCount`            | Returns the number of siblings this node has.                                 | `IHaveParent`                     |
| `GetSiblings`[1]             | Returns all siblings of this node.                                            | `IHaveParent`                     |
| `GetSiblingsUnsafe`[1]       | Returns all siblings of this node (no bound checks).                          | `IHaveParent`                     |
| `GetValues`                  | Retrieves all values of the children of this node.                            | `IHaveValue`                      |
| `GetValuesUnsafe`            | Retrieves all values of the children of this node (no bound checks).          | `IHaveValue`                      |
| `IsLeaf`                     | Returns true if the node has no children.                                     |                                   |
| `ReconstructPath`            | Reconstructs full path by walking to tree root.                               | `IHaveParent`, `IHavePathSegment` |
| `ToDictionary`               | Populates a dictionary from the children of the tree node.                    | `IHaveKey`, `IHaveValue`          |

[1] Siblings are determined on Value equality *when called from internal boxed struct*. This means, when called from struct, if all fields are the same on two nodes, they may be (incorrectly) assumed as same node.

[2] Method has variants for including and excluding itself (the root).

!!! note "All methods require one of the container interfaces such as `IHaveBoxedChildren` to be implemented, thus they are omitted from the table."

!!! note "Unless specified, the functions ignore the current node. e.g. `GetValues`, `CountLeaves` etc. will return only children."

# Benchmarks

!!! note "The interface implemented methods have no overhead compared to manually implemented methods."

| Method                          |     Mean |    Error |   StdDev | Code Size |
|---------------------------------|---------:|---------:|---------:|----------:|
| CountChildren_ViaInterface      | 20.90 ms | 0.398 ms | 0.372 ms |     215 B |
| CountChildren_Manual            | 21.25 ms | 0.076 ms | 0.064 ms |     229 B |
| CountChildrenDepth_ViaInterface | 20.91 ms | 0.090 ms | 0.084 ms |     212 B |
| CountChildrenDepth_Manual       | 20.90 ms | 0.075 ms | 0.066 ms |     206 B |
