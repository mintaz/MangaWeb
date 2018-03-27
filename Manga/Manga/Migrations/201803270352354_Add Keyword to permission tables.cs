namespace Manga.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddKeywordtopermissiontables : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Permissions", "Keyword", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Permissions", "Keyword");
        }
    }
}
