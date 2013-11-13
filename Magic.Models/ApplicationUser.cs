﻿using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Magic.Models
{
    public class ApplicationUser : IdentityUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime DateCreated { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string Title { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }
        [DataType(DataType.ImageUrl)]
        public string Image { get; set; }
        public string ColorCode { get; set; }

        // Constructor.
        public ApplicationUser()
        {
            assignRandomColorCode();
        }

        public override string ToString()
        {
            string toString = this.GetType().FullName + ": ";
            var classMembers = this.GetType().GetProperties();

            foreach (System.Reflection.PropertyInfo member in classMembers)
                toString += "\n" + member.Name + " : " + member.GetValue(this) + "; ";

            return toString;
        }

        #region HELPERS
        public void assignRandomColorCode()
        {
            Random random = new Random();
            int red = random.Next(255); // Not 256, because black is the system message color.
            int green = random.Next(255);
            int blue = random.Next(255);
            System.Drawing.Color color = System.Drawing.Color.FromArgb(red, green, blue);

            this.ColorCode = System.Drawing.ColorTranslator.ToHtml(color);
        }

        public ManageUserDetailsViewModel getViewModel()
        {
            return new ManageUserDetailsViewModel(this);
        }
        #endregion HELPERS
    }
    // IdentityDbContext included in DataContext namespace => see MagicDBContext.
}