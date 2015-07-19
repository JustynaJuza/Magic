using System;
using System.Data.Entity.Migrations;
using System.Linq;
using Magic.Models.Chat;
using Magic.Models.DataContext;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Magic.Models.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<Magic.Models.DataContext.MagicDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "Magic.Models.DataContext.MagicDBContext";
            SetSqlGenerator("System.Data.SqlClient", new GenerateDateSqlServerMigrationSqlGenerator());
        }

        protected override void Seed(MagicDbContext context)
        {
            context.Database.ExecuteSqlCommand("ALTER TABLE [dbo].[UserConnections] DROP CONSTRAINT [FK_dbo.UserConnections_dbo.Games_GameId]");
            context.Database.ExecuteSqlCommand("ALTER TABLE [dbo].[UserConnections] ADD CONSTRAINT [FK_dbo.UserConnections_dbo.Games_GameId] " +
                                               "FOREIGN KEY ([GameId]) REFERENCES [dbo].[Games] ([Id]) ON UPDATE NO ACTION ON DELETE SET NULL");

            context.ChatRooms.AddOrUpdate(new ChatRoom
            {
                Id = ChatRoom.DefaultRoomId,
                Name = "All",
                TabColorCode = "#445566",
                Log = new ChatLog(ChatRoom.DefaultRoomId)
            });

            foreach (var color in Enum.GetNames(typeof(Color)))
            {
                var colorId = (int)Enum.Parse(typeof(Color), color);
                var existingColor = context.ManaColors.Find(colorId);
                if (existingColor == null)
                {
                    context.ManaColors.AddOrUpdate(new ManaColor { Name = color });
                    context.SaveChanges(); // Needs to be called to actually find entities added in this foreach
                }
                else
                {
                    existingColor.Alias = color;
                    context.ManaColors.AddOrUpdate(existingColor);
                }
            }

            foreach (var type in Enum.GetValues(typeof(SuperType)).Cast<SuperType>())
            {
                context.CardTypes.AddOrUpdate(new CardSuperType
                {
                    Id = (int)type,
                    Name = type.ToString()
                });
            }

            foreach (var type in Enum.GetValues(typeof(MainType)).Cast<MainType>())
            {
                context.CardTypes.AddOrUpdate(new CardMainType
                {
                    Id = (int)type,
                    Name = type.ToString()
                });
            }

            foreach (var role in Enum.GetValues(typeof(Role)).Cast<Role>())
            {
                var roleName = role.ToString();
                var existingRole = context.Roles.FirstOrDefault(r => r.Name == roleName);
                if (existingRole == null)
                {
                    context.Roles.AddOrUpdate(new IdentityRole
                    {
                        Name = role.ToString()
                    });
                }
            }
        }
    }
}
