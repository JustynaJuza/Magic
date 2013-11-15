namespace Magic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Status", c => c.Int());
            DropColumn("dbo.ChatMessages", "TimeRead");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ChatMessages", "TimeRead", c => c.DateTime());
            DropColumn("dbo.AspNetUsers", "Status");
        }
    }
}
