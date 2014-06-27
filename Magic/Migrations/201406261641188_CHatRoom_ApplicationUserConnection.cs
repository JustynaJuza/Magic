namespace Magic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CHatRoom_ApplicationUserConnection : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ChatRoomApplicationUserConnections", "ChatRoom_Id", "dbo.ChatRooms");
            DropForeignKey("dbo.ChatRoomApplicationUserConnections", "ApplicationUserConnection_Id", "dbo.AspNetUserConnections");
            DropIndex("dbo.ChatRoomApplicationUserConnections", new[] { "ChatRoom_Id" });
            DropIndex("dbo.ChatRoomApplicationUserConnections", new[] { "ApplicationUserConnection_Id" });
            DropTable(name: "dbo.ChatRoomApplicationUserConnections");
            CreateTable(
                "dbo.ChatRoom_ApplicationUserConnection",
                c => new
                    {
                        ConnectionId = c.String(nullable: false, maxLength: 128),
                        ChatRoomId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ConnectionId, t.ChatRoomId })
                .ForeignKey("dbo.ChatRooms", t => t.ChatRoomId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUserConnections", t => t.ConnectionId, cascadeDelete: true)
                .Index(t => t.ChatRoomId)
                .Index(t => t.ConnectionId);
            
            AddColumn("dbo.AspNetUserConnections", "ChatRoom_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.AspNetUserConnections", "ChatRoom_Id");
            AddForeignKey("dbo.AspNetUserConnections", "ChatRoom_Id", "dbo.ChatRooms", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChatRoom_ApplicationUserConnection", "ConnectionId", "dbo.AspNetUserConnections");
            DropForeignKey("dbo.ChatRoom_ApplicationUserConnection", "ChatRoomId", "dbo.ChatRooms");
            DropForeignKey("dbo.AspNetUserConnections", "ChatRoom_Id", "dbo.ChatRooms");
            DropIndex("dbo.ChatRoom_ApplicationUserConnection", new[] { "ConnectionId" });
            DropIndex("dbo.ChatRoom_ApplicationUserConnection", new[] { "ChatRoomId" });
            DropIndex("dbo.AspNetUserConnections", new[] { "ChatRoom_Id" });
            DropColumn("dbo.AspNetUserConnections", "ChatRoom_Id");
            DropTable("dbo.ChatRoom_ApplicationUserConnection");
            CreateIndex("dbo.ChatRoomApplicationUserConnections", "ApplicationUserConnection_Id");
            CreateIndex("dbo.ChatRoomApplicationUserConnections", "ChatRoom_Id");
            AddForeignKey("dbo.ChatRoomApplicationUserConnections", "ApplicationUserConnection_Id", "dbo.AspNetUserConnections", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ChatRoomApplicationUserConnections", "ChatRoom_Id", "dbo.ChatRooms", "Id", cascadeDelete: true);
        }
    }
}
