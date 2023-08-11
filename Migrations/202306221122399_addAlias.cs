namespace PCShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addAlias : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.News", "alias", c => c.String(nullable: false));
            AlterColumn("dbo.News", "description", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.News", "description", c => c.String(nullable: false));
            DropColumn("dbo.News", "alias");
        }
    }
}
