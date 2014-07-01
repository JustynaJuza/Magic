using System;
using System.ComponentModel.DataAnnotations.Schema;
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

            modelBuilder.Entity<ApplicationUserConnection>().HasKey(k => new { k.Id, k.UserId });
            modelBuilder.Entity<ApplicationUserConnection>().HasRequired(c => c.User).WithMany(u => u.Connections).HasForeignKey(c => c.UserId);
            
            modelBuilder.Entity<ChatRoom_ApplicationUserConnection>().HasKey(k => new { k.ConnectionId, k.UserId, k.ChatRoomId });
            modelBuilder.Entity<ChatRoom_ApplicationUserConnection>().HasRequired(cruc => cruc.ChatRoom).WithMany(r => r.Connections).HasForeignKey(cruc => cruc.ChatRoomId);
            modelBuilder.Entity<ChatRoom_ApplicationUserConnection>().HasRequired(cruc => cruc.Connection).WithMany().HasForeignKey(cruc => new { cruc.ConnectionId, cruc.UserId } );

            modelBuilder.Entity<Player_GameStatus>().HasKey(k => new { k.GameId, k.UserId });
            modelBuilder.Entity<Player_GameStatus>().HasRequired(pgs => pgs.Game).WithMany(g => g.Players).HasForeignKey(pgs => pgs.GameId);
            modelBuilder.Entity<Player_GameStatus>().HasRequired(pgs => pgs.User).WithMany(u => u.Games).HasForeignKey(pgs => pgs.UserId);

            //modelBuilder.Entity<ApplicationUserConnection>().HasKey(c => new { c.Id, c.ChatRoomId });

            //modelBuilder.Entity<ChatLog>().HasKey(l => l.DateCreated);
            //modelBuilder.Entity<PlayerGameStatus>().HasKey(pgs => new { pgs.GameId, pgs.PlayerId });
        }

        public DbSet<ApplicationUserConnection> Connections { get; set; }
        public DbSet<ChatRoom_ApplicationUserConnection> ChatRoom_Connections { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatLog> ChatLogs { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<CardColor> CardColors { get; set; }
        public DbSet<CardType> CardTypes { get; set; }
        public DbSet<CardDeck> CardDecks { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Player_GameStatus> Player_GameStatuses { get; set; }

        #region CRUD
        public bool Create(Object item)
        {
            //try
            //{
                Entry(item).State = EntityState.Added;
                return SaveChanges() > 0;
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
        }

        public object Read(Object item)
        {
            var collectionType = item.GetType();
            var targetCollection = Set(collectionType);

            var itemKeyInfo = collectionType.GetProperty("Id") ?? collectionType.GetProperty("DateCreated");
            var itemKey = itemKeyInfo.GetValue(item);

            var foundItem = targetCollection.Find(itemKey);
            //if (foundItem == null)
            //{
            //    return "This item seems to no longer be there... It has probably been deleted in the meanwhile.";
            //}

            return foundItem;
        }

        public bool AddOrUpdate(Object item, bool updateOnly = false)
        {
            Object foundItem;
            if (updateOnly)
            {
                Entry(item).CurrentValues.SetValues(item);
            }
            else
            {
                foundItem = Read(item);
                if (foundItem == null){
                    Entry(item).State = EntityState.Added;
                }
                else {
                    Entry(foundItem).CurrentValues.SetValues(item);
                }
                //if (foundItem is string)
                //    return (string) foundItem; // Error string returned.
            }

            return SaveChanges() > 0;

            //try
            //{
                
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

            //return null;
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