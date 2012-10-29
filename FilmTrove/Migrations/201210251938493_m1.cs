namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class m1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Movies", "Amazon_LastPriceUpdate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Movies", "Amazon_LastPriceUpdate", c => c.DateTime(nullable: false));
        }
    }
}
