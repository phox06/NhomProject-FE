namespace NhomProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Test : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.CartItems", name: "OrderId", newName: "Order_Id");
            RenameColumn(table: "dbo.Orders", name: "UserId", newName: "User_UserId");
            RenameIndex(table: "dbo.Orders", name: "IX_UserId", newName: "IX_User_UserId");
            RenameIndex(table: "dbo.CartItems", name: "IX_OrderId", newName: "IX_Order_Id");
            CreateTable(
                "dbo.OrderDetails",
                c => new
                    {
                        OrderDetailId = c.Int(nullable: false, identity: true),
                        Quantity = c.Int(nullable: false),
                        UnitPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OrderId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.OrderDetailId)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.OrderId)
                .Index(t => t.ProductId);
            
            AddColumn("dbo.Orders", "TotalAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Orders", "PaymentMethod", c => c.String());
            AddColumn("dbo.Users", "PasswordHash", c => c.String());
            AddColumn("dbo.Users", "FirstName", c => c.String());
            AddColumn("dbo.Users", "LastName", c => c.String());
            AddColumn("dbo.Products", "StockQuantity", c => c.Int(nullable: false));
            AlterColumn("dbo.CartItems", "ProductName", c => c.String());
            AlterColumn("dbo.CartItems", "ImageUrl", c => c.String());
            AlterColumn("dbo.Orders", "CustomerName", c => c.String());
            AlterColumn("dbo.Orders", "Phone", c => c.String());
            AlterColumn("dbo.Orders", "Address", c => c.String());
            AlterColumn("dbo.Orders", "Status", c => c.String());
            AlterColumn("dbo.Users", "Email", c => c.String());
            AlterColumn("dbo.Users", "Address", c => c.String());
            AlterColumn("dbo.Categories", "Name", c => c.String());
            AlterColumn("dbo.Categories", "Description", c => c.String());
            AlterColumn("dbo.Products", "Name", c => c.String());
            AlterColumn("dbo.Products", "Description", c => c.String());
            AlterColumn("dbo.Products", "ImageUrl", c => c.String());
            DropColumn("dbo.Orders", "Notes");
            DropColumn("dbo.Users", "Username");
            DropColumn("dbo.Users", "Password");
            DropColumn("dbo.Users", "FullName");
            DropColumn("dbo.Users", "Phone");
            DropColumn("dbo.Users", "CreatedDate");
            DropColumn("dbo.Users", "IsActive");
            DropColumn("dbo.Categories", "ImageUrl");
            DropColumn("dbo.Categories", "IsActive");
            DropColumn("dbo.Products", "Stock");
            DropColumn("dbo.Products", "CreatedDate");
            DropColumn("dbo.Products", "IsActive");
            DropColumn("dbo.Products", "Brand");
            DropColumn("dbo.Products", "Model");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Products", "Model", c => c.String(maxLength: 50));
            AddColumn("dbo.Products", "Brand", c => c.String(maxLength: 50));
            AddColumn("dbo.Products", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.Products", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Products", "Stock", c => c.Int(nullable: false));
            AddColumn("dbo.Categories", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.Categories", "ImageUrl", c => c.String(maxLength: 200));
            AddColumn("dbo.Users", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.Users", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Users", "Phone", c => c.String(maxLength: 15));
            AddColumn("dbo.Users", "FullName", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Users", "Password", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Users", "Username", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.Orders", "Notes", c => c.String(maxLength: 500));
            DropForeignKey("dbo.OrderDetails", "ProductId", "dbo.Products");
            DropForeignKey("dbo.OrderDetails", "OrderId", "dbo.Orders");
            DropIndex("dbo.OrderDetails", new[] { "ProductId" });
            DropIndex("dbo.OrderDetails", new[] { "OrderId" });
            AlterColumn("dbo.Products", "ImageUrl", c => c.String(maxLength: 200));
            AlterColumn("dbo.Products", "Description", c => c.String(maxLength: 1000));
            AlterColumn("dbo.Products", "Name", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.Categories", "Description", c => c.String(maxLength: 500));
            AlterColumn("dbo.Categories", "Name", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Users", "Address", c => c.String(maxLength: 200));
            AlterColumn("dbo.Users", "Email", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Orders", "Status", c => c.String(maxLength: 20));
            AlterColumn("dbo.Orders", "Address", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.Orders", "Phone", c => c.String(nullable: false, maxLength: 15));
            AlterColumn("dbo.Orders", "CustomerName", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.CartItems", "ImageUrl", c => c.String(maxLength: 200));
            AlterColumn("dbo.CartItems", "ProductName", c => c.String(maxLength: 200));
            DropColumn("dbo.Products", "StockQuantity");
            DropColumn("dbo.Users", "LastName");
            DropColumn("dbo.Users", "FirstName");
            DropColumn("dbo.Users", "PasswordHash");
            DropColumn("dbo.Orders", "PaymentMethod");
            DropColumn("dbo.Orders", "TotalAmount");
            DropTable("dbo.OrderDetails");
            RenameIndex(table: "dbo.CartItems", name: "IX_Order_Id", newName: "IX_OrderId");
            RenameIndex(table: "dbo.Orders", name: "IX_User_UserId", newName: "IX_UserId");
            RenameColumn(table: "dbo.Orders", name: "User_UserId", newName: "UserId");
            RenameColumn(table: "dbo.CartItems", name: "Order_Id", newName: "OrderId");
        }
    }
}
