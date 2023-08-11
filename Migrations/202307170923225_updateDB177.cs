namespace PCShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateDB177 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Order", "customerName", c => c.String(nullable: false));
            AlterColumn("dbo.Order", "phone", c => c.String(nullable: false));
            AlterColumn("dbo.Order", "address", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Order", "address", c => c.String());
            AlterColumn("dbo.Order", "phone", c => c.String());
            AlterColumn("dbo.Order", "customerName", c => c.String());
        }
    }
}
