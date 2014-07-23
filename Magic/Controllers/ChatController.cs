using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Magic.Models.DataContext;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using Magic.Models;

namespace Magic.Controllers
{
    public class ChatController : Controller
    {
        private MagicDbContext context = new MagicDbContext();

        public ActionResult GetChatRoomPartial(string roomId = null, string[] recipientNames = null)
        {
            if (string.IsNullOrEmpty(roomId))
            {
                var chatUsers = new List<ChatUserViewModel>();
                foreach (var userName in recipientNames.Distinct())
                {
                    chatUsers.Add(new ChatUserViewModel(context.Users.FirstOrDefault(u => u.UserName == userName)));
                }

                return PartialView("_ChatRoomPartial", new ChatRoomViewModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    Users = chatUsers
                });
            }

            var chatRoom = context.ChatRooms.Include(r => r.Connections.Select(c => c.User)).First(r => r.Id == roomId);
            var userId = User.Identity.GetUserId();
            return PartialView("_ChatRoomPartial", chatRoom.GetViewModel(userId));
        }

        public ActionResult GetChatMessagePartial(string roomId = null, string[] recipientNames = null)
        {
            if (string.IsNullOrEmpty(roomId))
            {
                var chatUsers = new List<ChatUserViewModel>();
                foreach (var userName in recipientNames.Distinct())
                {
                    chatUsers.Add(new ChatUserViewModel(context.Users.FirstOrDefault(u => u.UserName == userName)));
                }

                return PartialView("_ChatRoomPartial", new ChatRoomViewModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    Users = chatUsers
                });
            }

            var chatRoom = context.ChatRooms.Include(r => r.Connections.Select(c => c.User)).First(r => r.Id == roomId);
            var userId = User.Identity.GetUserId();
            return PartialView("_ChatRoomPartial", chatRoom.GetViewModel(userId));
        }
    }
}