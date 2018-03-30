namespace Manga.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddpropertyforProductTables : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "Author", c => c.String());
            AddColumn("dbo.Products", "Description", c => c.String());
            AddColumn("dbo.Products", "ImageCoverPath", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "ImageCoverPath");
            DropColumn("dbo.Products", "Description");
            DropColumn("dbo.Products", "Author");
        }
    }
}
