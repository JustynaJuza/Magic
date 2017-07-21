using Juza.Magic.Models;
using Juza.Magic.Models.DataContext;
using Juza.Magic.Models.Entities;
using Juza.Magic.Models.Entities.Chat;
using Juza.Magic.Models.Extensions;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

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
                var chatRoom = _context.Set<ChatRoom>()
                    .Include(x => x.Users)
                    .First(x => x.Id == roomId);

                if (!chatRoom.IsPrivate)
                {
                    roomViewModel = chatRoom.ToViewModel<ChatRoom, ChatRoomViewModel>();
                    return PartialView("_ChatRoomPartial", roomViewModel);
                }
            }
            else if (recipientNames != null)
            {
                var currentUserName = User.Identity.GetUserName();
                var users = _context.Set<User>()
                    .Where(x => recipientNames.Contains(x.UserName))
                    .OrderBy(x => x.UserName == currentUserName)
                    .Select(x => new ChatUserViewModel
                    {
                        Id = x.Id,
                        UserName = x.UserName,
                        ColorCode = x.ColorCode,
                        Status = x.Status
                    });

                roomViewModel = new ChatRoomViewModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    IsPrivate = true,
                    Users = users
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
            //this.RenderRazorViewToString();
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
            var relation = _context.Read<UserRelation>().Find(userId, id);

            if (relation != null)
            {
                _context.Delete(relation);
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

        public ChatRoomViewModel GetDefaultRoom()
        {
            var chatRoom = _context.Set<ChatRoom>()
                .Include(x => x.Users)
                .Include(x => x.Log.Messages)
                .First(x => x.Id == ChatRoom.DefaultRoomId)
                .ToViewModel<ChatRoom, ChatRoomViewModel>();

            return chatRoom; //new JsonResult { Data = chatRoom };
        }

        //public ActionResult GetUserChatRooms(bool exceptDefaultRoom = false)
        //{
        //    var userId = User.Identity.GetUserId<int>();

        //    var chatRooms = _context.Set<ChatRoomConnection>()
        //        .Where(x => x.UserId == userId)
        //        .Select(rc => rc.ChatRoom).Distinct()
        //        .Where(r => !r.IsGameRoom)
        //        .Project<ChatRoom, ChatRoomViewModel>();

        //    if (exceptDefaultRoom)
        //    {
        //        chatRooms = chatRooms.Where(x => x.Id == ChatRoom.DefaultRoomId);
        //    }

        //    return PartialView("_ChatRoomListPartial", chatRooms.ToList());
        //}
    }
}