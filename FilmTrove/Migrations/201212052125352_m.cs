namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class m : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.UserListItems", "LastWatched", c => c.DateTime(nullable: true));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.UserListItems", "LastWatched", c => c.DateTime(nullable: false));
        }
    }
}
