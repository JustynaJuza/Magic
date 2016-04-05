using Microsoft.AspNet.SignalR;
using Magic.Models.DataContext;

namespace Magic.Hubs
{
    public class ChatConnectionManager
    {
        private readonly IDbContext _context;

        public ChatConnectionManager(IDbContext context)
        {
            _context = context;
        }
        
    }
}