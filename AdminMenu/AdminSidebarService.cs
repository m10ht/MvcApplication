using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace App.Menu {
    public class AdminSidebarService {
        private readonly IUrlHelper urlHelper;

        public AdminSidebarService(IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
        {
            this.urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            sidebarItems.Add(new SidebarItem() {
                sidebarItemType = SidebarItem.SidebarItemType.Divider
            });
            sidebarItems.Add(new SidebarItem() {
                sidebarItemType = SidebarItem.SidebarItemType.Heading,
                title = "Quản lý chung"
            });

            sidebarItems.Add(new SidebarItem() {
                sidebarItemType = SidebarItem.SidebarItemType.NavItem,
                area = "Database",
                controller = "DbManage",
                action = "Index", 
                title = "Quản lý Database",
                awesomeIcon = "fa-solid fa-database"
            });

            sidebarItems.Add(new SidebarItem() {
                sidebarItemType = SidebarItem.SidebarItemType.NavItem,
                area = "Contact",
                controller = "Contact",
                action = "Index", 
                title = "Quản lý liên hệ",
                awesomeIcon = "fa-solid fa-address-card"
            });

            sidebarItems.Add(new SidebarItem() {
                sidebarItemType = SidebarItem.SidebarItemType.Divider
            });

            sidebarItems.Add(new SidebarItem() {
                sidebarItemType = SidebarItem.SidebarItemType.NavItem, 
                title = "Thành viên và phân quyền",
                awesomeIcon = "fa-solid fa-folder-tree",
                collapseId = "role",
                items = new List<SidebarItem>() {
                    new SidebarItem() {
                        sidebarItemType = SidebarItem.SidebarItemType.NavItem,
                        area = "Identity",
                        controller = "Role",
                        action = "Index", 
                        title = "Các vai trò (roles)"
                    },
                    new SidebarItem() {
                        sidebarItemType = SidebarItem.SidebarItemType.NavItem,
                        area = "Identity",
                        controller = "Role",
                        action = "Create", 
                        title = "Tạo role"
                    },
                    new SidebarItem() {
                        sidebarItemType = SidebarItem.SidebarItemType.NavItem,
                        area = "Identity",
                        controller = "User",
                        action = "Index", 
                        title = "Danh sách thành viên"
                    }
                }
            });

            sidebarItems.Add(new SidebarItem() {
                sidebarItemType = SidebarItem.SidebarItemType.Divider
            });

            sidebarItems.Add(new SidebarItem() {
                sidebarItemType = SidebarItem.SidebarItemType.NavItem,
                title = "Quản lý Blogs",
                awesomeIcon = "fa-solid fa-folder-tree",
                collapseId = "blog",
                items = new List<SidebarItem>() {
                    new SidebarItem() {
                        sidebarItemType = SidebarItem.SidebarItemType.NavItem,
                        area = "Blog",
                        controller = "Category",
                        action = "Index", 
                        title = "Chuyên mục Blog"
                    },
                    new SidebarItem() {
                        sidebarItemType = SidebarItem.SidebarItemType.NavItem,
                        area = "Blog",
                        controller = "Category",
                        action = "Create", 
                        title = "Tạo chuyên mục Blog"
                    },
                    new SidebarItem() {
                        sidebarItemType = SidebarItem.SidebarItemType.NavItem,
                        area = "Blog",
                        controller = "Post",
                        action = "Index", 
                        title = "Danh sách bài Posts"
                    },
                    new SidebarItem() {
                        sidebarItemType = SidebarItem.SidebarItemType.NavItem,
                        area = "Blog",
                        controller = "Post",
                        action = "Create", 
                        title = "Tạo bài viết mới"
                    }
                }
            });

            sidebarItems.Add(new SidebarItem() {
                sidebarItemType = SidebarItem.SidebarItemType.Divider
            });

            sidebarItems.Add(new SidebarItem() {
                sidebarItemType = SidebarItem.SidebarItemType.NavItem,
                title = "Quản lý sản phẩm",
                awesomeIcon = "fa-solid fa-folder-tree",
                collapseId = "product",
                items = new List<SidebarItem>() {
                    new SidebarItem() {
                        sidebarItemType = SidebarItem.SidebarItemType.NavItem,
                        area = "Product",
                        controller = "CategoryProduct",
                        action = "Index", 
                        title = "Chuyên mục sản phẩm"
                    },
                    new SidebarItem() {
                        sidebarItemType = SidebarItem.SidebarItemType.NavItem,
                        area = "Product",
                        controller = "CategoryProduct",
                        action = "Create", 
                        title = "Tạo chuyên mục sản phẩm"
                    },
                    new SidebarItem() {
                        sidebarItemType = SidebarItem.SidebarItemType.NavItem,
                        area = "Product",
                        controller = "ProductManage",
                        action = "Index", 
                        title = "Danh sách sản phẩm"
                    },
                    new SidebarItem() {
                        sidebarItemType = SidebarItem.SidebarItemType.NavItem,
                        area = "Product",
                        controller = "ProductManage",
                        action = "Create", 
                        title = "Tạo sản phẩm mới"
                    }
                }
            });


        }

        List<SidebarItem> sidebarItems {get; set;} = new List<SidebarItem>();

        public string CreateHtml() {
            var html = new StringBuilder();
            foreach (var item in sidebarItems) {
                html.Append(item.RenderHtml(urlHelper));
            }
            return html.ToString();
        }

        public void SetActive(string Area, string Controller, string Action) {
            foreach (var item in sidebarItems) {
                if (item.area == Area && item.controller == Controller && item.action == Action) {
                    item.isActive = true;
                    return;
                } else {
                    if (item.items != null) {
                        foreach (var childrenItem in item.items) {
                            if (childrenItem.area == Area && childrenItem.controller == Controller && childrenItem.action == Action) {
                                childrenItem.isActive = true;
                                item.isActive = true;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}