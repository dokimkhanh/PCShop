namespace PCShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateorder1008231037 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tbOrder", "paymentType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tbOrder", "paymentType");
        }
    }
}
