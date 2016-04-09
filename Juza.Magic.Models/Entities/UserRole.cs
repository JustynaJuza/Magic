using Juza.Magic.Models.Entities;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Penna.Assessment.Models.Entities
{
    public class UserRole : IdentityUserRole<int> {

        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
    }
}