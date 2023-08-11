namespace PCShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Product", "alias", c => c.String());
            AddColumn("dbo.Product", "productCode", c => c.String());
            AddColumn("dbo.Product", "isNew", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Product", "isNew");
            DropColumn("dbo.Product", "productCode");
            DropColumn("dbo.Product", "alias");
        }
    }
}
