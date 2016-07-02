using Juza.Magic.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Juza.Magic.Models.Entities
{
    public class CardTypeFactory
    {
        public static IList<CardType> AssignTypes(IList<string> typeNames)
        {
            var types = new List<CardType>();

            //types.AddRange(!isSubtype
            //    ? typeNames.Select( typeName => IsSuperType(typeName) ? new CardSuperType { Name = typeName } : new CardType { Name = typeName })
            //    : typeNames.Select( typeName => IsSuperType(typeName) ? new CardSuperType { Name = typeName } : new CardType { Name = typeName }));

            //types.AddRange(typeNames.Select(typeName => new CardType
            //{
            //    Name = typeName,
            //    Type = IsSuperType(typeName) ? TypeCategory.SuperType : (IsMainType(typeName) ? TypeCategory.MainType : TypeCategory.Subtype)
            //}));

            foreach (var typeName in typeNames)
            {
                if (IsSuperType(typeName))
                {
                    types.Add(new CardSuperType { Name = typeName });
                }
                else if (IsMainType(typeName))
                {
                    types.Add(new CardMainType { Name = typeName });
                }
                else
                {
                    types.Add(new CardSubType { Name = typeName });
                }
            }

            //if (!isSubtype)
            //{
            //    types.AddRange(typeNames.Select(typeName => IsSuperType(typeName) ? new CardSuperType { Type = (SuperType) Enum.Parse(typeof(SuperType),typeName) } : new CardMainType { Type = (MainType) Enum.Parse(typeof(MainType),typeName) }));
            //}
            //else if ()
            //{
            //    types
            //    types.AddRange(typeNames.Select(typeName => IsSuperType(typeName) ? new CardSuperType { Name = typeName } : new CardType { Name = typeName }));
            //}

            return types;
        }

        public static bool IsSuperType(string typeName)
        {
            return Enum.GetNames(typeof(SuperType)).Any(t => t == typeName);
        }

        public static bool IsMainType(string typeName)
        {
            return Enum.GetNames(typeof(MainType)).Any(t => t == typeName);
        }
    }

    public abstract class CardType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<Card> Cards { get; set; }

        public static bool IsSuperType(string typeName)
        {
            return Enum.IsDefined(typeof(SuperType), typeName);
        }

        public static bool IsMainType(string typeName)
        {
            return Enum.IsDefined(typeof(MainType), typeName);
        }
    }

    public class CardMainType : CardType { }

    public class CardSuperType : CardType { }

    public class CardSubType : CardType { }

    //public class CreatureCardSubType : CardSubType
    //{
    //    public bool IsRace { get; set; }
    //}
}