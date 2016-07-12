using Juza.Magic.Models.Entities;
using Juza.Magic.Models.Entities.Chat;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace Juza.Magic.Models.DataContext
{
    public interface IDbContext
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        DbSet Set(Type entityType);
        int SaveChanges();
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        IEnumerable<DbEntityValidationResult> GetValidationErrors();
        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        DbEntityEntry Entry(object entity);
        void Dispose();
        string ToString();
        bool Equals(object obj);
        int GetHashCode();
        Type GetType();
        Database Database { get; }
    }

    public class MagicDbContext : IdentityDbContext<User, Role, int, UserLogin, UserRole, UserClaim>, IDbContext
    {
        public DbSet<UserConnection> Connections { get; set; }
        public DbSet<UserRelation> UserRelations { get; set; }
        public DbSet<ChatRoomUser> ChatRoomUsers { get; set; }
        public DbSet<ChatRoomConnection> ChatRoomConnections { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatLog> ChatLogs { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatMessageNotification> ChatMessageNotifications { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<ManaColor> ManaColors { get; set; }
        public DbSet<CardManaCost> ManaCosts { get; set; }
        public DbSet<CardType> CardTypes { get; set; }
        public DbSet<CardDeck> CardDecks { get; set; }
        public DbSet<CardSet> Sets { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<GamePlayerStatus> GameStatuses { get; set; }
        public DbSet<Player> Players { get; set; }

        public MagicDbContext() : base("MagicDB") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Make table names singular.
            // modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.PluralizingTableNameConvention>();

            base.OnModelCreating(modelBuilder);
            EntityConfig.ConfigureModelBuilder(modelBuilder);
        }

        public static string ShowErrorMessage(Exception ex)
        {
            if (ex is ArgumentNullException)
            {
                return "This item seems to no longer be there... It has probably been deleted in the meanwhile.";
            }

            if (ex is DbEntityValidationException)
            {
                var errors = string.Empty;
                foreach (var validationErrors in ((DbEntityValidationException) ex).EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errors += "Property: " + validationError.PropertyName +
                            " <span class=\"text-danger\">Error: " + validationError.ErrorMessage + "</span><br />";
                    }
                }

                return errors + ex;
            }

            return "There was a problem with saving to the database..." + ex;
            //    return "There was a problem with saving to the database... This is probably a connection problem, maybe try again."
        }
    }
}