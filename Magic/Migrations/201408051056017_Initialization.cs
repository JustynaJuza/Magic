namespace Magic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initialization : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CardColors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Color = c.Int(nullable: false),
                        CardAbility_Id = c.String(maxLength: 128),
                        CardDeck_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CardAbilities", t => t.CardAbility_Id)
                .ForeignKey("dbo.CardDecks", t => t.CardDeck_Id)
                .Index(t => t.CardAbility_Id)
                .Index(t => t.CardDeck_Id);
            
            CreateTable(
                "dbo.Cards",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Image = c.String(),
                        Power = c.Int(),
                        Toughness = c.Int(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        Type_Id = c.Int(),
                        CardDeck_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CardTypes", t => t.Type_Id)
                .ForeignKey("dbo.CardDecks", t => t.CardDeck_Id)
                .Index(t => t.Type_Id)
                .Index(t => t.CardDeck_Id);
            
            CreateTable(
                "dbo.CardAbilities",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        Card_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Cards", t => t.Card_Id)
                .Index(t => t.Card_Id);
            
            CreateTable(
                "dbo.CardTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MainType_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CardMainTypes", t => t.MainType_Id)
                .Index(t => t.MainType_Id);
            
            CreateTable(
                "dbo.CardMainTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Type = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CardSubTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CardDecks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        Creator_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.Creator_Id)
                .Index(t => t.Creator_Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserName = c.String(),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        DateCreated = c.DateTime(),
                        LastLoginDate = c.DateTime(),
                        Title = c.String(),
                        Status = c.Int(),
                        Email = c.String(),
                        BirthDate = c.DateTime(),
                        IsFemale = c.Boolean(),
                        Country = c.String(),
                        Image = c.String(),
                        ColorCode = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MessageRecipients",
                c => new
                    {
                        MessageId = c.Int(nullable: false),
                        LogId = c.String(nullable: false, maxLength: 128),
                        RecipientId = c.String(nullable: false, maxLength: 128),
                        IsRead = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.MessageId, t.LogId, t.RecipientId })
                .ForeignKey("dbo.ChatMessages", t => new { t.MessageId, t.LogId }, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.RecipientId, cascadeDelete: true)
                .Index(t => new { t.MessageId, t.LogId })
                .Index(t => t.RecipientId);
            
            CreateTable(
                "dbo.ChatMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LogId = c.String(nullable: false, maxLength: 128),
                        SenderId = c.String(maxLength: 128),
                        TimeSend = c.DateTime(),
                        Message = c.String(),
                    })
                .PrimaryKey(t => new { t.Id, t.LogId })
                .ForeignKey("dbo.ChatLogs", t => t.LogId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.SenderId)
                .Index(t => t.LogId)
                .Index(t => t.SenderId);
            
            CreateTable(
                "dbo.ChatLogs",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                        User_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.UserLogins",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.LoginProvider, t.ProviderKey })
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.Roles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.RoleId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserConnections",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                        GameId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Id, t.UserId })
                .ForeignKey("dbo.Games", t => t.GameId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.GameId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Games",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        IsPrivate = c.Boolean(nullable: false),
                        DateStarted = c.DateTime(),
                        DateEnded = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.GameUsers",
                c => new
                    {
                        GameId = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                        Status = c.Int(),
                    })
                .PrimaryKey(t => new { t.GameId, t.UserId })
                .ForeignKey("dbo.Games", t => t.GameId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.GameId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserRelations",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RelatedUserId = c.String(nullable: false, maxLength: 128),
                        DateCreated = c.DateTime(nullable: false),
                        Discriminator = c.Int(),
                    })
                .PrimaryKey(t => new { t.UserId, t.RelatedUserId })
                .ForeignKey("dbo.Users", t => t.RelatedUserId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.RelatedUserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.ChatRoomUserConnections",
                c => new
                    {
                        ConnectionId = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ChatRoomId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ConnectionId, t.UserId, t.ChatRoomId })
                .ForeignKey("dbo.ChatRooms", t => t.ChatRoomId, cascadeDelete: true)
                .ForeignKey("dbo.UserConnections", t => new { t.ConnectionId, t.UserId }, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.ChatRoomId)
                .Index(t => new { t.ConnectionId, t.UserId })
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.ChatRooms",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        TabColorCode = c.String(),
                        IsGameRoom = c.Boolean(nullable: false),
                        IsPrivate = c.Boolean(nullable: false),
                        Log_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ChatLogs", t => t.Log_Id)
                .Index(t => t.Log_Id);
            
            CreateTable(
                "dbo.ChatRoomUsers",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        ChatRoomId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.ChatRoomId })
                .ForeignKey("dbo.ChatRooms", t => t.ChatRoomId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.ChatRoomId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.CardCardColors",
                c => new
                    {
                        Card_Id = c.Int(nullable: false),
                        CardColor_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Card_Id, t.CardColor_Id })
                .ForeignKey("dbo.Cards", t => t.Card_Id, cascadeDelete: true)
                .ForeignKey("dbo.CardColors", t => t.CardColor_Id, cascadeDelete: true)
                .Index(t => t.Card_Id)
                .Index(t => t.CardColor_Id);
            
            CreateTable(
                "dbo.CardSubTypeCardMainTypes",
                c => new
                    {
                        CardSubType_Id = c.Int(nullable: false),
                        CardMainType_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CardSubType_Id, t.CardMainType_Id })
                .ForeignKey("dbo.CardSubTypes", t => t.CardSubType_Id, cascadeDelete: true)
                .ForeignKey("dbo.CardMainTypes", t => t.CardMainType_Id, cascadeDelete: true)
                .Index(t => t.CardSubType_Id)
                .Index(t => t.CardMainType_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChatRoomUserConnections", "UserId", "dbo.Users");
            DropForeignKey("dbo.ChatRoomUserConnections", new[] { "ConnectionId", "UserId" }, "dbo.UserConnections");
            DropForeignKey("dbo.ChatRoomUserConnections", "ChatRoomId", "dbo.ChatRooms");
            DropForeignKey("dbo.ChatRoomUsers", "UserId", "dbo.Users");
            DropForeignKey("dbo.ChatRoomUsers", "ChatRoomId", "dbo.ChatRooms");
            DropForeignKey("dbo.ChatRooms", "Log_Id", "dbo.ChatLogs");
            DropForeignKey("dbo.UserRelations", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserRelations", "RelatedUserId", "dbo.Users");
            DropForeignKey("dbo.CardDecks", "Creator_Id", "dbo.Users");
            DropForeignKey("dbo.UserConnections", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserConnections", "GameId", "dbo.Games");
            DropForeignKey("dbo.GameUsers", "UserId", "dbo.Users");
            DropForeignKey("dbo.GameUsers", "GameId", "dbo.Games");
            DropForeignKey("dbo.UserClaims", "User_Id", "dbo.Users");
            DropForeignKey("dbo.UserRoles", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserRoles", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.UserLogins", "UserId", "dbo.Users");
            DropForeignKey("dbo.MessageRecipients", "RecipientId", "dbo.Users");
            DropForeignKey("dbo.MessageRecipients", new[] { "MessageId", "LogId" }, "dbo.ChatMessages");
            DropForeignKey("dbo.ChatMessages", "SenderId", "dbo.Users");
            DropForeignKey("dbo.ChatMessages", "LogId", "dbo.ChatLogs");
            DropForeignKey("dbo.Cards", "CardDeck_Id", "dbo.CardDecks");
            DropForeignKey("dbo.CardColors", "CardDeck_Id", "dbo.CardDecks");
            DropForeignKey("dbo.Cards", "Type_Id", "dbo.CardTypes");
            DropForeignKey("dbo.CardTypes", "MainType_Id", "dbo.CardMainTypes");
            DropForeignKey("dbo.CardSubTypeCardMainTypes", "CardMainType_Id", "dbo.CardMainTypes");
            DropForeignKey("dbo.CardSubTypeCardMainTypes", "CardSubType_Id", "dbo.CardSubTypes");
            DropForeignKey("dbo.CardCardColors", "CardColor_Id", "dbo.CardColors");
            DropForeignKey("dbo.CardCardColors", "Card_Id", "dbo.Cards");
            DropForeignKey("dbo.CardAbilities", "Card_Id", "dbo.Cards");
            DropForeignKey("dbo.CardColors", "CardAbility_Id", "dbo.CardAbilities");
            DropIndex("dbo.ChatRoomUserConnections", new[] { "UserId" });
            DropIndex("dbo.ChatRoomUserConnections", new[] { "ConnectionId", "UserId" });
            DropIndex("dbo.ChatRoomUserConnections", new[] { "ChatRoomId" });
            DropIndex("dbo.ChatRoomUsers", new[] { "UserId" });
            DropIndex("dbo.ChatRoomUsers", new[] { "ChatRoomId" });
            DropIndex("dbo.ChatRooms", new[] { "Log_Id" });
            DropIndex("dbo.UserRelations", new[] { "UserId" });
            DropIndex("dbo.UserRelations", new[] { "RelatedUserId" });
            DropIndex("dbo.CardDecks", new[] { "Creator_Id" });
            DropIndex("dbo.UserConnections", new[] { "UserId" });
            DropIndex("dbo.UserConnections", new[] { "GameId" });
            DropIndex("dbo.GameUsers", new[] { "UserId" });
            DropIndex("dbo.GameUsers", new[] { "GameId" });
            DropIndex("dbo.UserClaims", new[] { "User_Id" });
            DropIndex("dbo.UserRoles", new[] { "UserId" });
            DropIndex("dbo.UserRoles", new[] { "RoleId" });
            DropIndex("dbo.UserLogins", new[] { "UserId" });
            DropIndex("dbo.MessageRecipients", new[] { "RecipientId" });
            DropIndex("dbo.MessageRecipients", new[] { "MessageId", "LogId" });
            DropIndex("dbo.ChatMessages", new[] { "SenderId" });
            DropIndex("dbo.ChatMessages", new[] { "LogId" });
            DropIndex("dbo.Cards", new[] { "CardDeck_Id" });
            DropIndex("dbo.CardColors", new[] { "CardDeck_Id" });
            DropIndex("dbo.Cards", new[] { "Type_Id" });
            DropIndex("dbo.CardTypes", new[] { "MainType_Id" });
            DropIndex("dbo.CardSubTypeCardMainTypes", new[] { "CardMainType_Id" });
            DropIndex("dbo.CardSubTypeCardMainTypes", new[] { "CardSubType_Id" });
            DropIndex("dbo.CardCardColors", new[] { "CardColor_Id" });
            DropIndex("dbo.CardCardColors", new[] { "Card_Id" });
            DropIndex("dbo.CardAbilities", new[] { "Card_Id" });
            DropIndex("dbo.CardColors", new[] { "CardAbility_Id" });
            DropTable("dbo.CardSubTypeCardMainTypes");
            DropTable("dbo.CardCardColors");
            DropTable("dbo.ChatRoomUsers");
            DropTable("dbo.ChatRooms");
            DropTable("dbo.ChatRoomUserConnections");
            DropTable("dbo.UserRelations");
            DropTable("dbo.GameUsers");
            DropTable("dbo.Games");
            DropTable("dbo.UserConnections");
            DropTable("dbo.Roles");
            DropTable("dbo.UserRoles");
            DropTable("dbo.UserLogins");
            DropTable("dbo.UserClaims");
            DropTable("dbo.ChatLogs");
            DropTable("dbo.ChatMessages");
            DropTable("dbo.MessageRecipients");
            DropTable("dbo.Users");
            DropTable("dbo.CardDecks");
            DropTable("dbo.CardSubTypes");
            DropTable("dbo.CardMainTypes");
            DropTable("dbo.CardTypes");
            DropTable("dbo.CardAbilities");
            DropTable("dbo.Cards");
            DropTable("dbo.CardColors");
        }
    }
}
