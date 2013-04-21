namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FullUpdateDTForNetflix : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Movies", "Netflix_LastFullUpdate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Movies", "Netflix_LastFullUpdate");
        }
    }
}
