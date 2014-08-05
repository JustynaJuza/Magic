﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Magic.Models.DataContext;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using Magic.Models;
using Magic.Hubs;

namespace Magic.Controllers
{
    public class ChatController : Controller
    {
        private MagicDbContext context = new MagicDbContext();

        public ActionResult GetChatRoomPartial(string roomId = null, bool isPrivate = true, string[] recipientNames = null)
        {
            ChatRoomViewModel roomViewModel;
            if (!string.IsNullOrEmpty(roomId))
            {
                var chatRoom = context.ChatRooms.Include(r => r.Users.Select(c => c.User)).First(r => r.Id == roomId);
                if (!isPrivate)
                {
                    roomViewModel = (ChatRoomViewModel) chatRoom.GetViewModel();
                    return PartialView("_ChatRoomPartial", roomViewModel);
                }

                var userId = User.Identity.GetUserId();
                roomViewModel = (ChatRoomViewModel) chatRoom.GetViewModel(userId);

                foreach (var user in chatRoom.Users)
                {
                    ChatHub.AddConnectionsToRoomGroup(user.User.Connections.Select(c => c.Id).ToList(), chatRoom.Id);
                }
            }
            else
            {
                var chatUsers = recipientNames.Distinct().Select(userName => new ChatUserViewModel(context.Users.FirstOrDefault(u => u.UserName == userName))).ToList();

                roomViewModel = new ChatRoomViewModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    IsPrivate = true,
                    Users = chatUsers
                };
            }

            var currentUserName = User.Identity.GetUserName();
            roomViewModel.Users = roomViewModel.Users.OrderBy(u => u.UserName == currentUserName).ToList();

            if (roomViewModel.Users.Count == 1 && string.IsNullOrEmpty(roomViewModel.TabColorCode))
            {
                roomViewModel.TabColorCode = roomViewModel.Users.First().ColorCode;
            }

            return PartialView("_ChatRoomPartial", roomViewModel);
        }

        public ActionResult GetAvailableUsersPartial()
        {
            return PartialView("_AvailableUsersPartial");
        }

        public string AddOrRemoveFriend(string id)
        {
            var userId = User.Identity.GetUserId();
            var relation = context.UserRelations.Find(userId, id);

            if (relation != null)
            {
                context.Delete(relation, true);
                return "Add to friends";
            }

            relation = new UserRelationFriend()
                {
                    UserId = userId,
                    RelatedUserId = id
                };
            context.Insert(relation);
            return "Remove from friends";
        }
    }
}