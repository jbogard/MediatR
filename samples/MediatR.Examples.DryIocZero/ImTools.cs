/*
The MIT License (MIT)

Copyright (c) 2016 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included AddOrUpdateServiceFactory
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

namespace ImTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Runtime.CompilerServices; // For [MethodImpl(AggressiveInlining)]

    /// <summary>Helpers for functional composition</summary>
    public static class Fun
    {
        /// <summary>Always a true condition.</summary>
        public static bool Always<T>(T _) => true;

        /// <summary>Always a false condition.</summary>
        public static bool Never<T>(T _) => false;

        /// <summary>Identity function returning passed argument as result.</summary>
        public static T Itself<T>(T x) => x;
    }

    /// <summary>Methods to work with immutable arrays, and general array sugar.</summary>
    public static class ArrayTools
    {
        /// <summary>Returns singleton empty array of provided type.</summary> 
        /// <typeparam name="T">Array item type.</typeparam> <returns>Empty array.</returns>
        public static T[] Empty<T>() => EmptyArray<T>.Value;

        private static class EmptyArray<T>
        {
            public static readonly T[] Value = new T[0];
        }

        /// <summary>Wraps item in array.</summary>
        public static T[] One<T>(this T one) => new[] { one };

        /// <summary>Returns true if array is null or have no items.</summary> <typeparam name="T">Type of array item.</typeparam>
        /// <param name="source">Source array to check.</param> <returns>True if null or has no items, false otherwise.</returns>
        public static bool IsNullOrEmpty<T>(this T[] source) => source == null || source.Length == 0;

        /// <summary>Returns empty array instead of null, or source array otherwise.</summary> <typeparam name="T">Type of array item.</typeparam>
        public static T[] EmptyIfNull<T>(this T[] source) => source ?? Empty<T>();

        /// <summary>Returns source enumerable if it is array, otherwise converts source to array.</summary>
        public static T[] ToArrayOrSelf<T>(this IEnumerable<T> source) =>
            source == null ? Empty<T>() : (source as T[] ?? source.ToArray());

        /// <summary>Returns new array consisting from all items from source array then all items from added array.
        /// If source is null or empty, then added array will be returned.
        /// If added is null or empty, then source will be returned.</summary>
        /// <typeparam name="T">Array item type.</typeparam>
        /// <param name="source">Array with leading items.</param>
        /// <param name="added">Array with following items.</param>
        /// <returns>New array with items of source and added arrays.</returns>
        public static T[] Append<T>(this T[] source, params T[] added)
        {
            if (added == null || added.Length == 0)
                return source;
            if (source == null || source.Length == 0)
                return added;

            var result = new T[source.Length + added.Length];
            Array.Copy(source, 0, result, 0, source.Length);
            if (added.Length == 1)
                result[source.Length] = added[0];
            else
                Array.Copy(added, 0, result, source.Length, added.Length);
            return result;
        }

        /// <summary>Performant concat of enumerables in case of arrays.
        /// But performance will degrade if you use Concat().Where().</summary>
        /// <typeparam name="T">Type of item.</typeparam>
        /// <param name="source">goes first.</param>
        /// <param name="other">appended to source.</param>
        /// <returns>empty array or concat of source and other.</returns>
        public static T[] Append<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            var sourceArr = source.ToArrayOrSelf();
            var otherArr = other.ToArrayOrSelf();
            return sourceArr.Append(otherArr);
        }

        /// <summary>Returns new array with <paramref name="value"/> appended, 
        /// or <paramref name="value"/> at <paramref name="index"/>, if specified.
        /// If source array could be null or empty, then single value item array will be created despite any index.</summary>
        /// <typeparam name="T">Array item type.</typeparam>
        /// <param name="source">Array to append value to.</param>
        /// <param name="value">Value to append.</param>
        /// <param name="index">(optional) Index of value to update.</param>
        /// <returns>New array with appended or updated value.</returns>
        public static T[] AppendOrUpdate<T>(this T[] source, T value, int index = -1)
        {
            if (source == null || source.Length == 0)
                return new[] { value };
            var sourceLength = source.Length;
            index = index < 0 ? sourceLength : index;
            var result = new T[index < sourceLength ? sourceLength : sourceLength + 1];
            Array.Copy(source, result, sourceLength);
            result[index] = value;
            return result;
        }

        /// <summary>Calls predicate on each item in <paramref name="source"/> array until predicate returns true,
        /// then method will return this item index, or if predicate returns false for each item, method will return -1.</summary>
        /// <typeparam name="T">Type of array items.</typeparam>
        /// <param name="source">Source array: if null or empty, then method will return -1.</param>
        /// <param name="predicate">Delegate to evaluate on each array item until delegate returns true.</param>
        /// <returns>Index of item for which predicate returns true, or -1 otherwise.</returns>
        public static int IndexOf<T>(this T[] source, Func<T, bool> predicate)
        {
            if (source != null && source.Length != 0)
                for (var i = 0; i < source.Length; ++i)
                    if (predicate(source[i]))
                        return i;
            return -1;
        }

        /// <summary>Looks up for item in source array equal to provided value, and returns its index, or -1 if not found.</summary>
        /// <typeparam name="T">Type of array items.</typeparam>
        /// <param name="source">Source array: if null or empty, then method will return -1.</param>
        /// <param name="value">Value to look up.</param>
        /// <returns>Index of item equal to value, or -1 item is not found.</returns>
        public static int IndexOf<T>(this T[] source, T value)
        {
            if (source != null && source.Length != 0)
                for (var i = 0; i < source.Length; ++i)
                {
                    var item = source[i];
                    if (Equals(item, value))
                        return i;
                }
            return -1;
        }

        /// <summary>Produces new array without item at specified <paramref name="index"/>. 
        /// Will return <paramref name="source"/> array if index is out of bounds, or source is null/empty.</summary>
        /// <typeparam name="T">Type of array item.</typeparam>
        /// <param name="source">Input array.</param> <param name="index">Index if item to remove.</param>
        /// <returns>New array with removed item at index, or input source array if index is not in array.</returns>
        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            if (source == null || source.Length == 0 || index < 0 || index >= source.Length)
                return source;
            if (index == 0 && source.Length == 1)
                return new T[0];
            var result = new T[source.Length - 1];
            if (index != 0)
                Array.Copy(source, 0, result, 0, index);
            if (index != result.Length)
                Array.Copy(source, index + 1, result, index, result.Length - index);
            return result;
        }

        /// <summary>Looks for item in array using equality comparison, and returns new array with found item remove, or original array if not item found.</summary>
        /// <typeparam name="T">Type of array item.</typeparam>
        /// <param name="source">Input array.</param> <param name="value">Value to find and remove.</param>
        /// <returns>New array with value removed or original array if value is not found.</returns>
        public static T[] Remove<T>(this T[] source, T value)
        {
            return source.RemoveAt(source.IndexOf(value));
        }

        /// <summary>Returns first item matching the <paramref name="predicate"/>, or default item value.</summary>
        /// <typeparam name="T">item type</typeparam>
        /// <param name="source">items collection to search</param>
        /// <param name="predicate">condition to evaluate for each item.</param>
        /// <returns>First item matching condition or default value.</returns>
        public static T FindFirst<T>(this T[] source, Func<T, bool> predicate)
        {
            if (source != null && source.Length != 0)
                for (var i = 0; i < source.Length; ++i)
                {
                    var item = source[i];
                    if (predicate(item))
                        return item;
                }
            return default(T);
        }

        /// <summary>Returns first item matching the <paramref name="predicate"/>, or `default(T)` value.</summary>
        /// <typeparam name="T">item type</typeparam>
        /// <param name="source">items collection to search</param>
        /// <param name="predicate">condition to evaluate for each item.</param>
        /// <returns>First item matching condition or default value.</returns>
        public static T FindFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var sourceArr = source as T[];
            if (sourceArr != null)
                return sourceArr.FindFirst(predicate);
            return source.FirstOrDefault(predicate);
        }

        private static T[] AppendTo<T>(T[] source, int sourcePos, int count, T[] results = null)
        {
            if (results == null)
            {
                var newResults = new T[count];
                if (count == 1)
                    newResults[0] = source[sourcePos];
                else
                    for (int i = 0, j = sourcePos; i < count; ++i, ++j)
                        newResults[i] = source[j];
                return newResults;
            }

            var matchCount = results.Length;
            var appendedResults = new T[matchCount + count];
            if (matchCount == 1)
                appendedResults[0] = results[0];
            else
                Array.Copy(results, 0, appendedResults, 0, matchCount);

            if (count == 1)
                appendedResults[matchCount] = source[sourcePos];
            else
                Array.Copy(source, sourcePos, appendedResults, matchCount, count);

            return appendedResults;
        }

        private static R[] AppendTo<T, R>(T[] source, int sourcePos, int count, Func<T, R> map, R[] results = null)
        {
            if (results == null || results.Length == 0)
            {
                var newResults = new R[count];
                if (count == 1)
                    newResults[0] = map(source[sourcePos]);
                else
                {
                    for (int i = 0, j = sourcePos; i < count; ++i, ++j)
                        newResults[i] = map(source[j]);
                }
                return newResults;
            }

            var oldResultsCount = results.Length;
            var appendedResults = new R[oldResultsCount + count];
            if (oldResultsCount == 1)
                appendedResults[0] = results[0];
            else
                Array.Copy(results, 0, appendedResults, 0, oldResultsCount);

            if (count == 1)
                appendedResults[oldResultsCount] = map(source[sourcePos]);
            else
            {
                for (int i = oldResultsCount, j = sourcePos; i < appendedResults.Length; ++i, ++j)
                    appendedResults[i] = map(source[j]);
             }

            return appendedResults;
        }

        /// <summary>Where method similar to Enumerable.Where but more performant and non necessary allocating.
        /// It returns source array and does Not create new one if all items match the condition.</summary>
        /// <typeparam name="T">Type of source items.</typeparam>
        /// <param name="source">If null, the null will be returned.</param>
        /// <param name="condition">Condition to keep items.</param>
        /// <returns>New array if some items are filter out. Empty array if all items are filtered out. Original array otherwise.</returns>
        public static T[] Match<T>(this T[] source, Func<T, bool> condition)
        {
            if (source == null || source.Length == 0)
                return source;

            if (source.Length == 1)
                return condition(source[0]) ? source : Empty<T>();

            if (source.Length == 2)
            {
                var condition0 = condition(source[0]);
                var condition1 = condition(source[1]);
                return condition0 && condition1 ? new[] { source[0], source[1] }
                    : condition0 ? new[] { source[0] }
                        : condition1 ? new[] { source[1] }
                            : Empty<T>();
            }

            var matchStart = 0;
            T[] matches = null;
            var matchFound = false;

            var i = 0;
            while (i < source.Length)
            {
                matchFound = condition(source[i]);
                if (!matchFound)
                {
                    // for accumulated matched items
                    if (i != 0 && i > matchStart)
                        matches = AppendTo(source, matchStart, i - matchStart, matches);
                    matchStart = i + 1; // guess the next match start will be after the non-matched item
                }
                ++i;
            }

            // when last match was found but not all items are matched (hence matchStart != 0)
            if (matchFound && matchStart != 0)
                return AppendTo(source, matchStart, i - matchStart, matches);

            if (matches != null)
                return matches;

            if (matchStart != 0) // no matches
                return Empty<T>();

            return source;
        }

        /// <summary>Where method similar to Enumerable.Where but more performant and non necessary allocating.
        /// It returns source array and does Not create new one if all items match the condition.</summary>
        /// <typeparam name="T">Type of source items.</typeparam> <typeparam name="R">Type of result items.</typeparam>
        /// <param name="source">If null, the null will be returned.</param>
        /// <param name="condition">Condition to keep items.</param> <param name="map">Converter from source to result item.</param>
        /// <returns>New array of result items.</returns>
        public static R[] Match<T, R>(this T[] source, Func<T, bool> condition, Func<T, R> map)
        {
            if (source == null)
                return null;

            if (source.Length == 0)
                return Empty<R>();

            if (source.Length == 1)
            {
                var item = source[0];
                return condition(item) ? new[] { map(item) } : Empty<R>();
            }

            if (source.Length == 2)
            {
                var condition0 = condition(source[0]);
                var condition1 = condition(source[1]);
                return condition0 && condition1 ? new[] { map(source[0]), map(source[1]) }
                : condition0 ? new[] { map(source[0]) }
                : condition1 ? new[] { map(source[1]) }
                : Empty<R>();
            }

            var matchStart = 0;
            R[] matches = null;
            var matchFound = false;

            var i = 0;
            while (i < source.Length)
            {
                matchFound = condition(source[i]);
                if (!matchFound)
                {
                    // for accumulated matched items
                    if (i != 0 && i > matchStart)
                        matches = AppendTo(source, matchStart, i - matchStart, map, matches);
                    matchStart = i + 1; // guess the next match start will be after the non-matched item
                }
                ++i;
            }

            // when last match was found but not all items are matched (hence matchStart != 0)
            if (matchFound && matchStart != 0)
                return AppendTo(source, matchStart, i - matchStart, map, matches);

            if (matches != null)
                return matches;

            if (matchStart != 0) // no matches
                return Empty<R>();

            return AppendTo(source, 0, source.Length, map);
        }

        /// <summary>Maps all items from source to result array.</summary>
        /// <typeparam name="T">Source item type</typeparam> <typeparam name="R">Result item type</typeparam>
        /// <param name="source">Source items</param> <param name="map">Function to convert item from source to result.</param>
        /// <returns>Converted items</returns>
        public static R[] Map<T, R>(this T[] source, Func<T, R> map)
        {
            if (source == null)
                return null;

            var sourceCount = source.Length;
            if (sourceCount == 0)
                return Empty<R>();

            if (sourceCount == 1)
                return new[] { map(source[0]) };

            if (sourceCount == 2)
                return new[] { map(source[0]), map(source[1]) };

            if (sourceCount == 3)
                return new[] { map(source[0]), map(source[1]), map(source[2]) };

            var results = new R[sourceCount];
            for (var i = 0; i < source.Length; i++)
                results[i] = map(source[i]);
            return results;
        }

        /// <summary>Maps all items from source to result collection. 
        /// If possible uses fast array Map otherwise Enumerable.Select.</summary>
        /// <typeparam name="T">Source item type</typeparam> <typeparam name="R">Result item type</typeparam>
        /// <param name="source">Source items</param> <param name="map">Function to convert item from source to result.</param>
        /// <returns>Converted items</returns>
        public static IEnumerable<R> Map<T, R>(this IEnumerable<T> source, Func<T, R> map)
        {
            if (source == null)
                return null;
            var arr = source as T[];
            if (arr != null)
                return arr.Map(map);
            return source.Select(map);
        }

        /// <summary>If <paramref name="source"/> is array uses more effective Match for array, otherwise just calls Where</summary>
        /// <typeparam name="T">Type of source items.</typeparam>
        /// <param name="source">If null, the null will be returned.</param>
        /// <param name="condition">Condition to keep items.</param>
        /// <returns>Result items, may be an array.</returns>
        public static IEnumerable<T> Match<T>(this IEnumerable<T> source, Func<T, bool> condition)
        {
            if (source == null)
                return null;
            var arr = source as T[];
            if (arr != null)
                return arr.Match(condition);
            return source.Where(condition);
        }

        /// <summary>If <paramref name="source"/> is array uses more effective Match for array,
        /// otherwise just calls Where, Select</summary>
        /// <typeparam name="T">Type of source items.</typeparam> <typeparam name="R">Type of result items.</typeparam>
        /// <param name="source">If null, the null will be returned.</param>
        /// <param name="condition">Condition to keep items.</param>  <param name="map">Converter from source to result item.</param>
        /// <returns>Result items, may be an array.</returns>
        public static IEnumerable<R> Match<T, R>(this IEnumerable<T> source, Func<T, bool> condition, Func<T, R> map)
        {
            if (source == null)
                return null;
            var arr = source as T[];
            if (arr != null)
                return arr.Match(condition, map);
            return source.Where(condition).Select(map);
        }
    }

    /// <summary>Wrapper that provides optimistic-concurrency Swap operation implemented using <see cref="Ref.Swap{T}"/>.</summary>
    /// <typeparam name="T">Type of object to wrap.</typeparam>
    public sealed class Ref<T> where T : class
    {
        /// <summary>Gets the wrapped value.</summary>
        public T Value { get { return _value; } }

        /// <summary>Creates ref to object, optionally with initial value provided.</summary>
        /// <param name="initialValue">(optional) Initial value.</param>
        public Ref(T initialValue = default(T))
        {
            _value = initialValue;
        }

        /// <summary>Exchanges currently hold object with <paramref name="getNewValue"/> - see <see cref="Ref.Swap{T}"/> for details.</summary>
        /// <param name="getNewValue">Delegate to produce new object value from current one passed as parameter.</param>
        /// <returns>Returns old object value the same way as <see cref="Interlocked.Exchange(ref int,int)"/></returns>
        /// <remarks>Important: <paramref name="getNewValue"/> May be called multiple times to retry update with value concurrently changed by other code.</remarks>
        public T Swap(Func<T, T> getNewValue)
        {
            return Ref.Swap(ref _value, getNewValue);
        }

        /// <summary>Just sets new value ignoring any intermingled changes.</summary>
        /// <param name="newValue"></param> <returns>old value</returns>
        public T Swap(T newValue)
        {
            return Interlocked.Exchange(ref _value, newValue);
        }

        /// <summary>Compares current Referred value with <paramref name="currentValue"/> and if equal replaces current with <paramref name="newValue"/></summary>
        /// <param name="currentValue"></param> <param name="newValue"></param>
        /// <returns>True if current value was replaced with new value, and false if current value is outdated (already changed by other party).</returns>
        /// <example><c>[!CDATA[
        /// var value = SomeRef.Value;
        /// if (!SomeRef.TrySwapIfStillCurrent(value, Update(value))
        ///     SomeRef.Swap(v => Update(v)); // fallback to normal Swap with delegate allocation
        /// ]]</c></example>
        public bool TrySwapIfStillCurrent(T currentValue, T newValue)
        {
            return Interlocked.CompareExchange(ref _value, newValue, currentValue) == currentValue;
        }

        private T _value;
    }

    /// <summary>Provides optimistic-concurrency consistent <see cref="Swap{T}"/> operation.</summary>
    public static class Ref
    {
        /// <summary>Factory for <see cref="Ref{T}"/> with type of value inference.</summary>
        /// <typeparam name="T">Type of value to wrap.</typeparam>
        /// <param name="value">Initial value to wrap.</param>
        /// <returns>New ref.</returns>
        public static Ref<T> Of<T>(T value) where T : class
        {
            return new Ref<T>(value);
        }

        /// <summary>Creates new ref to the value of original ref.</summary> <typeparam name="T">Ref value type.</typeparam>
        /// <param name="original">Original ref.</param> <returns>New ref to original value.</returns>
        public static Ref<T> NewRef<T>(this Ref<T> original) where T : class
        {
            return Of(original.Value);
        }

        /// <summary>First, it evaluates new value using <paramref name="getNewValue"/> function. 
        /// Second, it checks that original value is not changed. 
        /// If it is changed it will retry first step, otherwise it assigns new value and returns original (the one used for <paramref name="getNewValue"/>).</summary>
        /// <typeparam name="T">Type of value to swap.</typeparam>
        /// <param name="value">Reference to change to new value</param>
        /// <param name="getNewValue">Delegate to get value from old one.</param>
        /// <returns>Old/original value. By analogy with <see cref="Interlocked.Exchange(ref int,int)"/>.</returns>
        /// <remarks>Important: <paramref name="getNewValue"/> May be called multiple times to retry update with value concurrently changed by other code.</remarks>
        public static T Swap<T>(ref T value, Func<T, T> getNewValue) where T : class
        {
            var retryCount = 0;
            while (true)
            {
                var oldValue = value;
                var newValue = getNewValue(oldValue);
                if (Interlocked.CompareExchange(ref value, newValue, oldValue) == oldValue)
                    return oldValue;
                if (++retryCount > RETRY_COUNT_UNTIL_THROW)
                    throw new InvalidOperationException(_errorRetryCountExceeded);
            }
        }

        private const int RETRY_COUNT_UNTIL_THROW = 50;
        private static readonly string _errorRetryCountExceeded =
            "Ref retried to Update for " + RETRY_COUNT_UNTIL_THROW + " times But there is always someone else intervened.";
    }

    /// <summary>Immutable Key-Value pair. It is reference type (could be check for null), 
    /// which is different from System value type <see cref="KeyValuePair{TKey,TValue}"/>.
    /// In addition provides <see cref="Equals"/> and <see cref="GetHashCode"/> implementations.</summary>
    /// <typeparam name="K">Type of Key.</typeparam><typeparam name="V">Type of Value.</typeparam>
    public class KV<K, V>
    {
        /// <summary>Key.</summary>
        public readonly K Key;

        /// <summary>Value.</summary>
        public readonly V Value;

        /// <summary>Creates Key-Value object by providing key and value. Does Not check either one for null.</summary>
        /// <param name="key">key.</param><param name="value">value.</param>
        public KV(K key, V value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>Creates nice string view.</summary><returns>String representation.</returns>
        public override string ToString()
        {
            var s = new StringBuilder('{');
            if (Key != null)
                s.Append(Key);
            s.Append(',');
            if (Value != null)
                s.Append(Value);
            s.Append('}');
            return s.ToString();
        }

        /// <summary>Returns true if both key and value are equal to corresponding key-value of other object.</summary>
        /// <param name="obj">Object to check equality with.</param> <returns>True if equal.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as KV<K, V>;
            return other != null
                   && (ReferenceEquals(other.Key, Key) || Equals(other.Key, Key))
                   && (ReferenceEquals(other.Value, Value) || Equals(other.Value, Value));
        }

        /// <summary>Combines key and value hash code. R# generated default implementation.</summary>
        /// <returns>Combined hash code for key-value.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((object)Key == null ? 0 : Key.GetHashCode() * 397)
                       ^ ((object)Value == null ? 0 : Value.GetHashCode());
            }
        }
    }

    /// <summary>Helpers for <see cref="KV{K,V}"/>.</summary>
    public static class KV
    {
        /// <summary>Creates the key value pair.</summary>
        /// <typeparam name="K">Key type</typeparam> <typeparam name="V">Value type</typeparam>
        /// <param name="key">Key</param> <param name="value">Value</param> <returns>New pair.</returns>
        public static KV<K, V> Of<K, V>(K key, V value)
        {
            return new KV<K, V>(key, value);
        }

        /// <summary>Creates the new pair with new key and old value.</summary>
        /// <typeparam name="K">Key type</typeparam> <typeparam name="V">Value type</typeparam>
        /// <param name="source">Source value</param> <param name="key">New key</param> <returns>New pair</returns>
        public static KV<K, V> WithKey<K, V>(this KV<K, V> source, K key)
        {
            return new KV<K, V>(key, source.Value);
        }

        /// <summary>Creates the new pair with old key and new value.</summary>
        /// <typeparam name="K">Key type</typeparam> <typeparam name="V">Value type</typeparam>
        /// <param name="source">Source value</param> <param name="value">New value.</param> <returns>New pair</returns>
        public static KV<K, V> WithValue<K, V>(this KV<K, V> source, V value)
        {
            return new KV<K, V>(source.Key, value);
        }
    }

    /// <summary>Simple helper for creation of the pair of two parts.</summary>
    public static class KeyValuePair
    {
        /// <summary>Pairs key with value.</summary>
        public static KeyValuePair<K, V> Pair<K, V>(this K key, V value) =>
            new KeyValuePair<K, V>(key, value);
    }

    /// <summary>Helper structure which allows to distinguish null value from the default value for optional parameter.</summary>
    public struct Opt<T>
    {
        /// <summary>Allows to transparently convert parameter argument to opt structure.</summary>
        public static implicit operator Opt<T>(T value) => new Opt<T>(value);

        /// <summary>Argument value.</summary>
        public T Value;

        /// <summary>Indicates that value is provided.</summary>
        public bool HasValue;

        /// <summary>Wraps passed value in structure. Sets the flag that value is present.</summary>
        public Opt(T value)
        {
            HasValue = true;
            Value = value;
        }

        /// <summary>Helper to get value or default value if value is not present.</summary>
        public T OrDefault(T defaultValue = default(T)) => HasValue ? Value : defaultValue;
    }

    /// <summary>Immutable list - simplest linked list with Head and Rest.</summary>
    /// <typeparam name="T">Type of the item.</typeparam>
    public sealed class ImList<T>
    {
        /// <summary>Empty list to Push to.</summary>
        public static readonly ImList<T> Empty = new ImList<T>();

        /// <summary>True for empty list.</summary>
        public bool IsEmpty
        {
            get { return Tail == null; }
        }

        /// <summary>First value in a list.</summary>
        public readonly T Head;

        /// <summary>The rest of values or Empty if list has a single value.</summary>
        public readonly ImList<T> Tail;

        /// <summary>Prepends new value and returns new list.</summary>
        /// <param name="head">New first value.</param>
        /// <returns>List with the new head.</returns>
        public ImList<T> Prep(T head)
        {
            return new ImList<T>(head, this);
        }

        /// <summary>Enumerates the list.</summary>
        /// <returns>Each item in turn.</returns>
        public IEnumerable<T> Enumerate()
        {
            if (IsEmpty)
                yield break;
            for (var list = this; !list.IsEmpty; list = list.Tail)
                yield return list.Head;
        }

        #region Implementation

        private ImList() { }

        private ImList(T head, ImList<T> tail)
        {
            Head = head;
            Tail = tail;
        }

        #endregion
    }

    /// <summary>Extension methods providing basic operations on a list.</summary>
    public static class ImList
    {
        /// <summary>This a basically a Fold function, to address needs in Map, Filter, Reduce.</summary>
        /// <typeparam name="T">Type of list item.</typeparam>
        /// <typeparam name="R">Type of result.</typeparam>
        /// <param name="source">List to fold.</param>
        /// <param name="initialValue">From were to start.</param>
        /// <param name="collect">Collects list item into result</param>
        /// <returns>Return result or <paramref name="initialValue"/> for empty list.</returns>
        public static R To<T, R>(this ImList<T> source, R initialValue, Func<T, R, R> collect)
        {
            if (source.IsEmpty)
                return initialValue;
            var value = initialValue;
            for (; !source.IsEmpty; source = source.Tail)
                value = collect(source.Head, value);
            return value;
        }

        /// <summary>Form of fold function with element index for convenience.</summary>
        /// <typeparam name="T">Type of list item.</typeparam>
        /// <typeparam name="R">Type of result.</typeparam>
        /// <param name="source">List to fold.</param>
        /// <param name="initialValue">From were to start.</param>
        /// <param name="collect">Collects list item into result</param>
        /// <returns>Return result or <paramref name="initialValue"/> for empty list.</returns>
        public static R To<T, R>(this ImList<T> source, R initialValue, Func<T, int, R, R> collect)
        {
            if (source.IsEmpty)
                return initialValue;
            var value = initialValue;
            for (var i = 0; !source.IsEmpty; source = source.Tail)
                value = collect(source.Head, i++, value);
            return value;
        }

        /// <summary>Returns new list in reverse order.</summary>
        /// <typeparam name="T">List item type</typeparam> <param name="source">List to reverse.</param>
        /// <returns>New list. If list consist on single element, then the same list.</returns>
        public static ImList<T> Reverse<T>(this ImList<T> source)
        {
            if (source.IsEmpty || source.Tail.IsEmpty)
                return source;
            return source.To(ImList<T>.Empty, (it, _) => _.Prep(it));
        }

        /// <summary>Maps the items from the first list to the result list.</summary>
        /// <typeparam name="T">source item type.</typeparam> 
        /// <typeparam name="R">result item type.</typeparam>
        /// <param name="source">input list.</param> <param name="map">converter func.</param>
        /// <returns>result list.</returns>
        public static ImList<R> Map<T, R>(this ImList<T> source, Func<T, R> map)
        {
            return source.To(ImList<R>.Empty, (it, _) => _.Prep(map(it))).Reverse();
        }

        /// <summary>Maps the items from the first list to the result list with item index.</summary>
        /// <typeparam name="T">source item type.</typeparam> 
        /// <typeparam name="R">result item type.</typeparam>
        /// <param name="source">input list.</param> <param name="map">converter func.</param>
        /// <returns>result list.</returns>
        public static ImList<R> Map<T, R>(this ImList<T> source, Func<T, int, R> map)
        {
            return source.To(ImList<R>.Empty, (it, i, _) => _.Prep(map(it, i))).Reverse();
        }

        /// <summary>Copies list to array.</summary> 
        /// <param name="source">list to convert.</param> 
        /// <returns>Array with list items.</returns>
        public static T[] ToArray<T>(this ImList<T> source)
        {
            if (source.IsEmpty)
                return ArrayTools.Empty<T>();
            if (source.Tail.IsEmpty)
                return new[] { source.Head };
            return source.Enumerate().ToArray();
        }
    }

    /// <summary>Given the old value should and the new value should return result updated value.</summary>
    public delegate V Update<V>(V oldValue, V newValue);

    /// <summary>Immutable http://en.wikipedia.org/wiki/AVL_tree with integer keys and <typeparamref name="V"/> values.</summary>
    public sealed class ImMap<V>
    {
        /// <summary>Empty tree to start with.</summary>
        public static readonly ImMap<V> Empty = new ImMap<V>();

        /// <summary>Key.</summary>
        public readonly int Key;

        /// <summary>Value.</summary>
        public readonly V Value;

        /// <summary>Left sub-tree/branch, or empty.</summary>
        public readonly ImMap<V> Left;

        /// <summary>Right sub-tree/branch, or empty.</summary>
        public readonly ImMap<V> Right;

        /// <summary>Height of longest sub-tree/branch plus 1. It is 0 for empty tree, and 1 for single node tree.</summary>
        public readonly int Height;

        /// <summary>Returns true is tree is empty.</summary>
        public bool IsEmpty
        {
            get { return Height == 0; }
        }

        /// <summary>Returns new tree with added or updated value for specified key.</summary>
        /// <param name="key"></param> <param name="value"></param>
        /// <returns>New tree.</returns>
        public ImMap<V> AddOrUpdate(int key, V value)
        {
            return AddOrUpdateImpl(key, value);
        }

        /// <summary>Returns new tree with added or updated value for specified key.</summary>
        /// <param name="key">Key</param> <param name="value">Value</param>
        /// <param name="updateValue">(optional) Delegate to calculate new value from and old and a new value.</param>
        /// <returns>New tree.</returns>
        public ImMap<V> AddOrUpdate(int key, V value, Update<V> updateValue)
        {
            return AddOrUpdateImpl(key, value, false, updateValue);
        }

        /// <summary>Returns new tree with updated value for the key, Or the same tree if key was not found.</summary>
        /// <param name="key"></param> <param name="value"></param>
        /// <returns>New tree if key is found, or the same tree otherwise.</returns>
        public ImMap<V> Update(int key, V value)
        {
            return AddOrUpdateImpl(key, value, true, null);
        }

        /// <summary>Get value for found key or null otherwise.</summary>
        /// <param name="key"></param> <param name="defaultValue">(optional) Value to return if key is not found.</param>
        /// <returns>Found value or <paramref name="defaultValue"/>.</returns>
        public V GetValueOrDefault(int key, V defaultValue = default(V))
        {
            var node = this;
            while (node.Height != 0 && node.Key != key)
                node = key < node.Key ? node.Left : node.Right;
            return node.Height != 0 ? node.Value : defaultValue;
        }

        /// <summary>Returns true if key is found and sets the value.</summary>
        /// <param name="key">Key to look for.</param> <param name="value">Result value</param>
        /// <returns>True if key found, false otherwise.</returns>
        public bool TryFind(int key, out V value)
        {
            var hash = key.GetHashCode();

            var node = this;
            while (node.Height != 0 && node.Key != key)
                node = hash < node.Key ? node.Left : node.Right;

            if (node.Height != 0)
            {
                value = node.Value;
                return true;
            }

            value = default(V);
            return false;
        }

        /// <summary>Returns all sub-trees enumerated from left to right.</summary> 
        /// <returns>Enumerated sub-trees or empty if tree is empty.</returns>
        public IEnumerable<ImMap<V>> Enumerate()
        {
            if (Height == 0)
                yield break;

            var parents = new ImMap<V>[Height];

            var node = this;
            var parentCount = -1;
            while (node.Height != 0 || parentCount != -1)
            {
                if (node.Height != 0)
                {
                    parents[++parentCount] = node;
                    node = node.Left;
                }
                else
                {
                    node = parents[parentCount--];
                    yield return node;
                    node = node.Right;
                }
            }
        }

        /// <summary>Removes or updates value for specified key, or does nothing if key is not found.
        /// Based on Eric Lippert http://blogs.msdn.com/b/ericlippert/archive/2008/01/21/immutability-in-c-part-nine-academic-plus-my-avl-tree-implementation.aspx </summary>
        /// <param name="key">Key to look for.</param> 
        /// <returns>New tree with removed or updated value.</returns>
        public ImMap<V> Remove(int key)
        {
            return RemoveImpl(key);
        }

        /// <summary>Outputs key value pair</summary>
        public override string ToString()
        {
            return Key + " : " + Value;
        }

        #region Implementation

        private ImMap() { }

        private ImMap(int key, V value)
        {
            Key = key;
            Value = value;
            Left = Empty;
            Right = Empty;
            Height = 1;
        }

        private ImMap(int key, V value, ImMap<V> left, ImMap<V> right, int height)
        {
            Key = key;
            Value = value;
            Left = left;
            Right = right;
            Height = height;
        }

        private ImMap(int key, V value, ImMap<V> left, ImMap<V> right)
        {
            Key = key;
            Value = value;
            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        private ImMap<V> AddOrUpdateImpl(int key, V value)
        {
            return Height == 0  // add new node
                ? new ImMap<V>(key, value)
                : (key == Key // update found node
                    ? new ImMap<V>(key, value, Left, Right)
                    : (key < Key  // search for node
                        ? (Height == 1
                            ? new ImMap<V>(Key, Value, new ImMap<V>(key, value), Right, height: 2)
                            : new ImMap<V>(Key, Value, Left.AddOrUpdateImpl(key, value), Right).KeepBalance())
                        : (Height == 1
                            ? new ImMap<V>(Key, Value, Left, new ImMap<V>(key, value), height: 2)
                            : new ImMap<V>(Key, Value, Left, Right.AddOrUpdateImpl(key, value)).KeepBalance())));
        }

        private ImMap<V> AddOrUpdateImpl(int key, V value, bool updateOnly, Update<V> update)
        {
            return Height == 0 ? // tree is empty
                (updateOnly ? this : new ImMap<V>(key, value))
                : (key == Key ? // actual update
                    new ImMap<V>(key, update == null ? value : update(Value, value), Left, Right)
                    : (key < Key    // try update on left or right sub-tree
                        ? new ImMap<V>(Key, Value, Left.AddOrUpdateImpl(key, value, updateOnly, update), Right)
                        : new ImMap<V>(Key, Value, Left, Right.AddOrUpdateImpl(key, value, updateOnly, update)))
                    .KeepBalance());
        }

        private ImMap<V> KeepBalance()
        {
            var delta = Left.Height - Right.Height;
            if (delta >= 2) // left is longer by 2, rotate left
            {
                var left = Left;
                var leftLeft = left.Left;
                var leftRight = left.Right;
                if (leftRight.Height - leftLeft.Height == 1)
                {
                    // double rotation:
                    //      5     =>     5     =>     4
                    //   2     6      4     6      2     5
                    // 1   4        2   3        1   3     6
                    //    3        1
                    return new ImMap<V>(leftRight.Key, leftRight.Value,
                        left: new ImMap<V>(left.Key, left.Value,
                            left: leftLeft, right: leftRight.Left), right: new ImMap<V>(Key, Value,
                            left: leftRight.Right, right: Right));
                }

                // todo: do we need this?
                // one rotation:
                //      5     =>     2
                //   2     6      1     5
                // 1   4              4   6
                return new ImMap<V>(left.Key, left.Value,
                    left: leftLeft, right: new ImMap<V>(Key, Value,
                        left: leftRight, right: Right));
            }

            if (delta <= -2)
            {
                var right = Right;
                var rightLeft = right.Left;
                var rightRight = right.Right;
                if (rightLeft.Height - rightRight.Height == 1)
                {
                    return new ImMap<V>(rightLeft.Key, rightLeft.Value,
                        left: new ImMap<V>(Key, Value,
                            left: Left, right: rightLeft.Left), right: new ImMap<V>(right.Key, right.Value,
                            left: rightLeft.Right, right: rightRight));
                }

                return new ImMap<V>(right.Key, right.Value,
                    left: new ImMap<V>(Key, Value,
                        left: Left, right: rightLeft), right: rightRight);
            }

            return this;
        }

        private ImMap<V> RemoveImpl(int key, bool ignoreKey = false)
        {
            if (Height == 0)
                return this;

            ImMap<V> result;
            if (key == Key || ignoreKey) // found node
            {
                if (Height == 1) // remove node
                    return Empty;

                if (Right.IsEmpty)
                    result = Left;
                else if (Left.IsEmpty)
                    result = Right;
                else
                {
                    // we have two children, so remove the next highest node and replace this node with it.
                    var successor = Right;
                    while (!successor.Left.IsEmpty) successor = successor.Left;
                    result = new ImMap<V>(successor.Key, successor.Value,
                        Left, Right.RemoveImpl(successor.Key, ignoreKey: true));
                }
            }
            else if (key < Key)
                result = new ImMap<V>(Key, Value, Left.RemoveImpl(key), Right);
            else
                result = new ImMap<V>(Key, Value, Left, Right.RemoveImpl(key));

            return result.KeepBalance();
        }

        #endregion
    }

    /// <summary>Immutable http://en.wikipedia.org/wiki/AVL_tree 
    /// where node key is the hash code of <typeparamref name="K"/>.</summary>
    public sealed class ImHashMap<K, V>
    {
        /// <summary>Empty tree to start with.</summary>
        public static readonly ImHashMap<K, V> Empty = new ImHashMap<K, V>();

        /// <summary>Calculated key hash.</summary>
        public int Hash
        {
            get { return _data.Hash; }
        }

        /// <summary>Key of type K that should support <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/>.</summary>
        public K Key
        {
            get { return _data.Key; }
        }

        /// <summary>Value of any type V.</summary>
        public V Value
        {
            get { return _data.Value; }
        }

        /// <summary>In case of <see cref="Hash"/> conflicts for different keys contains conflicted keys with their values.</summary>
        public KV<K, V>[] Conflicts
        {
            get { return _data.Conflicts; }
        }

        /// <summary>Left sub-tree/branch, or empty.</summary>
        public readonly ImHashMap<K, V> Left;

        /// <summary>Right sub-tree/branch, or empty.</summary>
        public readonly ImHashMap<K, V> Right;

        /// <summary>Height of longest sub-tree/branch plus 1. It is 0 for empty tree, and 1 for single node tree.</summary>
        public readonly int Height;

        /// <summary>Returns true if tree is empty.</summary>
        public bool IsEmpty
        {
            get { return Height == 0; }
        }

        /// <summary>Returns new tree with added key-value. 
        /// If value with the same key is exist then the value is replaced.</summary>
        /// <param name="key">Key to add.</param><param name="value">Value to add.</param>
        /// <returns>New tree with added or updated key-value.</returns>
        public ImHashMap<K, V> AddOrUpdate(K key, V value)
        {
            return AddOrUpdate(key.GetHashCode(), key, value);
        }

        /// <summary>Returns new tree with added key-value. If value with the same key is exist, then
        /// if <paramref name="update"/> is not specified: then existing value will be replaced by <paramref name="value"/>;
        /// if <paramref name="update"/> is specified: then update delegate will decide what value to keep.</summary>
        /// <param name="key">Key to add.</param><param name="value">Value to add.</param>
        /// <param name="update">Update handler.</param>
        /// <returns>New tree with added or updated key-value.</returns>
        public ImHashMap<K, V> AddOrUpdate(K key, V value, Update<V> update)
        {
            return AddOrUpdate(key.GetHashCode(), key, value, update);
        }

        /// <summary>Looks for <paramref name="key"/> and replaces its value with new <paramref name="value"/>, or 
        /// runs custom update handler (<paramref name="update"/>) with old and new value to get the updated result.</summary>
        /// <param name="key">Key to look for.</param>
        /// <param name="value">New value to replace key value with.</param>
        /// <param name="update">(optional) Delegate for custom update logic, it gets old and new <paramref name="value"/>
        /// as inputs and should return updated value as output.</param>
        /// <returns>New tree with updated value or the SAME tree if no key found.</returns>
        public ImHashMap<K, V> Update(K key, V value, Update<V> update = null)
        {
            return Update(key.GetHashCode(), key, value, update);
        }

        /// <summary>Looks for key in a tree and returns the key value if found, or <paramref name="defaultValue"/> otherwise.</summary>
        /// <param name="key">Key to look for.</param> <param name="defaultValue">(optional) Value to return if key is not found.</param>
        /// <returns>Found value or <paramref name="defaultValue"/>.</returns>
        [MethodImpl((MethodImplOptions)256)]
        public V GetValueOrDefault(K key, V defaultValue = default(V))
        {
            var t = this;
            var hash = key.GetHashCode();
            while (t.Height != 0 && t.Hash != hash)
                t = hash < t.Hash ? t.Left : t.Right;
            return t.Height != 0 && (ReferenceEquals(key, t.Key) || key.Equals(t.Key))
                ? t.Value : t.GetConflictedValueOrDefault(key, defaultValue);
        }

        /// <summary>Returns true if key is found and sets the value.</summary>
        /// <param name="key">Key to look for.</param> <param name="value">Result value</param>
        /// <returns>True if key found, false otherwise.</returns>
        [MethodImpl((MethodImplOptions)256)]
        public bool TryFind(K key, out V value)
        {
            var hash = key.GetHashCode();

            var t = this;
            while (t.Height != 0 && t._data.Hash != hash)
                t = hash < t._data.Hash ? t.Left : t.Right;

            if (t.Height != 0 && (ReferenceEquals(key, t._data.Key) || key.Equals(t._data.Key)))
            {
                value = t._data.Value;
                return true;
            }

            return t.TryFindConflictedValue(key, out value);
        }

        /// <summary>Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
        /// The only difference is using fixed size array instead of stack for speed-up (~20% faster than stack).</summary>
        /// <returns>Sequence of enumerated key value pairs.</returns>
        public IEnumerable<KV<K, V>> Enumerate()
        {
            if (Height == 0)
                yield break;

            var parents = new ImHashMap<K, V>[Height];

            var node = this;
            var parentCount = -1;
            while (node.Height != 0 || parentCount != -1)
            {
                if (node.Height != 0)
                {
                    parents[++parentCount] = node;
                    node = node.Left;
                }
                else
                {
                    node = parents[parentCount--];
                    yield return new KV<K, V>(node.Key, node.Value);

                    if (node.Conflicts != null)
                        for (var i = 0; i < node.Conflicts.Length; i++)
                            yield return node.Conflicts[i];

                    node = node.Right;
                }
            }
        }

        /// <summary>Removes or updates value for specified key, or does nothing if key is not found.
        /// Based on Eric Lippert http://blogs.msdn.com/b/ericlippert/archive/2008/01/21/immutability-in-c-part-nine-academic-plus-my-avl-tree-implementation.aspx </summary>
        /// <param name="key">Key to look for.</param> 
        /// <returns>New tree with removed or updated value.</returns>
        public ImHashMap<K, V> Remove(K key)
        {
            return Remove(key.GetHashCode(), key);
        }

        /// <summary>Outputs key value pair</summary>
        public override string ToString()
        {
            return Key + " : " + Value;
        }

        #region Implementation

        private sealed class Data
        {
            public readonly int Hash;
            public readonly K Key;
            public readonly V Value;

            public readonly KV<K, V>[] Conflicts;

            public Data() { }

            public Data(int hash, K key, V value, KV<K, V>[] conflicts = null)
            {
                Hash = hash;
                Key = key;
                Value = value;
                Conflicts = conflicts;
            }
        }

        private readonly Data _data;

        private ImHashMap() { _data = new Data(); }

        private ImHashMap(Data data)
        {
            _data = data;
            Left = Empty;
            Right = Empty;
            Height = 1;
        }

        private ImHashMap(Data data, ImHashMap<K, V> left, ImHashMap<K, V> right)
        {
            _data = data;
            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        private ImHashMap(Data data, ImHashMap<K, V> left, ImHashMap<K, V> right, int height)
        {
            _data = data;
            Left = left;
            Right = right;
            Height = height;
        }

        internal ImHashMap<K, V> AddOrUpdate(int hash, K key, V value)
        {
            return Height == 0  // add new node
                ? new ImHashMap<K, V>(new Data(hash, key, value))
                : (hash == Hash // update found node
                    ? (ReferenceEquals(Key, key) || Key.Equals(key)
                        ? new ImHashMap<K, V>(new Data(hash, key, value, Conflicts), Left, Right)
                        : UpdateValueAndResolveConflicts(key, value, null, false))
                    : (hash < Hash  // search for node
                        ? (Height == 1
                            ? new ImHashMap<K, V>(_data,
                                new ImHashMap<K, V>(new Data(hash, key, value)), Right, height: 2)
                            : new ImHashMap<K, V>(_data,
                                Left.AddOrUpdate(hash, key, value), Right).KeepBalance())
                        : (Height == 1
                            ? new ImHashMap<K, V>(_data,
                                Left, new ImHashMap<K, V>(new Data(hash, key, value)), height: 2)
                            : new ImHashMap<K, V>(_data,
                                Left, Right.AddOrUpdate(hash, key, value)).KeepBalance())));
        }

        private ImHashMap<K, V> AddOrUpdate(int hash, K key, V value, Update<V> update)
        {
            return Height == 0
                ? new ImHashMap<K, V>(new Data(hash, key, value))
                : (hash == Hash // update
                    ? (ReferenceEquals(Key, key) || Key.Equals(key)
                        ? new ImHashMap<K, V>(new Data(hash, key, update(Value, value), Conflicts), Left, Right)
                        : UpdateValueAndResolveConflicts(key, value, update, false))
                    : (hash < Hash
                        ? With(Left.AddOrUpdate(hash, key, value, update), Right)
                        : With(Left, Right.AddOrUpdate(hash, key, value, update)))
                    .KeepBalance());
        }

        internal ImHashMap<K, V> Update(int hash, K key, V value, Update<V> update)
        {
            return Height == 0 ? this
                : (hash == Hash
                    ? (ReferenceEquals(Key, key) || Key.Equals(key)
                        ? new ImHashMap<K, V>(new Data(hash, key, update == null ? value : update(Value, value), Conflicts), Left, Right)
                        : UpdateValueAndResolveConflicts(key, value, update, true))
                    : (hash < Hash
                        ? With(Left.Update(hash, key, value, update), Right)
                        : With(Left, Right.Update(hash, key, value, update)))
                    .KeepBalance());
        }

        private ImHashMap<K, V> UpdateValueAndResolveConflicts(K key, V value, Update<V> update, bool updateOnly)
        {
            if (Conflicts == null) // add only if updateOnly is false.
                return updateOnly ? this
                    : new ImHashMap<K, V>(new Data(Hash, Key, Value, new[] { new KV<K, V>(key, value) }), Left, Right);

            var found = Conflicts.Length - 1;
            while (found >= 0 && !Equals(Conflicts[found].Key, Key)) --found;
            if (found == -1)
            {
                if (updateOnly) return this;
                var newConflicts = new KV<K, V>[Conflicts.Length + 1];
                Array.Copy(Conflicts, 0, newConflicts, 0, Conflicts.Length);
                newConflicts[Conflicts.Length] = new KV<K, V>(key, value);
                return new ImHashMap<K, V>(new Data(Hash, Key, Value, newConflicts), Left, Right);
            }

            var conflicts = new KV<K, V>[Conflicts.Length];
            Array.Copy(Conflicts, 0, conflicts, 0, Conflicts.Length);
            conflicts[found] = new KV<K, V>(key, update == null ? value : update(Conflicts[found].Value, value));
            return new ImHashMap<K, V>(new Data(Hash, Key, Value, conflicts), Left, Right);
        }

        internal V GetConflictedValueOrDefault(K key, V defaultValue)
        {
            if (Conflicts != null)
                for (var i = Conflicts.Length - 1; i >= 0; --i)
                    if (Equals(Conflicts[i].Key, key))
                        return Conflicts[i].Value;
            return defaultValue;
        }

        private bool TryFindConflictedValue(K key, out V value)
        {
            if (Height != 0 && Conflicts != null)
                for (var i = Conflicts.Length - 1; i >= 0; --i)
                    if (Equals(Conflicts[i].Key, key))
                    {
                        value = Conflicts[i].Value;
                        return true;
                    }

            value = default(V);
            return false;
        }

        private ImHashMap<K, V> KeepBalance()
        {
            var delta = Left.Height - Right.Height;
            if (delta >= 2) // left is longer by 2, rotate left
            {
                var left = Left;
                var leftLeft = left.Left;
                var leftRight = left.Right;
                if (leftRight.Height - leftLeft.Height == 1)
                {
                    // double rotation:
                    //      5     =>     5     =>     4
                    //   2     6      4     6      2     5
                    // 1   4        2   3        1   3     6
                    //    3        1
                    return new ImHashMap<K, V>(leftRight._data,
                        left: new ImHashMap<K, V>(left._data,
                            left: leftLeft, right: leftRight.Left), right: new ImHashMap<K, V>(_data,
                            left: leftRight.Right, right: Right));
                }

                // todo: do we need this?
                // one rotation:
                //      5     =>     2
                //   2     6      1     5
                // 1   4              4   6
                return new ImHashMap<K, V>(left._data,
                    left: leftLeft, right: new ImHashMap<K, V>(_data,
                        left: leftRight, right: Right));
            }

            if (delta <= -2)
            {
                var right = Right;
                var rightLeft = right.Left;
                var rightRight = right.Right;
                if (rightLeft.Height - rightRight.Height == 1)
                {
                    return new ImHashMap<K, V>(rightLeft._data,
                        left: new ImHashMap<K, V>(_data,
                            left: Left, right: rightLeft.Left), right: new ImHashMap<K, V>(right._data,
                            left: rightLeft.Right, right: rightRight));
                }

                return new ImHashMap<K, V>(right._data,
                    left: new ImHashMap<K, V>(_data,
                        left: Left, right: rightLeft), right: rightRight);
            }

            return this;
        }

        private ImHashMap<K, V> With(ImHashMap<K, V> left, ImHashMap<K, V> right)
        {
            return left == Left && right == Right ? this : new ImHashMap<K, V>(_data, left, right);
        }

        internal ImHashMap<K, V> Remove(int hash, K key, bool ignoreKey = false)
        {
            if (Height == 0)
                return this;

            ImHashMap<K, V> result;
            if (hash == Hash) // found node
            {
                if (ignoreKey || Equals(Key, key))
                {
                    if (!ignoreKey && Conflicts != null)
                        return ReplaceRemovedWithConflicted();

                    if (Height == 1) // remove node
                        return Empty;

                    if (Right.IsEmpty)
                        result = Left;
                    else if (Left.IsEmpty)
                        result = Right;
                    else
                    {
                        // we have two children, so remove the next highest node and replace this node with it.
                        var successor = Right;
                        while (!successor.Left.IsEmpty) successor = successor.Left;
                        result = new ImHashMap<K, V>(successor._data,
                            Left, Right.Remove(successor.Hash, default(K), ignoreKey: true));
                    }
                }
                else if (Conflicts != null)
                    return TryRemoveConflicted(key);
                else
                    return this; // if key is not matching and no conflicts to lookup - just return
            }
            else if (hash < Hash)
                result = new ImHashMap<K, V>(_data, Left.Remove(hash, key, ignoreKey), Right);
            else
                result = new ImHashMap<K, V>(_data, Left, Right.Remove(hash, key, ignoreKey));

            if (result.Height == 1)
                return result;

            return result.KeepBalance();
        }

        private ImHashMap<K, V> TryRemoveConflicted(K key)
        {
            var index = Conflicts.Length - 1;
            while (index >= 0 && !Equals(Conflicts[index].Key, key)) --index;
            if (index == -1) // key is not found in conflicts - just return
                return this;

            if (Conflicts.Length == 1)
                return new ImHashMap<K, V>(new Data(Hash, Key, Value), Left, Right);
            var shrinkConflicts = new KV<K, V>[Conflicts.Length - 1];
            var newIndex = 0;
            for (var i = 0; i < Conflicts.Length; ++i)
                if (i != index) shrinkConflicts[newIndex++] = Conflicts[i];
            return new ImHashMap<K, V>(new Data(Hash, Key, Value, shrinkConflicts), Left, Right);
        }

        private ImHashMap<K, V> ReplaceRemovedWithConflicted()
        {
            if (Conflicts.Length == 1)
                return new ImHashMap<K, V>(new Data(Hash, Conflicts[0].Key, Conflicts[0].Value), Left, Right);
            var shrinkConflicts = new KV<K, V>[Conflicts.Length - 1];
            Array.Copy(Conflicts, 1, shrinkConflicts, 0, shrinkConflicts.Length);
            return new ImHashMap<K, V>(new Data(Hash, Conflicts[0].Key, Conflicts[0].Value, shrinkConflicts), Left, Right);
        }

        #endregion
    }
}