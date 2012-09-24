using System;
using System.Linq;
using System.Web.Mvc;
using DevDay.Authentication;
using DevDay.Models;

namespace DevDay.Controllers
{
    public class DesafioController : Controller
    {
        private readonly DevdayEntities _db = new DevdayEntities();

        private readonly bool _dataLimiteSubmissaoExcedido =
            DateTime.Now.CompareTo(new DateTime(2012, 10, 10, 23, 59, 59)) > 0;
       
        [Authorize]
        public ActionResult Index()
        {
            var userID = ((CustomPrincipal)HttpContext.User).UserID;

            var submissions = _db.Submissions.Where(t => t.UserID == userID);

            var user = _db.Users.SingleOrDefault(t => t.ID == userID);

            if (user == null)
                return RedirectToAction("Authenticate", "Account");
            
            ViewBag.User = user;

            return View(submissions.ToList());
        }

        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Create(Submission submission)
        {
            if (_dataLimiteSubmissaoExcedido)
            {
                TempData["message"] = "Data limite de submissão excedido";
                return RedirectToAction("Index");
            }

            const int LIMITE_SUBMISSOES = 10;

            var userID = ((CustomPrincipal)HttpContext.User).UserID;
            
            if (_db.Submissions.Count(t => t.UserID == userID) >= LIMITE_SUBMISSOES)
            {
                ViewBag.Message = "Limite de 10 submissões atingido";
                return RedirectToAction("Index");
            }

            submission.UserID = userID;
            submission.CreatedOn = DateTime.Now;
            submission.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                _db.Submissions.Add(submission);
                _db.SaveChanges();

                return Request.Files.Count == 0 ? RedirectToAction("Index") : UploadFile(submission);
            }

            return View(submission);
        }

        private ActionResult UploadFile(Submission submission)
        {
            if (Request.Files.Count == 0)
                return RedirectToAction("Index");

            var postedFile = Request.Files[0];

            if (postedFile == null)
                return RedirectToAction("Index");

            if (postedFile.FileName.Length < 4)
                return RedirectToAction("Index");

            var extension = postedFile.FileName.Substring(postedFile.FileName.Length - 4, 4);

            if (!extension.Equals(".zip"))
                TempData["Message"] = "Formato do arquivo diferente de .zip";
            else if (postedFile.ContentLength / 1024 > 10240)
                TempData["Message"] = "Tamanho máximo permitido: 10Mb";
            else
            {
                var fileName = string.Concat(Guid.NewGuid(), ".zip");
                var filePath = string.Concat(Server.MapPath("~/Uploads/"), fileName);

                postedFile.SaveAs(filePath);

                submission.FilePath = fileName;
                submission.ModifiedOn = DateTime.Now;

                using (var partialdb = new DevdayEntities())
                {
                    partialdb.Submissions.Attach(submission);
                    partialdb.Entry(submission).Property(e => e.FilePath).IsModified = true;
                    partialdb.Entry(submission).Property(e => e.ModifiedOn).IsModified = true;
                    partialdb.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            var userID = ((CustomPrincipal)HttpContext.User).UserID;
            var submission = _db.Submissions.SingleOrDefault(t => t.UserID == userID && t.ID == id);

            return submission == null ? View("Index") : View(submission);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Edit(Submission submission)
        {
            if (_dataLimiteSubmissaoExcedido)
            {
                TempData["message"] = "Data limite de submissão excedido";
                return RedirectToAction("Index");
            }

            var userID = ((CustomPrincipal)HttpContext.User).UserID;
            var submissionCheck = _db.Submissions.FirstOrDefault(t => t.UserID == userID &&
                                                                      t.ID == submission.ID);
            if (submissionCheck == null)
                return RedirectToAction("Index");

            submission.UserID = userID;
            submission.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                using (var partialdb = new DevdayEntities())
                {
                    partialdb.Submissions.Attach(submission);
                    partialdb.Entry(submission).Property(e => e.Name).IsModified = true;
                    partialdb.Entry(submission).Property(e => e.Description).IsModified = true;
                    partialdb.Entry(submission).Property(e => e.ModifiedOn).IsModified = true;
                    partialdb.SaveChanges();
                }
                return Request.Files.Count == 0 ? RedirectToAction("Index") : UploadFile(submission);
            }

            return View(submission);
        }

        [Authorize]
        public ActionResult Delete(int id)
        {
            var userID = ((CustomPrincipal)HttpContext.User).UserID;
            var submission = _db.Submissions.SingleOrDefault(t => t.UserID == userID && t.ID == id);

            var user = _db.Users.SingleOrDefault(t => t.ID == userID);

            ViewBag.User = user;

            return submission == null ? View("Index") : View(submission);
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var userID = ((CustomPrincipal)HttpContext.User).UserID;
            var user = _db.Users.SingleOrDefault(t => t.ID == userID);
            ViewBag.User = user;

            if (_dataLimiteSubmissaoExcedido)
                return View("Index");

            var submission = _db.Submissions.SingleOrDefault(t => t.UserID == userID && t.ID == id);

            if (submission != null)
            {
                _db.Submissions.Remove(submission);
                _db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}