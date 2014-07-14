namespace Magic.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Magic.Hubs;
    using Magic.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<Magic.Models.DataContext.MagicDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "Magic.Models.DataContext.MagicDBContext";
        }

        protected override void Seed(Magic.Models.DataContext.MagicDbContext context)
        {
            foreach (Color color in Enum.GetValues(typeof(Color)))
            {
                context.CardColors.AddOrUpdate(new CardColor { Color = color });
            }

            context.ChatRooms.AddOrUpdate(new ChatRoom { Id = ChatHub.DefaultRoomId, Name = "All" });
        }
    }
}
