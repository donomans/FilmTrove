namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class merges3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.MergeCandidates", "Id", c => c.Int(nullable: false, identity: true));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.MergeCandidates", "Id", c => c.Int(nullable: false, identity: false));
        }
    }
}
