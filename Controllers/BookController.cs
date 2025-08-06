using Microsoft.AspNetCore.Mvc;

namespace BookDragon.Controllers
{
    public class BookController : Controller
    {
        // GET: BookController
        public ActionResult Index()
        {
            return View();
        }

    }
}
