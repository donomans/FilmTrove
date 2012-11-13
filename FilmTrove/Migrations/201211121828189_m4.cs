namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class m4 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Movies", "Netflix_IdUrl", c => c.String());
            AddColumn("dbo.People", "Netflix_IdUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.People", "Netflix_IdUrl");
            DropColumn("dbo.Movies", "Netflix_IdUrl");
        }
    }
}
