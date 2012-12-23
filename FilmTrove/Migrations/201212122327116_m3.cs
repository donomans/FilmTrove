namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class m3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Movies", "Rating", c => c.String(maxLength: 10));
            AlterColumn("dbo.Movies", "BestPosterUrl", c => c.String(maxLength: 250));
            AlterColumn("dbo.Movies", "Netflix_IdUrl", c => c.String(maxLength: 150));
            AlterColumn("dbo.Movies", "Netflix_PosterUrlLarge", c => c.String(maxLength: 250));
            AlterColumn("dbo.Movies", "Netflix_OfficialWebsiteUrl", c => c.String(maxLength: 250));
            AlterColumn("dbo.Movies", "Netflix_Url", c => c.String(maxLength: 250));
            AlterColumn("dbo.Movies", "Imdb_Studio", c => c.String(maxLength: 100));
            AlterColumn("dbo.Movies", "Imdb_Url", c => c.String(maxLength: 250));
            AlterColumn("dbo.Movies", "Amazon_PosterUrlMedium", c => c.String(maxLength: 250));
            AlterColumn("dbo.Movies", "Amazon_PosterUrlLarge", c => c.String(maxLength: 250));
            AlterColumn("dbo.Movies", "Amazon_Studio", c => c.String(maxLength: 100));
            AlterColumn("dbo.Movies", "Amazon_Url", c => c.String(maxLength: 250));
            AlterColumn("dbo.Movies", "RottenTomatoes_PosterUrlMedium", c => c.String(maxLength: 250));
            AlterColumn("dbo.Movies", "RottenTomatoes_PosterUrlLarge", c => c.String(maxLength: 250));
            AlterColumn("dbo.Movies", "RottenTomatoes_Studio", c => c.String(maxLength: 100));
            AlterColumn("dbo.Movies", "RottenTomatoes_TrailerUrl", c => c.String(maxLength: 250));
            AlterColumn("dbo.Movies", "RottenTomatoes_Url", c => c.String(maxLength: 250));
            AlterColumn("dbo.Movies", "RedBox_Url", c => c.String(maxLength: 250));
            AlterColumn("dbo.Genres", "Name", c => c.String(maxLength: 150));
            AlterColumn("dbo.People", "RottenTomatoes_Url", c => c.String(maxLength: 250));
            AlterColumn("dbo.People", "Imdb_Url", c => c.String(maxLength: 250));
            AlterColumn("dbo.People", "Netflix_Url", c => c.String(maxLength: 250));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.People", "Netflix_Url", c => c.String());
            AlterColumn("dbo.People", "Imdb_Url", c => c.String());
            AlterColumn("dbo.People", "RottenTomatoes_Url", c => c.String());
            AlterColumn("dbo.Genres", "Name", c => c.String());
            AlterColumn("dbo.Movies", "RedBox_Url", c => c.String());
            AlterColumn("dbo.Movies", "RottenTomatoes_Url", c => c.String());
            AlterColumn("dbo.Movies", "RottenTomatoes_TrailerUrl", c => c.String());
            AlterColumn("dbo.Movies", "RottenTomatoes_Studio", c => c.String());
            AlterColumn("dbo.Movies", "RottenTomatoes_PosterUrlLarge", c => c.String());
            AlterColumn("dbo.Movies", "RottenTomatoes_PosterUrlMedium", c => c.String());
            AlterColumn("dbo.Movies", "Amazon_Url", c => c.String());
            AlterColumn("dbo.Movies", "Amazon_Studio", c => c.String());
            AlterColumn("dbo.Movies", "Amazon_PosterUrlLarge", c => c.String());
            AlterColumn("dbo.Movies", "Amazon_PosterUrlMedium", c => c.String());
            AlterColumn("dbo.Movies", "Imdb_Url", c => c.String());
            AlterColumn("dbo.Movies", "Imdb_Studio", c => c.String());
            AlterColumn("dbo.Movies", "Netflix_Url", c => c.String());
            AlterColumn("dbo.Movies", "Netflix_OfficialWebsiteUrl", c => c.String());
            AlterColumn("dbo.Movies", "Netflix_PosterUrlLarge", c => c.String());
            AlterColumn("dbo.Movies", "Netflix_IdUrl", c => c.String());
            AlterColumn("dbo.Movies", "BestPosterUrl", c => c.String());
            AlterColumn("dbo.Movies", "Rating", c => c.String());
        }
    }
}
