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
using WebMatrix.WebData;
using FlixSharp.Holders;

namespace FilmTrove.Models
{
    [Table("UserProfile")]
    public class UserProfile// : IEquatable<UserProfile>
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public Int32 UserId { get; set; }
        public String UserName { get; set; }
        public String Provider { get; set; }

        public String Name { get; set; }
        public String Email { get; set; } ///not sure if I'm guaranteed to get an email back from Facebook?

        //public UserList Collection { get; set; }
        public virtual ICollection<UserList> UserLists { get; set; }

        public NetflixAccount NetflixAccount { get; set; }

        //public override bool Equals(Object obj)
        //{
        //    return this.Equals((UserProfile)obj);
        //}
        //public bool Equals(UserProfile other)
        //{
        //    return GetHashCode() == other.GetHashCode();
        //}
        //public override int GetHashCode()
        //{
        //    return UserId == 0 ? base.GetHashCode() : UserId.GetHashCode();
        //}
    }


    public class UserList : Dateable//, IEquatable<UserList>
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public Int32 ListId { get; set; }
        public String ListName { get; set; }
        public UserProfile Owner { get; set; }

        public virtual ICollection<UserListItem> Items { get; set; }

        //public override bool Equals(Object obj)
        //{
        //    return this.Equals((UserList)obj);
        //}
        //public bool Equals(UserList other)
        //{
        //    return GetHashCode() == other.GetHashCode();
        //}
        //public override int GetHashCode()
        //{
        //    return ListId == 0 ? base.GetHashCode() : ListId.GetHashCode();
        //}
    }
    public struct ListInfo
    {
        public Int32 ListId { get; set; }
        public String ListName { get; set; }
        public Boolean InList { get; set; }
    }

    public class UserListItem : Dateable//, IEquatable<UserListItem>
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public Int32 ListItemId { get; set; }

        public String MovieTitle { get; set; }
        public Int32? Rating { get; set; }

        public DateTime? LastWatched { get; set; }
        public Boolean LoanedOut { get; set; }
        public virtual UserProfile LoanedTo { get; set; }
        public Format OwnedFormats { get; set; }


        public virtual Movie Movie { get; set; }
        public Int32 MovieId { get; set; }
        public UserList List { get; set; }

        //public override bool Equals(Object obj)
        //{
        //    return this.Equals((UserListItem)obj);
        //}
        //public bool Equals(UserListItem other)
        //{
        //    return GetHashCode() == other.GetHashCode();
        //}
        //public override int GetHashCode()
        //{
        //    return ListItemId == 0 ? base.GetHashCode() : ListItemId.GetHashCode();
        //}
    }

    [Flags]
    public enum Format
    {
        DVD = 1,
        Digital = 2,
        Bluray = 4
    }

    [ComplexType]
    public class NetflixAccount
    {
        public String Token { get; set; }
        public String TokenSecret { get; set; }
        public String UserId { get; set; }

        public static Account GetCurrentUserNetflixUserInfo()
        {
            Object netflixuserid = HttpContext.Current.Session["netflixuserid"];
            if (netflixuserid == null)
            {
                if (WebMatrix.WebData.WebSecurity.IsAuthenticated)
                    using (FilmTroveContext ftc = new FilmTroveContext())
                    {
                        UserProfile profile = ftc.UserProfiles.Find(WebSecurity.CurrentUserId);

                        Account na = new Account()
                        {
                            Token = profile.NetflixAccount.Token,
                            TokenSecret = profile.NetflixAccount.TokenSecret,
                            UserId = profile.NetflixAccount.UserId
                        };
                        HttpContext.Current.Session["netflixuserid"] = na;

                        return na;
                    }
                else
                    return null;
            }
            else
                return (Account)netflixuserid;
        }
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
            if (up.UserName.Contains("@"))
                Email = up.UserName;
            else
                Email = up.Email;

            Name = up.Name;
        }
        public String Name { get; set; }
        public String Email { get; set; }
    }
}