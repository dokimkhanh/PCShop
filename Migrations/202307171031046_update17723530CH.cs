namespace PCShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update17723530CH : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tbOrder", "quantity", c => c.Int(nullable: false));
            DropColumn("dbo.tbOrder", "quatity");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tbOrder", "quatity", c => c.Int(nullable: false));
            DropColumn("dbo.tbOrder", "quantity");
        }
    }
}
