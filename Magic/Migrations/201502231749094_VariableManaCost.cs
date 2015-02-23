namespace Magic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VariableManaCost : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CardManaCosts", "HasVariableCost", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CardManaCosts", "HasVariableCost");
        }
    }
}
