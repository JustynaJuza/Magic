using System.Threading.Tasks;

namespace Juza.Magic.Models.Extensions
{
    public static class TaskExtensions
    {
        public static readonly Task CompletedTask = System.Threading.Tasks.Task.FromResult(false);
        
        //public static Task<T> CompletedGenericTask<T>
        //{
        //    get { return Task.FromResult(default(T)); }
        //}
        //public static Task CompletedTask
        //{
        //    get { return Task.FromResult(default(int)); }
        //}
    }
}