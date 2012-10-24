namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class i : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Movies",
                c => new
                    {
                        MovieId = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        AltTitle = c.String(),
                        Description = c.String(),
                        Rating = c.String(),
                        RatingType = c.Int(nullable: false),
                        BestPosterUrl = c.String(),
                        Year = c.Int(nullable: false),
                        RunTime = c.Int(),
                        Netflix_AvgRating = c.Int(),
                        Netflix_Synopsis = c.String(),
                        Netflix_Studio = c.String(),
                        Netflix_PosterUrlLarge = c.String(),
                        Netflix_SimilarTitlesCompact = c.String(),
                        Netflix_RelatedTitlesCompact = c.String(),
                        Netflix_AwardsCompact = c.String(),
                        Netflix_ScreenFormat = c.Int(nullable: false),
                        Netflix_Format = c.Int(nullable: false),
                        Netflix_Id = c.String(maxLength: 20),
                        Netflix_Url = c.String(),
                        Netflix_NeedsUpdate = c.Boolean(nullable: false),
                        Imdb_AvgRating = c.Int(),
                        Imdb_Synopsis = c.String(),
                        Imdb_Studio = c.String(),
                        Imdb_Id = c.String(maxLength: 20),
                        Imdb_Url = c.String(),
                        Imdb_NeedsUpdate = c.Boolean(nullable: false),
                        Amazon_PosterUrlMedium = c.String(),
                        Amazon_PosterUrlLarge = c.String(),
                        Amazon_AvgRating = c.Int(),
                        Amazon_Synopsis = c.String(),
                        Amazon_Studio = c.String(),
                        Amazon_Id = c.String(maxLength: 20),
                        Amazon_Url = c.String(),
                        Amazon_NeedsUpdate = c.Boolean(nullable: false),
                        RottenTomatoes_CriticScore = c.Int(),
                        RottenTomatoes_CriticConsensus = c.String(),
                        RottenTomatoes_PosterUrlMedium = c.String(),
                        RottenTomatoes_PosterUrlLarge = c.String(),
                        RottenTomatoes_TheatricalRelase = c.DateTime(),
                        RottenTomatoes_DvdRelease = c.DateTime(),
                        RottenTomatoes_AvgRating = c.Int(),
                        RottenTomatoes_Synopsis = c.String(),
                        RottenTomatoes_Studio = c.String(),
                        RottenTomatoes_Id = c.String(maxLength: 20),
                        RottenTomatoes_Url = c.String(),
                        RottenTomatoes_NeedsUpdate = c.Boolean(nullable: false),
                        GenresCompact = c.String(),
                        DateCreated = c.DateTime(),
                        DateLastModified = c.DateTime(),
                    })
                .PrimaryKey(t => t.MovieId)
                .Index(t => t.Netflix_Id, unique: true)
                .Index(t => t.Amazon_Id)
                .Index(t => t.Imdb_Id)
                .Index(t => t.RottenTomatoes_Id);
            
            CreateTable(
                "dbo.UserLists",
                c => new
                    {
                        ListId = c.Int(nullable: false, identity: true),
                        ListName = c.String(),
                        DateCreated = c.DateTime(),
                        DateLastModified = c.DateTime(),
                        Owner_UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ListId)
                .ForeignKey("dbo.UserProfile", t => t.Owner_UserId, cascadeDelete: true)
                .Index(t => t.Owner_UserId);
            
            CreateTable(
                "dbo.UserProfile",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        UserName = c.String(),
                        Provider = c.String(),
                        Name = c.String(),
                        Email = c.String(),
                        NetflixAccount_Token = c.String(),
                        NetflixAccount_TokenSecret = c.String(),
                        NetflixAccount_UserId = c.String(),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        RoleId = c.Int(nullable: false, identity: true),
                        RoleName = c.String(),
                        InRole = c.Int(nullable: false),
                        Movie_MovieId = c.Int(),
                        Person_PersonId = c.Int(),
                    })
                .PrimaryKey(t => t.RoleId)
                .ForeignKey("dbo.Movies", t => t.Movie_MovieId)
                .ForeignKey("dbo.People", t => t.Person_PersonId)
                .Index(t => t.Movie_MovieId)
                .Index(t => t.Person_PersonId);
            
            CreateTable(
                "dbo.People",
                c => new
                    {
                        PersonId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Bio = c.String(),
                        RottenTomatoes_Id = c.String(maxLength: 20),
                        RottenTomatoes_Url = c.String(),
                        RottenTomatoes_NeedsUpdate = c.Boolean(nullable: false),
                        Imdb_Id = c.String(maxLength: 20),
                        Imdb_Url = c.String(),
                        Imdb_NeedsUpdate = c.Boolean(nullable: false),
                        Netflix_Id = c.String(maxLength: 20),
                        Netflix_Url = c.String(),
                        Netflix_NeedsUpdate = c.Boolean(nullable: false),
                        DateCreated = c.DateTime(),
                        DateLastModified = c.DateTime(),
                    })
                .PrimaryKey(t => t.PersonId)
                .Index(t => t.Netflix_Id, unique: true)
                //.Index(t => t.Amazon_Id)
                .Index(t => t.Imdb_Id)
                .Index(t => t.RottenTomatoes_Id);
            
            CreateTable(
                "dbo.MoviesLists",
                c => new
                    {
                        ListId = c.Int(nullable: false),
                        MovieId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ListId, t.MovieId })
                .ForeignKey("dbo.UserLists", t => t.ListId, cascadeDelete: true)
                .ForeignKey("dbo.Movies", t => t.MovieId, cascadeDelete: true)
                .Index(t => t.ListId)
                .Index(t => t.MovieId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.MoviesLists", new[] { "MovieId" });
            DropIndex("dbo.MoviesLists", new[] { "ListId" });
            DropIndex("dbo.Roles", new[] { "Person_PersonId" });
            DropIndex("dbo.Roles", new[] { "Movie_MovieId" });
            DropIndex("dbo.UserLists", new[] { "Owner_UserId" });
            DropForeignKey("dbo.MoviesLists", "MovieId", "dbo.Movies");
            DropForeignKey("dbo.MoviesLists", "ListId", "dbo.UserLists");
            DropForeignKey("dbo.Roles", "Person_PersonId", "dbo.People");
            DropForeignKey("dbo.Roles", "Movie_MovieId", "dbo.Movies");
            DropForeignKey("dbo.UserLists", "Owner_UserId", "dbo.UserProfile");
            DropTable("dbo.MoviesLists");
            DropTable("dbo.People");
            DropTable("dbo.Roles");
            DropTable("dbo.UserProfile");
            DropTable("dbo.UserLists");
            DropTable("dbo.Movies");
        }
    }
}
