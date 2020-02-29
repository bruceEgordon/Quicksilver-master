using EPiServer.Tracking.Commerce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Demos.Controllers
{
    public class DemosController : Controller
    {
        [CommerceTracking(TrackingType.Category)]
        public ActionResult Index()
        {
            return View();
        }
    }
}