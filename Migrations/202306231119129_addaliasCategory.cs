namespace PCShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addaliasCategory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Category", "alias", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Category", "alias");
        }
    }
}
