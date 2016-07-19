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

    public partial class MagicDbContext : IdentityDbContext<User, Role, int, UserLogin, UserRole, UserClaim>, IDbContext
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
            modelBuilder.Conventions.Add(new UseDateTime2Convention());
            modelBuilder.Conventions.Add(new DateCreatedIsGeneratedConvention());

            EntityConfig.ConfigureModelBuilder(modelBuilder);
        }
    }
}