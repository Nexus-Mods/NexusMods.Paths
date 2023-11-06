# IHaveChildrenWithKey<TKey, TSelf>

!!! info "`IHaveChildrenWithKey<TKey, TSelf>` is an interface for Nodes which have children accessed by key."

The interface enforces a structure where every implementor must contain a `Dictionary` holding its children, identified by keys.

## Properties

- `Children`: A `Dictionary` containing all the children of this node, where the key is of type `TKey` and the value is a `ChildrenWithKeyBox<TKey, TSelf>`.

## Type Parameters

- `TKey`: The type of the key used in the dictionary. Must be a non-nullable type.
- `TSelf`: The type of the children stored within the dictionary. Must be a struct implementing `IHaveChildrenWithKey<TKey, TSelf>`.

## ChildrenWithKeyBox<TKey, TSelf>

!!! tip "`ChildrenWithKeyBox<TKey, TSelf>` is a helper class that boxes a constrained generic structure type."

This boxing allows for the `TSelf` value to be used in places where a reference type is required.

### Fields

- `Item`: Contains the item deriving from `IHaveChildrenWithKey<TKey, TSelf>`.

### Implicit Operators

Allows for the implicit conversion between `TSelf` and `ChildrenWithKeyBox<TKey, TSelf>`.

## IHaveChildrenWithKeyExtensions

!!! summary "The `IHaveChildrenWithKeyExtensions` class provides extension methods for structures implementing the `IHaveChildrenWithKey<TKey, TSelf>` interface."

### Methods

#### EnumerateChildren

Enumerates all child nodes of the current node in a depth-first manner.

```csharp
public static IEnumerable<KeyValuePair<TKey, ChildrenWithKeyBox<TKey, TSelf>>> EnumerateChildren<TSelf, TKey>(this TSelf item)
```

#### CountChildren

Counts the number of direct child nodes of the current node.

```csharp
public static int CountChildren<TSelf, TKey>(this TSelf item)
```

### Usage

!!! note "These methods extend functionality for types implementing `IHaveChildrenWithKey<TKey, TSelf>`, providing ways to traverse and interact with the tree-like structures."

```csharp
// Assuming `someNode` is an instance of a structure implementing `IHaveChildrenWithKey<TKey, TSelf>`
// To enumerate children:
foreach (var child in someNode.EnumerateChildren())
{
    // Process child
}

// To count children:
int count = someNode.CountChildren();
```

## Benchmark

```
| Method                     | Mean     | Error    | StdDev   | Code Size |
|--------------------------- |---------:|---------:|---------:|----------:|
| CountChildren_ViaInterface | 21.53 ms | 0.103 ms | 0.096 ms |     199 B |
| CountChildren_This         | 21.48 ms | 0.080 ms | 0.071 ms |     205 B |
```

Calling `CountChildren` effectively has no impact on performance.
