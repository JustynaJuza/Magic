using System.Threading.Tasks;

namespace Magic.Models.Extensions
{
    public static class TaskExtensions
    {
        public static readonly Task CompletedTask = Task.FromResult(false);
    }
}