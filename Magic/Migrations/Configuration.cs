namespace Magic.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Magic.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<Magic.Models.DataContext.MagicDBContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;
            ContextKey = "Magic.Models.DataContext.MagicDBContext";
        }


        protected override void Seed(Magic.Models.DataContext.MagicDBContext context)
        {
            foreach (Color color in Enum.GetValues(typeof(Color)))
            {
                context.CardColors.AddOrUpdate(new CardColor { Color = color });
            }
        }
    }
}
