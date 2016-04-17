using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Juza.Magic.Models;
using Juza.Magic.Models.DataContext;
using Juza.Magic.Models.Entities;
using Juza.Magic.Models.Entities.Chat;
using Microsoft.AspNet.Identity;

namespace Juza.Magic.Controllers
{
    public class ChatController : Controller
    {
        private readonly IDbContext _context;

        public ChatController(IDbContext context)
        {
            _context = context;
        }

        public ActionResult GetChatRoomPartial(string roomId = null, bool isPrivate = true, IEnumerable<string> recipientNames = null, bool createHidden = false)
        {
            ViewBag.CreateHidden = createHidden;
            ChatRoomViewModel roomViewModel = null;

            if (!string.IsNullOrEmpty(roomId))
            {
                var chatRoom = _context.Read<ChatRoom>().Include(x => x.Connections.Select(y => y.User)).FindOrFetchEntity(roomId); //context.ChatRooms.Include(r => r.Users.Select(c => c.User)).First(r => r.Id == roomId);
                if (!chatRoom.IsPrivate)
                {
                    roomViewModel = (ChatRoomViewModel)chatRoom.GetViewModel();
                    return PartialView("_ChatRoomPartial", roomViewModel);
                }

                var userId = User.Identity.GetUserId();
                roomViewModel = (ChatRoomViewModel)chatRoom.GetViewModel(userId);

                //ChatHub.SubscribeActiveConnections(roomId, userId);
            }
            else if (recipientNames != null)
            {
                var currentUserName = User.Identity.GetUserName();
                var users = _context.Set<User>().Where(x => recipientNames.Contains(x.UserName)).OrderBy(x => x.UserName == currentUserName);
                var chatUsers = users.Select(x => new ChatUserViewModel(x));

                roomViewModel = new ChatRoomViewModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    IsPrivate = true,
                    Users = chatUsers
                };
            }

            if (roomViewModel != null)
            {
                if (!roomViewModel.Users.Skip(1).Any() && string.IsNullOrEmpty(roomViewModel.TabColorCode))
                {
                    roomViewModel.TabColorCode = roomViewModel.Users.First().ColorCode;
                }

                return PartialView("_ChatRoomPartial", roomViewModel);
            }

            return Content("");
        }

        public ActionResult GetAvailableUsersPartial()
        {
            return PartialView("_AvailableUsersPartial");
        }

        public ActionResult GetUserProfileTooltipPartial(string userName)
        {
            var user = _context.Set<User>().First(u => u.UserName == userName);

            return PartialView("_UserProfileTooltipPartial", user.GetProfileViewModel());
        }

        //public ActionResult GetGameChatRoomPartial(string userName)
        //{
        //    var user = context.Users.First(u => u.UserName == userName);

        //    return PartialView("_UserProfileTooltipPartial", user.GetProfileViewModel());
        //}

        public string AddOrRemoveFriend(int id)
        {
            var userId = User.Identity.GetUserId<int>();
            var relation = _context.Read<UserRelation>().FindOrFetchEntity(userId, id);

            if (relation != null)
            {
                _context.Delete(relation, true);
                return "Add to friends";
            }

            relation = new UserRelationFriend()
            {
                UserId = userId,
                RelatedUserId = id
            };
            _context.Insert(relation);
            return "Remove from friends";
        }
    }
}