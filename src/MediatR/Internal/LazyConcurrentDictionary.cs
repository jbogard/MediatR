using System;
using System.Collections.Concurrent;
using System.Threading;

namespace MediatR.Internal;

internal sealed class LazyConcurrentDictionary<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _concurrentDictionary;

    public LazyConcurrentDictionary() => _concurrentDictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>();

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        var lazyResult = _concurrentDictionary.GetOrAdd(key,
            k => new Lazy<TValue>(() => valueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication));
        return lazyResult.Value;
    }

    public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
    {
        var lazyResult = _concurrentDictionary.AddOrUpdate(
            key,
            new Lazy<TValue>(() => addValue, LazyThreadSafetyMode.ExecutionAndPublication),
            (k, currentValue) => new Lazy<TValue>(() => updateValueFactory(k, currentValue.Value),
                LazyThreadSafetyMode.ExecutionAndPublication));
        return lazyResult.Value;
    }


    public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory,
        Func<TKey, TValue, TValue> updateValueFactory)
    {
        var lazyResult = _concurrentDictionary.AddOrUpdate(
            key,
            k => new Lazy<TValue>(() => addValueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication),
            (k, currentValue) => new Lazy<TValue>(() => updateValueFactory(k, currentValue.Value),
                LazyThreadSafetyMode.ExecutionAndPublication));
        return lazyResult.Value;
    }
}