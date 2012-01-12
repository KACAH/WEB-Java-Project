using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity;

namespace Web_Java_Project.Models
{
    public abstract class ProjectFile
    {
        [Key]
        public int FileID { get; set; }
        public string FileName { get; set; }

        public virtual Project Project { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MMM.yyyy HH:mm}")]
        public DateTime AddedDate { get; set; }
        public virtual WJPUser Adder { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MMM.yyyy HH:mm}")]
        public DateTime LastModifiedDate { get; set; }
        public virtual WJPUser Modifier { get; set; }

        public ProjectFile()
        {
            AddedDate = DateTime.Now;
            LastModifiedDate = DateTime.Now;
        }
    }

    public class SourceFile : ProjectFile
    {
        public SourceFile() 
            : base()
        {
        }
    }

    public class DataFile : ProjectFile
    {
        public DataFile() 
            : base()
        {
        }
    }

    public class LibraryFile : ProjectFile
    {
        public LibraryFile()
            : base()
        {
        }
    }

    public class Project
    {
        [Key]
        public int ProjectID { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 5, ErrorMessageResourceName = "ProjectNameLength", ErrorMessageResourceType = typeof(WJP_Resources.Lang))]
        [Display(Name = "ProjectName", ResourceType = typeof(WJP_Resources.Lang))]
        public string Name { get; set; }
        [Display(Name = "ProjectImage", ResourceType = typeof(WJP_Resources.Lang))]
        public string ImageURL { get; set; }
        [Display(Name = "ProjectClosed", ResourceType = typeof(WJP_Resources.Lang))]
        public bool Closed { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd.MMM.yyyy}")]
        public DateTime StartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MMM.yyyy HH:mm}")]
        public DateTime LastChangesDate { get; set; }
        public virtual WJPUser LastChanger { get; set; }

        public virtual WJPUser Owner { get; set; }
        public virtual ICollection<WJPUser> AllowedUsers { get; set; }

        public virtual ICollection<SourceFile> SourceFiles { get; set; }
        public virtual ICollection<LibraryFile> LibraryFiles { get; set; }
        public virtual ICollection<DataFile> DataFiles { get; set; }

        public Project()
        {
            this.Closed = false;
            this.LastChangesDate = DateTime.Now;
            this.StartDate = DateTime.Now;
        }
    }

    public class WJPUser
    {
        [Key]
        [Display(Name = "LogOnName", ResourceType = typeof(WJP_Resources.Lang))]
        public string UserName { get; set; } //Link with accounts database. User names are Unique
        public bool blocked { get; set; }

        public int SourcesAdded { get; set; }
        public int LibrariesAdded { get; set; }
        public int DataAdded { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd.MMM.yyyy HH:mm}")]
        public DateTime LastChangesDate { get; set; }

        public virtual Project LastProject { get; set; }

        public virtual ICollection<Project> MyProjects { get; set; }
        public virtual ICollection<Project> AllowedProjects { get; set; }

        public WJPUser()
        {
            this.LastProject = null;
            this.SourcesAdded = 0;
            this.LibrariesAdded = 0;
            this.DataAdded = 0;
            this.LastChangesDate = DateTime.Now;
            this.blocked = false;
        }
    }

    public class FindProjectModel
    {
        [Required]
        [StringLength(100, MinimumLength = 5, ErrorMessageResourceName = "ProjectNameLength", ErrorMessageResourceType = typeof(WJP_Resources.Lang))]
        [Display(Name = "ProjectName", ResourceType = typeof(WJP_Resources.Lang))]
        public string ProjectName { get; set; }
    }

    public class ProfileDBContext : DbContext
    {
        public DbSet<SourceFile> SourceFiles { get; set; }
        public DbSet<LibraryFile> LibraryFiles { get; set; }
        public DbSet<DataFile> DataFiles { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<WJPUser> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<WJPUser>().HasMany(c => c.AllowedProjects).WithMany(i => i.AllowedUsers)
                .Map(t => t.MapLeftKey("UserID").MapRightKey("ProjectID").ToTable("UserAllowedProjects"));

            modelBuilder.Entity<WJPUser>().HasOptional(c => c.LastProject).WithOptionalDependent();
            modelBuilder.Entity<WJPUser>().HasMany(c => c.MyProjects).WithOptional(i => i.Owner);

            modelBuilder.Entity<Project>().HasOptional(c => c.LastChanger).WithOptionalDependent();

            //modelBuilder.Entity<Project>().HasMany(c => c.SourceFiles).WithOptional(i => i.Project);
            //modelBuilder.Entity<Project>().HasMany(c => c.DataFiles).WithOptional(i => i.Project);
            //modelBuilder.Entity<Project>().HasMany(c => c.LibraryFiles).WithOptional(i => i.Project);
        }
    }

    public class DatabaseInitializer : DropCreateDatabaseIfModelChanges<ProfileDBContext>
    {
        protected override void Seed(ProfileDBContext context)
        {
            var students = new List<WJPUser>
            {
                new WJPUser { UserName = "KACAH" },
                new WJPUser { UserName = "TestUser1" },
                new WJPUser { UserName = "TestUser2" }
            };
            students.ForEach(s => context.Users.Add(s));
            context.SaveChanges();
        }
    }
}