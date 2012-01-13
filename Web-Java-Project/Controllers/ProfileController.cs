using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Mvc;
using Web_Java_Project.Models;
using System.Web.Security;
using System.IO;

namespace Web_Java_Project.Controllers
{
    public class ProfileController : BaseController
    {
        ProfileDBContext profileDB = new ProfileDBContext();
        //
        // GET: /Profile/

        private void checkRoles()
        {
            if (Roles.GetRolesForUser(User.Identity.Name).Contains("Administrator"))
            {
                ViewBag.Mode = "God";
            } else
                ViewBag.Mode = "Normal";
        }

        [Authorize]
        public ActionResult Projects()
        {
            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            checkRoles();

            return View(curUser);
        }

        [Authorize]
        public ActionResult CreateProject()
        {
            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            checkRoles();

            return View();
        }

        [Authorize, HttpPost]
        public ActionResult CreateProject(Project project)
        {
            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            checkRoles();

            bool alreadyHas = false;
            foreach (Project pr in profileDB.Projects)
            {
                if (pr.Name.Trim().ToUpper() == project.Name.Trim().ToUpper())
                    alreadyHas = true;
            }
            if (alreadyHas)
            {
                return RedirectToAction("Error", "Home", new { message = "Our system already have project with this name" });
            }

            project.Owner = curUser;
            project.LastChanger = curUser;

            if (project.ImageURL == null || project.ImageURL.Trim() == "")
                project.ImageURL = "/content/Images/no-pre.png";

            if (ModelState.IsValid)
            {
                curUser.LastProject = project;
                curUser.LastChangesDate = DateTime.Now;
                profileDB.Entry(curUser).State = EntityState.Modified;
                profileDB.Projects.Add(project);
                curUser.MyProjects.Add(project);
                profileDB.SaveChanges();
                Directory.CreateDirectory(Server.MapPath("~/Projects/") + project.ProjectID);
                Directory.CreateDirectory(Server.MapPath("~/Projects/") + project.ProjectID + "/Source");
                Directory.CreateDirectory(Server.MapPath("~/Projects/") + project.ProjectID + "/Library");
                Directory.CreateDirectory(Server.MapPath("~/Projects/") + project.ProjectID + "/Data");
                Directory.CreateDirectory(Server.MapPath("~/Projects/") + project.ProjectID + "/Classes");
                printLog("New project '" + project.Name + "' created by user '" + curUser.UserName + "'");
                return RedirectToAction("Projects");
            }

            return View(project);
        }

        [Authorize]
        public ActionResult Statistics()
        {
            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            checkRoles();

            return View(curUser);
        }

        [Authorize]
        public ActionResult Settings()
        {
            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            checkRoles();

            ViewBag.CurEmail = Membership.GetUser().Email;
            return View(curUser);
        }

        [Authorize]
        public ActionResult FindUsers()
        {
            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            checkRoles();

            ViewBag.FoundList = new List<WJPUser>();
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult FindUsers(FindUserModel find)
        {
            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            checkRoles();

            find.UserName = find.UserName.Trim().ToUpper();
            if (find.UserName.Count() < 3)
                return View(find);

            List<WJPUser> res = new List<WJPUser>();
            foreach (WJPUser user in profileDB.Users)
            {
                if (user.UserName.ToUpper().Contains(find.UserName) && user != curUser)
                {
                    res.Add(user);
                }
            }
            ViewBag.FoundList = res;

            return View(find);
        }

        [Authorize]
        public ActionResult Info(string userName)
        {
            WJPUser user = profileDB.Users.Find(userName);
            if (user == null)
                return RedirectToAction("Error", "Home", new { message = "User not found!" });

            checkRoles();

            return View(user);
        }

        [Authorize]
        public ActionResult AddToProjectList(string userName)
        {
            WJPUser userMe = profileDB.Users.Find(User.Identity.Name);
            if (userMe == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            checkRoles();

            WJPUser userTo = profileDB.Users.Find(userName);
            if (userTo == null)
                return RedirectToAction("Error", "Home", new { message = "User not found!" });

            ViewBag.UserTo = userTo;

            return View(userMe);
        }

        [Authorize]
        public ActionResult AddToProject(string userName, int projectID)
        {
            WJPUser userMe = profileDB.Users.Find(User.Identity.Name);
            if (userMe == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            checkRoles();

            WJPUser userTo = profileDB.Users.Find(userName);
            if (userTo == null)
                return RedirectToAction("Error", "Home", new { message = "User not found!" });

            if (userTo == userMe)
                return RedirectToAction("Error", "Home", new { message = "Trying to add self into own project oO" });

            Project proj = profileDB.Projects.Find(projectID);
            if (proj == null)
                return RedirectToAction("Error", "Home", new { message = "Project not found" });

            if ( !userTo.AllowedProjects.Contains(proj) )
            {
                userTo.AllowedProjects.Add(proj);
                proj.AllowedUsers.Add(userTo);
                profileDB.SaveChanges();
                printLog("User '" + userTo.UserName + "' added to project '" + proj.Name + "'");
            }

            return RedirectToAction("Projects");
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult FindProjects()
        {
            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            checkRoles();

            ViewBag.FoundList = new List<Project>();
            return View();
        }

        [Authorize(Roles = "Administrator"), HttpPost]
        public ActionResult FindProjects(FindProjectModel find)
        {
            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            checkRoles();

            find.ProjectName = find.ProjectName.Trim().ToUpper();
            if (find.ProjectName.Count() < 3)
                return View(find);

            List<Project> res = new List<Project>();
            foreach (Project project in profileDB.Projects)
            {
                if (project.Name.ToUpper().Contains(find.ProjectName))
                {
                    res.Add(project);
                }
            }
            ViewBag.FoundList = res;

            return View(find);
        }
    }
}
