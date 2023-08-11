namespace PCShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateProductImage943 : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.ProductImage", "ProductId");
            AddForeignKey("dbo.ProductImage", "ProductId", "dbo.Product", "id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProductImage", "ProductId", "dbo.Product");
            DropIndex("dbo.ProductImage", new[] { "ProductId" });
        }
    }
}
