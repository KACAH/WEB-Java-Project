using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace Web_Java_Project.Models
{

    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "ChangePassCurr", ResourceType = typeof(WJP_Resources.Lang))]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 7, ErrorMessageResourceName = "ChangePassLength", ErrorMessageResourceType = typeof(WJP_Resources.Lang))]
        [DataType(DataType.Password)]
        [Display(Name = "ChangePassNewPass", ResourceType = typeof(WJP_Resources.Lang))]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ChangePassConf", ResourceType = typeof(WJP_Resources.Lang))]
        [Compare("NewPassword", ErrorMessageResourceName = "ChangePassCompFail", ErrorMessageResourceType = typeof(WJP_Resources.Lang))]
        public string ConfirmPassword { get; set; }
    }

    public class LogOnModel
    {
        [Required]
        [Display(Name = "LogOnName", ResourceType = typeof(WJP_Resources.Lang))]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "LogOnPass", ResourceType = typeof(WJP_Resources.Lang))]
        public string Password { get; set; }

        [Display(Name = "LogOnRemember", ResourceType = typeof(WJP_Resources.Lang))]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessageResourceName = "UserNameLength", ErrorMessageResourceType = typeof(WJP_Resources.Lang))]
        [Display(Name = "LogOnName", ResourceType = typeof(WJP_Resources.Lang))]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "RegEmail", ResourceType = typeof(WJP_Resources.Lang))]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 7, ErrorMessageResourceName = "ChangePassLength", ErrorMessageResourceType = typeof(WJP_Resources.Lang))]
        [DataType(DataType.Password)]
        [Display(Name = "LogOnPass", ResourceType = typeof(WJP_Resources.Lang))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ChangePassConf", ResourceType = typeof(WJP_Resources.Lang))]
        [Compare("Password", ErrorMessageResourceName = "ChangePassCompFail", ErrorMessageResourceType = typeof(WJP_Resources.Lang))]
        public string ConfirmPassword { get; set; }
    }

    public class FindUserModel
    {
        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessageResourceName = "UserNameLength", ErrorMessageResourceType = typeof(WJP_Resources.Lang))]
        [Display(Name = "LogOnName", ResourceType = typeof(WJP_Resources.Lang))]
        public string UserName { get; set; }
    }
}
