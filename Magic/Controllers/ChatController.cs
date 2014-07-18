using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Magic.Models.DataContext;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using Magic.Hubs;

namespace Magic.Controllers
{
    public class ChatController : Controller
    {
        private MagicDbContext context = new MagicDbContext();

        public ActionResult GetChatRoomPartial(string roomId = ChatHub.DefaultRoomId)
        {
                var chatRoom = context.ChatRooms.Include(r => r.Connections.Select(c => c.User)).First(r => r.Id == roomId);
                var userId = User.Identity.GetUserId();

            return PartialView("_ChatRoomPartial", chatRoom.GetViewModel(userId));
        }
	}
}