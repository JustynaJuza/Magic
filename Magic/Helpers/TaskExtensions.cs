using System.Threading.Tasks;

namespace Magic.Helpers
{
    public static class TaskExtensions
    {
        public static readonly Task CompletedTask = Task.FromResult(false);
    }
}