using QuizWiz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuizWiz.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Authorize]
    public class SubmissionsController : Controller
    {
        private IContextFactory factory = new ContextFactory();

        /// <summary>
        /// 
        /// </summary>
        public SubmissionsController()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        public SubmissionsController(IContextFactory factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(int examId)
        {
            using (var db = this.factory.GetExamContext())
            {
                var submissions = (from s in db.Submissions
                                   where s.Exam.ExamId == examId
                                   select s).ToList();

                ViewBag.Submissions = submissions;
            }

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Show(int id)
        {
            using (var db = this.factory.GetExamContext())
            {
                var submission = (from s in db.Submissions.Include("Responses.Question")
                                  where s.SubmissionId == id
                                  select s).SingleOrDefault();

                ViewBag.Submission = submission;
            }

            return View();
        }
    }
}