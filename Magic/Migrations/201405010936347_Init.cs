namespace Magic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
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
                        Name = c.String(),
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
                .ForeignKey("dbo.AspNetUsers", t => t.Creator_Id)
                .Index(t => t.Creator_Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserName = c.String(),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        DateCreated = c.DateTime(defaultValueSql: "GETDATE()"),
                        LastLoginDate = c.DateTime(),
                        Title = c.String(),
                        Status = c.Int(),
                        Email = c.String(),
                        BirthDate = c.DateTime(),
                        Image = c.String(),
                        ColorCode = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                        User_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.LoginProvider, t.ProviderKey })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.RoleId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetUserConnections",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        ChatRoom_Id = c.String(maxLength: 128),
                        User_Id = c.String(maxLength: 128),
                        Game_Id = c.String(maxLength: 128),
                        ApplicationUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ChatRooms", t => t.ChatRoom_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .ForeignKey("dbo.Games", t => t.Game_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .Index(t => t.ChatRoom_Id)
                .Index(t => t.User_Id)
                .Index(t => t.Game_Id)
                .Index(t => t.ApplicationUser_Id);
            
            CreateTable(
                "dbo.ChatRooms",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        Log_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ChatLogs", t => t.Log_Id)
                .Index(t => t.Log_Id);
            
            CreateTable(
                "dbo.ChatLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DateCreated = c.DateTime(nullable: false, defaultValueSql: "GETDATE()"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ChatMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsUnread = c.Boolean(nullable: false, defaultValue: true),
                        TimeSend = c.DateTime(defaultValueSql: "GETDATE()"),
                        Message = c.String(),
                        Log_Id = c.Int(),
                        Recipient_Id = c.String(maxLength: 128),
                        Sender_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ChatLogs", t => t.Log_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Recipient_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Sender_Id)
                .Index(t => t.Log_Id)
                .Index(t => t.Recipient_Id)
                .Index(t => t.Sender_Id);
            
            CreateTable(
                "dbo.Games",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        DateStarted = c.DateTime(),
                        DateEnded = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PlayerGameStatus",
                c => new
                    {
                        GameId = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                        Status = c.Int(),
                    })
                .PrimaryKey(t => new { t.GameId, t.UserId })
                .ForeignKey("dbo.Games", t => t.GameId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.GameId)
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
            DropForeignKey("dbo.AspNetUserConnections", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.CardDecks", "Creator_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserConnections", "Game_Id", "dbo.Games");
            DropForeignKey("dbo.PlayerGameStatus", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PlayerGameStatus", "GameId", "dbo.Games");
            DropForeignKey("dbo.AspNetUserConnections", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserConnections", "ChatRoom_Id", "dbo.ChatRooms");
            DropForeignKey("dbo.ChatRooms", "Log_Id", "dbo.ChatLogs");
            DropForeignKey("dbo.ChatMessages", "Sender_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.ChatMessages", "Recipient_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.ChatMessages", "Log_Id", "dbo.ChatLogs");
            DropForeignKey("dbo.AspNetUserClaims", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
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
            DropIndex("dbo.AspNetUserConnections", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.CardDecks", new[] { "Creator_Id" });
            DropIndex("dbo.AspNetUserConnections", new[] { "Game_Id" });
            DropIndex("dbo.PlayerGameStatus", new[] { "UserId" });
            DropIndex("dbo.PlayerGameStatus", new[] { "GameId" });
            DropIndex("dbo.AspNetUserConnections", new[] { "User_Id" });
            DropIndex("dbo.AspNetUserConnections", new[] { "ChatRoom_Id" });
            DropIndex("dbo.ChatRooms", new[] { "Log_Id" });
            DropIndex("dbo.ChatMessages", new[] { "Sender_Id" });
            DropIndex("dbo.ChatMessages", new[] { "Recipient_Id" });
            DropIndex("dbo.ChatMessages", new[] { "Log_Id" });
            DropIndex("dbo.AspNetUserClaims", new[] { "User_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
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
            DropTable("dbo.PlayerGameStatus");
            DropTable("dbo.Games");
            DropTable("dbo.ChatMessages");
            DropTable("dbo.ChatLogs");
            DropTable("dbo.ChatRooms");
            DropTable("dbo.AspNetUserConnections");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
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
