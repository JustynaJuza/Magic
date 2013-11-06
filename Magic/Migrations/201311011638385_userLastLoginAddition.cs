namespace Magic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class userLastLoginAddition : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CardColors", "Color", c => c.Int(nullable: false));
            AddColumn("dbo.AspNetUsers", "DateOfLastLogin", c => c.DateTime());
            AddColumn("dbo.AspNetUsers", "DateOfBirth", c => c.DateTime());
            DropColumn("dbo.CardColors", "Name");
            DropColumn("dbo.AspNetUsers", "BirthDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "BirthDate", c => c.DateTime());
            AddColumn("dbo.CardColors", "Name", c => c.String());
            DropColumn("dbo.AspNetUsers", "DateOfBirth");
            DropColumn("dbo.AspNetUsers", "DateOfLastLogin");
            DropColumn("dbo.CardColors", "Color");
        }
    }
}
