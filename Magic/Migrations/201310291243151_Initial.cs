namespace Magic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AspNetUsers", "DateCreated", c => c.DateTime(nullable: false, defaultValueSql: "GETDATE()"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsers", "DateCreated", c => c.DateTime(nullable: false, defaultValueSql: "GETDATE()"));
        }
    }
}
