namespace NhomProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserTable : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Orders", name: "User_UserId", newName: "UserId");
            RenameIndex(table: "dbo.Orders", name: "IX_User_UserId", newName: "IX_UserId");
            AddColumn("dbo.Users", "Username", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.Users", "Password", c => c.String(nullable: false, maxLength: 200));
            AddColumn("dbo.Users", "FullName", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Users", "Phone", c => c.String(maxLength: 15));
            AddColumn("dbo.Users", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Users", "IsActive", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Users", "Email", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Users", "Address", c => c.String(maxLength: 200));
            DropColumn("dbo.Users", "PasswordHash");
            DropColumn("dbo.Users", "FirstName");
            DropColumn("dbo.Users", "LastName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "LastName", c => c.String());
            AddColumn("dbo.Users", "FirstName", c => c.String());
            AddColumn("dbo.Users", "PasswordHash", c => c.String());
            AlterColumn("dbo.Users", "Address", c => c.String());
            AlterColumn("dbo.Users", "Email", c => c.String());
            DropColumn("dbo.Users", "IsActive");
            DropColumn("dbo.Users", "CreatedDate");
            DropColumn("dbo.Users", "Phone");
            DropColumn("dbo.Users", "FullName");
            DropColumn("dbo.Users", "Password");
            DropColumn("dbo.Users", "Username");
            RenameIndex(table: "dbo.Orders", name: "IX_UserId", newName: "IX_User_UserId");
            RenameColumn(table: "dbo.Orders", name: "UserId", newName: "User_UserId");
        }
    }
}
