using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MediatR.Examples.StructureMap.WebAPI.Controllers
{
    public class ValuesController : ApiController
    {
        private readonly IMediator _mediator;

        public ValuesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET api/values
        public IEnumerable<string> Get()
        {
            var result = _mediator.Send(new Ping());

            return new string[] { result.Message };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
