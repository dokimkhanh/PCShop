using PCShop.Models.EF;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PCShop.Models
{
    public class RoleGroupItemViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int PermissionCount { get; set; }
        public int TotalPermissions { get; set; }
        public int UserCount { get; set; }
    }

    public class RoleManagementViewModel
    {
        public string ActiveTab { get; set; }
        public List<RoleGroupItemViewModel> Roles { get; set; }
        public List<Permission> Permissions { get; set; }
    }

    public class RoleFormViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên nhóm quyền.")]
        [StringLength(100)]
        public string Name { get; set; }

        public List<int> SelectedPermissionIds { get; set; }
        public List<Permission> AllPermissions { get; set; }
    }

    public class AdminUserListItemViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
    }
}
