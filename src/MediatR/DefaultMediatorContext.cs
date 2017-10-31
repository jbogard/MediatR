using System.Collections.Generic;
using System.Threading;

namespace MediatR
{
    /// <summary>
    /// Default Context
    /// </summary>
    public class DefaultMediatorContext
        : IMediatorContext
    {
        public CancellationToken CancellationToken { get; set; } = default(CancellationToken);
        public IDictionary<object, object> Items { get; set; }
    }
}