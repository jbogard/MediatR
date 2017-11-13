using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediatR.Examples
{
    public class Jing : IRequest
    {
        public string Message { get; set; }
    }
}
