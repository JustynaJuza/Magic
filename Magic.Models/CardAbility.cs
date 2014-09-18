using System.Reflection;
using Magic.Models.DataContext;
using Magic.Models.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class CardAbility : AbstractExtensions
    {
        private delegate int Callback();
        private Callback callback;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Reminder { get; set; }
        public IList<string> EffectNames { get; set; }
        public string CallbackName { get; set; }
        public IList<CardAvailableAbility> Cards { get; set; }

        //[NotMapped]
        //public virtual Card Target { get; set; }

        public CardAbility()
        {
            callback = (Callback) Delegate.CreateDelegate(typeof(Callback), GetType().GetMethod(CallbackName));
        }

        public void Effect()
        {
            foreach (var effectName in EffectNames)
            {
                //GetType().GetMethod(effectName).Invoke();
            }
            callback();
        }

        public void ReturnToHand(CardViewModel target, string gameId)
        {
            using (var context = new MagicDbContext())
            {
                var player = context.Players.Find(target.CasterId, gameId);
                //player.Deck.
            }
        }
    }
    
    public class ActiveAbility : CardAbility
    {
        public bool RequiresTap { get; set; }
        public IList<CardManaCost> Cost { get; set; }
    }

    public class TargetAbility : ActiveAbility
    {
        public int PlayerTargets { get; set; }
        public int CardTargets { get; set; }
        public IList<CardType> CardTypesRequired { get; set; }
        public int TargetsRequired { get; set; }
    }
}