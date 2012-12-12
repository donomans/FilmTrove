namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class m2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Movies", "Title", c => c.String(maxLength: 250));
            AlterColumn("dbo.Movies", "AltTitle", c => c.String(maxLength: 100));
            CreateIndex("dbo.Movies", "Title");
            CreateIndex("dbo.Movies", "AltTitle");
            CreateIndex("dbo.Movies", "Year");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Movies", "Title");
            DropIndex("dbo.Movies", "AltTitle");
            DropIndex("dbo.Movies", "Year");
            AlterColumn("dbo.Movies", "AltTitle", c => c.String());
            AlterColumn("dbo.Movies", "Title", c => c.String());
        }
    }
}
