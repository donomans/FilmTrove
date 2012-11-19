namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class p : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.People", "Netflix_Id", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.People", "Netflix_Id");
        }
    }
}
