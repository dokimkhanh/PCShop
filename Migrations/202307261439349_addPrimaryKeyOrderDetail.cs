﻿namespace PCShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPrimaryKeyOrderDetail : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.tbOrderDetail");
            AlterColumn("dbo.tbOrderDetail", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.tbOrderDetail", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.tbOrderDetail");
            AlterColumn("dbo.tbOrderDetail", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.tbOrderDetail", new[] { "Id", "orderID" });
        }
    }
}
