using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Magic.Models.Chat;
using Magic.Models.Extensions;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Magic.Models.DataContext
{
    public interface IDbContext
    {
        //TEntity Read<TEntity, TKey>(TKey id)
        //    where TEntity : class
        //    where TKey : struct;

        //TEntity Read<TEntity, TKey>(TKey id, out string errorText)
        //    where TEntity : class
        //    where TKey : struct;

        //TEntity Read<TEntity>(TEntity entity)
        //    where TEntity : class;

        //bool Insert<TEntity>(TEntity entity, bool withSave = false)
        //    where TEntity : class;

        //bool Insert<TEntity>(TEntity entity, out string errorText, bool withSave = false)
        //    where TEntity : class;

        //bool InsertWithSave<TEntity>(TEntity entity)
        //    where TEntity : class;

        ///// <summary>
        ///// Updates an existing entity with the same Id in the context or inserts it as a new entity if none is found with the same Id.
        ///// Enable the updateOnly flag if there is no need for searching for an existing entity (existing entity is already attached to the context).
        ///// </summary>
        ///// <typeparam name="TEntity">Type of the entity to be updated.</typeparam>
        ///// <param name="item">The entity to be updated.</param>
        ///// <param name="updateOnly">Optional flag for applying updates to an existing entity only but without attaching the entity to the context.</param>
        //bool InsertOrUpdate<TEntity>(TEntity entity, bool withSave = false, bool updateOnly = false)
        //    where TEntity : class;

        //bool InsertOrUpdate<TEntity>(TEntity entity, out string errorText, bool withSave = false, bool updateOnly = false)
        //    where TEntity : class;

        //bool InsertOrUpdateWithSave<TEntity>(TEntity entity, bool updateOnly = false)
        //    where TEntity : class;

        //bool Delete<TEntity>(TEntity entity, bool withSave = false, bool deleteOnly = false)
        //    where TEntity : class;

        //bool Delete<TEntity>(TEntity entity, out string errorText, bool withSave = false, bool deleteOnly = false)
        //    where TEntity : class;

        //bool DeleteAndSave<TEntity>(TEntity entity, bool deleteOnly = false)
        //    where TEntity : class;

        DbSet<TEntity> Query<TEntity>()
            where TEntity : class;

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

    public class MagicDbContext : IdentityDbContext<User>, IDbContext
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

        #region CRUD
        public TEntity Read<TEntity, TKey>(TKey id)
            where TEntity : class
            where TKey : struct
        {
            string errorText;
            return Read<TEntity, TKey>(id, out errorText);
        }

        public TEntity Read<TEntity, TKey>(TKey id, out string errorText)
            where TEntity : class
            where TKey : struct
        {
            errorText = null;

            var foundEntity = Set<TEntity>().Find(id);
            if (foundEntity == null)
            {
                errorText = ShowErrorMessage(new ArgumentNullException());
            }
            return foundEntity;
        }

        public TEntity Read<TEntity>(TEntity entity)
            where TEntity : class
        {
            var collectionType = entity.GetType();
            var itemKeyInfo = collectionType.GetProperty("Id") ?? collectionType.GetProperty(collectionType.Name + "Id");
            
            if (itemKeyInfo == null)
            {
                var entityException = new TargetException("The entity could not be read, " +
                                                            "no valid Id was found with the name 'Id' or '" + collectionType.Name + "Id', " +
                                                            "please use the Read method overload which accepts a key structure");
                entityException.LogException();
                throw entityException;
            }

            var itemKey = itemKeyInfo.GetValue(entity);

            var foundEntity = Set<TEntity>().Find(itemKey);
            return foundEntity;
        }

        public bool Insert<TEntity>(TEntity entity, bool withSave = false)
            where TEntity : class
        {
            string errorText;
            return Insert(entity, out errorText);
        }

        public bool Insert<TEntity>(TEntity entity, out string errorText, bool withSave = false)
            where TEntity : class
        {
            errorText = null;

            Entry(entity).State = EntityState.Added;

            if (!withSave)
            {
                return true;
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

        public bool InsertWithSave<TEntity>(TEntity entity)
            where TEntity : class
        {
            return Insert(entity, withSave: true);
        }

        public bool InsertOrUpdate<TEntity>(TEntity entity, bool withSave = false, bool updateOnly = false)
            where TEntity : class
        {
            string errorText;
            return InsertOrUpdate(entity, out errorText, updateOnly);
        }

        public bool InsertOrUpdate<TEntity>(TEntity entity, out string errorText, bool withSave = false, bool updateOnly = false)
            where TEntity : class
        {
            errorText = null;

            if (updateOnly)
            {
                Entry(entity).CurrentValues.SetValues(entity);
            }
            else
            {
                var foundEntity = Read(entity);
                if (foundEntity == null)
                {
                    Entry(entity).State = EntityState.Added;
                }
                else
                {
                    Entry(foundEntity).CurrentValues.SetValues(entity);
                }
            }

            if (!withSave)
            {
                return true;
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
        
        public bool InsertOrUpdateWithSave<TEntity>(TEntity entity, bool updateOnly = false)
            where TEntity : class
        {
            return InsertOrUpdate(entity, withSave: true, updateOnly: updateOnly);
        }

        public bool Delete<TEntity>(TEntity entity, bool withSave = false, bool deleteOnly = false)
            where TEntity : class
        {
            string errorText;
            return Delete(entity, out errorText, deleteOnly);
        }

        public bool Delete<TEntity>(TEntity entity, out string errorText, bool withSave = false, bool deleteOnly = false)
            where TEntity : class
        {
            errorText = null;

            if (!deleteOnly)
            {
                entity = Read(entity);
                if (entity == null)
                {
                    return false;
                }
            }

            Entry(entity).State = EntityState.Deleted;

            if (!withSave)
            {
                return true;
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

        public bool DeleteAndSave<TEntity>(TEntity entity, bool deleteOnly = false)
            where TEntity : class
        {
            return Delete(entity, withSave: true, deleteOnly: deleteOnly);
        }

        public DbSet<TEntity> Query<TEntity>()
            where TEntity : class
        {
            return Set<TEntity>();
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
                foreach (var validationErrors in ((DbEntityValidationException)ex).EntityValidationErrors)
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
        #endregion CRUD
    }
}