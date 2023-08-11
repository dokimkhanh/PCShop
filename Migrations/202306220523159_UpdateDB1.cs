namespace PCShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDB1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Adv", "createdBy", c => c.String(nullable: false));
            AlterColumn("dbo.Category", "createdBy", c => c.String(nullable: false));
            AlterColumn("dbo.Product", "createdBy", c => c.String(nullable: false));
            AlterColumn("dbo.News", "createdBy", c => c.String(nullable: false));
            AlterColumn("dbo.Order", "createdBy", c => c.String(nullable: false));
            AlterColumn("dbo.OrderDetail", "createdBy", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.OrderDetail", "createdBy", c => c.String());
            AlterColumn("dbo.Order", "createdBy", c => c.String());
            AlterColumn("dbo.News", "createdBy", c => c.String());
            AlterColumn("dbo.Product", "createdBy", c => c.String());
            AlterColumn("dbo.Category", "createdBy", c => c.String());
            AlterColumn("dbo.Adv", "createdBy", c => c.String());
        }
    }
}
