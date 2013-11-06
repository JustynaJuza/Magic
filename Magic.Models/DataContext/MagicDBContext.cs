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
        public MagicDBContext() : base("MagicDB") { }

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

        public DbSet<Magic.Models.Card> AllCards { get; set; }
        public DbSet<Magic.Models.CardColor> AllCardColors { get; set; }
        public DbSet<Magic.Models.CardType> AllCardTypes { get; set; }

        #region CRUD
        public string Create(Object item)
        {
            try
            {
                this.Entry(item).State = EntityState.Added;
                this.SaveChanges();
            }
            catch (Exception)
            {
                return "There was a problem with saving to the database... This is probably a connection problem, maybe try again.";
            }

            return null;
        }

        public Object Read(Object item)
        {
            Type collectionType = item.GetType();
            DbSet targetCollection = this.Set(collectionType);

            var itemId = collectionType.GetProperty("Id").GetValue(item);
            var foundItem = targetCollection.Find(itemId);
            if (foundItem == null)
            {
                return "This item seems to no longer be there... It has probably been deleted in the meanwhile.";
            }

            return foundItem;
        }

        public string Update(Object item)
        {
            var foundItem = Read(item);
            if (foundItem.GetType() == typeof(string))
                return (string) foundItem; // Error string returned.

            try
            {
                this.Entry(foundItem).CurrentValues.SetValues(item);
                this.SaveChanges();
            }
            catch (Exception)
            {
                return "There was a problem with saving to the database... This is probably a connection problem, maybe try again.";
            }

            return null;
        }

        public string Delete(Object item)
        {
            var foundItem = Read(item);
            if (foundItem.GetType() == typeof(string))
                return (string) foundItem; // Error string returned.

            try
            {
                this.Entry(foundItem).State = EntityState.Deleted;
                this.SaveChanges();
            }
            catch (Exception)
            {
                return "There was a problem with saving to the database... This is probably a connection problem, maybe try again.";
            }
            return null;
        }
        #endregion CRUD
    }
}