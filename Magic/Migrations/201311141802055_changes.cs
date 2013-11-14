namespace Magic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changes : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.CardDecks", "TotalCardNumber");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CardDecks", "TotalCardNumber", c => c.Int(nullable: false));
        }
    }
}
