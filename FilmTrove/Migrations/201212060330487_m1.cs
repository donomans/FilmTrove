namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class m1 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.UserListItems", name: "Movie_MovieId", newName: "MovieId");
            CreateIndex("dbo.UserListItems", "MovieId");
        }
        
        public override void Down()
        {
            RenameColumn(table: "dbo.UserListItems", name: "MovieId", newName: "Movie_MovieId");
            DropIndex("dbo.UserListItems", "MovieId");
        }
    }
}
