﻿using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;

namespace Magic.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime DateCreated { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }
        [DataType(DataType.ImageUrl)]
        public string UserImage { get; set; }

        public override string ToString()
        {
            string toString = this.GetType().FullName + ": ";
            var classMembers = this.GetType().GetProperties();

            foreach (System.Reflection.PropertyInfo member in classMembers)
                toString += "\n" + member.Name + " : " + member.GetValue(this) + "; ";

            return toString;
        }
    }
    // IdentityDbContext included in DataContext namespace => see MagicDBContext.
}