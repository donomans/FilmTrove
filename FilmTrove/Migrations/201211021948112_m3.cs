namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class m3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Movies", "Netflix_OfficialWebsiteUrl", c => c.String());
            DropColumn("dbo.Movies", "Netflix_RelatedTitlesCompact");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Movies", "Netflix_RelatedTitlesCompact", c => c.String());
            DropColumn("dbo.Movies", "Netflix_OfficialWebsiteUrl");
        }
    }
}
