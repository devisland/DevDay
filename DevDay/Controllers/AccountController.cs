using System;
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
    public class AccountController : Controller
    {
        private readonly DevdayEntities _db = new DevdayEntities();

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

                user.LastLoggedOn = DateTime.Now;
                _db.SaveChanges();

                return RedirectToAction("Index", "Desafio");
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
        [HttpPost]
        public ActionResult Update(User user)
        {
            var userID = ((CustomPrincipal)HttpContext.User).UserID;
            var loggedUser = _db.Users.SingleOrDefault(t => t.ID == userID);

            if (loggedUser == null)
            {
                ViewBag.Message = "Credenciais inválidas";
                return View("Authenticate");
            }

            loggedUser.IsCompetitor = user.IsCompetitor;
            _db.SaveChanges();

            TempData["message"] = "Dados pessoais atualizados com sucesso.";

            return RedirectToAction("Index", "Desafio");
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
