using Microsoft.AspNet.Identity.EntityFramework;

namespace Juza.Magic.Models.Entities
{
    public class UserRole : IdentityUserRole<int> {

        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
    }
}