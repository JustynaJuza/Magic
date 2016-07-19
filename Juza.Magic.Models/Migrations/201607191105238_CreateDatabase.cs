namespace Juza.Magic.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CardDecks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        DateCreated = c.DateTime(nullable: false, precision: 0, storeType: "datetime2"),
                        Creator_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.Creator_Id)
                .Index(t => t.Creator_Id);
            
            CreateTable(
                "dbo.Cards",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MultiverseId = c.Int(nullable: false),
                        Name = c.String(nullable: false),
                        SetId = c.String(maxLength: 128),
                        SetNumber = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false, precision: 0, storeType: "datetime2"),
                        DateReleased = c.DateTime(nullable: false, precision: 0, storeType: "datetime2"),
                        Image = c.String(),
                        ImagePreview = c.String(),
                        Artist = c.String(),
                        IsPermanent = c.Boolean(nullable: false),
                        IsTapped = c.Boolean(nullable: false),
                        Rarity = c.Int(nullable: false),
                        ConvertedManaCost = c.Int(nullable: false),
                        Description = c.String(),
                        Flavor = c.String(),
                        IsToken = c.Boolean(),
                        Power = c.Int(),
                        Toughness = c.Int(),
                        Loyalty = c.Int(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CardSets", t => t.SetId)
                .Index(t => t.SetId);
            
            CreateTable(
                "dbo.CardAvailableAbilities",
                c => new
                    {
                        CardId = c.Int(nullable: false),
                        AbilityId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CardId, t.AbilityId })
                .ForeignKey("dbo.CardAbilities", t => t.AbilityId, cascadeDelete: true)
                .ForeignKey("dbo.Cards", t => t.CardId, cascadeDelete: true)
                .Index(t => t.CardId)
                .Index(t => t.AbilityId);
            
            CreateTable(
                "dbo.CardAbilities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Reminder = c.String(),
                        CallbackName = c.String(),
                        RequiresTap = c.Boolean(),
                        PlayerTargets = c.Int(),
                        CardTargets = c.Int(),
                        TargetsRequired = c.Int(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CardManaCosts",
                c => new
                    {
                        CardId = c.Int(nullable: false),
                        ColorId = c.Int(nullable: false),
                        Cost = c.Int(nullable: false),
                        HasVariableCost = c.Boolean(nullable: false),
                        HybridColorId = c.Int(),
                        Discriminator = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CardId, t.ColorId })
                .ForeignKey("dbo.Cards", t => t.CardId, cascadeDelete: true)
                .ForeignKey("dbo.ManaColors", t => t.ColorId, cascadeDelete: true)
                .ForeignKey("dbo.ManaColors", t => t.HybridColorId)
                .Index(t => t.CardId)
                .Index(t => t.ColorId)
                .Index(t => t.HybridColorId);
            
            CreateTable(
                "dbo.ManaColors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Alias = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CardTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Discriminator = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CardSets",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        Type = c.String(),
                        Block = c.String(),
                        Description = c.String(),
                        DateReleased = c.DateTime(nullable: false, precision: 0, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DateCreated = c.DateTime(nullable: false, precision: 0, storeType: "datetime2"),
                        LastLoginDate = c.DateTime(precision: 0, storeType: "datetime2"),
                        Title = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Status = c.Int(nullable: false),
                        BirthDate = c.DateTime(precision: 0, storeType: "datetime2"),
                        IsFemale = c.Boolean(nullable: false),
                        Country = c.String(),
                        Image = c.String(),
                        ColorCode = c.String(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(precision: 0, storeType: "datetime2"),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.ChatMessageNotifications",
                c => new
                    {
                        MessageId = c.Int(nullable: false),
                        LogId = c.String(nullable: false, maxLength: 128),
                        RecipientId = c.Int(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.MessageId, t.LogId, t.RecipientId })
                .ForeignKey("dbo.ChatMessages", t => new { t.MessageId, t.LogId })
                .ForeignKey("dbo.Users", t => t.RecipientId, cascadeDelete: true)
                .Index(t => new { t.MessageId, t.LogId })
                .Index(t => t.RecipientId);
            
            CreateTable(
                "dbo.ChatMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LogId = c.String(nullable: false, maxLength: 128),
                        SenderId = c.Int(nullable: false),
                        TimeSent = c.DateTime(nullable: false, precision: 0, storeType: "datetime2"),
                        Message = c.String(),
                    })
                .PrimaryKey(t => new { t.Id, t.LogId })
                .ForeignKey("dbo.ChatLogs", t => t.LogId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.SenderId, cascadeDelete: true)
                .Index(t => t.LogId)
                .Index(t => t.SenderId);
            
            CreateTable(
                "dbo.ChatLogs",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        DateCreated = c.DateTime(nullable: false, precision: 0, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserConnections",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserId = c.Int(nullable: false),
                        GameId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Id, t.UserId })
                .ForeignKey("dbo.Games", t => t.GameId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.GameId);
            
            CreateTable(
                "dbo.Games",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        IsPrivate = c.Boolean(nullable: false),
                        TimePlayed = c.Time(nullable: false, precision: 7),
                        DateStarted = c.DateTime(precision: 0, storeType: "datetime2"),
                        DateResumed = c.DateTime(precision: 0, storeType: "datetime2"),
                        DateEnded = c.DateTime(precision: 0, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.GamePlayerStatus",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        GameId = c.String(nullable: false, maxLength: 128),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.GameId })
                .ForeignKey("dbo.Games", t => t.GameId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.GameId);
            
            CreateTable(
                "dbo.Players",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        GameId = c.String(nullable: false, maxLength: 128),
                        HealthTotal = c.Int(nullable: false),
                        HealthCurrent = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.GameId })
                .ForeignKey("dbo.Games", t => t.GameId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.GamePlayerStatus", t => new { t.UserId, t.GameId }, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => new { t.UserId, t.GameId })
                .Index(t => t.GameId);
            
            CreateTable(
                "dbo.PlayerCardDecks",
                c => new
                    {
                        DeckId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        GameId = c.String(nullable: false, maxLength: 128),
                        CardsPlayed = c.Int(nullable: false),
                        Player_UserId = c.Int(nullable: false),
                        Player_GameId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.DeckId, t.UserId, t.GameId })
                .ForeignKey("dbo.CardDecks", t => t.DeckId, cascadeDelete: true)
                .ForeignKey("dbo.Players", t => new { t.Player_UserId, t.Player_GameId })
                .Index(t => t.DeckId)
                .Index(t => new { t.Player_UserId, t.Player_GameId });
            
            CreateTable(
                "dbo.PlayerCards",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        GameId = c.String(nullable: false, maxLength: 128),
                        CardId = c.Int(nullable: false),
                        DeckId = c.Int(nullable: false),
                        Index = c.Int(nullable: false),
                        Location = c.Int(nullable: false),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.GameId, t.CardId })
                .ForeignKey("dbo.Cards", t => t.CardId, cascadeDelete: true)
                .ForeignKey("dbo.PlayerCardDecks", t => new { t.DeckId, t.UserId, t.GameId })
                .ForeignKey("dbo.Players", t => new { t.UserId, t.GameId }, cascadeDelete: true)
                .Index(t => new { t.DeckId, t.UserId, t.GameId })
                .Index(t => t.CardId);
            
            CreateTable(
                "dbo.UserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserRelations",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        RelatedUserId = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false, precision: 0, storeType: "datetime2"),
                        Discriminator = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.RelatedUserId })
                .ForeignKey("dbo.Users", t => t.RelatedUserId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RelatedUserId);
            
            CreateTable(
                "dbo.UserRoles",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.Roles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.ChatRoomConnections",
                c => new
                    {
                        ConnectionId = c.String(nullable: false, maxLength: 128),
                        UserId = c.Int(nullable: false),
                        ChatRoomId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ConnectionId, t.UserId, t.ChatRoomId })
                .ForeignKey("dbo.ChatRooms", t => t.ChatRoomId, cascadeDelete: true)
                .ForeignKey("dbo.UserConnections", t => new { t.ConnectionId, t.UserId }, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => new { t.ConnectionId, t.UserId })
                .Index(t => t.ChatRoomId);
            
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
                        UserId = c.Int(nullable: false),
                        ChatRoomId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.ChatRoomId })
                .ForeignKey("dbo.ChatRooms", t => t.ChatRoomId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.ChatRoomId);
            
            CreateTable(
                "dbo.ActiveAbilityCardManaCosts",
                c => new
                    {
                        ActiveAbility_Id = c.Int(nullable: false),
                        CardManaCost_CardId = c.Int(nullable: false),
                        CardManaCost_ColorId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ActiveAbility_Id, t.CardManaCost_CardId, t.CardManaCost_ColorId })
                .ForeignKey("dbo.CardAbilities", t => t.ActiveAbility_Id, cascadeDelete: true)
                .ForeignKey("dbo.CardManaCosts", t => new { t.CardManaCost_CardId, t.CardManaCost_ColorId }, cascadeDelete: true)
                .Index(t => t.ActiveAbility_Id)
                .Index(t => new { t.CardManaCost_CardId, t.CardManaCost_ColorId });
            
            CreateTable(
                "dbo.CardTypeCards",
                c => new
                    {
                        CardType_Id = c.Int(nullable: false),
                        Card_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CardType_Id, t.Card_Id })
                .ForeignKey("dbo.CardTypes", t => t.CardType_Id, cascadeDelete: true)
                .ForeignKey("dbo.Cards", t => t.Card_Id, cascadeDelete: true)
                .Index(t => t.CardType_Id)
                .Index(t => t.Card_Id);
            
            CreateTable(
                "dbo.TargetAbilityCardTypes",
                c => new
                    {
                        TargetAbility_Id = c.Int(nullable: false),
                        CardType_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.TargetAbility_Id, t.CardType_Id })
                .ForeignKey("dbo.CardAbilities", t => t.TargetAbility_Id, cascadeDelete: true)
                .ForeignKey("dbo.CardTypes", t => t.CardType_Id, cascadeDelete: true)
                .Index(t => t.TargetAbility_Id)
                .Index(t => t.CardType_Id);
            
            CreateTable(
                "dbo.CardDeckCards",
                c => new
                    {
                        CardDeck_Id = c.Int(nullable: false),
                        Card_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CardDeck_Id, t.Card_Id })
                .ForeignKey("dbo.CardDecks", t => t.CardDeck_Id, cascadeDelete: true)
                .ForeignKey("dbo.Cards", t => t.Card_Id, cascadeDelete: true)
                .Index(t => t.CardDeck_Id)
                .Index(t => t.Card_Id);
            
            CreateTable(
                "dbo.CardDeckManaColors",
                c => new
                    {
                        CardDeck_Id = c.Int(nullable: false),
                        ManaColor_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CardDeck_Id, t.ManaColor_Id })
                .ForeignKey("dbo.CardDecks", t => t.CardDeck_Id, cascadeDelete: true)
                .ForeignKey("dbo.ManaColors", t => t.ManaColor_Id, cascadeDelete: true)
                .Index(t => t.CardDeck_Id)
                .Index(t => t.ManaColor_Id);
            
            CreateTable(
                "dbo.CardDeckUsers",
                c => new
                    {
                        CardDeck_Id = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CardDeck_Id, t.User_Id })
                .ForeignKey("dbo.CardDecks", t => t.CardDeck_Id, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.CardDeck_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChatRoomConnections", "UserId", "dbo.Users");
            DropForeignKey("dbo.ChatRoomConnections", new[] { "ConnectionId", "UserId" }, "dbo.UserConnections");
            DropForeignKey("dbo.ChatRoomConnections", "ChatRoomId", "dbo.ChatRooms");
            DropForeignKey("dbo.ChatRoomUsers", "UserId", "dbo.Users");
            DropForeignKey("dbo.ChatRoomUsers", "ChatRoomId", "dbo.ChatRooms");
            DropForeignKey("dbo.ChatRooms", "Log_Id", "dbo.ChatLogs");
            DropForeignKey("dbo.CardDeckUsers", "User_Id", "dbo.Users");
            DropForeignKey("dbo.CardDeckUsers", "CardDeck_Id", "dbo.CardDecks");
            DropForeignKey("dbo.CardDecks", "Creator_Id", "dbo.Users");
            DropForeignKey("dbo.UserRoles", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserRoles", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.UserRelations", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserRelations", "RelatedUserId", "dbo.Users");
            DropForeignKey("dbo.UserLogins", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserConnections", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserConnections", "GameId", "dbo.Games");
            DropForeignKey("dbo.GamePlayerStatus", "UserId", "dbo.Users");
            DropForeignKey("dbo.Players", new[] { "UserId", "GameId" }, "dbo.GamePlayerStatus");
            DropForeignKey("dbo.Players", "UserId", "dbo.Users");
            DropForeignKey("dbo.Players", "GameId", "dbo.Games");
            DropForeignKey("dbo.PlayerCardDecks", new[] { "Player_UserId", "Player_GameId" }, "dbo.Players");
            DropForeignKey("dbo.PlayerCardDecks", "DeckId", "dbo.CardDecks");
            DropForeignKey("dbo.PlayerCards", new[] { "UserId", "GameId" }, "dbo.Players");
            DropForeignKey("dbo.PlayerCards", new[] { "DeckId", "UserId", "GameId" }, "dbo.PlayerCardDecks");
            DropForeignKey("dbo.PlayerCards", "CardId", "dbo.Cards");
            DropForeignKey("dbo.GamePlayerStatus", "GameId", "dbo.Games");
            DropForeignKey("dbo.UserClaims", "UserId", "dbo.Users");
            DropForeignKey("dbo.ChatMessageNotifications", "RecipientId", "dbo.Users");
            DropForeignKey("dbo.ChatMessageNotifications", new[] { "MessageId", "LogId" }, "dbo.ChatMessages");
            DropForeignKey("dbo.ChatMessages", "SenderId", "dbo.Users");
            DropForeignKey("dbo.ChatMessages", "LogId", "dbo.ChatLogs");
            DropForeignKey("dbo.CardDeckManaColors", "ManaColor_Id", "dbo.ManaColors");
            DropForeignKey("dbo.CardDeckManaColors", "CardDeck_Id", "dbo.CardDecks");
            DropForeignKey("dbo.CardDeckCards", "Card_Id", "dbo.Cards");
            DropForeignKey("dbo.CardDeckCards", "CardDeck_Id", "dbo.CardDecks");
            DropForeignKey("dbo.Cards", "SetId", "dbo.CardSets");
            DropForeignKey("dbo.CardAvailableAbilities", "CardId", "dbo.Cards");
            DropForeignKey("dbo.CardAvailableAbilities", "AbilityId", "dbo.CardAbilities");
            DropForeignKey("dbo.TargetAbilityCardTypes", "CardType_Id", "dbo.CardTypes");
            DropForeignKey("dbo.TargetAbilityCardTypes", "TargetAbility_Id", "dbo.CardAbilities");
            DropForeignKey("dbo.CardTypeCards", "Card_Id", "dbo.Cards");
            DropForeignKey("dbo.CardTypeCards", "CardType_Id", "dbo.CardTypes");
            DropForeignKey("dbo.ActiveAbilityCardManaCosts", new[] { "CardManaCost_CardId", "CardManaCost_ColorId" }, "dbo.CardManaCosts");
            DropForeignKey("dbo.ActiveAbilityCardManaCosts", "ActiveAbility_Id", "dbo.CardAbilities");
            DropForeignKey("dbo.CardManaCosts", "HybridColorId", "dbo.ManaColors");
            DropForeignKey("dbo.CardManaCosts", "ColorId", "dbo.ManaColors");
            DropForeignKey("dbo.CardManaCosts", "CardId", "dbo.Cards");
            DropIndex("dbo.CardDeckUsers", new[] { "User_Id" });
            DropIndex("dbo.CardDeckUsers", new[] { "CardDeck_Id" });
            DropIndex("dbo.CardDeckManaColors", new[] { "ManaColor_Id" });
            DropIndex("dbo.CardDeckManaColors", new[] { "CardDeck_Id" });
            DropIndex("dbo.CardDeckCards", new[] { "Card_Id" });
            DropIndex("dbo.CardDeckCards", new[] { "CardDeck_Id" });
            DropIndex("dbo.TargetAbilityCardTypes", new[] { "CardType_Id" });
            DropIndex("dbo.TargetAbilityCardTypes", new[] { "TargetAbility_Id" });
            DropIndex("dbo.CardTypeCards", new[] { "Card_Id" });
            DropIndex("dbo.CardTypeCards", new[] { "CardType_Id" });
            DropIndex("dbo.ActiveAbilityCardManaCosts", new[] { "CardManaCost_CardId", "CardManaCost_ColorId" });
            DropIndex("dbo.ActiveAbilityCardManaCosts", new[] { "ActiveAbility_Id" });
            DropIndex("dbo.ChatRoomUsers", new[] { "ChatRoomId" });
            DropIndex("dbo.ChatRoomUsers", new[] { "UserId" });
            DropIndex("dbo.ChatRooms", new[] { "Log_Id" });
            DropIndex("dbo.ChatRoomConnections", new[] { "ChatRoomId" });
            DropIndex("dbo.ChatRoomConnections", new[] { "ConnectionId", "UserId" });
            DropIndex("dbo.Roles", "RoleNameIndex");
            DropIndex("dbo.UserRoles", new[] { "RoleId" });
            DropIndex("dbo.UserRoles", new[] { "UserId" });
            DropIndex("dbo.UserRelations", new[] { "RelatedUserId" });
            DropIndex("dbo.UserRelations", new[] { "UserId" });
            DropIndex("dbo.UserLogins", new[] { "UserId" });
            DropIndex("dbo.PlayerCards", new[] { "CardId" });
            DropIndex("dbo.PlayerCards", new[] { "DeckId", "UserId", "GameId" });
            DropIndex("dbo.PlayerCardDecks", new[] { "Player_UserId", "Player_GameId" });
            DropIndex("dbo.PlayerCardDecks", new[] { "DeckId" });
            DropIndex("dbo.Players", new[] { "GameId" });
            DropIndex("dbo.Players", new[] { "UserId", "GameId" });
            DropIndex("dbo.Players", new[] { "UserId" });
            DropIndex("dbo.GamePlayerStatus", new[] { "GameId" });
            DropIndex("dbo.GamePlayerStatus", new[] { "UserId" });
            DropIndex("dbo.UserConnections", new[] { "GameId" });
            DropIndex("dbo.UserConnections", new[] { "UserId" });
            DropIndex("dbo.UserClaims", new[] { "UserId" });
            DropIndex("dbo.ChatMessages", new[] { "SenderId" });
            DropIndex("dbo.ChatMessages", new[] { "LogId" });
            DropIndex("dbo.ChatMessageNotifications", new[] { "RecipientId" });
            DropIndex("dbo.ChatMessageNotifications", new[] { "MessageId", "LogId" });
            DropIndex("dbo.Users", "UserNameIndex");
            DropIndex("dbo.CardManaCosts", new[] { "HybridColorId" });
            DropIndex("dbo.CardManaCosts", new[] { "ColorId" });
            DropIndex("dbo.CardManaCosts", new[] { "CardId" });
            DropIndex("dbo.CardAvailableAbilities", new[] { "AbilityId" });
            DropIndex("dbo.CardAvailableAbilities", new[] { "CardId" });
            DropIndex("dbo.Cards", new[] { "SetId" });
            DropIndex("dbo.CardDecks", new[] { "Creator_Id" });
            DropTable("dbo.CardDeckUsers");
            DropTable("dbo.CardDeckManaColors");
            DropTable("dbo.CardDeckCards");
            DropTable("dbo.TargetAbilityCardTypes");
            DropTable("dbo.CardTypeCards");
            DropTable("dbo.ActiveAbilityCardManaCosts");
            DropTable("dbo.ChatRoomUsers");
            DropTable("dbo.ChatRooms");
            DropTable("dbo.ChatRoomConnections");
            DropTable("dbo.Roles");
            DropTable("dbo.UserRoles");
            DropTable("dbo.UserRelations");
            DropTable("dbo.UserLogins");
            DropTable("dbo.PlayerCards");
            DropTable("dbo.PlayerCardDecks");
            DropTable("dbo.Players");
            DropTable("dbo.GamePlayerStatus");
            DropTable("dbo.Games");
            DropTable("dbo.UserConnections");
            DropTable("dbo.UserClaims");
            DropTable("dbo.ChatLogs");
            DropTable("dbo.ChatMessages");
            DropTable("dbo.ChatMessageNotifications");
            DropTable("dbo.Users");
            DropTable("dbo.CardSets");
            DropTable("dbo.CardTypes");
            DropTable("dbo.ManaColors");
            DropTable("dbo.CardManaCosts");
            DropTable("dbo.CardAbilities");
            DropTable("dbo.CardAvailableAbilities");
            DropTable("dbo.Cards");
            DropTable("dbo.CardDecks");
        }
    }
}