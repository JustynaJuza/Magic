using System.Threading.Tasks;

namespace Magic.Models.Extensions
{
    public static class TaskExtensions<T>
    {
        //public static readonly Task CompletedTask = System.Threading.Tasks.Task.FromResult(false);
        
        public static Task<T> CompletedTask
        {
            get { return Task.FromResult(default(T)); }
        }
    }
}