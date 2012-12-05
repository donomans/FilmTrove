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
                ViewBag.Id = id;
                FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
                UserProfile up = ftc.UserProfiles.Find(WebSecurity.CurrentUserId);
                if (up.UserLists.Count < 1)
                {
                    ///this is only temporary
                    ftc.Lists.Add(new UserList() { Owner = up, ListName = "My Collection" });
                    ftc.SaveChanges();
                }
                ViewBag.Lists = up.UserLists.Select(l => new ListInfo { ListId = l.ListId, ListName = l.ListName }).ToList();

            }
            else
            {
                ViewBag.Id = id;
                ViewBag.FriendlyTitle = "Please log in to add titles to your lists.";
            }
            return View();
        }

        public JsonResult Add(String movieid, String listid, String formats)
        {
            FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
            UserList list = ftc.Lists.Find(Convert.ToInt32(listid));
            Movie movie = ftc.Movies.Find(Convert.ToInt32(movieid));
            UserListItem uli = ftc.ListItems.Create();
            uli.List = list;
            uli.Movie = movie;
            uli.MovieTitle = movie.Title;
            uli.OwnedFormats = (Format)Enum.Parse(typeof(Format), formats);

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

        [HttpPost]
        public JsonResult New(String listname, String movieid)
        {
            Int32 listid = -1;
            if (WebSecurity.IsAuthenticated)
            {
                FilmTroveContext ftc = (FilmTroveContext)HttpContext.Items["ftcontext"];
                UserProfile up = ftc.UserProfiles.Find(WebSecurity.CurrentUserId);
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
            {
            }
            return this.Json(data: new { ListId = listid, ListName = listname, MovieId = movieid });
        }
    }
}
