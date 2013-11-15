using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models.Repositories
{
    public interface IUserRepository
    {
        IEnumerable<ApplicationUser> GetAll();
        ApplicationUser GetByUserName(ApplicationUser user);

        void CreateUser(ApplicationUser user);
        void UpdateUser(ApplicationUser user);
        void DeleteUser(ApplicationUser user);
    }

    public class UserRepository : IUserRepository
    {
        private List<ApplicationUser> repository = new List<ApplicationUser>();

        public Exception ExceptionToThrow { get; set; }

        public ApplicationUser GetByUserName(ApplicationUser user)
        {
            return repository.FirstOrDefault(d => d.UserName == user.UserName);
        }

        public void CreateUser(ApplicationUser user) {
            if (ExceptionToThrow != null)
                throw ExceptionToThrow;

            repository.Add(user);
        }

        public IEnumerable<ApplicationUser> GetAll() {
            return repository;
        }

        public void UpdateUser(ApplicationUser user)
        {
            foreach (ApplicationUser appUser in repository)
            {
                if (appUser.Id == user.Id)
                {
                    repository.Remove(appUser);
                    repository.Add(user);
                    break;
                }
            }
        }

        public void DeleteUser(ApplicationUser user)
        {
            repository.Remove(GetByUserName(user));
        }
    }
}