using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Examples.ExceptionHandler
{
    public class PingResourceHandler : IRequestHandler<PingResource, Pong>
    {
        private readonly TextWriter _writer;

        public PingResourceHandler(TextWriter writer) => _writer = writer;

        public Task<Pong> HandleAsync(PingResource request, CancellationToken cancellationToken)
        {
            throw new ResourceNotFoundException();
        }
    }

    public class PingNewResourceHandler : IRequestHandler<PingNewResource, Pong>
    {
        private readonly TextWriter _writer;

        public PingNewResourceHandler(TextWriter writer) => _writer = writer;

        public Task<Pong> HandleAsync(PingNewResource request, CancellationToken cancellationToken)
        {
            throw new ServerException();
        }
    }

    public class PingResourceTimeoutHandler : IRequestHandler<PingResourceTimeout, Pong>
    {
        private readonly TextWriter _writer;

        public PingResourceTimeoutHandler(TextWriter writer) => _writer = writer;

        public Task<Pong> HandleAsync(PingResourceTimeout request, CancellationToken cancellationToken)
        {
            throw new TaskCanceledException();
        }
    }

    public class PingResourceTimeoutOverrideHandler : IRequestHandler<Overrides.PingResourceTimeout, Pong>
    {
        private readonly TextWriter _writer;

        public PingResourceTimeoutOverrideHandler(TextWriter writer) => _writer = writer;

        public Task<Pong> HandleAsync(Overrides.PingResourceTimeout request, CancellationToken cancellationToken)
        {
            throw new TaskCanceledException();
        }
    }

    public class PingProtectedResourceHandler : IRequestHandler<PingProtectedResource, Pong>
    {
        private readonly TextWriter _writer;

        public PingProtectedResourceHandler(TextWriter writer) => _writer = writer;

        public Task<Pong> HandleAsync(PingProtectedResource request, CancellationToken cancellationToken)
        {
            throw new ForbiddenException();
        }
    }
}
