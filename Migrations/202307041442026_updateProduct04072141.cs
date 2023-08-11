namespace PCShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateProduct04072141 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Product", "isSale", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Product", "isSale");
        }
    }
}
