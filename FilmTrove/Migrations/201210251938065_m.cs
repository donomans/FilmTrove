namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class m : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Movies", "Amazon_LastPrice", c => c.Double());
            AddColumn("dbo.Movies", "Amazon_LastPriceUpdate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Movies", "Amazon_LastPriceUpdate");
            DropColumn("dbo.Movies", "Amazon_LastPrice");
        }
    }
}
