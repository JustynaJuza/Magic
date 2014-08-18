using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Magic.Models.DataContext;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using Magic.Models;
using Magic.Hubs;

namespace Magic.Controllers
{
    public class ChatController : Controller
    {
        private MagicDbContext context = new MagicDbContext();

        public ActionResult GetChatRoomPartial(string roomId = null, bool isPrivate = true, string[] recipientNames = null, bool createHidden = false)
        {
            ViewBag.CreateHidden = createHidden;
            ChatRoomViewModel roomViewModel;

            if (!string.IsNullOrEmpty(roomId))
            {
                var chatRoom = context.ChatRooms.Find(roomId); //context.ChatRooms.Include(r => r.Users.Select(c => c.User)).First(r => r.Id == roomId);
                if (!chatRoom.IsPrivate)
                {
                    context.Entry(chatRoom).Collection(r => r.Connections).Query().Include(c => c.User).Load();
                    roomViewModel = (ChatRoomViewModel) chatRoom.GetViewModel();
                    return PartialView("_ChatRoomPartial", roomViewModel);
                }

                context.Entry(chatRoom).Collection(r => r.Users).Query().Include(u => u.User).Load();
                var userId = User.Identity.GetUserId();
                roomViewModel = (ChatRoomViewModel) chatRoom.GetViewModel(userId);

                ChatHub.SubscribeActiveConnections(roomId, userId);
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

        public ActionResult GetUserProfileTooltipPartial(string userName)
        {
            var user = context.Users.First(u => u.UserName == userName);

            return PartialView("_UserProfileTooltipPartial", user.GetProfileViewModel());
        }

        //public ActionResult GetGameChatRoomPartial(string userName)
        //{
        //    var user = context.Users.First(u => u.UserName == userName);

        //    return PartialView("_UserProfileTooltipPartial", user.GetProfileViewModel());
        //}

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