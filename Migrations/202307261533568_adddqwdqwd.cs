namespace PCShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adddqwdqwd : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.tbOrderDetail", "createdDate");
            DropColumn("dbo.tbOrderDetail", "createdBy");
            DropColumn("dbo.tbOrderDetail", "modifiedDate");
            DropColumn("dbo.tbOrderDetail", "modifiedBy");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tbOrderDetail", "modifiedBy", c => c.String());
            AddColumn("dbo.tbOrderDetail", "modifiedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.tbOrderDetail", "createdBy", c => c.String());
            AddColumn("dbo.tbOrderDetail", "createdDate", c => c.DateTime(nullable: false));
        }
    }
}
