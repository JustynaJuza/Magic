﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Magic.Models
{
    public class Card : AbstractToString
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual CardType Type { get; set; }
        public List<int> CostPerColor { get; set; }
        public virtual List<CardColor> Colors { get; set; }
        public virtual List<CardAbility> Abilities { get; set; }
    }

    public class CardViewModel : AbstractToString
    {
        public int Id { get {return this.Id;} }
        [Required(ErrorMessage = "The card must have a name.")]
        public string Name { get; set; }
        public bool Tapped { get; set; }
        public bool Permanent { get; set; }
        public List<int> CostPerColor { get; set; }
        public virtual List<CardColor> Colors { get; set; }
        public virtual List<CardAbility> Abilities { get; set; }

        public void Tap()
        {
            Tapped = true;
        }
        public void Untap()
        {
            Tapped = false;
        }
    }

    public class CreatureCard : Card
    {
        public int Power { get; set; }
        public int Toughness { get; set; }
    }
}