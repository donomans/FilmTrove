namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class u : DbMigration
    {
        public override void Up()
        {
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
                        LoanedTo_UserId = c.Int(),
                        Movie_MovieId = c.Int(),
                        List_ListId = c.Int(),
                    })
                .PrimaryKey(t => t.ListItemId)
                .ForeignKey("dbo.UserProfile", t => t.LoanedTo_UserId)
                .ForeignKey("dbo.Movies", t => t.Movie_MovieId)
                .ForeignKey("dbo.UserLists", t => t.List_ListId)
                .Index(t => t.LoanedTo_UserId)
                .Index(t => t.Movie_MovieId)
                .Index(t => t.List_ListId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.UserListItems", new[] { "List_ListId" });
            DropIndex("dbo.UserListItems", new[] { "Movie_MovieId" });
            DropIndex("dbo.UserListItems", new[] { "LoanedTo_UserId" });
            DropForeignKey("dbo.UserListItems", "List_ListId", "dbo.UserLists");
            DropForeignKey("dbo.UserListItems", "Movie_MovieId", "dbo.Movies");
            DropForeignKey("dbo.UserListItems", "LoanedTo_UserId", "dbo.UserProfile");
            DropTable("dbo.UserListItems");
        }
    }
}
