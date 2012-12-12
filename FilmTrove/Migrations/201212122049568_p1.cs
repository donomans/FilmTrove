namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class p1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.People", "Name", c => c.String(maxLength: 100));
            CreateIndex("dbo.People", "Name");
        }
        
        public override void Down()
        {
            DropIndex("dbo.People", "Name");
            AlterColumn("dbo.People", "Name", c => c.String());
        }
    }
}
