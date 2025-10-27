namespace NhomProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSeparateProductImages : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "ThumbnailUrl", c => c.String());
            AddColumn("dbo.Products", "MainImageUrl", c => c.String());
            DropColumn("dbo.Products", "ImageUrl");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Products", "ImageUrl", c => c.String());
            DropColumn("dbo.Products", "MainImageUrl");
            DropColumn("dbo.Products", "ThumbnailUrl");
        }
    }
}
