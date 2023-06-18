using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace App.Menu {
    public class SidebarItem {
        public enum SidebarItemType
        {
            Divider,
            Heading,
            NavItem
        }
        public string title {get; set;}
        public bool isActive {get; set;}
        public SidebarItemType sidebarItemType {get; set;}

        public string area {get; set;}
        public string controller {get; set;}
        public string action {get; set;}
        public string awesomeIcon {get; set;}
        public List<SidebarItem> items {get; set;}

        public string collapseId {get;set;}

        public string GetLink(IUrlHelper urlHelper) {
            return urlHelper.Action(action, controller, new { area = area});
        }

        public string RenderHtml(IUrlHelper urlHelper) {
            var html = new StringBuilder();
            if (sidebarItemType == SidebarItemType.Divider) {
                html.Append("<hr class=\"sidebar-divider my-2\">");
            } else if (sidebarItemType == SidebarItemType.Heading) {
                html.Append(@$"<div class=""sidebar-heading\"">
                                {title}
                            </div>");
            } else {
                if (items == null) {
                    var url = GetLink(urlHelper);
                    var icon = (awesomeIcon != null)? $"<i class=\"{awesomeIcon}\"></i>" : "";
                    var cssClass = $"nav-item";
                    if (isActive)
                        cssClass+= " active";

                    html.Append(@$"
                    <li class=""{cssClass}"">
                        <a class=""nav-link"" href=""{url}"">
                        {icon}
                        <span>{title}</span></a>
                    </li>
                    ");
                } else {
                    var icon = (awesomeIcon != null)? $"<i class=\"{awesomeIcon}\"></i>" : "";
                    var cssClass = $"nav-item";
                    if (isActive)
                        cssClass+= " active";
                    var collapseCss = "collapse";
                    if (isActive)
                        collapseCss += " active";

                    var itemMenu = "";

                    foreach (var item in items) {
                        var url = item.GetLink(urlHelper);
                        var cssItem = "collapse-item";
                        if (item.isActive)
                            cssItem += " active";
                        itemMenu = itemMenu + $"<a class=\"{cssItem}\" href=\"{url}\">{item.title}</a>";
                    }

                    html.Append(@$"
                        <li class=""{cssClass}"">
                            <a class=""nav-link collapsed"" href=""#"" data-toggle=""collapse"" data-target=""#{collapseId}""
                                aria-expanded=""true"" aria-controls=""{collapseId}"">
                                    {icon}
                                <span>{title}</span>
                            </a>
                            <div id=""{collapseId}"" class=""{collapseCss}"" aria-labelledby=""headingTwo"" data-parent=""#accordionSidebar"">
                                <div class=""bg-white py-2 collapse-inner rounded"">
                                    {itemMenu}
                                </div>
                            </div>
                        </li>
                    ");
                }
            }
            return html.ToString();
        }
    }
}