using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Magic.Models
{
    public class CardSet
    {
        private IEnumerable<int> cardIds;
 
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Block { get; set; }
        public string Description { get; set; }
        public DateTime DateReleased { get; set; }
        public IList<Card> Cards { get; set; }

        public CardSet()
        {
            Cards = new List<Card>();
        }

        public CardSet(JObject jObject)
        {
            Id = jObject.Value<string>("id");
            Name = jObject.Value<string>("name");
            Type = jObject.Value<string>("type");
            Block = jObject.Value<string>("block");
            Description = jObject.Value<string>("description");
            DateReleased = jObject.Value<DateTime>("releasedAt");

            cardIds = jObject.Values<int>("cardIds");
        }
    }
}
