using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FilmTrove.Code;

namespace FilmTrove.Models
{
    public class Person : Dateable//, IEquatable<Person>
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public Int32 PersonId { get; set; }
        [MaxLength(100)]
        public String Name { get; set; }
        public String Bio { get; set; }

        public RottenTomatoesPersonInfo RottenTomatoes { get; set; }
        public ImdbPersonInfo Imdb { get; set; }
        public NetflixPersonInfo Netflix { get; set; }
        //public AmazonPersonInfo Amazon { get; set; }

        public virtual ICollection<Role> Roles { get; set; }

        public String GetDetailsUrl()
        {
            return "/people/details/" + PersonId + "/" + Name.UrlFriendly();
        }
        public override String ToString()
        {
            return Name;
        }

        public Person()
        {
            RottenTomatoes = new RottenTomatoesPersonInfo();
            Imdb = new ImdbPersonInfo();
            Netflix = new NetflixPersonInfo();
        }

        //public override bool Equals(Object obj)
        //{
        //    return this.Equals((Person)obj);
        //}
        //public bool Equals(Person other)
        //{
        //    return GetHashCode() == other.GetHashCode();
        //}
        //public override int GetHashCode()
        //{
        //    return PersonId == 0 ? base.GetHashCode() : PersonId.GetHashCode();
        //}
    }

    public class Role//: IEquatable<Role>
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public Int32 RoleId { get; set; }
        public String RoleName { get; set; }
        public RoleType InRole { get; set; }

        public virtual Movie Movie { get; set; }
        public virtual Person Person { get; set; }

        public Role()
        {
            InRole = RoleType.None;
        }
        //public override bool Equals(Object obj)
        //{
        //    return this.Equals((Role)obj);
        //}
        //public bool Equals(Role other)
        //{
        //    return GetHashCode() == other.GetHashCode();
        //}
        //public override int GetHashCode()
        //{
        //    return RoleId == 0 ? base.GetHashCode() : RoleId.GetHashCode();
        //}
    }


    //public class PersonDisplay : Dateable
    //{
    //    public Int32 PersonId { get; set; }
    //    public String Name { get; set; }

    //    public Int32 RoleId { get; set; }
    //    public String RoleName { get; set; }
    //    public RoleType InRole { get; set; }

    //    public Int32 MovieYear { get; set; }

    //    public Boolean NetflixNeedsUpdate { get; set; }
    //    public Boolean RottenTomatoesNeedsUpdate { get; set; }
    //    public Boolean ImdbNeedsUpdate { get; set; }

    //    public String GetDetailsUrl()
    //    {
    //        return "/people/details/" + PersonId + "/" + Name.UrlFriendly();
    //    }
    //}
}