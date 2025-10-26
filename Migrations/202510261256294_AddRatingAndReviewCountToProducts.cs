namespace NhomProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRatingAndReviewCountToProducts : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "Rating", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Products", "ReviewCount", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "ReviewCount");
            DropColumn("dbo.Products", "Rating");
        }
    }
}
