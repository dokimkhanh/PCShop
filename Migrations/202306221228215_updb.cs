namespace PCShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updb : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.News", "alias", c => c.String());
            AlterColumn("dbo.News", "description", c => c.String(nullable: false));
            AlterColumn("dbo.News", "image", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.News", "image", c => c.String());
            AlterColumn("dbo.News", "description", c => c.String());
            AlterColumn("dbo.News", "alias", c => c.String(nullable: false));
        }
    }
}
