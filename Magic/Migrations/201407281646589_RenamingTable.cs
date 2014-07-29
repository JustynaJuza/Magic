namespace Magic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenamingTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ApplicationUserRelations",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RelatedUserId = c.String(nullable: false, maxLength: 128),
                        DateSet = c.DateTime(nullable: false, defaultValueSql: "GETDATE()"),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RelatedUserId })
                .ForeignKey("dbo.AspNetUsers", t => t.RelatedUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.RelatedUserId)
                .Index(t => t.UserId);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ApplicationUserRelations", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ApplicationUserRelations", "RelatedUserId", "dbo.AspNetUsers");
            DropIndex("dbo.ApplicationUserRelations", new[] { "UserId" });
            DropIndex("dbo.ApplicationUserRelations", new[] { "RelatedUserId" });
            DropTable("dbo.ApplicationUserRelations");
        }
    }
}
