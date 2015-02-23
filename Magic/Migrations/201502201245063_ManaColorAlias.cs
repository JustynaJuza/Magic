namespace Magic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ManaColorAlias : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ManaColors", "Alias", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ManaColors", "Alias");
        }
    }
}
