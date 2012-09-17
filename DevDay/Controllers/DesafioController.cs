using System;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DevDay.Authentication;
using DevDay.Helpers;
using DevDay.Models;
using DevDay.Models.VM;

namespace DevDay.Controllers
{
    public class DesafioController : Controller
    {
        private readonly DevdayEntities _db = new DevdayEntities();

        private readonly bool _dataLimiteSubmissaoExcedido =
            DateTime.Now.CompareTo(new DateTime(2012, 10, 1, 23, 59, 59)) > 0;

        public ActionResult Authenticate()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Authenticate(Credential credential)
        {
            if (ModelState.IsValid)
            {
                var user = _db.Users.FirstOrDefault
                    (t => t.Email == credential.Email &&
                          t.Password == credential.Password);

                if (user == null)
                {
                    ViewBag.Message = "Credenciais inválidas";
                    return View();
                }

                SaveCookie(user);

                return RedirectToAction("Index");
            }

            ViewBag.Message = "Credenciais inválidas";
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return View("Authenticate");
        }

        [Authorize]
        public ViewResult Index()
        {
            var userID = ((CustomPrincipal)HttpContext.User).UserID;

            var submissions = _db.Submissions.Where(t => t.UserID == userID);

            var user = _db.Users.SingleOrDefault(t => t.ID == userID);

            if (user == null)
                return View("Authenticate");

            ViewBag.Username = user.Name;

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
                return View("Index");

            const int LIMITE_SUBMISSOES = 10;

            var userID = ((CustomPrincipal)HttpContext.User).UserID;

            if (_db.Submissions.Count(t => t.UserID == userID) >= LIMITE_SUBMISSOES)
            {
                ViewBag.Message = "Limite de 10 submissões atingido";
                return View("Index");
            }

            submission.UserID = userID;
            submission.CreatedOn = DateTime.Now;
            submission.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                _db.Submissions.Add(submission);
                _db.SaveChanges();

                return Request.Files.Count == 0 ? View("Index") : UploadFile(submission);
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
                return View("Index");

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
                return Request.Files.Count == 0 ? View("Index") : UploadFile(submission);
            }

            return View(submission);
        }

        [Authorize]
        public ActionResult Delete(int id)
        {
            var userID = ((CustomPrincipal)HttpContext.User).UserID;
            var submission = _db.Submissions.SingleOrDefault(t => t.UserID == userID && t.ID == id);

            return submission == null ? View("Index") : View(submission);
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (_dataLimiteSubmissaoExcedido)
                return View("Index");

            var userID = ((CustomPrincipal)HttpContext.User).UserID;
            var submission = _db.Submissions.SingleOrDefault(t => t.UserID == userID && t.ID == id);

            if (submission != null)
            {
                _db.Submissions.Remove(submission);
                _db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult SendPassword(FormCollection form)
        {
            var email = form["email"] != null ? form["email"].Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(email))
                return RedirectToAction("Authenticate");

            var user = _db.Users.SingleOrDefault(t => t.Email.Equals(email));

            if (user == null)
            {
                ViewBag.Message =
                    "Seu e-mail não consta em nossos registros. Entre em contato conosco pelo e-mail devday@devisland.com.";
                return View("Authenticate");
            }

            var emailEnviado = MailHelper.Send(user.Email, user.Password);

            ViewBag.Message = emailEnviado
                                  ? string.Format("Senha enviada para {0}. Confira sua caixa de entrada.", user.Email)
                                  : "Ocorreu um problema ao tentar enviar o e-mail. Entre em contato conosco pelo e-mail devday@devisland.com";

            return View("Authenticate");
        }

        private void SaveCookie(User user)
        {
            var ticket = new FormsAuthenticationTicket(1, user.Email,
                DateTime.Now, DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes),
                true, user.ID.ToString(CultureInfo.InvariantCulture));

            var hashedTicket = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, hashedTicket);

            HttpContext.Response.Cookies.Add(cookie);
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
    }
}