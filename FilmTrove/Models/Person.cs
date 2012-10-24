using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FilmTrove.Models
{
    public class Person : Dateable
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public Int32 PersonId { get; set; }
        public String Name { get; set; }
        public String Bio { get; set; }

        public RottenTomatoesPersonInfo RottenTomatoes { get; set; }
        public ImdbPersonInfo Imdb { get; set; }
        public NetflixPersonInfo Netflix { get; set; }
        //public AmazonPersonInfo Amazon { get; set; }

        public virtual ICollection<Role> Roles { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public Person()
        {
            RottenTomatoes = new RottenTomatoesPersonInfo();
            Imdb = new ImdbPersonInfo();
            Netflix = new NetflixPersonInfo();
        }
    }

    public class Role
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public Int32 RoleId { get; set; }
        public String RoleName { get; set; }
        public RoleType InRole { get; set; }

        public virtual Movie Movie { get; set; }
        public virtual Person Person { get; set; }

    }

}