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
                if (prof.UserLists == null || prof.UserLists.Count < 1)//.Collection == null)
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
                ViewBag.FriendlyTitle = title;
            }
            else
            {
                ViewBag.Id = id;
                ViewBag.FriendlyTitle = "Please log in to add titles to your lists.";
            }
            return View();
        }

        public ActionResult Add(String movieid, String movietitle, String listid, String listtitle)
        {
            return View();
        }

        public ActionResult Delete(String list, String id, String title)
        {
            return View("Lists");
        }

        public ActionResult Rename(String list, String id, String title)
        {
            return View("Lists");
        }

        public ActionResult New(String list, String id, String title)
        {
            
            return View("Lists");
        }
    }
}
