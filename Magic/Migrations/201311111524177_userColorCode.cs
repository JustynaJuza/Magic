namespace Magic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class userColorCode : DbMigration
    {
        public override void Up()
        {
            //AddColumn("dbo.AspNetUsers", "Image", c => c.String());
            //AddColumn("dbo.AspNetUsers", "ColorCode", c => c.String());
            //DropColumn("dbo.AspNetUsers", "UserImage");
        }
        
        public override void Down()
        {
            //AddColumn("dbo.AspNetUsers", "UserImage", c => c.String());
            //DropColumn("dbo.AspNetUsers", "ColorCode");
            //DropColumn("dbo.AspNetUsers", "Image");
        }
    }
}
