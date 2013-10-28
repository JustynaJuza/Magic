namespace Magic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CardColors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Cards",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 25),
                    })
                .PrimaryKey(t => t.Id);
            
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CardCardColors", "CardColor_Id", "dbo.CardColors");
            DropForeignKey("dbo.CardCardColors", "Card_Id", "dbo.Cards");
            DropIndex("dbo.CardCardColors", new[] { "CardColor_Id" });
            DropIndex("dbo.CardCardColors", new[] { "Card_Id" });
            DropTable("dbo.CardCardColors");
            DropTable("dbo.Cards");
            DropTable("dbo.CardColors");
        }
    }
}
