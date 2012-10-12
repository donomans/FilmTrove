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
    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public Int32 UserId { get; set; }
        public String UserName { get; set; }

        public String Provider { get; set; }

        public String Name { get; set; }
        public String Email { get; set; } ///not sure if I'm guaranteed to get an email back from Facebook

        public NetflixAccount NetflixAccount { get; set; }
    }

    [ComplexType]
    public class NetflixAccount
    {
        public String Token { get; set; }
        public String TokenSecret { get; set; }
        public String UserId { get; set; }
    }



    public class UserUpdate
    {
        public UserUpdate()
        {
            Name = "";
            Email = "";
        }
        public UserUpdate(UserProfile up)
        {
            Name = up.Name;
            Email = up.Email;
        }
        public String Name { get; set; }
        public String Email { get; set; }
    }
}