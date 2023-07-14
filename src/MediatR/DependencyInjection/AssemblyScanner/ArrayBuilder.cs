using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MediatR.DependencyInjection.AssemblyScanner;

internal readonly struct ArrayBuilder<T>
{
    public static readonly ArrayBuilder<T> Instance = new();

    [ThreadStatic]
    private static List<T>? _builder;
    
    public int Count
    {
        get
        {
            EnsureBuilderIsInitialized();
            return _builder!.Count;
        }
    }
    public bool IsReadOnly => false;

    public void Add(T item)
    {
        EnsureBuilderIsInitialized();
        _builder!.Add(item);
    }

    public void AddRange(IEnumerable<T> items)
    {
        EnsureBuilderIsInitialized();
        _builder!.AddRange(items);
    }

    public void Clear() => _builder?.Clear();

    public bool Contains(T item)
    {
        EnsureBuilderIsInitialized();
        return _builder!.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        EnsureBuilderIsInitialized();
        _builder!.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        EnsureBuilderIsInitialized();
        return _builder!.Remove(item);
    }

    public void Insert(int index, T item)
    {
        EnsureBuilderIsInitialized();
        _builder!.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        EnsureBuilderIsInitialized();
        _builder!.RemoveAt(index);
    }

    public T[] ToArray()
    {
        if (_builder is null)
        {
            return Array.Empty<T>();
        }

        var result = _builder.ToArray();
        Clear();
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EnsureBuilderIsInitialized() =>
        _builder ??= new List<T>();
}