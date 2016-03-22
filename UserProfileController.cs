using MGM.Web.Models.ViewModels;
using MGM.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MGM.Web.Controllers
{
    [Authorize]
    public class UserProfileController : BaseController
    {
        public UserProfileController(ISecurityService securityService): base(securityService)
        {

        }

        // GET: UserProfile
        public ActionResult Index()
        {
            return View();
        }

        [Route("~/profile")]
        public ActionResult UserProfile()
        {
            ItemViewModel<KeyValuePair<int, string>[]> model = GetViewModel<ItemViewModel<KeyValuePair<int, string>[]>>();

            model.Item = UserRoles.NotSet.ToJsonDictionary();

            return View(model);
        }
    }
}
