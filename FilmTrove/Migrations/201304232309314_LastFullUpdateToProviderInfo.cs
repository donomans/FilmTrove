namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LastFullUpdateToProviderInfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Movies", "Imdb_LastFullUpdate", c => c.DateTime());
            AddColumn("dbo.Movies", "RedBox_LastFullUpdate", c => c.DateTime());
            AddColumn("dbo.People", "RottenTomatoes_LastFullUpdate", c => c.DateTime());
            AddColumn("dbo.People", "Imdb_LastFullUpdate", c => c.DateTime());
            AddColumn("dbo.People", "Netflix_LastFullUpdate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.People", "Netflix_LastFullUpdate");
            DropColumn("dbo.People", "Imdb_LastFullUpdate");
            DropColumn("dbo.People", "RottenTomatoes_LastFullUpdate");
            DropColumn("dbo.Movies", "RedBox_LastFullUpdate");
            DropColumn("dbo.Movies", "Imdb_LastFullUpdate");
        }
    }
}
