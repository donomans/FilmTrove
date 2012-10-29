namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class m2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Movies", "Amazon_LastFullUpdate", c => c.DateTime());
            AddColumn("dbo.Movies", "RottenTomatoes_LastFullUpdate", c => c.DateTime());
            AddColumn("dbo.Movies", "RottenTomatoes_TrailerUrl", c => c.String());
            AddColumn("dbo.Movies", "ViewCount", c => c.Long(nullable: false));
            AddColumn("dbo.Movies", "SearchCount", c => c.Long(nullable: false));
            AlterColumn("dbo.Movies", "Netflix_AvgRating", c => c.Single());
            DropColumn("dbo.Movies", "Netflix_Studio");
            DropColumn("dbo.Movies", "Netflix_ScreenFormat");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Movies", "Netflix_ScreenFormat", c => c.Int(nullable: false));
            AddColumn("dbo.Movies", "Netflix_Studio", c => c.String());
            AlterColumn("dbo.Movies", "Netflix_AvgRating", c => c.Int());
            DropColumn("dbo.Movies", "SearchCount");
            DropColumn("dbo.Movies", "ViewCount");
            DropColumn("dbo.Movies", "RottenTomatoes_TrailerUrl");
            DropColumn("dbo.Movies", "RottenTomatoes_LastFullUpdate");
            DropColumn("dbo.Movies", "Amazon_LastFullUpdate");
        }
    }
}
