namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
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
                        Netflix_AvgRating = c.Single(),
                        Netflix_Synopsis = c.String(),
                        Netflix_IdUrl = c.String(),
                        Netflix_Id = c.String(maxLength: 20),
                        Netflix_PosterUrlLarge = c.String(),
                        Netflix_SimilarTitlesCompact = c.String(),
                        Netflix_AwardsCompact = c.String(),
                        Netflix_Format = c.Int(nullable: false),
                        Netflix_OfficialWebsiteUrl = c.String(),
                        Netflix_Url = c.String(),
                        Netflix_NeedsUpdate = c.Boolean(nullable: false),
                        Imdb_AvgRating = c.Int(),
                        Imdb_Synopsis = c.String(),
                        Imdb_Studio = c.String(),
                        Imdb_Id = c.String(maxLength: 20),
                        Imdb_Url = c.String(),
                        Imdb_NeedsUpdate = c.Boolean(nullable: false),
                        Amazon_LastPrice = c.Double(),
                        Amazon_LastPriceUpdate = c.DateTime(),
                        Amazon_LastFullUpdate = c.DateTime(),
                        Amazon_PosterUrlMedium = c.String(),
                        Amazon_PosterUrlLarge = c.String(),
                        Amazon_AvgRating = c.Int(),
                        Amazon_Synopsis = c.String(),
                        Amazon_Studio = c.String(),
                        Amazon_Id = c.String(maxLength: 20),
                        Amazon_Url = c.String(),
                        Amazon_NeedsUpdate = c.Boolean(nullable: false),
                        RottenTomatoes_LastFullUpdate = c.DateTime(),
                        RottenTomatoes_CriticScore = c.Int(),
                        RottenTomatoes_CriticConsensus = c.String(),
                        RottenTomatoes_PosterUrlMedium = c.String(),
                        RottenTomatoes_PosterUrlLarge = c.String(),
                        RottenTomatoes_TheatricalRelase = c.DateTime(),
                        RottenTomatoes_DvdRelease = c.DateTime(),
                        RottenTomatoes_AvgRating = c.Int(),
                        RottenTomatoes_Synopsis = c.String(),
                        RottenTomatoes_Studio = c.String(),
                        RottenTomatoes_TrailerUrl = c.String(),
                        RottenTomatoes_Id = c.String(maxLength: 20),
                        RottenTomatoes_Url = c.String(),
                        RottenTomatoes_NeedsUpdate = c.Boolean(nullable: false),
                        RedBox_Id = c.String(maxLength: 36),
                        RedBox_Url = c.String(),
                        RedBox_NeedsUpdate = c.Boolean(nullable: false),
                        ViewCount = c.Long(nullable: false),
                        SearchCount = c.Long(nullable: false),
                        DateCreated = c.DateTime(),
                        DateLastModified = c.DateTime(),
                    })
                .PrimaryKey(t => t.MovieId)
                .Index(t => t.Netflix_Id)
                .Index(t => t.Amazon_Id)
                .Index(t => t.RottenTomatoes_Id)
                .Index(t => t.RedBox_Id)
                .Index(t => t.Imdb_Id);
            
            CreateTable(
                "dbo.MovieGenres",
                c => new
                    {
                        MovieGenreId = c.Int(nullable: false, identity: true),
                        Genre_GenreId = c.Int(),
                        Movie_MovieId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MovieGenreId)
                .ForeignKey("dbo.Genres", t => t.Genre_GenreId)
                .ForeignKey("dbo.Movies", t => t.Movie_MovieId, cascadeDelete: true)
                .Index(t => t.Genre_GenreId)
                .Index(t => t.Movie_MovieId);
            
            CreateTable(
                "dbo.Genres",
                c => new
                    {
                        GenreId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 40),
                    })
                .PrimaryKey(t => t.GenreId)
                .Index(t=>t.Name, unique: true);
            
            CreateTable(
                "dbo.UserListItems",
                c => new
                    {
                        ListItemId = c.Int(nullable: false, identity: true),
                        MovieTitle = c.String(),
                        Rating = c.Int(),
                        LastWatched = c.DateTime(nullable: false),
                        LoanedOut = c.Boolean(nullable: false),
                        OwnedFormats = c.Int(nullable: false),
                        DateCreated = c.DateTime(),
                        DateLastModified = c.DateTime(),
                        List_ListId = c.Int(),
                        LoanedTo_UserId = c.Int(),
                        Movie_MovieId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ListItemId)
                .ForeignKey("dbo.UserLists", t => t.List_ListId)
                .ForeignKey("dbo.UserProfile", t => t.LoanedTo_UserId)
                .ForeignKey("dbo.Movies", t => t.Movie_MovieId, cascadeDelete: true)
                .Index(t => t.List_ListId)
                .Index(t => t.LoanedTo_UserId)
                .Index(t => t.Movie_MovieId);
            
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
                        Netflix_IdUrl = c.String(),
                        Netflix_Id = c.String(maxLength: 20),
                        Netflix_Url = c.String(),
                        Netflix_NeedsUpdate = c.Boolean(nullable: false),
                        DateCreated = c.DateTime(),
                        DateLastModified = c.DateTime(),
                    })
                .PrimaryKey(t => t.PersonId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Roles", new[] { "Person_PersonId" });
            DropIndex("dbo.Roles", new[] { "Movie_MovieId" });
            DropIndex("dbo.UserLists", new[] { "Owner_UserId" });
            DropIndex("dbo.UserListItems", new[] { "Movie_MovieId" });
            DropIndex("dbo.UserListItems", new[] { "LoanedTo_UserId" });
            DropIndex("dbo.UserListItems", new[] { "List_ListId" });
            DropIndex("dbo.MovieGenres", new[] { "Movie_MovieId" });
            DropIndex("dbo.MovieGenres", new[] { "Genre_GenreId" });
            DropForeignKey("dbo.Roles", "Person_PersonId", "dbo.People");
            DropForeignKey("dbo.Roles", "Movie_MovieId", "dbo.Movies");
            DropForeignKey("dbo.UserLists", "Owner_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.UserListItems", "Movie_MovieId", "dbo.Movies");
            DropForeignKey("dbo.UserListItems", "LoanedTo_UserId", "dbo.UserProfile");
            DropForeignKey("dbo.UserListItems", "List_ListId", "dbo.UserLists");
            DropForeignKey("dbo.MovieGenres", "Movie_MovieId", "dbo.Movies");
            DropForeignKey("dbo.MovieGenres", "Genre_GenreId", "dbo.Genres");
            DropTable("dbo.People");
            DropTable("dbo.Roles");
            DropTable("dbo.UserLists");
            DropTable("dbo.UserProfile");
            DropTable("dbo.UserListItems");
            DropTable("dbo.Genres");
            DropTable("dbo.MovieGenres");
            DropTable("dbo.Movies");
        }
    }
}
