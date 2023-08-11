namespace PCShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateNameOrder : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.OrderDetail", newName: "tbOrderDetail");
            RenameTable(name: "dbo.Order", newName: "tbOrder");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.tbOrder", newName: "Order");
            RenameTable(name: "dbo.tbOrderDetail", newName: "OrderDetail");
        }
    }
}
