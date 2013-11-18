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

            //modelBuilder.Entity<ApplicationUser>()
            //    .HasOptional(u => u.DeckCollection).WithOptionalDependent(d => d.);

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

        public DbSet<Magic.Models.Card> Cards { get; set; }
        public DbSet<Magic.Models.CardColor> CardColors { get; set; }
        public DbSet<Magic.Models.CardType> CardTypes { get; set; }
        public DbSet<Magic.Models.CardDeck> CardDecks { get; set; }
        public DbSet<Magic.Models.ChatLog> ChatLogs { get; set; }
        public DbSet<Magic.Models.ChatMessage> ChatMessages { get; set; }

        #region CRUD
        public string Create(Object item)
        {
            try
            {
                this.Entry(item).State = EntityState.Added;
                this.SaveChanges();
            }
            //catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            //{
            //    foreach (var validationErrors in ex.EntityValidationErrors)
            //    {
            //        foreach (var validationError in validationErrors.ValidationErrors)
            //        {
            //            System.Diagnostics.Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
            //        }
            //    }
            //}
            catch (Exception)
            {
                return "There was a problem with saving to the database... This is probably a connection problem, maybe try again.";
            }

            return null;
        }

        public object Read(Object item)
        {
            Type collectionType = item.GetType();
            DbSet targetCollection = this.Set(collectionType);

            var itemKeyInfo = (collectionType.GetProperty("Id") != null ? collectionType.GetProperty("Id") 
                : collectionType.GetProperty("DateCreated") != null ? collectionType.GetProperty("DateCreated") : null);
            var itemKey = itemKeyInfo.GetValue(item);

            var foundItem = targetCollection.Find(itemKey);
            if (foundItem == null)
            {
                return "This item seems to no longer be there... It has probably been deleted in the meanwhile.";
            }

            return foundItem;
        }

        public string Update(Object item, bool updateOnly = false)
        {
            var foundItem = item;
            if (!updateOnly)
            {
                foundItem = Read(item);
                if (foundItem.GetType() == typeof(string))
                    return (string) foundItem; // Error string returned.
            }

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