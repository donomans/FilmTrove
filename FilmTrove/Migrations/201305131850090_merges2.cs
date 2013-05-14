namespace FilmTrove.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class merges2 : DbMigration
    {
        public override void Up()
        {
            //DropForeignKey("dbo.MergeCandidates", "Primary_MovieId", "dbo.Movies");
            //DropForeignKey("dbo.MergeCandidates", "Secondary_MovieId", "dbo.Movies");
            //DropIndex("dbo.MergeCandidates", new[] { "Primary_MovieId" });
            //DropIndex("dbo.MergeCandidates", new[] { "Secondary_MovieId" });
            //AddColumn("dbo.MergeCandidates", "PrimaryId", c => c.String(maxLength: 20));
            //AddColumn("dbo.MergeCandidates", "SecondaryId", c => c.String(maxLength: 20));
            //AddColumn("dbo.MergeCandidates", "PrimaryType", c => c.Int(nullable: false));
            //AddColumn("dbo.MergeCandidates", "SecondaryType", c => c.Int(nullable: false));
            //DropColumn("dbo.MergeCandidates", "Primary_MovieId");
            //DropColumn("dbo.MergeCandidates", "Secondary_MovieId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MergeCandidates", "Secondary_MovieId", c => c.Int());
            AddColumn("dbo.MergeCandidates", "Primary_MovieId", c => c.Int());
            DropColumn("dbo.MergeCandidates", "SecondaryType");
            DropColumn("dbo.MergeCandidates", "PrimaryType");
            DropColumn("dbo.MergeCandidates", "SecondaryId");
            DropColumn("dbo.MergeCandidates", "PrimaryId");
            CreateIndex("dbo.MergeCandidates", "Secondary_MovieId");
            CreateIndex("dbo.MergeCandidates", "Primary_MovieId");
            AddForeignKey("dbo.MergeCandidates", "Secondary_MovieId", "dbo.Movies", "MovieId");
            AddForeignKey("dbo.MergeCandidates", "Primary_MovieId", "dbo.Movies", "MovieId");
        }
    }
}
