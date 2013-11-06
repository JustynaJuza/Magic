namespace Magic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LoginDateChange : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "LastLoginDate", c => c.DateTime());
            DropColumn("dbo.AspNetUsers", "DateOfLastLogin");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "DateOfLastLogin", c => c.DateTime());
            DropColumn("dbo.AspNetUsers", "LastLoginDate");
        }
    }
}
