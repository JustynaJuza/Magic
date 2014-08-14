using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Magic.Models.DataContext
{
    public class MagicDbContext : IdentityDbContext<User>
    {
        public MagicDbContext() : base("MagicDB") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Make table names singular.
            // modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.PluralizingTableNameConvention>();

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUser>().ToTable("Users");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");

            modelBuilder.Entity<User>().Property(u => u.DateCreated).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            modelBuilder.Entity<ChatLog>().Property(r => r.DateCreated).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            modelBuilder.Entity<UserRelation>().Property(r => r.DateCreated).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            modelBuilder.Entity<ChatMessage>().Property(m => m.TimeSend).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            
            modelBuilder.Entity<UserConnection>().HasKey(k => new { k.Id, k.UserId });
            modelBuilder.Entity<UserConnection>().HasRequired(c => c.User).WithMany(u => u.Connections).HasForeignKey(c => c.UserId);
            modelBuilder.Entity<UserConnection>().HasOptional(c => c.Game).WithMany().HasForeignKey(c => c.GameId);

            modelBuilder.Entity<UserRelation>().HasKey(k => new { k.UserId, k.RelatedUserId });
            modelBuilder.Entity<UserRelation>().HasRequired(uu => uu.User).WithMany(u => u.Relations).HasForeignKey(uu => uu.UserId);
            modelBuilder.Entity<UserRelation>().HasRequired(uu => uu.RelatedUser).WithMany().HasForeignKey(uu => uu.RelatedUserId).WillCascadeOnDelete(false);
            modelBuilder.Entity<UserRelation>().Map<UserRelationFriend>(r => r.Requires("Discriminator").HasValue((int)UserRelationship.Friend));
            modelBuilder.Entity<UserRelation>().Map<UserRelationIgnored>(r => r.Requires("Discriminator").HasValue((int)UserRelationship.Ignored));

            modelBuilder.Entity<ChatRoomUserConnection>().HasKey(k => new { k.ConnectionId, k.UserId, k.ChatRoomId });
            modelBuilder.Entity<ChatRoomUserConnection>().HasRequired(ruc => ruc.ChatRoom).WithMany(r => r.Connections).HasForeignKey(ruc => ruc.ChatRoomId);
            modelBuilder.Entity<ChatRoomUserConnection>().HasRequired(ruc => ruc.User).WithMany().HasForeignKey(ruc => ruc.UserId).WillCascadeOnDelete(false);
            modelBuilder.Entity<ChatRoomUserConnection>().HasRequired(ruc => ruc.Connection).WithMany().HasForeignKey(ruc => new { ruc.ConnectionId, ruc.UserId });

            modelBuilder.Entity<ChatRoomUser>().HasKey(k => new { k.UserId, k.ChatRoomId });
            modelBuilder.Entity<ChatRoomUser>().HasRequired(ru => ru.ChatRoom).WithMany(r => r.Users).HasForeignKey(ru => ru.ChatRoomId);
            modelBuilder.Entity<ChatRoomUser>().HasRequired(ru => ru.User).WithMany().HasForeignKey(ru => ru.UserId);

            modelBuilder.Entity<Game>().Ignore(g => g.Observers);
            modelBuilder.Entity<Game>().Ignore(g => g.PlayerCapacity); 

            modelBuilder.Entity<GamePlayerStatus>().HasKey(k => new { k.UserId, k.GameId });
            modelBuilder.Entity<GamePlayerStatus>().HasRequired(gps => gps.Game).WithMany(g => g.Players).HasForeignKey(gps => gps.GameId);
            modelBuilder.Entity<GamePlayerStatus>().HasRequired(gps => gps.User).WithMany(u => u.Games).HasForeignKey(gps => gps.UserId);
            modelBuilder.Entity<GamePlayerStatus>().HasOptional(gps => gps.Player).WithRequired();

            modelBuilder.Entity<Player>().HasKey(k => new { k.UserId, k.GameId });
            modelBuilder.Entity<Player>().HasRequired(p => p.Game).WithMany().HasForeignKey(p => p.GameId);
            modelBuilder.Entity<Player>().HasRequired(p => p.User).WithMany().HasForeignKey(p => p.UserId);
            modelBuilder.Entity<Player>().HasOptional(p => p.Deck).WithRequired(d => d.Player);

            modelBuilder.Entity<PlayerCard>().HasKey(k => new { k.UserId, k.GameId, k.CardId });
            modelBuilder.Entity<PlayerCard>().HasRequired(pc => pc.Card).WithMany().HasForeignKey(pc => pc.CardId);
            modelBuilder.Entity<PlayerCard>().HasRequired(pc => pc.Player).WithMany().HasForeignKey(pc => new { pc.UserId, pc.GameId });
            modelBuilder.Entity<PlayerCard>().HasRequired(pc => pc.Deck).WithMany(d => d.Cards).HasForeignKey(pc => new { pc.DeckId, pc.UserId, pc.GameId });

            modelBuilder.Entity<PlayerCardDeck>().HasKey(k => new { k.DeckId, k.UserId, k.GameId });
            modelBuilder.Entity<PlayerCardDeck>().HasRequired(pcd => pcd.Deck).WithMany().HasForeignKey(pcd => pcd.DeckId);

            modelBuilder.Entity<ChatMessage>().HasKey(m => new { m.Id, m.LogId });
            modelBuilder.Entity<ChatMessage>().Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<ChatMessage>().HasRequired(m => m.Log).WithMany(l => l.Messages).HasForeignKey(m => m.LogId);

            modelBuilder.Entity<ChatMessageNotification>().HasKey(mr => new { mr.MessageId, mr.LogId, mr.RecipientId });
            modelBuilder.Entity<ChatMessageNotification>().HasRequired(mn => mn.Recipient).WithMany(r => r.ChatMessages).HasForeignKey(mn => mn.RecipientId);
            modelBuilder.Entity<ChatMessageNotification>().HasRequired(mn => mn.Message).WithMany(m => m.Recipients).HasForeignKey(mn => new { mn.MessageId, mn.LogId });

            modelBuilder.Entity<ChatRoom>().HasOptional(r => r.Log).WithOptionalDependent();
        }

        public DbSet<UserConnection> Connections { get; set; }
        public DbSet<UserRelation> UserRelations { get; set; }
        public DbSet<ChatRoomUser> ChatRoomUsers { get; set; }
        public DbSet<ChatRoomUserConnection> ChatRoomConnections { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatLog> ChatLogs { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatMessageNotification> ChatMessageNotifications { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<CardColor> CardColors { get; set; }
        public DbSet<CardType> CardTypes { get; set; }
        public DbSet<CardDeck> CardDecks { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<GamePlayerStatus> GameStatuses { get; set; }
        public DbSet<Player> Players { get; set; }

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
            errorText = null;

            if (updateOnly)
            {
                Entry(item).CurrentValues.SetValues(item);
            }
            else
            {
                var foundItem = ReadObject(item);
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
            if (ex is ArgumentNullException)
            {
                return "This item seems to no longer be there... It has probably been deleted in the meanwhile.";
            }
            
            if (ex is System.Data.Entity.Validation.DbEntityValidationException)
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

            return "There was a problem with saving to the database..." + ex.ToString();
            //    return "There was a problem with saving to the database... This is probably a connection problem, maybe try again."
        }
        #endregion CRUD
    }
}