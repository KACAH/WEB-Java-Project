using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Web_Java_Project.Models;

namespace Web_Java_Project.Controllers
{
    public class AccountController : BaseController
    {
        static ProfileDBContext profileDB = new ProfileDBContext();

        public static bool ValidateLogOn(LogOnModel model)
        {
            if (Membership.ValidateUser(model.UserName, model.Password) && !profileDB.Users.Find(model.UserName).blocked)
            {
                FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                printLog("User '" + model.UserName + "' logged in");
                return true;
            }

            return false;
        }

        //
        // GET: /Account/LogOn

        public ActionResult SingleLogOn()
        {
            return View();
        }

        //
        // POST: /Account/LogOn

        [HttpPost]
        public ActionResult SingleLogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if ( ValidateLogOn(model) )
                {
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", WJP_Resources.Lang.IncorrectLoginOrPass);
                }
            }

            // If we got this far, something failed, redisplay form
            printLog("User '" + model.UserName + "' login fail");
            return View(model);
        }


        //
        // GET: /Account/LogOff

        public ActionResult LogOff()
        {
            printLog("User '" + Membership.GetUser().UserName + "' logged out");
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus;
                Membership.CreateUser(model.UserName, model.Password, model.Email, "question", "answer", true, null, out createStatus);

                if (createStatus == MembershipCreateStatus.Success)
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, false /* createPersistentCookie */);
                    WJPUser newUser = new WJPUser();
                    newUser.UserName = model.UserName;
                    profileDB.Users.Add(newUser);
                    profileDB.SaveChanges();
                    printLog("User '" + model.UserName + "' registred");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePassword

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/ChangePassword

        [Authorize, HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    printLog("User '" + Membership.GetUser().UserName + "' changed his password");
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", WJP_Resources.Lang.ChangePasswordFail);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [Authorize, HttpPost]
        public ActionResult ChangeEmail(string email)
        {
            MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true);
            currentUser.Email = email;
            printLog("User '" + Membership.GetUser().UserName + "' changed his email");
            Membership.UpdateUser(currentUser);

            return View();
        }

        //
        // GET: /Account/ChangePasswordSuccess

        [Authorize]
        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult SetBlockUser(string userName, bool block, string returnUrl)
        {
            WJPUser curUser = profileDB.Users.Find(userName);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            curUser.blocked = block;
            profileDB.SaveChanges();
            printLog("User '" + Membership.GetUser().UserName + "' bloked");

            if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return WJP_Resources.Lang.DuplicateUserName;

                case MembershipCreateStatus.DuplicateEmail:
                    return WJP_Resources.Lang.DuplicateEmail;

                case MembershipCreateStatus.InvalidPassword:
                    return WJP_Resources.Lang.InvalidPassword;

                case MembershipCreateStatus.InvalidEmail:
                    return WJP_Resources.Lang.InvalidEmail;

                case MembershipCreateStatus.InvalidAnswer:
                    return WJP_Resources.Lang.InvalidAnswer;

                case MembershipCreateStatus.InvalidQuestion:
                    return WJP_Resources.Lang.InvalidQuestion;

                case MembershipCreateStatus.InvalidUserName:
                    return WJP_Resources.Lang.InvalidUserName;

                case MembershipCreateStatus.ProviderError:
                    return WJP_Resources.Lang.ProviderError;

                case MembershipCreateStatus.UserRejected:
                    return WJP_Resources.Lang.UserRejected;

                default:
                    return WJP_Resources.Lang.UnknownError;
            }
        }
        #endregion
    }
}
