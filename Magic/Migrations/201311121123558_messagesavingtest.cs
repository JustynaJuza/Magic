namespace Magic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class messagesavingtest : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ChatMessages", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.ChatMessages", "Recipient_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.ChatMessages", "Sender_Id", "dbo.AspNetUsers");
            DropIndex("dbo.ChatMessages", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.ChatMessages", new[] { "Recipient_Id" });
            DropIndex("dbo.ChatMessages", new[] { "Sender_Id" });
            DropColumn("dbo.ChatMessages", "ApplicationUser_Id");
            DropColumn("dbo.ChatMessages", "Recipient_Id");
            DropColumn("dbo.ChatMessages", "Sender_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ChatMessages", "Sender_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.ChatMessages", "Recipient_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.ChatMessages", "ApplicationUser_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.ChatMessages", "Sender_Id");
            CreateIndex("dbo.ChatMessages", "Recipient_Id");
            CreateIndex("dbo.ChatMessages", "ApplicationUser_Id");
            AddForeignKey("dbo.ChatMessages", "Sender_Id", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.ChatMessages", "Recipient_Id", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.ChatMessages", "ApplicationUser_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
