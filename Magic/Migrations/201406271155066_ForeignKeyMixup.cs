namespace Magic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ForeignKeyMixup : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ChatRoom_ApplicationUserConnection", new[] { "Connection_Id", "Connection_UserId" }, "dbo.AspNetUserConnections");
            DropIndex("dbo.ChatRoom_ApplicationUserConnection", new[] { "Connection_Id", "Connection_UserId" });
            AddColumn("dbo.ChatRoom_ApplicationUserConnection", "UserId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.AspNetUserConnections", "Id", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.ChatRoom_ApplicationUserConnection", new[] { "ConnectionId", "UserId" });
            AddForeignKey("dbo.ChatRoom_ApplicationUserConnection", new[] { "ConnectionId", "UserId" }, "dbo.AspNetUserConnections", new[] { "Id", "UserId" }, cascadeDelete: true);
            DropColumn("dbo.ChatRoom_ApplicationUserConnection", "Connection_Id");
            DropColumn("dbo.ChatRoom_ApplicationUserConnection", "Connection_UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ChatRoom_ApplicationUserConnection", "Connection_UserId", c => c.String(maxLength: 128));
            AddColumn("dbo.ChatRoom_ApplicationUserConnection", "Connection_Id", c => c.String(maxLength: 128));
            DropForeignKey("dbo.ChatRoom_ApplicationUserConnection", new[] { "ConnectionId", "UserId" }, "dbo.AspNetUserConnections");
            DropIndex("dbo.ChatRoom_ApplicationUserConnection", new[] { "ConnectionId", "UserId" });
            AlterColumn("dbo.AspNetUserConnections", "Id", c => c.String(nullable: false, maxLength: 128));
            DropColumn("dbo.ChatRoom_ApplicationUserConnection", "UserId");
            CreateIndex("dbo.ChatRoom_ApplicationUserConnection", new[] { "Connection_Id", "Connection_UserId" });
            AddForeignKey("dbo.ChatRoom_ApplicationUserConnection", new[] { "Connection_Id", "Connection_UserId" }, "dbo.AspNetUserConnections", new[] { "Id", "UserId" });
        }
    }
}
