using System;
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

        public ActionResult GetChatRoomPartial(string roomId = null, string[] recipientNames = null)
        {
            ChatRoomViewModel roomViewModel;
            if (string.IsNullOrEmpty(roomId))
            {
                var chatUsers = new List<ChatUserViewModel>();
                foreach (var userName in recipientNames.Distinct())
                {
                    chatUsers.Add(new ChatUserViewModel(context.Users.FirstOrDefault(u => u.UserName == userName)));
                }

                roomViewModel = new ChatRoomViewModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    Users = chatUsers
                };
            }
            else
            {
                var chatRoom = context.ChatRooms.Include(r => r.Connections.Select(c => c.User)).First(r => r.Id == roomId);
                var userId = User.Identity.GetUserId();
                roomViewModel = (ChatRoomViewModel)chatRoom.GetViewModel(userId);
            }

            var currentUserName = User.Identity.GetUserName();
            roomViewModel.Users = roomViewModel.Users.OrderBy(u => u.UserName == currentUserName).ToList();
            if (roomId != ChatHub.DefaultRoomId && roomViewModel.Users.Count == 1 && string.IsNullOrEmpty(roomViewModel.TabColorCode))
                //if (roomId != ChatHub.DefaultRoomId && roomViewModel.Users.Count <= 2 && string.IsNullOrEmpty(roomViewModel.TabColorCode))
            {
                roomViewModel.TabColorCode = roomViewModel.Users.First().ColorCode;
            }


            return PartialView("_ChatRoomPartial", roomViewModel);
        }

        //public ActionResult GetChatMessagePartial(string roomId = null, string[] recipientNames = null)
        //{
        //    if (string.IsNullOrEmpty(roomId))
        //    {
        //        var chatUsers = new List<ChatUserViewModel>();
        //        foreach (var userName in recipientNames.Distinct())
        //        {
        //            chatUsers.Add(new ChatUserViewModel(context.Users.FirstOrDefault(u => u.UserName == userName)));
        //        }

        //        return PartialView("_ChatRoomPartial", new ChatRoomViewModel()
        //        {
        //            Id = Guid.NewGuid().ToString(),
        //            Users = chatUsers
        //        });
        //    }

        //    var chatRoom = context.ChatRooms.Include(r => r.Connections.Select(c => c.User)).First(r => r.Id == roomId);
        //    var userId = User.Identity.GetUserId();
        //    return PartialView("_ChatRoomPartial", chatRoom.GetViewModel(userId));
        //}
    }
}