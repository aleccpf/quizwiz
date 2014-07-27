﻿namespace QuizWiz.Controllers
{
    using QuizWiz.Filters;
    using QuizWiz.Models;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// 
    /// </summary>
    [InitializeExam]
    [Authorize]
    public class ExamsController : Controller
    {
        private IContextFactory factory = new ContextFactory();

        /// <summary>
        /// 
        /// </summary>
        public ExamsController()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        public ExamsController(IContextFactory factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit(long? id)
        {
            Exam exam = null;

            if (id.HasValue)
            {
                using (var db = this.factory.GetExamContext())
                {
                    exam = (from e in db.Exams.Include("Questions.Answers")
                            where e.ExamId == id.Value
                            select e).SingleOrDefault();
                }
            }

            if (exam == null)
            {
                exam = new Exam { Name = "", Questions = new List<Question>() };
            }

            return this.View(exam);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exam"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(Exam exam)
        {
            exam.UserId = this.User.Identity.Name;

            using (var db = this.factory.GetExamContext())
            {
                db.Exams.Add(exam);
                db.SaveChanges();
            }

            return new EmptyResult();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ActionResult Take(long id, string slug)
        {
            var model = new ExamSectionModel();

            using (ExamContext db = this.factory.GetExamContext())
            {
                var exam = (from e in db.Exams.Include("Questions.Answers")
                            where e.ExamId == id
                            select e).SingleOrDefault();

                if (exam == null || exam.Questions.Count == 0)
                {
                    return HttpNotFound("Exam not available.");
                }

                var submission = (from s in db.Submissions.Include("Exam")
                                      .Include("Responses.Question")
                                  where s.Exam.ExamId == id && s.UserId == this.User.Identity.Name
                                  select s).SingleOrDefault();

                var selectedQuestion = exam.Questions[0];

                if (submission != null)
                {
                    if (submission.Elapsed > TimeSpan.FromMinutes(exam.Duration) || submission.Finished != null)
                    {
                        return this.RedirectToAction("Finished", "Exams");
                    }

                    submission.Elapsed += TimeSpan.FromSeconds(90);
                    var response = submission.Responses.LastOrDefault();

                    if (response != null)
                    {
                        selectedQuestion = (from q in exam.Questions
                                            where q.OrderIndex > response.Question.OrderIndex
                                            orderby q.OrderIndex
                                            select q).FirstOrDefault() ?? selectedQuestion;
                    }
                }
                else
                {
                    submission = new Submission
                        {
                            Started = DateTime.UtcNow,
                            Elapsed = new TimeSpan(0, 0, 0),
                            Heartbeat = DateTime.UtcNow,
                            Exam = exam,
                            UserId = this.User.Identity.Name,
                            Responses = new List<Response>()
                        };

                    db.Submissions.Add(submission);
                }

                db.SaveChanges();

                model.Submission = submission;
                model.Question = selectedQuestion;
                model.Name = exam.Name;
                model.Exam = exam;
            }

            return this.View(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Finished()
        {
            return this.View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult SubmitResponse(int QuestionId, int? AnswerId, int SubmissionId, string value)
        {
            Question nextQuestion = null;

            using (ExamContext db = this.factory.GetExamContext())
            {
                var submission = (from s in db.Submissions.Include("Responses.Question").Include("Exam")
                                  where s.SubmissionId == SubmissionId
                                  select s).FirstOrDefault();

                var question = db.Questions.Find(QuestionId);

                if (submission.Elapsed > TimeSpan.FromMinutes(20))
                {
                    db.SaveChanges();

                    return new HttpStatusCodeResult(300);
                }

                if (AnswerId.HasValue)
                {
                    Response response = null;

                    response = (from r in submission.Responses
                                where r.Question.QuestionId == question.QuestionId
                                select r).FirstOrDefault();

                    if (response == null)
                    {
                        response = new Response
                        {
                            Answer = db.Answers.Find(AnswerId),
                            Question = question,
                            Value = value
                        };

                        db.Responses.Add(response);
                        submission.Responses.Add(response);
                    }
                    else
                    {
                        response.Answer = db.Answers.Find(AnswerId);
                        response.Value = value;
                    }
                }

                var exam = (from e in db.Exams.Include("Questions.Answers")
                            where e.Name == submission.Exam.Name
                            select e).FirstOrDefault();

                nextQuestion = (from q in exam.Questions
                                where q.OrderIndex > question.OrderIndex
                                orderby q.OrderIndex
                                select q).FirstOrDefault();

                if (question.OrderIndex == exam.Questions.Count)
                {
                    submission.Finished = DateTime.UtcNow;
                }

                db.SaveChanges();
            }

            if (nextQuestion != null)
            {
                return this.Json(new { HasNext = true, OrderIndex = nextQuestion.OrderIndex });
            }
            else
            {
                return this.Json(new { HasNext = false });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns></returns>
        public ActionResult GetQuestion(int orderIndex, int submissionId)
        {
            var model = new ExamQuestionModel();

            using (ExamContext db = this.factory.GetExamContext())
            {
                var submission = (from s in db.Submissions.Include("Exam")
                                  where s.SubmissionId == submissionId
                                  select s).FirstOrDefault();

                if (submission != null && submission.Elapsed > TimeSpan.FromMinutes(20))
                {
                    return new HttpStatusCodeResult(300);
                }

                var exam = (from e in db.Exams.Include("Questions.Answers")
                            where e.Name == submission.Exam.Name
                            select e).SingleOrDefault();

                model.Question = (from q in exam.Questions
                                  where q.OrderIndex == orderIndex
                                  select q).SingleOrDefault();
            }

            return this.Json(model, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Heartbeat(int submissionId)
        {
            using (ExamContext db = this.factory.GetExamContext())
            {
                var submission = (from s in db.Submissions
                                  where s.SubmissionId == submissionId
                                  select s).SingleOrDefault();

                if (submission != null)
                {
                    TimeSpan delta = TimeSpan.FromSeconds(5);
                    submission.Elapsed += delta;

                    db.SaveChanges();
                }
            }

            return new EmptyResult();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult ShowMe()
        {
            return this.View("Index");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Me(int offset, int limit)
        {
            var exams = new List<Exam>();

            using (var db = this.factory.GetExamContext())
            {
                exams = (from e in db.Exams
                         where e.UserId == this.User.Identity.Name
                         select e).OrderBy(a => a.ExamId).Skip(offset).Take(limit).ToList();
            }

            return this.Json(exams, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Search(string q)
        {
            var exams = new List<Exam>();

            using (var db = this.factory.GetExamContext())
            {
                exams = (from e in db.Exams
                         where e.Name.Contains(q)
                         select e).Take(20).ToList();
            }

            return this.Json(exams, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadTest(string name, HttpPostedFileBase submission)
        {
            if (submission != null)
            {
                if (submission.ContentLength > 1000000)
                {
                    ModelState.AddModelError("submission", "The size of the file should not exceed 10 KB");

                    return this.View();
                }

                var supportedTypes = new[] { "zip", "7z", "rar", "gz" };
                var fileExt = Path.GetExtension(submission.FileName).Substring(1);

                if (!supportedTypes.Contains(fileExt))
                {
                    ModelState.AddModelError("submission", "Invalid type. Only the following types (zip, 7z, rar, gz) are supported.");

                    return this.View();
                }

                submission.SaveAs(Path.Combine(ConfigurationManager.AppSettings["FilesRoot"],
                     string.Format("{0}_{1}_{2}", name, this.User.Identity.Name, submission.FileName)));
            }

            return this.RedirectToAction("Index", "Home");
        }
    }
}
