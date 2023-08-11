namespace PCShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateorder1008231106 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tbOrder", "paymentStatus", c => c.Int(nullable: false));
            DropColumn("dbo.tbOrder", "payment");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tbOrder", "payment", c => c.Int(nullable: false));
            DropColumn("dbo.tbOrder", "paymentStatus");
        }
    }
}
