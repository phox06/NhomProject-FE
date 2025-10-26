namespace NhomProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_CartItem_Model : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CartItems", "ProductId", c => c.Int(nullable: false));
            AddColumn("dbo.CartItems", "ImageUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CartItems", "ImageUrl");
            DropColumn("dbo.CartItems", "ProductId");
        }
    }
}
