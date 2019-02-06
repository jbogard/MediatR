using System.Threading.Tasks;

namespace MediatR.Extension {
    public class GetResponse<TResponse> {
        public GetResponse(IMediator mediator) {
            this.mediator = mediator;
        }

        public Task<TResponse> By<TRequest>(TRequest request) where TRequest : IRequest<TResponse> {
            return mediator.Send<TRequest, TResponse>(request);
        }

        private readonly IMediator mediator;
    }

    public class MediatorEnvelope {

    }

    public static class MediatorExtension {
        public static GetResponse<TResponse> Get<TResponse>(this IMediator genericMediator) {
            return new GetResponse<TResponse>(genericMediator);
        }
        
    }


}
