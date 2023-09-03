using System;
using System.Runtime.CompilerServices;

namespace MediatR.Internal
{
    internal static class ObjectExtensions
    {
        public static void ThrowIfNull(this object obj, [CallerArgumentExpression("obj")] string name = "")
        {
            if (obj is null)
                throw new ArgumentNullException(name);
        }
    }
}

#if !NET6_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class CallerArgumentExpressionAttribute : Attribute
    {
        public CallerArgumentExpressionAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }

        public string ParameterName { get; }
    }
}
#endif
