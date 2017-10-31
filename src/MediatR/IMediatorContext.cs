using System.Collections.Generic;
using System.Threading;

namespace MediatR
{
    /// <summary>
    /// Context
    /// </summary>
    public interface IMediatorContext
    {
        /// <summary>
        /// Cancellation Token
        /// </summary>
        CancellationToken CancellationToken { get; set; }

        /// <summary>
        ///  Gets or sets a key/value collection that can be used to share data within the scope of this request/notification.
        /// </summary>
        IDictionary<object, object> Items { get; set; }
    }
}