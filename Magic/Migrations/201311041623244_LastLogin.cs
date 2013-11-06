namespace Magic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LastLogin : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CardTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MainType = c.Int(nullable: false),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.CardColors", "Color", c => c.Int(nullable: false));
            AddColumn("dbo.Cards", "CardType_Id", c => c.Int());
            AddColumn("dbo.AspNetUsers", "DateOfLastLogin", c => c.DateTime());
            AlterColumn("dbo.Cards", "Name", c => c.String(nullable: false));
            CreateIndex("dbo.Cards", "CardType_Id");
            AddForeignKey("dbo.Cards", "CardType_Id", "dbo.CardTypes", "Id");
            DropColumn("dbo.CardColors", "Name");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CardColors", "Name", c => c.String());
            DropForeignKey("dbo.Cards", "CardType_Id", "dbo.CardTypes");
            DropIndex("dbo.Cards", new[] { "CardType_Id" });
            AlterColumn("dbo.Cards", "Name", c => c.String());
            DropColumn("dbo.AspNetUsers", "DateOfLastLogin");
            DropColumn("dbo.Cards", "CardType_Id");
            DropColumn("dbo.CardColors", "Color");
            DropTable("dbo.CardTypes");
        }
    }
}
