namespace PCShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPayment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Order", "payment", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Order", "payment");
        }
    }
}
