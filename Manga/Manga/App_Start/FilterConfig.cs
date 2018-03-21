using System.Web.Mvc;
using Manga.Filter;

namespace Manga
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {

            filters.Add(new HandleErrorAttribute());
            //filters.Add(new MVCAuthPermission());
        }
    }
}
