﻿using Juza.Magic.Models.Entities;
using Juza.Magic.Models.Enums;
using Juza.Magic.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Juza.Magic.Models
{
    public class ProfileViewModel : IViewModel<User>
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public UserStatus Status { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Image { get; set; }
        public string ColorCode { get; set; }
        //public virtual IList<CardDeck> DeckCollection { get; set; }
        //public virtual IList<Player> Games { get; set; }

        public bool IsFriend { get; set; }
        public bool IsCurrentUser { get; set; }

        public ProfileViewModel()
        {
            IsCurrentUser = false;
            IsFriend = false;
        }

        public ProfileViewModel(User user) : this()
        {
            Id = user.Id;
            UserName = user.UserName;
            Title = user.Title;
            Email = user.Email;
            BirthDate = user.BirthDate;
            Image = user.Image;
            ColorCode = user.ColorCode;
            Status = user.Status;
            LastLoginDate = user.LastLoginDate;
        }
    }

    public class UserViewModel : IViewModel<User>
    {
        public int Id { get; private set; }

        [StringLength(30, ErrorMessage = "Player name can only be between 3-30 characters long.", MinimumLength = 3)]
        [RegularExpression("^([a-zA-Z]+[a-zA-Z0-9]*(-|\\.|_)?[a-zA-Z0-9]+)$", ErrorMessage = "A Player name can only start with letters, may contain numbers and a few selected special characters.")]
        [Display(Name = "New Player")]
        public string UserName { get; private set; }

        public string Title { get; set; }

        [EmailAddress(ErrorMessage = "The email doesn't seem valid...")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The text entered is too long, consider using your initials instead.")]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        [StringLength(100, ErrorMessage = "The text entered is too long.")]
        public string LastName { get; set; }

        [BirthDateRange(ErrorMessage = "The date doesn't seem valid...")]
        [DataType(DataType.Date, ErrorMessage = "You need to enter a date in format similar to 31/12/2000.")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Your Birthday")]
        public DateTime? BirthDate { get; set; }

        [DataType(DataType.ImageUrl)]
        [Display(Name = "Your Plainswalker")]
        public string Image { get; set; }

        public string ColorCode { get; private set; }

        // Constructor.
        public UserViewModel() { }
        // Constructor.
        public UserViewModel(User user)
        {
            Id = user.Id;
            UserName = user.UserName;
            Title = user.Title;
            Email = user.Email;
            BirthDate = user.BirthDate;
            Image = user.Image;
            ColorCode = user.ColorCode;
        }
    }

    public class ChatUserViewModel : IViewModel<User>
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string ColorCode { get; set; }
        public UserStatus Status { get; set; }

        public ChatUserViewModel() { }

        public ChatUserViewModel(User user)
        {
            Id = user.Id;
            UserName = user.UserName;
            ColorCode = user.ColorCode;
            Status = user.Status;
        }
    }

    public class ChatUserViewModel_UserComparer : IEqualityComparer<ChatUserViewModel>
    {
        public bool Equals(ChatUserViewModel x, ChatUserViewModel y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(ChatUserViewModel obj)
        {
            return obj.Id.GetHashCode();
        }
    }

    public class ManagePasswordViewModel : IViewModel<User>
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "You must set a password to be able to log in.")]
        [StringLength(100, ErrorMessage = "The new password must be at least 6 characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
