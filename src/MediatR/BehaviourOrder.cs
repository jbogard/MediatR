using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediatR
{
    public delegate void RegisterFunc(Type serviceType);

    public interface IBehaviorOrder
    {
        public void Register(Type type);
        public IReadOnlyList<IPipelineBehavior<TRequest, TResponse>> GetPipelineBehaviors<TRequest, TResponse>(ServiceFactory serviceFactory) where TRequest : notnull;
    }

    public class BehaviorOrder : IBehaviorOrder
    {
        private readonly RegisterFunc _registerFunc;

        internal BehaviorOrder(RegisterFunc registerFunc) => _registerFunc = registerFunc;

        private readonly List<Type> _registeredTypes = new List<Type>();

        public void Register(Type type)
        {
            if (!_registeredTypes.Contains(type))
            {
                _registeredTypes.Add(type);
            }

            _registerFunc(type);
        }

        public IReadOnlyList<IPipelineBehavior<TRequest, TResponse>> GetPipelineBehaviors<TRequest, TResponse>(ServiceFactory serviceFactory)
            where TRequest : notnull
        {
            var allMatchingTypes =
                (IEnumerable<IPipelineBehavior<TRequest, TResponse>>) serviceFactory(
                    typeof(IEnumerable<IPipelineBehavior<TRequest, TResponse>>));

            var constructedTypes = _registeredTypes
                .Select(t =>
                {
                    try
                    {
                        return t.MakeGenericType(typeof(TRequest), typeof(TResponse));
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                })
                .Where(t => t != null)
                .ToList();

            int OrderFunc(Type t)
            {
                var matchedType = constructedTypes.FirstOrDefault(x => x!.IsAssignableFrom(t));
                if (matchedType != null)
                {
                    return constructedTypes.IndexOf(matchedType);
                }

                return -1;
            }

            return allMatchingTypes
                .OrderBy(t => OrderFunc(t.GetType()))
                .ToList();
        }
    }

    public static class BehaviorOrderBuilder
    {
        public static BehaviorOrder Create(RegisterFunc func) => new BehaviorOrder(func);

        public static BehaviorOrder RegisterBehaviour(this BehaviorOrder behaviorOrder, Type t)
        {
            behaviorOrder.Register(t);
            return behaviorOrder;
        }
    }
}
