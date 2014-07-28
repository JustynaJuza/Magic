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
                        DateSet = c.DateTime(nullable: false),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        ApplicationUser_Id = c.String(maxLength: 128),
                        ApplicationUser_Id1 = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RelatedUserId })
                .ForeignKey("dbo.AspNetUsers", t => t.RelatedUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id1)
                .Index(t => t.RelatedUserId)
                .Index(t => t.UserId)
                .Index(t => t.ApplicationUser_Id)
                .Index(t => t.ApplicationUser_Id1);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ApplicationUserRelations", "ApplicationUser_Id1", "dbo.AspNetUsers");
            DropForeignKey("dbo.ApplicationUserRelations", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.ApplicationUserRelations", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ApplicationUserRelations", "RelatedUserId", "dbo.AspNetUsers");
            DropIndex("dbo.ApplicationUserRelations", new[] { "ApplicationUser_Id1" });
            DropIndex("dbo.ApplicationUserRelations", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.ApplicationUserRelations", new[] { "UserId" });
            DropIndex("dbo.ApplicationUserRelations", new[] { "RelatedUserId" });
            DropTable("dbo.ApplicationUserRelations");
        }
    }
}
