namespace Magic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ImgePreview : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Cards", "ImagePreview", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Cards", "ImagePreview");
        }
    }
}
