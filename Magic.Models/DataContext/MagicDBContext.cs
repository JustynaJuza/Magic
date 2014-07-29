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

            modelBuilder.Entity<ApplicationUserConnection >().HasKey(k => new { k.Id, k.UserId });
            modelBuilder.Entity<ApplicationUserConnection>().HasRequired(c => c.User).WithMany(u => u.Connections).HasForeignKey(c => c.UserId);

            modelBuilder.Entity<ApplicationUserRelation>().HasKey(k => new { k.UserId, k.RelatedUserId });
            modelBuilder.Entity<ApplicationUserRelation>().HasRequired(uu => uu.User).WithMany().HasForeignKey(uu => uu.UserId);
            modelBuilder.Entity<ApplicationUserRelation>().HasRequired(uu => uu.RelatedUser).WithMany().HasForeignKey(uu => uu.RelatedUserId);
            modelBuilder.Entity<ApplicationUserRelation_Friend>().HasRequired(uu => uu.User).WithMany(u => u.Friends).HasForeignKey(uu => uu.UserId);
            modelBuilder.Entity<ApplicationUserRelation_Friend>().HasRequired(uu => uu.RelatedUser).WithMany().HasForeignKey(uu => uu.RelatedUserId);
            modelBuilder.Entity<ApplicationUserRelation_Ignored>().HasRequired(uu => uu.User).WithMany(u => u.Ignored).HasForeignKey(uu => uu.UserId);
            modelBuilder.Entity<ApplicationUserRelation_Ignored>().HasRequired(uu => uu.RelatedUser).WithMany().HasForeignKey(uu => uu.RelatedUserId);

            modelBuilder.Entity<ChatRoom_ApplicationUserConnection>().HasKey(k => new { k.ConnectionId, k.UserId, k.ChatRoomId });
            modelBuilder.Entity<ChatRoom_ApplicationUserConnection>().HasRequired(ruc => ruc.ChatRoom).WithMany(r => r.Connections).HasForeignKey(ruc => ruc.ChatRoomId);
            modelBuilder.Entity<ChatRoom_ApplicationUserConnection>().HasRequired(ruc => ruc.User).WithMany().HasForeignKey(ruc => ruc.UserId).WillCascadeOnDelete(false);
            modelBuilder.Entity<ChatRoom_ApplicationUserConnection>().HasRequired(ruc => ruc.Connection).WithMany().HasForeignKey(ruc => new { ruc.ConnectionId, ruc.UserId });

            modelBuilder.Entity<ChatRoom_ApplicationUser>().HasKey(k => new { k.UserId, k.ChatRoomId });
            modelBuilder.Entity<ChatRoom_ApplicationUser>().HasRequired(ru => ru.ChatRoom).WithMany(r => r.Users).HasForeignKey(ru => ru.ChatRoomId);
            modelBuilder.Entity<ChatRoom_ApplicationUser>().HasRequired(ru => ru.User).WithMany().HasForeignKey(ru => ru.UserId);

            modelBuilder.Entity<Player_GameStatus>().HasKey(k => new { k.GameId, k.UserId });
            modelBuilder.Entity<Player_GameStatus>().HasRequired(pgs => pgs.Game).WithMany(g => g.Players).HasForeignKey(pgs => pgs.GameId);
            modelBuilder.Entity<Player_GameStatus>().HasRequired(pgs => pgs.User).WithMany(u => u.Games).HasForeignKey(pgs => pgs.UserId);

            modelBuilder.Entity<Recipient_ChatMessageStatus>().HasKey(rms => new { rms.MessageId, rms.RecipientId });
            modelBuilder.Entity<Recipient_ChatMessageStatus>().HasRequired(rms => rms.Message).WithMany(m => m.Recipients).HasForeignKey(rms => rms.MessageId);
            modelBuilder.Entity<Recipient_ChatMessageStatus>().HasRequired(rms => rms.Recipient).WithMany(r => r.ChatMessages).HasForeignKey(rms => rms.RecipientId);
        }

        public DbSet<ApplicationUserConnection> Connections { get; set; }
        public DbSet<ApplicationUserRelation> UserRelations { get; set; }
        public DbSet<ChatRoom_ApplicationUser> ChatRoom_Users { get; set; }
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
        public T Read<T, U>(U id)
            where T : class
            where U : struct
        {
            string errorText;
            return Read<T, U>(id, out errorText);
        }

        public T Read<T, U>(U id, out string errorText)
            where T : class
            where U : struct
        {
            errorText = null;

            var collectionType = typeof(T);
            var targetCollection = Set(collectionType);

            var foundItem = targetCollection.Find(id);

            if (foundItem == null)
            {
                errorText = ShowErrorMessage(new ArgumentNullException());
            }

            return (T)foundItem;
        }

        public object ReadObject(object item)
        {
            var collectionType = item.GetType();
            var targetCollection = Set(collectionType);

            var itemKeyInfo = collectionType.GetProperty("Id"); // ?? collectionType.GetProperty("DateCreated");
            var itemKey = itemKeyInfo.GetValue(item);

            var foundItem = targetCollection.Find(itemKey);

            return foundItem;
        }

        public bool Insert(Object item)
        {
            string errorText;
            return Insert(item, out errorText);
        }

        public bool Insert(Object item, out string errorText)
        {
            errorText = null;

            Entry(item).State = EntityState.Added;

            try
            {
                return SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                errorText = ShowErrorMessage(ex);
                return false;
            }
        }

        public bool InsertOrUpdate(Object item, bool updateOnly = false)
        {
            string errorText;
            return InsertOrUpdate(item, out errorText, updateOnly);
        }

        public bool InsertOrUpdate(Object item, out string errorText, bool updateOnly = false)
        {
            Object foundItem;
            errorText = null;

            if (updateOnly)
            {
                Entry(item).CurrentValues.SetValues(item);
            }
            else
            {
                foundItem = ReadObject(item);
                if (foundItem == null)
                {
                    Entry(item).State = EntityState.Added;
                }
                else
                {
                    Entry(foundItem).CurrentValues.SetValues(item);
                }
            }

            try
            {
                return SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                errorText = ShowErrorMessage(ex);
                return false;
            }
        }

        public bool Delete(Object item, bool deleteOnly = false)
        {
            string errorText;
            return Delete(item, out errorText, deleteOnly);
        }

        public bool Delete(Object item, out string errorText, bool deleteOnly = false)
        {
            var foundItem = item;
            errorText = null;

            if (!deleteOnly)
            {
                foundItem = ReadObject(item);
                if (foundItem == null)
                {
                    return false;
                }
            }

            Entry(foundItem).State = EntityState.Deleted;

            try
            {
                return SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                errorText = ShowErrorMessage(ex);
                return false;
            }
        }

        public string ShowErrorMessage(Exception ex)
        {
            if (ex is ArgumentNullException) return "This item seems to no longer be there... It has probably been deleted in the meanwhile.";
            else if (ex is System.Data.Entity.Validation.DbEntityValidationException)
            {
                var errors = string.Empty;
                foreach (var validationErrors in ((System.Data.Entity.Validation.DbEntityValidationException)ex).EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errors += "Property: " + validationError.PropertyName + "Error:" + validationError.ErrorMessage + "<br />";
                    }
                }

                return errors + ex.ToString();
            }
            else return "There was a problem with saving to the database..." + ex.ToString();
            //    return "There was a problem with saving to the database... This is probably a connection problem, maybe try again."
        }
        #endregion CRUD
    }
}