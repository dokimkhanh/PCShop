namespace PCShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateProduct851 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Product", "title", c => c.String(nullable: false));
            AlterColumn("dbo.Product", "productCode", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Product", "productCode", c => c.String());
            AlterColumn("dbo.Product", "title", c => c.String());
        }
    }
}
