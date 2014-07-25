namespace Magic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChatRoomAllowedUsers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChatRoom_ApplicationUser",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        ChatRoomId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.ChatRoomId })
                .ForeignKey("dbo.ChatRooms", t => t.ChatRoomId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.ChatRoomId)
                .Index(t => t.UserId);
        }
        
        public override void Down()
        {            
            DropForeignKey("dbo.ChatRoom_ApplicationUser", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ChatRoom_ApplicationUser", "ChatRoomId", "dbo.ChatRooms");
            DropIndex("dbo.ChatRoom_ApplicationUser", new[] { "UserId" });
            DropIndex("dbo.ChatRoom_ApplicationUser", new[] { "ChatRoomId" });
            DropTable("dbo.ChatRoom_ApplicationUser");
        }
    }
}
