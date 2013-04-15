namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rtyear : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Movies", "RottenTomatoes_Year", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Movies", "RottenTomatoes_Year");
        }
    }
}
