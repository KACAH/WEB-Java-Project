using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.IO;
using Web_Java_Project.Models;
using System.Diagnostics;
using ICSharpCode.SharpZipLib.Zip;
using System.Net.Mail;

namespace Web_Java_Project.Controllers
{
    public class ProjectController : BaseController
    {
        ProfileDBContext profileDB = new ProfileDBContext();

        bool checkRole(WJPUser user, Project proj)
        {
            if (Roles.GetRolesForUser(User.Identity.Name).Contains("Administrator"))
            {
                ViewBag.Mode = "God";
                return true;
            }

            if (proj.Owner == user)
            {
                ViewBag.Mode = "Owner";
                return true;
            }

            if (user.AllowedProjects.Contains(proj))
            {
                ViewBag.Mode = "User";
                return true;
            }
            return false;
        }

        [Authorize]
        public ActionResult General(int ID)
        {
            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            Project proj = profileDB.Projects.Find(ID);
            if (proj == null)
                return RedirectToAction("Error", "Home", new { message = "Projects database error!" });
            
            if ( !checkRole(curUser, proj) )
                return RedirectToAction("Error", "Home", new { message = "Project access violation!" });

            return View(proj);
        }

        [Authorize, HttpPost]
        public ActionResult General(int ID, Project project)
        {
            Project proj = profileDB.Projects.Find(ID);
                if (proj == null)
                    return RedirectToAction("Error", "Home", new { message = "Projects database error!" });

            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
                if (curUser == null)
                    return RedirectToAction("Error", "Home", new { message = "User database error!" });

            if (!checkRole(curUser, proj))
                return RedirectToAction("Error", "Home", new { message = "Project access violation!" });

            proj.ImageURL = project.ImageURL;
            proj.Closed = project.Closed;
            proj.LastChanger = curUser;
            proj.LastChangesDate = DateTime.Now;
            curUser.LastProject = proj;
            curUser.LastChangesDate = DateTime.Now;
            profileDB.Entry(curUser).State = EntityState.Modified;
            profileDB.Entry(proj).State = EntityState.Modified;
            profileDB.SaveChanges();
            printLog("Project '" + project.Name + "' settings were changed by user '" + curUser.UserName + "'");

            return View(proj);
        }

        private void addFile(Project proj, WJPUser curUser, HttpPostedFileBase file, string FileType)
        {
            string needExt;
            if (FileType == "Source")
                needExt = ".java";
            else
            if (FileType == "Library")
                needExt = ".jar";
            else
                needExt = "";

            if (file != null && file.ContentLength > 0)
            {
                if (needExt == "" || Path.GetExtension(file.FileName) == needExt)
                {
                    string fileName = Path.GetFileName(file.FileName);
                    bool found = false;
                    if (FileType == "Source")
                        foreach (SourceFile f in proj.SourceFiles)
                        {
                            if (f.FileName == fileName) { found = true; break; }
                        }
                    else
                    if (FileType == "Library")
                        foreach (LibraryFile f in proj.LibraryFiles)
                        {
                            if (f.FileName == fileName) { found = true; break; }
                        }
                    else
                        foreach (DataFile f in proj.DataFiles)
                        {
                            if (f.FileName == fileName) { found = true; break; }
                        }

                    if (!found)
                    {
                        string path = Path.Combine(
                            Server.MapPath("~/Projects/") + proj.ProjectID + "/" + FileType, fileName);
                        file.SaveAs(path);

                        ProjectFile f; 
                        if (FileType == "Source")
                            f = new SourceFile();
                        else
                        if (FileType == "Library")
                            f = new LibraryFile();
                        else
                            f = new DataFile();

                        f.Adder = curUser;
                        f.Modifier = curUser;
                        f.FileName = fileName;
                        f.Project = proj;

                        curUser.LastChangesDate = DateTime.Now;
                        curUser.LastProject = proj;

                        if (FileType == "Source")
                        {
                            curUser.SourcesAdded++;
                            proj.SourceFiles.Add( (SourceFile)f );
                            profileDB.SourceFiles.Add( (SourceFile)f );
                        } else

                        if (FileType == "Library")
                        {
                            curUser.LibrariesAdded++;
                            proj.LibraryFiles.Add((LibraryFile)f);
                            profileDB.LibraryFiles.Add((LibraryFile)f);
                        }
                        else
                        {
                            curUser.DataAdded++;
                            proj.DataFiles.Add((DataFile)f);
                            profileDB.DataFiles.Add((DataFile)f);
                        }

                        proj.LastChangesDate = DateTime.Now;
                        proj.LastChanger = curUser;
                        

                        profileDB.Entry(curUser).State = EntityState.Modified;
                        profileDB.Entry(proj).State = EntityState.Modified;
                        profileDB.SaveChanges();
                    }
                    else
                        ViewBag.FileTypeError = "File with such name already exists";
                }
                else
                    ViewBag.FileTypeError = "Uploaded file isn't " + FileType + " file";
            }
        }

        [Authorize]
        public ActionResult Source(int ID)
        {
            Project proj = profileDB.Projects.Find(ID);
            if (proj == null)
                return RedirectToAction("Error", "Home", new { message = "Projects database error!" });

            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            if (!checkRole(curUser, proj))
                return RedirectToAction("Error", "Home", new { message = "Project access violation!" });

            if (proj.Closed)
                return RedirectToAction("Error", "Home", new { message = "Project is closed by owner or Administrator!" });

            return View(proj);
        }

        [Authorize, HttpPost]
        public ActionResult Source(int ID, HttpPostedFileBase file)
        {
            Project proj = profileDB.Projects.Find(ID);
            if (proj == null)
                return RedirectToAction("Error", "Home", new { message = "Projects database error!" });

            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            if (!checkRole(curUser, proj))
                return RedirectToAction("Error", "Home", new { message = "Project access violation!" });

            if (proj.Closed)
                return RedirectToAction("Error", "Home", new { message = "Project is closed by owner or Administrator!" });

            addFile(proj, curUser, file, "Source");
            printLog("Source file '" + file.FileName + 
                    "' added into project '" + proj.Name + "' by user '" + curUser.UserName + "'");
            return View(proj);
        }

        [Authorize]
        public ActionResult DeleteSource(int ID, int fileID)
        {
            Project proj = profileDB.Projects.Find(ID);
            if (proj == null)
                return RedirectToAction("Error", "Home", new { message = "Projects database error!" });

            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            if (!checkRole(curUser, proj))
                return RedirectToAction("Error", "Home", new { message = "Project access violation!" });

            if (proj.Closed)
                return RedirectToAction("Error", "Home", new { message = "Project is closed by owner or Administrator!" });

            SourceFile file = profileDB.SourceFiles.Find(fileID);
            if (file == null || file.Project.ProjectID != proj.ProjectID)
                return RedirectToAction("Error", "Home", new { message = "Source files database error!" });

            proj.SourceFiles.Remove(file);
            proj.LastChangesDate = DateTime.Now;
            proj.LastChanger = curUser;
            curUser.LastChangesDate = DateTime.Now;
            curUser.LastProject = proj;
            file.Adder.SourcesAdded--;
            profileDB.Entry(curUser).State = EntityState.Modified;
            profileDB.Entry(file.Adder).State = EntityState.Modified;
            profileDB.Entry(proj).State = EntityState.Modified;
            System.IO.File.Delete(Server.MapPath("~/Projects/") + proj.ProjectID + "/Source/" + file.FileName);
            profileDB.SourceFiles.Remove(file);
            profileDB.SaveChanges();
            printLog("Source file '" + file.FileName +
                    "' deleted from project '" + proj.Name + "' by user '" + curUser.UserName + "'");

            return RedirectToAction("Source", new { ID = ID });
        }

        [Authorize]
        public ActionResult Library(int ID)
        {
            Project proj = profileDB.Projects.Find(ID);
            if (proj == null)
                return RedirectToAction("Error", "Home", new { message = "Projects database error!" });

            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            if (!checkRole(curUser, proj))
                return RedirectToAction("Error", "Home", new { message = "Project access violation!" });

            if (proj.Closed)
                return RedirectToAction("Error", "Home", new { message = "Project is closed by owner or Administrator!" });

            return View(proj);
        }

        [Authorize, HttpPost]
        public ActionResult Library(int ID, HttpPostedFileBase file)
        {
            Project proj = profileDB.Projects.Find(ID);
            if (proj == null)
                return RedirectToAction("Error", "Home", new { message = "Projects database error!" });

            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            if (!checkRole(curUser, proj))
                return RedirectToAction("Error", "Home", new { message = "Project access violation!" });

            if (proj.Closed)
                return RedirectToAction("Error", "Home", new { message = "Project is closed by owner or Administrator!" });

            addFile(proj, curUser, file, "Library");
            printLog("Library file '" + file.FileName +
                    "' added into project '" + proj.Name + "' by user '" + curUser.UserName + "'");
            return View(proj);
        }

        [Authorize]
        public ActionResult DeleteLibrary(int ID, int fileID)
        {
            Project proj = profileDB.Projects.Find(ID);
            if (proj == null)
                return RedirectToAction("Error", "Home", new { message = "Projects database error!" });

            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            if (!checkRole(curUser, proj))
                return RedirectToAction("Error", "Home", new { message = "Project access violation!" });

            if (proj.Closed)
                return RedirectToAction("Error", "Home", new { message = "Project is closed by owner or Administrator!" });

            LibraryFile file = profileDB.LibraryFiles.Find(fileID);
            if (file == null || file.Project.ProjectID != proj.ProjectID)
                return RedirectToAction("Error", "Home", new { message = "Source files database error!" });

            proj.LibraryFiles.Remove(file);
            proj.LastChangesDate = DateTime.Now;
            proj.LastChanger = curUser;
            curUser.LastChangesDate = DateTime.Now;
            curUser.LastProject = proj;
            file.Adder.LibrariesAdded--;
            profileDB.Entry(curUser).State = EntityState.Modified;
            profileDB.Entry(file.Adder).State = EntityState.Modified;
            profileDB.Entry(proj).State = EntityState.Modified;
            System.IO.File.Delete(Server.MapPath("~/Projects/") + proj.ProjectID + "/Library/" + file.FileName);
            profileDB.LibraryFiles.Remove(file);
            profileDB.SaveChanges();
            printLog("Library file '" + file.FileName +
                    "' deleted from project '" + proj.Name + "' by user '" + curUser.UserName + "'");

            return RedirectToAction("Library", new { ID = ID });
        }

        [Authorize]
        public ActionResult Data(int ID)
        {
            Project proj = profileDB.Projects.Find(ID);
            if (proj == null)
                return RedirectToAction("Error", "Home", new { message = "Projects database error!" });

            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            if (!checkRole(curUser, proj))
                return RedirectToAction("Error", "Home", new { message = "Project access violation!" });

            if (proj.Closed)
                return RedirectToAction("Error", "Home", new { message = "Project is closed by owner or Administrator!" });

            return View(proj);
        }

        [Authorize, HttpPost]
        public ActionResult Data(int ID, HttpPostedFileBase file)
        {
            Project proj = profileDB.Projects.Find(ID);
            if (proj == null)
                return RedirectToAction("Error", "Home", new { message = "Projects database error!" });

            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            if (!checkRole(curUser, proj))
                return RedirectToAction("Error", "Home", new { message = "Project access violation!" });

            if (proj.Closed)
                return RedirectToAction("Error", "Home", new { message = "Project is closed by owner or Administrator!" });

            addFile(proj, curUser, file, "Data");
            printLog("Data file '" + file.FileName +
                    "' added into project '" + proj.Name + "' by user '" + curUser.UserName + "'");
            return View(proj);
        }

        [Authorize]
        public ActionResult DeleteData(int ID, int fileID)
        {
            Project proj = profileDB.Projects.Find(ID);
            if (proj == null)
                return RedirectToAction("Error", "Home", new { message = "Projects database error!" });

            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            if (!checkRole(curUser, proj))
                return RedirectToAction("Error", "Home", new { message = "Project access violation!" });

            if (proj.Closed)
                return RedirectToAction("Error", "Home", new { message = "Project is closed by owner or Administrator!" });

            DataFile file = profileDB.DataFiles.Find(fileID);
            if (file == null || file.Project.ProjectID != proj.ProjectID)
                return RedirectToAction("Error", "Home", new { message = "Source files database error!" });

            proj.DataFiles.Remove(file);
            proj.LastChangesDate = DateTime.Now;
            proj.LastChanger = curUser;
            curUser.LastChangesDate = DateTime.Now;
            curUser.LastProject = proj;
            file.Adder.DataAdded--;
            profileDB.Entry(curUser).State = EntityState.Modified;
            profileDB.Entry(file.Adder).State = EntityState.Modified;
            profileDB.Entry(proj).State = EntityState.Modified;
            System.IO.File.Delete(Server.MapPath("~/Projects/") + proj.ProjectID + "/Data/" + file.FileName);
            profileDB.DataFiles.Remove(file);
            profileDB.SaveChanges();
            printLog("Data file '" + file.FileName +
                    "' deleted from project '" + proj.Name + "' by user '" + curUser.UserName + "'");

            return RedirectToAction("Data", new { ID = ID });
        }

        [Authorize]
        public ActionResult Download(int ID, string fileName)
        {
            Project proj = profileDB.Projects.Find(ID);
            if (proj == null)
                return RedirectToAction("Error", "Home", new { message = "Projects database error!" });

            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            if (!checkRole(curUser, proj))
                return RedirectToAction("Error", "Home", new { message = "Project access violation!" });

            if (fileName.Contains(".."))
                return RedirectToAction("Error", "Home", new { message = "Injection alert!" });

            FileStream fileStream = new FileStream(Server.MapPath("~/Projects/") + proj.ProjectID + fileName, FileMode.Open);
            return File(fileStream, "application/octet-stream", Path.GetFileName(fileName));
        }

        [Authorize]
        public ActionResult EditCode(int ID, string fileName)
        {
            Project proj = profileDB.Projects.Find(ID);
            if (proj == null)
                return RedirectToAction("Error", "Home", new { message = "Projects database error!" });

            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            if (!checkRole(curUser, proj))
                return RedirectToAction("Error", "Home", new { message = "Project access violation!" });

            if (proj.Closed)
                return RedirectToAction("Error", "Home", new { message = "Project is closed by owner or Administrator!" });

            StreamReader fileStream = new StreamReader(Server.MapPath("~/Projects/") + proj.ProjectID + "/Source/" + fileName);
            ViewBag.code = fileStream.ReadToEnd();
            fileStream.Close();
            ViewBag.FileName = fileName;

            return View(proj);
        }

        [Authorize, HttpPost, ValidateInput(false)]
        public ActionResult EditCode(int ID, string fileName, string Code)
        {
            Project proj = profileDB.Projects.Find(ID);
            if (proj == null)
                return RedirectToAction("Error", "Home", new { message = "Projects database error!" });

            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            if (!checkRole(curUser, proj))
                return RedirectToAction("Error", "Home", new { message = "Project access violation!" });

            SourceFile file = null;
            foreach (SourceFile f in proj.SourceFiles)
                if (f.FileName == fileName)
                {
                    file = f;
                    break;
                }

            if (file == null)
                return RedirectToAction("Error", "Home", new { message = "Source file database error!" });

            StreamWriter fileStream = new StreamWriter(Server.MapPath("~/Projects/") + proj.ProjectID + "/Source/" + fileName);
            fileStream.Write(Code);
            fileStream.Close();

            ViewBag.code = Code;
            ViewBag.FileName = fileName;

            proj.LastChangesDate = DateTime.Now;
            proj.LastChanger = curUser;
            curUser.LastChangesDate = DateTime.Now;
            curUser.LastProject = proj;
            file.LastModifiedDate = DateTime.Now;
            file.Modifier = curUser;
            profileDB.Entry(file).State = EntityState.Modified;
            profileDB.Entry(curUser).State = EntityState.Modified;
            profileDB.Entry(proj).State = EntityState.Modified;
            profileDB.SaveChanges();
            printLog("Source file '" + file.FileName +
                    "' editted in project '" + proj.Name + "' by user '" + curUser.UserName + "'");

            return View(proj);
        }

        [Authorize]
        public ActionResult Compile(int ID)
        {
            Project proj = profileDB.Projects.Find(ID);
            if (proj == null)
                return RedirectToAction("Error", "Home", new { message = "Projects database error!" });

            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            if (!checkRole(curUser, proj))
                return RedirectToAction("Error", "Home", new { message = "Project access violation!" });

            if (proj.Closed)
                return RedirectToAction("Error", "Home", new { message = "Project is closed by owner or Administrator!" });

            return View(proj);
        }

        private void Empty(System.IO.DirectoryInfo directory)
        {
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
        }

        private bool checkJavaLog(string fileName)
        {
            string last = "";
            StreamReader fileStream = new StreamReader(fileName);
            while (!fileStream.EndOfStream)
            {
                string cur = fileStream.ReadLine();
                if (cur != "")
                    last = cur;
            }
            fileStream.Close();

            if ( last.Contains("error") )
                return false;

            return true;
        }

        [Authorize, HttpPost]
        public ActionResult CompileResult(int ID, string version, string deprecation, string warnings)
        {
            Project proj = profileDB.Projects.Find(ID);
            if (proj == null)
                return RedirectToAction("Error", "Home", new { message = "Projects database error!" });

            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            if (!checkRole(curUser, proj))
                return RedirectToAction("Error", "Home", new { message = "Project access violation!" });

            string pathToCompiller;
            if (version == "J7")
            {
                pathToCompiller = "D:/Programing/Libraries And Engines/JDK7/bin/";
            } else
                return RedirectToAction("Error", "Home", new { message = "Unknown java compiller version!" });

            string options = " ";
            if (deprecation == "Deprecation")
            {
                options += "-deprecation ";
            }
            if (warnings != "Warnings")
            {
                options += "-nowarn ";
            }

            string pathToProject = Server.MapPath("~/Projects/") + proj.ProjectID + "/";
            options += "-sourcepath \"" + pathToProject + "Source/\" ";
            options += "-classpath \"" + pathToProject + "Classes/\"";
            string[] fileEntries = Directory.GetFiles(pathToProject + "Library/");
            foreach (string fileName in fileEntries)
            {
                options += ";\"" + fileName + "\"";
            }
            options += " ";
            options += "-d \"" + pathToProject + "Classes/\" ";
            options += "-verbose ";
            options += "-Xstdout \"" + pathToProject + "log.txt\" ";

            string sourceFiles = "\"" + pathToProject + "Source/*.java\"";

            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(pathToProject + "Classes/");
            Empty(directory);
            directory = new System.IO.DirectoryInfo(pathToProject);
            Empty(directory);

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = true;
            startInfo.FileName = pathToCompiller + "javac";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = options + sourceFiles;

            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch
            {
                return RedirectToAction("Error", "Home", new { message = "Java compiller error!" });
            }

            StreamReader fileStream = new StreamReader(pathToProject + "log.txt");
            ViewBag.message = fileStream.ReadToEnd();
            fileStream.Close();

            FastZip zipFile = new FastZip();
            zipFile.CreateEmptyDirectories = false;
            zipFile.CreateZip(pathToProject + proj.Name + ".zip", pathToProject + "Classes", true, string.Empty);

            if (checkJavaLog(pathToProject + "log.txt"))
            {
                ViewBag.Succseed = true;
            } else
                ViewBag.Succseed = false;

            ViewBag.DefEmail = Membership.GetUser().Email;

            printLog("Project '" + proj.Name + "' compiled by user '" + curUser.UserName + "'");
            return View(proj);
        }

        [Authorize, HttpPost]
        public ActionResult SendMail(int ID, string email)
        {
            Project proj = profileDB.Projects.Find(ID);
            if (proj == null)
                return RedirectToAction("Error", "Home", new { message = "Projects database error!" });

            WJPUser curUser = profileDB.Users.Find(User.Identity.Name);
            if (curUser == null)
                return RedirectToAction("Error", "Home", new { message = "User database error!" });

            if (!checkRole(curUser, proj))
                return RedirectToAction("Error", "Home", new { message = "Project access violation!" });
            
            if (proj.Closed)
                return RedirectToAction("Error", "Home", new { message = "Project is closed by owner or Administrator!" });

            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            message.Body = "Do not reply on this message!";
            message.IsBodyHtml = false;
            message.To.Add(email);
            message.From = new System.Net.Mail.MailAddress("web.java.project@gmail.com");
            message.Subject = proj.Name + " COMPILED CODE";
            string fileAttach = Server.MapPath("~\\Projects\\") + proj.ProjectID + "\\" + proj.Name + ".zip";
            Attachment attach = new Attachment(fileAttach);
            message.Attachments.Add(attach);
        
            System.Net.Mail.SmtpClient  s = new System.Net.Mail.SmtpClient();
            s.Timeout = 5000;
            s.SendAsync(message, proj);
            printLog("Email send to address '" + email + "' by user '" + curUser.UserName + "'");

            return View(proj);
        }
    }
}
