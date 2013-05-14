namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class merges : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MergeCandidates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: false),
                        Primary_MovieId = c.Int(),
                        Secondary_MovieId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Movies", t => t.Primary_MovieId)
                .ForeignKey("dbo.Movies", t => t.Secondary_MovieId)
                .Index(t => t.Primary_MovieId)
                .Index(t => t.Secondary_MovieId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.MergeCandidates", new[] { "Secondary_MovieId" });
            DropIndex("dbo.MergeCandidates", new[] { "Primary_MovieId" });
            DropForeignKey("dbo.MergeCandidates", "Secondary_MovieId", "dbo.Movies");
            DropForeignKey("dbo.MergeCandidates", "Primary_MovieId", "dbo.Movies");
            DropTable("dbo.MergeCandidates");
        }
    }
}
