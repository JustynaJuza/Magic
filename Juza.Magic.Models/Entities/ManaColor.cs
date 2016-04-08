using Juza.Magic.Models.Extensions;

namespace Juza.Magic.Models.Entities
{    
    public class ManaColor : AbstractExtensions
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
    }
}