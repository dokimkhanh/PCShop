namespace PCShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addMailField26723831 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tbOrder", "email", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tbOrder", "email");
        }
    }
}
