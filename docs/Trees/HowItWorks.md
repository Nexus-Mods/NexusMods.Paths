# How it Works


## Devirtualization of Generic Parameters

!!! note "This section is a very simple ELI5 to give only the necessary behind the scenes context."

!!! info "When you pass a generic to a method, the behaviour of the compiled method will differ depending on whether the item is a ValueType (struct/primitive) and a reference type (class)."

!!! tip "[This conference talk](https://youtu.be/4yALYEINbyI), is a nice watch, and covers this."

Suppose we have an interface that adds an item to an existing structure, for example, increments an internal number:
```csharp
static HasNumberS AddToStruct(HasNumberS s, int value) =>  AddNumber(s, value);
static HasNumberC AddToClass(HasNumberC c, int value) => AddNumber(c, value);

static T AddNumber<T>(T item, int value) where T : IAmNumber
{
    item.Add(value);
    return item;
}

public interface IAmNumber
{
    void Add(int other);
}

public struct HasNumberS : IAmNumber
{
    public int _number;
    public void Add(int other) => _number += other;
}

public class HasNumberC : IAmNumber
{
    public int _number;
    public void Add(int other) => _number += other;
}
```

In the above code, we defined `HasNumberC` as class, and `HasNumberS` as struct.

When we call the generic method, you will observe the following code generated (x64, Linux, .NET 7.0):

=== "AddNumber&lt;HasNumberC&gt;"

    ```
    G_M49945_IG01:
        push     rbp
        push     rbx
        push     rax
        lea      rbp, [rsp+10H]
        mov      rbx, rdi
    G_M49945_IG02:
        mov      rdi, rbx
        mov      r11, 0xD1FFAB1E      ; code for IAmNumber:Add
        call     [r11]IAmNumber:Add(int):this
        mov      rax, rbx
    G_M49945_IG03:
        add      rsp, 8
        pop      rbx
        pop      rbp
        ret
    ```

=== "AddNumber&lt;HasNumberS&gt;"

    ```
    lea  eax, [rdi+rsi] ; SystemV x64 ABI: first argument in rdi, second in rsi
    ret
    ```

As you can see, the class does not get devirtualized at all. Instead it does a virtual function table call, as a regular
interface would. The JIT shares the code between reference types, to save on code size, in favour of performance.
Therefore the calls stay virtual.

!!! tip "If you have ever used [Harmony](https://harmony.pardeike.net/) for .NET hooking, this is the exact reason why hooking generics is problematic."

## Devirtualization of Generic Children

!!! note "Unfortunately, due to code sharing described above, classes cannot be efficiently used with generics; due to virtual method calls."

Therefore, an interface cannot be declared as this:

```csharp
/// <summary>
///     An interface used by Tree implementations to indicate that they have a keyed child.
/// </summary>
/// <typeparam name="TKey">The name of the key used in the File Tree.</typeparam>
/// <typeparam name="TSelf">The type of the child stored in this FileTree.</typeparam>
public interface IHaveChildrenWithKey<TKey, TSelf>
    where TSelf : IHaveChildrenWithKey<TKey, TSelf>
    where TKey : notnull
{
    public Dictionary<TKey, TSelf> Children { get; }
}
```

Because if the user passes a `class` as `TSelf`, the code will be full of inefficient virtual method calls.

When the user passes a `struct` as `TSelf`, the item will be passed by value, which is undesirable for if the `Children` are stored in a `Dictionary`.  

For instance:  

- Storing full structs is wasteful on memory due to internal dictionary design.  
- User expects pass-by-reference semantics when dealing with dictionaries.  
    - For example, if they use `TryGetValue`, they expect to be able to mutate the resulting value as if they received a reference.  

!!! note "`CollectionsMarshal.GetValueRefOrNullRef()` can be used to manipulate structs by reference in a Dictionary, but it's still not worthwhile due to memory use."

In order to work around this, we can box the struct, by wrapping it in a class.

```csharp
/// <summary>
///     An interface used by Tree implementations to indicate that they have a keyed child.
/// </summary>
/// <typeparam name="TKey">The name of the key used in the File Tree.</typeparam>
/// <typeparam name="TSelf">The type of the child stored in this FileTree.</typeparam>
public interface IHaveChildrenWithKey<TKey, TSelf>
    where TSelf : struct, IHaveChildrenWithKey<TKey, TSelf>
    where TKey : notnull
{
    /// <summary>
    ///     A Dictionary containing all the children of this node.
    /// </summary>
    /// <remarks>
    ///     This should point to an empty dictionary if there are no items.
    /// </remarks>
    public Dictionary<TKey, ChildWithKeyBox<TKey, TSelf>> Children { get; }
}

/// <summary>
///     A boxed element that implements <see cref="IHaveChildrenWithKey{TKey,TSelf}" />
/// </summary>
public class ChildWithKeyBox<TKey, TSelf>
    where TSelf : struct, IHaveChildrenWithKey<TKey, TSelf>
    where TKey : notnull
{
    /// <summary>
    ///     Contains item deriving from <see cref="IHaveChildrenWithKey{TKey,TSelf}" />
    /// </summary>
    public TSelf Item;

    // Implicit Conversions for convenience
    /// <summary />
    public static implicit operator TSelf(ChildWithKeyBox<TKey, TSelf> box) => box.Item;

    /// <summary />
    public static implicit operator ChildWithKeyBox<TKey, TSelf>(TSelf item) => new() { Item = item };
}
```

The trick here is to wrap the struct around in a `class`, then use a generic constraint to ensure that the `TSelf` struct implements the interface.

This way, when you access the dictionary, you get the boxed struct `ChildWithKeyBox<TKey, TSelf>`. When you use
the internal field `TSelf`, you are now operating on a struct, thus avoiding code sharing, and therefore virtual method calls. 

## Implementing New Functionality

!!! info "In order to implement functionality, you should make use of C# extension methods, and constrain them to the interface."

In other words, in case of the interface above, you should implement an extension method constrained to `where TSelf : struct, IHaveChildrenWithKey<TKey, TSelf>`.

```csharp
/// <summary>
///     Counts the number of direct child nodes of the current node.
/// </summary>
/// <param name="item">The node whose children are to be counted.</param>
/// <typeparam name="TKey">The type of key used to identify children.</typeparam>
/// <typeparam name="TSelf">The type of child node.</typeparam>
/// <returns>The count of direct child nodes.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static int CountChildren<TSelf, TKey>(this ChildWithKeyBox<TKey, TSelf> item)
    where TSelf : struct, IHaveBoxedChildrenWithKey<TKey, TSelf>
    where TKey : notnull
    => item.Item.CountChildren<TSelf, TKey>(); // <= Redirect for boxed elements.

/// <summary>
///     Counts the number of direct child nodes of the current node.
/// </summary>
/// <param name="item">The node whose children are to be counted.</param>
/// <typeparam name="TKey">The type of key used to identify children.</typeparam>
/// <typeparam name="TSelf">The type of child node.</typeparam>
/// <returns>The count of direct child nodes.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static int CountChildren<TSelf, TKey>(this TSelf item)
    where TSelf : struct, IHaveChildrenWithKey<TKey, TSelf>
    where TKey : notnull
{
    var result = 0;
    item.CountChildrenRecursive<TSelf, TKey>(ref result);
    return result;
}

/// <summary>
///     Enumerates all child nodes of this current node.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static void CountChildrenRecursive<TSelf, TKey>(this TSelf item, ref int accumulator)
    where TSelf : struct, IHaveChildrenWithKey<TKey, TSelf> where TKey : notnull
{
    accumulator += item.Children.Count;
    foreach (var child in item.Children)
        child.Value.Item.CountChildrenRecursive<TSelf, TKey>(ref accumulator);
}
```

Once the extension method is made, any struct that implements the interface will have access to the method. This method
will be zero-overhead, i.e. it will be just as fast as if you implemented it manually.

!!! note 

    If the functionality you are implementing requires navigating children (via `IHaveBoxedChildrenWithKey` or 
    `IHaveBoxedChildren`, you will need to implement said functionality it for all 'child' providing interfaces.

    To do this, it's recommended to implement for `IHaveBoxedChildren` first, then just copy the implementation for
    `IHaveObservableChildren` and `IHaveObservableChildrenWithKey`, while changing the types involved. 99% of code 
    should be identical.

### Implementing Functionality with Multiple Interfaces

!!! info "When you want to make more specialized methods, such as e.g. `GetAllPaths` or `SumAllChildren`, you can implement them by adding multiple generic constraints."

!!! note "Below is an arbitrary example."

```csharp
/// <summary>
///     Sums the 'depth' field of all child nodes.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static int SumChildrenDepth<TSelf, TKey>(this TSelf item)
    where TSelf : struct, IHaveChildrenWithKey<TKey, TSelf>, IHaveDepthInformation
    where TKey : notnull
{
    var result = 0;
    item.SumChildrenDepthRecursive<TSelf, TKey>(ref result);
    return result;
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static void SumChildrenDepthRecursive<TSelf, TKey>(this TSelf item, ref int accumulator)
    where TSelf : struct, IHaveChildrenWithKey<TKey, TSelf>, IHaveDepthInformation where TKey : notnull
{
    accumulator += item.Depth;
    foreach (var child in item.Children)
        child.Value.Item.SumChildrenDepthRecursive<TSelf, TKey>(ref accumulator);
}
```

```
| Method                          | Mean     | Error    | StdDev   | Code Size |
|-------------------------------- |---------:|---------:|---------:|----------:|
| CountChildrenDepth_ViaInterface | 21.20 ms | 0.054 ms | 0.045 ms |     212 B |
| CountChildrenDepth_ManuallyImpl | 21.11 ms | 0.178 ms | 0.167 ms |     206 B |
```
