using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Magic.Models.Repositories
{
    public interface IUserRepository
    {
        IEnumerable<User> GetAll();
        User GetByUserName(User user);

        void CreateUser(User user);
        void UpdateUser(User user);
        void DeleteUser(User user);
    }

    public class UserRepository : IUserRepository
    {
        private List<User> repository = new List<User>();

        public Exception ExceptionToThrow { get; set; }

        public User GetByUserName(User user)
        {
            return repository.FirstOrDefault(d => d.UserName == user.UserName);
        }

        public void CreateUser(User user) {
            if (ExceptionToThrow != null)
                throw ExceptionToThrow;

            repository.Add(user);
        }

        public IEnumerable<User> GetAll() {
            return repository;
        }

        public void UpdateUser(User user)
        {
            foreach (User appUser in repository)
            {
                if (appUser.Id == user.Id)
                {
                    repository.Remove(appUser);
                    repository.Add(user);
                    break;
                }
            }
        }

        public void DeleteUser(User user)
        {
            repository.Remove(GetByUserName(user));
        }
    }
}