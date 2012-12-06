using FilmTrove.Filters;
using FilmTrove.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace FilmTrove.Controllers
{
    [InitializeSimpleMembership]
    public class ListsController : Controller
    {
        //
        // GET: /Lists/

        public ActionResult Collection()
        {
            if (WebSecurity.IsAuthenticated)
            {
                FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
                UserProfile prof = ftc.UserProfiles.Find(WebSecurity.CurrentUserId);
                if (prof.UserLists.Count < 1)//.Collection == null)
                {
                    UserList collection = new UserList();
                    collection.ListName = "My Collection";
                    collection.Owner = prof;
                    //collection.Items = new HashSet<UserListItem>();
                    //prof.UserLists = new List<UserList>() { collection };
                    ftc.Lists.Add(collection);
                }
                ftc.SaveChanges();
            }
            return View();
        }


        [HttpGet]
        public ActionResult Links(String id, String title)
        {
            if (WebSecurity.IsAuthenticated)
            {
                Int32 movieid = Convert.ToInt32(id);
                ViewBag.Id = id;
                FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
                Movie movie = ftc.Movies.Find(movieid);
                UserProfile up = ftc.UserProfiles.Include("UserLists.Items").Where(u=> u.UserId == WebSecurity.CurrentUserId).Single();
                if (up.UserLists.Count < 1)
                {
                    ///this is only temporary
                    ftc.Lists.Add(new UserList() { Owner = up, ListName = "My Collection" });
                    ftc.SaveChanges();
                }
                ViewBag.Lists = up.UserLists.Select(l => 
                    new ListInfo { 
                        ListId = l.ListId, 
                        ListName = l.ListName, 
                        InList = l.Items.Any(i => i.MovieId == movie.MovieId) 
                    }).ToList();

            }
            else
            {
                ViewBag.Id = id;
                ViewBag.FriendlyTitle = "Please log in to add titles to your lists.";
            }
            return View();
        }

        [HttpPost] ///add title to list
        public JsonResult Add(String movieid, String listid, String formats)
        {
            formats = formats.Trim().Replace(" ", ", ").Replace("-","");
            FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
            UserProfile up = ftc.UserProfiles.Include("UserLists.Items").Where(u => u.UserId == WebSecurity.CurrentUserId).Single();
            Int32 lid = Convert.ToInt32(listid);
            Int32 mid = Convert.ToInt32(movieid);
            UserList list = up.UserLists.Where(l => l.ListId == lid).Single();
            //UserList list = ftc.Lists.Where(l => l.ListId == lid && l.Owner == up).Single();
            if (list.Items.Any(i => i.MovieId == mid))
            {
                return Json(new { Success = false, Message = "Err. The list already contains that title." });
            }
            Movie movie = ftc.Movies.Find(Convert.ToInt32(movieid));
            UserListItem uli = ftc.ListItems.Create();
            uli.List = list;
            uli.Movie = movie;
            uli.MovieId = movie.MovieId;
            uli.MovieTitle = movie.Title;
            uli.OwnedFormats = (Format)Enum.Parse(typeof(Format), formats);
            
            ftc.ListItems.Add(uli);
            ftc.SaveChanges();

            return Json(new { Success = true});
        }

        public ActionResult Delete(String list, String id, String title)
        {
            return View("Lists");
        }

        public ActionResult Rename(String list, String id, String title)
        {
            return View("Lists");
        }

        [HttpPost]///new list
        public JsonResult New(String listname, String movieid)
        {
            Int32 listid = -1;
            if (WebSecurity.IsAuthenticated)
            {
                FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
                UserProfile up = ftc.UserProfiles.Find(WebSecurity.CurrentUserId);
                if (up.UserLists.Where(l => l.ListName == listname).Count() < 1)
                {
                    UserList ul = ftc.Lists.Add(new UserList()
                    {
                        ListName = listname,
                        Owner = up
                    });
                    ftc.SaveChanges();
                    //UserList ul = ftc.Lists.Where(l => l.ListName == listname).Single();
                    listid = ul.ListId;
                }
                else
                    return this.Json(data: new { Success = false, Message = "You already have a list named \"" + listname + "\"" });
            }
            else
            {
            }
            return this.Json(data: new { Success = true, ListId = listid, ListName = listname, MovieId = movieid });
        }
    }
}
