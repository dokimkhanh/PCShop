namespace PCShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.News", "title", c => c.String(nullable: false));
            AlterColumn("dbo.News", "description", c => c.String(nullable: false));
            AlterColumn("dbo.News", "detail", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.News", "detail", c => c.String());
            AlterColumn("dbo.News", "description", c => c.String());
            AlterColumn("dbo.News", "title", c => c.String());
        }
    }
}
