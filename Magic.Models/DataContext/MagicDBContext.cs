using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Magic.Models.DataContext
{
    public class MagicDBContext : IdentityDbContext<ApplicationUser>
    {
        public MagicDBContext(): base("MagicDB") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Make table names singular.
            // modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.PluralizingTableNameConvention>();

            //modelBuilder.Entity<CardColor>()
            //    .HasMany(cc => cc.Cards)
            //    .WithRequired(p => p.CardCardColor)
            //    .WillCascadeOnDelete(true);

            //modelBuilder.Entity<CardColor>()
            //    .HasMany<Card>(c => c.Cards)
            //    .WithMany(cc => cc.CardColors)
            //    .Map(p =>
            //    {
            //        p.MapLeftKey(new string[] { });
            //        p.MapRightKey(new string[] { });
            //        p.ToTable("CardsCardColor");
            //    });
        }

        public DbSet<Magic.Models.CardColor> AllCardColors { get; set; }
        public DbSet<Magic.Models.Card> AllCards { get; set; }
        //public DbSet<Magic.Models.Player> AllPlayers { get; set; }
    }
}