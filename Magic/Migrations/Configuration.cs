using Magic.Models.DataContext;
using System.Data.Entity.Migrations;
using System;
using Magic.Hubs;
using Magic.Models;
using System.Linq;

namespace Magic.Migrations
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
                Id = ChatHub.DefaultRoomId,
                Name = "All",
                TabColorCode = "#445566",
                Log = new ChatLog(ChatHub.DefaultRoomId)
            });

            foreach (var color in Enum.GetValues(typeof(Color)).Cast<Color>())
            {
                context.CardColors.AddOrUpdate(new ManaColor { Color = color });
            }

            //foreach (var role in Enum.GetNames(typeof(Role)))
            //{
            //    context.Roles.AddOrUpdate(new IdentityRole { Name = role });
            //}
        }
    }
}
