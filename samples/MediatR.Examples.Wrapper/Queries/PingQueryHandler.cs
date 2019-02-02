using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Examples.Wrapper.Core;

namespace MediatR.Examples.Wrapper.Queries
{
    public class PingQueryHandler : QueryHandler<PingQuery, PongResponse>
    {
        private readonly TextWriter _writer;

        public PingQueryHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public override async Task<PongResponse> Handle(PingQuery query, CancellationToken cancellationToken)
        {
            await _writer.WriteLineAsync($"--- Handled PingQuery: {query.Message}");
            return new PongResponse {Message = query.Message + " PongResponse" };
        }
    }
}