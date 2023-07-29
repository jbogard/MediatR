using System.Runtime.CompilerServices;
using MediatR;

[assembly: InternalsVisibleTo("MediatR.Tests")]

[assembly: TypeForwardedTo(typeof(INotification))]
[assembly: TypeForwardedTo(typeof(IRequest))]
[assembly: TypeForwardedTo(typeof(IRequest<>))]
[assembly: TypeForwardedTo(typeof(IStreamRequest<>))]