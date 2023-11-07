# Introduction

!!! tip "NexusMods.Paths has an extensible system for creating high performance, efficient tree structures."

## How to Use

First, create a type to represent a node in the tree structure.

```csharp
public struct TreeNode { }
```

Then implement the following interfaces (as needed):

| Interface                   | Description                                                               |
|-----------------------------|---------------------------------------------------------------------------|
| `IHaveBoxedChildren`        | Provides access to `Children` array.                                      |
| `IHaveBoxedChildrenWithKey` | Provides access to `Children` dictionary.                                 |
| `IHaveParent`               | Provides access to the parent of the current node.                        |
| `IHaveAFileOrDirectory`     | Provides access to file specific info for a given files.                  |
| `IHaveDepthInformation`     | Provides access to depth of a given node, with depth 0 representing root. |

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

Invoking these methods may be inconvenient when there are multiple generic types involved, because you have to manually specify the generic types (unfortunately).

For convenience, you may wish to re-export them in the base type for convenience.

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

# Benchmarks

!!! note "The interface implemented methods have no overhead compared to manually implemented methods."

| Method                          |     Mean |    Error |   StdDev | Code Size |
|---------------------------------|---------:|---------:|---------:|----------:|
| CountChildren_ViaInterface      | 20.90 ms | 0.398 ms | 0.372 ms |     215 B |
| CountChildren_Manual            | 21.25 ms | 0.076 ms | 0.064 ms |     229 B |
| CountChildrenDepth_ViaInterface | 20.91 ms | 0.090 ms | 0.084 ms |     212 B |
| CountChildrenDepth_Manual       | 20.90 ms | 0.075 ms | 0.066 ms |     206 B |
