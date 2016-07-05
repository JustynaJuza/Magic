using Juza.Magic.Models.DataContext;
using Juza.Magic.Models.Entities;
using Juza.Magic.Models.Entities.Chat;
using Juza.Magic.Models.Enums;
using System;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Juza.Magic.Models.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<MagicDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "Magic.Models.DataContext.MagicDBContext";
            SetSqlGenerator("System.Data.SqlClient", new GenerateDateSqlServerMigrationSqlGenerator());
        }

        protected override void Seed(MagicDbContext context)
        {
            context.Database.ExecuteSqlCommand("ALTER TABLE [dbo].[UserConnections] DROP CONSTRAINT [FK_dbo.UserConnections_dbo.Games_GameId]");
            context.Database.ExecuteSqlCommand("ALTER TABLE [dbo].[UserConnections] ADD CONSTRAINT [FK_dbo.UserConnections_dbo.Games_GameId] " +
                                               "FOREIGN KEY ([GameId]) REFERENCES [dbo].[Games] ([Id]) ON UPDATE NO ACTION ON DELETE SET NULL");

            context.Set<ChatRoom>().AddOrUpdate(new ChatRoom
            {
                Id = ChatRoom.DefaultRoomId,
                Name = "All",
                TabColorCode = "#445566",
                Log = new ChatLog
                {
                    Id = ChatRoom.DefaultRoomId
                }
            });

            foreach (var color in Enum.GetNames(typeof(Color)))
            {
                var colorId = (int) Enum.Parse(typeof(Color), color);
                var existingColor = context.Set<ManaColor>().Find(colorId);
                if (existingColor == null)
                {
                    context.Set<ManaColor>().AddOrUpdate(new ManaColor { Name = color });
                    context.SaveChanges(); // Needs to be called to actually find entities added in this foreach
                }
                else
                {
                    existingColor.Alias = color;
                    context.Set<ManaColor>().AddOrUpdate(existingColor);
                }
            }

            foreach (var type in Enum.GetValues(typeof(SuperType)).Cast<SuperType>())
            {
                context.Set<CardType>().AddOrUpdate(new CardSuperType
                {
                    Id = (int) type,
                    Name = type.ToString()
                });
            }

            foreach (var type in Enum.GetValues(typeof(MainType)).Cast<MainType>())
            {
                context.Set<CardType>().AddOrUpdate(new CardMainType
                {
                    Id = (int) type,
                    Name = type.ToString()
                });
            }

            foreach (var role in Enum.GetValues(typeof(InternalRole)).Cast<InternalRole>())
            {
                var roleName = role.ToString();
                var existingRole = context.Roles.FirstOrDefault(r => r.Name == roleName);
                if (existingRole == null)
                {
                    context.Roles.AddOrUpdate(new Role
                    {
                        Name = role.ToString()
                    });
                }
            }
        }
    }
}
