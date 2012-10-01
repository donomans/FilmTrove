//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Data.Entity;
//using System.Globalization;
//using System.Web.Mvc;
//using System.Web.Security;

//namespace FilmTrove.Models
//{
//    public class AccountsContext : DbContext
//    {
//        public DbSet<UserProfile> UserProfiles { get; set; }
//    }

//    [Table("UserProfile")]
//    public class UserProfile
//    {
//        [Key]
//        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
//        public int UserId { get; set; }
//        public string UserName { get; set; }

//        public string Provider {get;set;}
//    }
//}
