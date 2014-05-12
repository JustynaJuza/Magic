using System;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Magic.Models.DataContext
{
    public class MagicDbContext : IdentityDbContext<ApplicationUser>
    {
        public MagicDbContext() : base("MagicDB") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Make table names singular.
            // modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.PluralizingTableNameConvention>();

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationUserConnection>().ToTable("AspNetUserConnections");

            modelBuilder.Entity<ChatRoom>()
                .HasRequired(r => r.Log)
                .WithOptional();
                //.WithOptional(l => l.Room);

            //modelBuilder.Entity<ApplicationUserConnection>().HasKey(c => new { c.Id, c.ChatRoomId });

            //modelBuilder.Entity<ChatLog>().HasKey(l => l.DateCreated);
            //modelBuilder.Entity<PlayerGameStatus>().HasKey(pgs => new { pgs.GameId, pgs.PlayerId });
        }

        public DbSet<ApplicationUserConnection> Connections { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatLog> ChatLogs { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<CardColor> CardColors { get; set; }
        public DbSet<CardType> CardTypes { get; set; }
        public DbSet<CardDeck> CardDecks { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<PlayerGameStatus> PlayerGameStatuses { get; set; }

        #region CRUD
        public string Create(Object item)
        {
            //try
            //{
                Entry(item).State = EntityState.Added;
                SaveChanges();
            //}
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
            //catch (Exception ex)
            //{
            //    System.Diagnostics.Debug.WriteLine(ex.ToString());
            //    return "There was a problem with saving to the database... This is probably a connection problem, maybe try again.";
            //}

            return null;
        }

        public object Read(Object item)
        {
            Type collectionType = item.GetType();
            DbSet targetCollection = Set(collectionType);

            var itemKeyInfo = collectionType.GetProperty("Id") ?? collectionType.GetProperty("DateCreated");
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
                if (foundItem is string)
                    return (string) foundItem; // Error string returned.
            }

            //try
            //{
                Entry(foundItem).CurrentValues.SetValues(item);
                SaveChanges();
            //}
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
            //catch (Exception ex)
            //{
            //    System.Diagnostics.Debug.WriteLine(ex.ToString());
            //    return "There was a problem with saving to the database... This is probably a connection problem, maybe try again.";
            //}

            return null;
        }

        public string Delete(Object item, bool deleteOnly = false)
        {
            var foundItem = item;
            if (!deleteOnly)
            {
                foundItem = Read(item);
                if (foundItem is string)
                    return (string) foundItem; // Error string returned.
            }

            //try
            //{
                Entry(foundItem).State = EntityState.Deleted;
                SaveChanges();
            //}
            //catch (Exception ex)
            //{
            //    System.Diagnostics.Debug.WriteLine(ex.ToString());
            //    return "There was a problem with saving to the database... This is probably a connection problem, maybe try again.";
            //}

            return null;
        }
        #endregion CRUD
    }
}