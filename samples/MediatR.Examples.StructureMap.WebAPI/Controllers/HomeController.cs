using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MediatR.Examples.StructureMap.WebAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMediator _mediator;

        public HomeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public ActionResult Index()
        {
            var result = _mediator.Send(new Ping());

            ViewBag.Title = result.Message;

            return View();
        }
    }
}
