using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediatR.Examples
{
    public class JingHandler : IRequestHandler<Jing>
    {
        private readonly TextWriter _writer;

        public JingHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public void Handle(Jing message)
        {
            _writer.WriteLine($"--- Handled Jing: {message.Message}, no Jong");
        }
    }
}
