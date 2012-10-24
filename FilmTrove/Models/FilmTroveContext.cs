using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Collections;
using System.Linq;
using System.Web;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Data;

namespace FilmTrove.Models
{
    public class FilmTroveContext : DbContext
    {
        public FilmTroveContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserList> Lists { get; set; }
        public DbSet<Role> Roles { get; set; }

        public override Int32 SaveChanges()
        {
            ObjectContext context = ((IObjectContextAdapter)this).ObjectContext;

            //Find all Entities that are Added/Modified that inherit from my EntityBase
            IEnumerable<ObjectStateEntry> objectStateEntries =
                from e in context.ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Modified)
                where
                    e.IsRelationship == false &&
                    e.Entity != null &&
                    (e.Entity is Dateable)
                //typeof(Dateable).IsAssignableFrom(e.Entity.GetType())
                select e;

            var currentTime = DateTime.Now;

            foreach (var entry in objectStateEntries)
            {
                var entityBase = entry.Entity as Dateable;

                if (entry.State == EntityState.Added)
                {
                    entityBase.DateCreated = currentTime;
                }

                entityBase.DateLastModified = currentTime;
            }

            return base.SaveChanges();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Movie>().
            //  HasMany(m => m.Director).
            //  WithMany(p => p.Directed).
            //  Map(
            //   m =>
            //   {
            //       m.MapLeftKey("MovieId");
            //       m.MapRightKey("PersonId");
            //       m.ToTable("MovieDirected");
            //   });

            //modelBuilder.Entity<Movie>().
            //  HasMany(m => m.Actors).
            //  WithMany(p => p.Acted).
            //  Map(
            //   m =>
            //   {
            //       m.MapLeftKey("MovieId");
            //       m.MapRightKey("PersonId");
            //       m.ToTable("MovieActed");
            //   });

            //modelBuilder.Entity<Movie>().
            //  HasMany(m => m.Writer).
            //  WithMany(p => p.Wrote).
            //  Map(
            //   m =>
            //   {
            //       m.MapLeftKey("MovieId");
            //       m.MapRightKey("PersonId");
            //       m.ToTable("MovieWrote");
            //   });

            //modelBuilder.Entity<Movie>().
            // HasMany(m => m.Producer).
            // WithMany(p => p.Produced).
            // Map(
            //  m =>
            //  {
            //      m.MapLeftKey("MovieId");
            //      m.MapRightKey("PersonId");
            //      m.ToTable("MovieProduced");
            //  });

            //modelBuilder.Entity<Movie>().
            //    HasMany(m => m.SimilarTitles);



            modelBuilder.Entity<UserList>().
                HasRequired(l => l.Owner).
                WithMany(u => u.UserLists);

            modelBuilder.Entity<UserProfile>().
                HasMany(u => u.UserLists).
                WithRequired(l => l.Owner);



            modelBuilder.Entity<UserList>().
                HasMany(m => m.Movies).
                WithMany(m => m.OnLists).
                Map(
                m =>
                {
                    m.MapLeftKey("ListId");
                    m.MapRightKey("MovieId");
                    m.ToTable("MoviesLists");
                });

        }
    }

}