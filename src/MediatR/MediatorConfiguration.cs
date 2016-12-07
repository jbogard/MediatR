using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediatR
{
    public enum PublishAsyncOptions
    {
        Parallel,
        Sequential
    }

    public class MediatorConfiguration
    {
        public PublishAsyncOptions PublishAsyncOptions { get; set; } = PublishAsyncOptions.Parallel;
    }
}
