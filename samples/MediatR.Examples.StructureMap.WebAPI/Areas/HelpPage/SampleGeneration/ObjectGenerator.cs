using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace MediatR.Examples.StructureMap.WebAPI.Areas.HelpPage
{
    /// <summary>
    /// This class will create an object of a given type and populate it with sample data.
    /// </summary>
    public class ObjectGenerator
    {
        internal const int DefaultCollectionSize = 2;
        private readonly SimpleTypeObjectGenerator SimpleObjectGenerator = new SimpleTypeObjectGenerator();

        /// <summary>
        /// Generates an object for a given type. The type needs to be public, have a public default constructor and settable public properties/fields. Currently it supports the following types:
        /// Simple types: <see cref="int"/>, <see cref="string"/>, <see cref="Enum"/>, <see cref="DateTime"/>, <see cref="Uri"/>, etc.
        /// Complex types: POCO types.
        /// Nullables: <see cref="Nullable{T}"/>.
        /// Arrays: arrays of simple types or complex types.
        /// Key value pairs: <see cref="KeyValuePair{TKey,TValue}"/>
        /// Tuples: <see cref="Tuple{T1}"/>, <see cref="Tuple{T1,T2}"/>, etc
        /// Dictionaries: <see cref="IDictionary{TKey,TValue}"/> or anything deriving from <see cref="IDictionary{TKey,TValue}"/>.
        /// Collections: <see cref="IList{T}"/>, <see cref="IEnumerable{T}"/>, <see cref="ICollection{T}"/>, <see cref="IList"/>, <see cref="IEnumerable"/>, <see cref="ICollection"/> or anything deriving from <see cref="ICollection{T}"/> or <see cref="IList"/>.
        /// Queryables: <see cref="IQueryable"/>, <see cref="IQueryable{T}"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>An object of the given type.</returns>
        public object GenerateObject(Type type)
        {
            return GenerateObject(type, new Dictionary<Type, object>());
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Here we just want to return null if anything goes wrong.")]
        private object GenerateObject(Type type, Dictionary<Type, object> createdObjectReferences)
        {
            try
            {
                if (SimpleTypeObjectGenerator.CanGenerateObject(type))
                {
                    return SimpleObjectGenerator.GenerateObject(type);
                }

                if (type.IsArray)
                {
                    return GenerateArray(type, DefaultCollectionSize, createdObjectReferences);
                }

                if (type.IsGenericType)
                {
                    return GenerateGenericType(type, DefaultCollectionSize, createdObjectReferences);
                }

                if (type == typeof(IDictionary))
                {
                    return GenerateDictionary(typeof(Hashtable), DefaultCollectionSize, createdObjectReferences);
                }

                if (typeof(IDictionary).IsAssignableFrom(type))
                {
                    return GenerateDictionary(type, DefaultCollectionSize, createdObjectReferences);
                }

                if (type == typeof(IList) ||
                    type == typeof(IEnumerable) ||
                    type == typeof(ICollection))
                {
                    return GenerateCollection(typeof(ArrayList), DefaultCollectionSize, createdObjectReferences);
                }

                if (typeof(IList).IsAssignableFrom(type))
                {
                    return GenerateCollection(type, DefaultCollectionSize, createdObjectReferences);
                }

                if (type == typeof(IQueryable))
                {
                    return GenerateQueryable(type, DefaultCollectionSize, createdObjectReferences);
                }

                if (type.IsEnum)
                {
                    return GenerateEnum(type);
                }

                if (type.IsPublic || type.IsNestedPublic)
                {
                    return GenerateComplexObject(type, createdObjectReferences);
                }
            }
            catch
            {
                // Returns null if anything fails
                return null;
            }

            return null;
        }

        private static object GenerateGenericType(Type type, int collectionSize, Dictionary<Type, object> createdObjectReferences)
        {
            Type genericTypeDefinition = type.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(Nullable<>))
            {
                return GenerateNullable(type, createdObjectReferences);
            }

            if (genericTypeDefinition == typeof(KeyValuePair<,>))
            {
                return GenerateKeyValuePair(type, createdObjectReferences);
            }

            if (IsTuple(genericTypeDefinition))
            {
                return GenerateTuple(type, createdObjectReferences);
            }

            Type[] genericArguments = type.GetGenericArguments();
            if (genericArguments.Length == 1)
            {
                if (genericTypeDefinition == typeof(IList<>) ||
                    genericTypeDefinition == typeof(IEnumerable<>) ||
                    genericTypeDefinition == typeof(ICollection<>))
                {
                    Type collectionType = typeof(List<>).MakeGenericType(genericArguments);
                    return GenerateCollection(collectionType, collectionSize, createdObjectReferences);
                }

                if (genericTypeDefinition == typeof(IQueryable<>))
                {
                    return GenerateQueryable(type, collectionSize, createdObjectReferences);
                }

                Type closedCollectionType = typeof(ICollection<>).MakeGenericType(genericArguments[0]);
                if (closedCollectionType.IsAssignableFrom(type))
                {
                    return GenerateCollection(type, collectionSize, createdObjectReferences);
                }
            }

            if (genericArguments.Length == 2)
            {
                if (genericTypeDefinition == typeof(IDictionary<,>))
                {
                    Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(genericArguments);
                    return GenerateDictionary(dictionaryType, collectionSize, createdObjectReferences);
                }

                Type closedDictionaryType = typeof(IDictionary<,>).MakeGenericType(genericArguments[0], genericArguments[1]);
                if (closedDictionaryType.IsAssignableFrom(type))
                {
                    return GenerateDictionary(type, collectionSize, createdObjectReferences);
                }
            }

            if (type.IsPublic || type.IsNestedPublic)
            {
                return GenerateComplexObject(type, createdObjectReferences);
            }

            return null;
        }

        private static object GenerateTuple(Type type, Dictionary<Type, object> createdObjectReferences)
        {
            Type[] genericArgs = type.GetGenericArguments();
            object[] parameterValues = new object[genericArgs.Length];
            bool failedToCreateTuple = true;
            ObjectGenerator objectGenerator = new ObjectGenerator();
            for (int i = 0; i < genericArgs.Length; i++)
            {
                parameterValues[i] = objectGenerator.GenerateObject(genericArgs[i], createdObjectReferences);
                failedToCreateTuple &= parameterValues[i] == null;
            }
            if (failedToCreateTuple)
            {
                return null;
            }
            object result = Activator.CreateInstance(type, parameterValues);
            return result;
        }

        private static bool IsTuple(Type genericTypeDefinition)
        {
            return genericTypeDefinition == typeof(Tuple<>) ||
                genericTypeDefinition == typeof(Tuple<,>) ||
                genericTypeDefinition == typeof(Tuple<,,>) ||
                genericTypeDefinition == typeof(Tuple<,,,>) ||
                genericTypeDefinition == typeof(Tuple<,,,,>) ||
                genericTypeDefinition == typeof(Tuple<,,,,,>) ||
                genericTypeDefinition == typeof(Tuple<,,,,,,>) ||
                genericTypeDefinition == typeof(Tuple<,,,,,,,>);
        }

        private static object GenerateKeyValuePair(Type keyValuePairType, Dictionary<Type, object> createdObjectReferences)
        {
            Type[] genericArgs = keyValuePairType.GetGenericArguments();
            Type typeK = genericArgs[0];
            Type typeV = genericArgs[1];
            ObjectGenerator objectGenerator = new ObjectGenerator();
            object keyObject = objectGenerator.GenerateObject(typeK, createdObjectReferences);
            object valueObject = objectGenerator.GenerateObject(typeV, createdObjectReferences);
            if (keyObject == null && valueObject == null)
            {
                // Failed to create key and values
                return null;
            }
            object result = Activator.CreateInstance(keyValuePairType, keyObject, valueObject);
            return result;
        }

        private static object GenerateArray(Type arrayType, int size, Dictionary<Type, object> createdObjectReferences)
        {
            Type type = arrayType.GetElementType();
            Array result = Array.CreateInstance(type, size);
            bool areAllElementsNull = true;
            ObjectGenerator objectGenerator = new ObjectGenerator();
            for (int i = 0; i < size; i++)
            {
                object element = objectGenerator.GenerateObject(type, createdObjectReferences);
                result.SetValue(element, i);
                areAllElementsNull &= element == null;
            }

            if (areAllElementsNull)
            {
                return null;
            }

            return result;
        }

        private static object GenerateDictionary(Type dictionaryType, int size, Dictionary<Type, object> createdObjectReferences)
        {
            Type typeK = typeof(object);
            Type typeV = typeof(object);
            if (dictionaryType.IsGenericType)
            {
                Type[] genericArgs = dictionaryType.GetGenericArguments();
                typeK = genericArgs[0];
                typeV = genericArgs[1];
            }

            object result = Activator.CreateInstance(dictionaryType);
            MethodInfo addMethod = dictionaryType.GetMethod("Add") ?? dictionaryType.GetMethod("TryAdd");
            MethodInfo containsMethod = dictionaryType.GetMethod("Contains") ?? dictionaryType.GetMethod("ContainsKey");
            ObjectGenerator objectGenerator = new ObjectGenerator();
            for (int i = 0; i < size; i++)
            {
                object newKey = objectGenerator.GenerateObject(typeK, createdObjectReferences);
                if (newKey == null)
                {
                    // Cannot generate a valid key
                    return null;
                }

                bool containsKey = (bool)containsMethod.Invoke(result, new object[] { newKey });
                if (!containsKey)
                {
                    object newValue = objectGenerator.GenerateObject(typeV, createdObjectReferences);
                    addMethod.Invoke(result, new object[] { newKey, newValue });
                }
            }

            return result;
        }

        private static object GenerateEnum(Type enumType)
        {
            Array possibleValues = Enum.GetValues(enumType);
            if (possibleValues.Length > 0)
            {
                return possibleValues.GetValue(0);
            }
            return null;
        }

        private static object GenerateQueryable(Type queryableType, int size, Dictionary<Type, object> createdObjectReferences)
        {
            bool isGeneric = queryableType.IsGenericType;
            object list;
            if (isGeneric)
            {
                Type listType = typeof(List<>).MakeGenericType(queryableType.GetGenericArguments());
                list = GenerateCollection(listType, size, createdObjectReferences);
            }
            else
            {
                list = GenerateArray(typeof(object[]), size, createdObjectReferences);
            }
            if (list == null)
            {
                return null;
            }
            if (isGeneric)
            {
                Type argumentType = typeof(IEnumerable<>).MakeGenericType(queryableType.GetGenericArguments());
                MethodInfo asQueryableMethod = typeof(Queryable).GetMethod("AsQueryable", new[] { argumentType });
                return asQueryableMethod.Invoke(null, new[] { list });
            }

            return Queryable.AsQueryable((IEnumerable)list);
        }

        private static object GenerateCollection(Type collectionType, int size, Dictionary<Type, object> createdObjectReferences)
        {
            Type type = collectionType.IsGenericType ?
                collectionType.GetGenericArguments()[0] :
                typeof(object);
            object result = Activator.CreateInstance(collectionType);
            MethodInfo addMethod = collectionType.GetMethod("Add");
            bool areAllElementsNull = true;
            ObjectGenerator objectGenerator = new ObjectGenerator();
            for (int i = 0; i < size; i++)
            {
                object element = objectGenerator.GenerateObject(type, createdObjectReferences);
                addMethod.Invoke(result, new object[] { element });
                areAllElementsNull &= element == null;
            }

            if (areAllElementsNull)
            {
                return null;
            }

            return result;
        }

        private static object GenerateNullable(Type nullableType, Dictionary<Type, object> createdObjectReferences)
        {
            Type type = nullableType.GetGenericArguments()[0];
            ObjectGenerator objectGenerator = new ObjectGenerator();
            return objectGenerator.GenerateObject(type, createdObjectReferences);
        }

        private static object GenerateComplexObject(Type type, Dictionary<Type, object> createdObjectReferences)
        {
            object result = null;

            if (createdObjectReferences.TryGetValue(type, out result))
            {
                // The object has been created already, just return it. This will handle the circular reference case.
                return result;
            }

            if (type.IsValueType)
            {
                result = Activator.CreateInstance(type);
            }
            else
            {
                ConstructorInfo defaultCtor = type.GetConstructor(Type.EmptyTypes);
                if (defaultCtor == null)
                {
                    // Cannot instantiate the type because it doesn't have a default constructor
                    return null;
                }

                result = defaultCtor.Invoke(new object[0]);
            }
            createdObjectReferences.Add(type, result);
            SetPublicProperties(type, result, createdObjectReferences);
            SetPublicFields(type, result, createdObjectReferences);
            return result;
        }

        private static void SetPublicProperties(Type type, object obj, Dictionary<Type, object> createdObjectReferences)
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            ObjectGenerator objectGenerator = new ObjectGenerator();
            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite)
                {
                    object propertyValue = objectGenerator.GenerateObject(property.PropertyType, createdObjectReferences);
                    property.SetValue(obj, propertyValue, null);
                }
            }
        }

        private static void SetPublicFields(Type type, object obj, Dictionary<Type, object> createdObjectReferences)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            ObjectGenerator objectGenerator = new ObjectGenerator();
            foreach (FieldInfo field in fields)
            {
                object fieldValue = objectGenerator.GenerateObject(field.FieldType, createdObjectReferences);
                field.SetValue(obj, fieldValue);
            }
        }

        private class SimpleTypeObjectGenerator
        {
            private long _index = 0;
            private static readonly Dictionary<Type, Func<long, object>> DefaultGenerators = InitializeGenerators();

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These are simple type factories and cannot be split up.")]
            private static Dictionary<Type, Func<long, object>> InitializeGenerators()
            {
                return new Dictionary<Type, Func<long, object>>
                {
                    { typeof(Boolean), index => true },
                    { typeof(Byte), index => (Byte)64 },
                    { typeof(Char), index => (Char)65 },
                    { typeof(DateTime), index => DateTime.Now },
                    { typeof(DateTimeOffset), index => new DateTimeOffset(DateTime.Now) },
                    { typeof(DBNull), index => DBNull.Value },
                    { typeof(Decimal), index => (Decimal)index },
                    { typeof(Double), index => (Double)(index + 0.1) },
                    { typeof(Guid), index => Guid.NewGuid() },
                    { typeof(Int16), index => (Int16)(index % Int16.MaxValue) },
                    { typeof(Int32), index => (Int32)(index % Int32.MaxValue) },
                    { typeof(Int64), index => (Int64)index },
                    { typeof(Object), index => new object() },
                    { typeof(SByte), index => (SByte)64 },
                    { typeof(Single), index => (Single)(index + 0.1) },
                    { 
                        typeof(String), index =>
                        {
                            return String.Format(CultureInfo.CurrentCulture, "sample string {0}", index);
                        }
                    },
                    { 
                        typeof(TimeSpan), index =>
                        {
                            return TimeSpan.FromTicks(1234567);
                        }
                    },
                    { typeof(UInt16), index => (UInt16)(index % UInt16.MaxValue) },
                    { typeof(UInt32), index => (UInt32)(index % UInt32.MaxValue) },
                    { typeof(UInt64), index => (UInt64)index },
                    { 
                        typeof(Uri), index =>
                        {
                            return new Uri(String.Format(CultureInfo.CurrentCulture, "http://webapihelppage{0}.com", index));
                        }
                    },
                };
            }

            public static bool CanGenerateObject(Type type)
            {
                return DefaultGenerators.ContainsKey(type);
            }

            public object GenerateObject(Type type)
            {
                return DefaultGenerators[type](++_index);
            }
        }
    }
}