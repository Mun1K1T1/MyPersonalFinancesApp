using Azure;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinanceManager.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string? FirstName { get; set; }

        [PersonalData]
        public string? LastName { get; set; }

        [PersonalData]
        public string? Patronymic { get; set; }

        public string? AvatarPath { get; set; }

        public string? ProfilePurpose { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}