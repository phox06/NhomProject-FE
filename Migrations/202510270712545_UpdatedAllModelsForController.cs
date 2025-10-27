namespace NhomProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedAllModelsForController : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CartItems", "Order_Id", "dbo.Orders");
            DropIndex("dbo.CartItems", new[] { "Order_Id" });
            RenameColumn(table: "dbo.CartItems", name: "Order_Id", newName: "OrderId");
            AlterColumn("dbo.Products", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.CartItems", "OrderId", c => c.Int(nullable: false));
            CreateIndex("dbo.CartItems", "OrderId");
            AddForeignKey("dbo.CartItems", "OrderId", "dbo.Orders", "Id", cascadeDelete: true);
            DropColumn("dbo.Products", "StockQuantity");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Products", "StockQuantity", c => c.Int(nullable: false));
            DropForeignKey("dbo.CartItems", "OrderId", "dbo.Orders");
            DropIndex("dbo.CartItems", new[] { "OrderId" });
            AlterColumn("dbo.CartItems", "OrderId", c => c.Int());
            AlterColumn("dbo.Products", "Name", c => c.String());
            RenameColumn(table: "dbo.CartItems", name: "OrderId", newName: "Order_Id");
            CreateIndex("dbo.CartItems", "Order_Id");
            AddForeignKey("dbo.CartItems", "Order_Id", "dbo.Orders", "Id");
        }
    }
}
