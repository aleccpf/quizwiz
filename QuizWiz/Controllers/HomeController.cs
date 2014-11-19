namespace QuizWiz.Controllers
{
    using System.Web.Mvc;

    /// <summary>
    /// 
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Ping()
        {
            return new EmptyResult();
        }
    }
}
