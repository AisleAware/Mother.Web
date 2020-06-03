using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Mother.Web.ViewModels
{
    public class UserRoleViewModel
    {
        public string RoleName { get; set; }
        public List<UserRoleDetails> Users { get; set; }

        public UserRoleViewModel()
        {
            // Parameterless constructor
        }

        public UserRoleViewModel(string roleName)
        {
            RoleName = roleName;
            Users = new List<UserRoleDetails>();
        }
    }

    public class UserRoleDetails
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool IsSelected { get; set; }
    }
}
