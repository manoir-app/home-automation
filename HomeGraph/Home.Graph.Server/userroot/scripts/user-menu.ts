﻿

namespace Manoir.User {

    class ExtensionPoint {
        isForAdmin: boolean;
        name: string;
        url: string;
        icon: string;
    }

    export class MainMenuBar extends HTMLElement {
        connectedCallback() {
            

            var self = this;
            this.innerHTML = ''
                + '		<nav class="sidebar">                                                                                                                   '
                + '      <div class="sidebar-header">                                                                                                           '
                + '        <a href="/me/" class="sidebar-brand">                                                                                                   '
                + '          <span>ma</span>NOIR<span class="dev-tag" style="display:none">(dev)</span>'
                + '        </a>                                                                                                                                 '
                + '        <div class="sidebar-toggler not-active">                                                                                             '
                + '          <span></span>                                                                                                                      '
                + '          <span></span>                                                                                                                      '
                + '          <span></span>                                                                                                                      '
                + '        </div>                                                                                                                               '
                + '      </div>                                                                                                                                 '
                + '      <div class="sidebar-body">                                                                                                             '
                + '        <ul class="nav" id="maNoirMainMenu">                                                                                                                     '

                + '          <li class="nav-item nav-category">HOME APPS</li>                                                                                        '
                + '          <li class="nav-item">                                                                                                              '
                + '            <a href="/me/" class="nav-link">                                                                                       '
                + '              <i class="link-icon" data-feather="home"></i>                                                                                   '
                + '              <span class="link-title">Dashboard</span>                                                                                      '
                + '            </a>                                                                                                                             '
                + '          </li>                                                                                                                              '
                + '          <li class="nav-item">                                                                                                              '
                + '            <a class="nav-link" data-bs-toggle="collapse" href="#mehomeauto" role="button"'
                + '                 aria-expanded="false" aria-controls="mekitchen">         '
                + '              <i class="link-icon" data-feather="box"></i>                                                                                  '
                + '              <span class="link-title">Home automation</span>                                                                                          '
                + '              <i class="link-arrow" data-feather="chevron-down"></i>                                                                         '
                + '            </a>                                                                                                                             '
                + '            <div class="collapse" id="mehomeauto">                                                                                               '
                + '              <ul class="nav sub-menu">                                                                                                      '
                + '                <li class="nav-item">                                                                                                        '
                + '                  <a href="/me/home-automation.htm" class="nav-link">Home</a>                                                                '
                + '                </li>                                                                                                                        '
                + '                <li class="nav-item">                                                                                                        '
                + '                  <a href="/me/home-automation-control.htm" class="nav-link">Devices</a>                                                                  '
                + '                </li>                                                                                                                        '
                + '              </ul>                                                                                                                          '
                + '            </div>                                                                                                                           '
                + '          </li>                                                                                                                              '
                + '          <li class="nav-item">                                                                                                              '
                + '            <a href="/me/chat.htm" class="nav-link">                                                                                       '
                + '              <i class="link-icon" data-feather="message-square"></i>                                                                                   '
                + '              <span class="link-title">Chat <span class="position-absolute top-11 ms-1 p-1 bg-primary border border-light rounded-circle" id="mainNavChatPill"></span></span>    '
                + '            </a>                                                                                                                             '
                + '          </li>                                                                                                                              '
                + '          <li class="nav-item disabled">                                                                                                              '
                + '            <a href="/me/calendar.htm" class="nav-link">                                                                                       '
                + '              <i class="link-icon" data-feather="calendar"></i>                                                                                   '
                + '              <span class="link-title">Calendar</span>                                                                                      '
                + '            </a>                                                                                                                             '
                + '          </li>                                                                                                                              '
                + '          <li class="nav-item">                                                                                                              '
                + '            <a class="nav-link" data-bs-toggle="collapse" href="#mekitchen" role="button"'
                + '                 aria-expanded="false" aria-controls="mekitchen">         '
                + '              <i class="link-icon" data-feather="coffee"></i>                                                                                  '
                + '              <span class="link-title">Food & co</span>                                                                                          '
                + '              <i class="link-arrow" data-feather="chevron-down"></i>                                                                         '
                + '            </a>                                                                                                                             '
                + '            <div class="collapse" id="mekitchen">                                                                                               '
                + '              <ul class="nav sub-menu">                                                                                                      '
                + '                <li class="nav-item disabled">                                                                                                        '
                + '                  <a href="/me/recipes.htm" class="nav-link">Recipes</a>                                                                '
                + '                </li>                                                                                                                        '
                + '                <li class="nav-item disabled">                                                                                                        '
                + '                  <a href="/me/kitchen-meals.htm" class="nav-link">Meal planning</a>                                                                  '
                + '                </li>                                                                                                                        '
                + '              </ul>                                                                                                                          '
                + '            </div>                                                                                                                           '
                + '          </li>                                                                                                                              '
                + '          <li class="nav-item">                                                                                                              '
                + '            <a class="nav-link" data-bs-toggle="collapse" href="#meproducts" role="button"'
                + '                 aria-expanded="false" aria-controls="meproducts">         '
                + '              <i class="link-icon" data-feather="archive"></i>                                                                                  '
                + '              <span class="link-title">Products</span>                                                                                          '
                + '              <i class="link-arrow" data-feather="chevron-down"></i>                                                                         '
                + '            </a>                                                                                                                             '
                + '            <div class="collapse" id="meproducts">                                                                                               '
                + '              <ul class="nav sub-menu">                                                                                                      '
                + '                <li class="nav-item">                                                                                                        '
                + '                  <a href="/me/products.htm" class="nav-link">List</a>                                                                '
                + '                </li>                                                                                                                        '
                + '                <li class="nav-item">                                                                                                        '
                + '                  <a href="/me/stocks.htm" class="nav-link">Stocks</a>                                                                  '
                + '                </li>                                                                                                                        '
                + '              </ul>                                                                                                                          '
                + '            </div>                                                                                                                           '
                + '          </li>                                                                                                                              '
                + '          <li class="nav-item disabled">                                                                                                              '
                + '            <a href="/me/shopping-list.htm" class="nav-link">                                                                                       '
                + '              <i class="link-icon" data-feather="shopping-cart"></i>                                                                                   '
                + '              <span class="link-title">Shopping List</span>                                                                                      '
                + '            </a>                                                                                                                             '
                + '          </li>                                                                                                                              '
                + '          <li class="nav-item">                                                                                                              '
                + '            <a href="/me/oauth.htm" class="nav-link">                                                                                       '
                + '              <i class="link-icon" data-feather="cloud"></i>                                                                                   '
                + '              <span class="link-title">Connected Services</span>                                                                                      '
                + '            </a>                                                                                                                             '
                + '          </li>                                                                                                                              '
                + '          <li class="nav-item nav-category" data-kind="extensions-category" style="display:none">EXTENSIONS</li>       '


                + '          <li class="nav-item nav-category" data-kind="admin-category" style="display:none"><a href="/admin/">ADMINISTRATION </a></li>       '
                + '          <li class="nav-item" data-kind="admin-category" style="display:none">                                                                                                              '
                + '            <a class="nav-link" data-bs-toggle="collapse" href="#adminlocations" role="button"'
                + '                 aria-expanded="false" aria-controls="adminlocations">         '
                + '              <i class="link-icon" data-feather="map"></i>                                                                                  '
                + '              <span class="link-title">Locations & Mesh</span>                                                                                          '
                + '              <i class="link-arrow" data-feather="chevron-down"></i>                                                                         '
                + '            </a>                                                                                                                             '
                + '            <div class="collapse" id="adminlocations">                                                                                               '
                + '              <ul class="nav sub-menu">                                                                                                      '
                + '                <li class="nav-item">                                                                                                        '
                + '                  <a href="/admin/locations.htm" class="nav-link">Locations</a>                                                                '
                + '                </li>                                                                                                                        '
                + '                <li class="nav-item">                                                                                                        '
                + '                  <a href="/admin/local-mesh.htm" class="nav-link">Local Mesh</a>                                                                  '
                + '                </li>                                                                                                                        '
                + '              </ul>                                                                                                                          '
                + '            </div>                                                                                                                           '
                + '          </li>                                                                                                                              '

                //+ '          <li class="nav-item" data-kind="admin-category" style="display:none">                                                                                                              '
                //+ '            <a href="/admin/agents.htm" class="nav-link" >                                                                                       '
                //+ '              <i class="link-icon" data-feather="cpu"></i>                                                                                   '
                //+ '              <span class="link-title">Agents</span>                                                                                      '
                //+ '            </a>                                                                                                                             '
                //+ '          </li>                                                                                                                              '

                + '          <li class="nav-item" data-kind="admin-category" style="display:none">                                                                                                              '
                + '            <a class="nav-link" data-bs-toggle="collapse" href="#adminapps" role="button"'
                + '                 aria-expanded="false" aria-controls="adminapps">         '
                + '              <i class="link-icon" data-feather="grid"></i>                                                                                  '
                + '              <span class="link-title">Modules</span>                                                                                          '
                + '              <i class="link-arrow" data-feather="chevron-down"></i>                                                                         '
                + '            </a>                                                                                                                             '
                + '            <div class="collapse" id="adminapps">                                                                                               '
                + '              <ul class="nav sub-menu">                                                                                                      '
                + '                <li class="nav-item">                                                                                                        '
                + '                  <a href="/admin/agents.htm" class="nav-link">Agents</a>                                                                '
                + '                </li>                                                                                                                        '
                + '                <li class="nav-item disabled">                                                                                                        '
                + '                  <a href="/admin/apps.htm" class="nav-link">Web Apps</a>                                                                '
                + '                </li>                                                                                                                        '
                + '                <li class="nav-item">                                                                                                        '
                + '                  <a href="/admin/extensions.htm" class="nav-link">Extensions</a>                                                                  '
                + '                </li>                                                                                                                        '
                + '                <li class="nav-item">                                                                                                        '
                + '                  <a href="/admin/tokens.htm" class="nav-link">Integrations</a>                                                                  '
                + '                </li>                                                                                                                        '
                + '              </ul>                                                                                                                          '
                + '            </div>                                                                                                                           '
                + '          </li>                                                                                                                              '

                + '          <li class="nav-item" data-kind="admin-category" style="display:none">                                                                                                              '
                + '            <a href="/admin/logs.htm" class="nav-link" >                                                                                       '
                + '              <i class="link-icon" data-feather="align-justify"></i>                                                                                   '
                + '              <span class="link-title">Logs</span>                                                                                      '
                + '            </a>                                                                                                                             '
                + '          </li>  '

                + '          <li class="nav-item" data-kind="admin-category" style="display:none">                                                                                                              '
                + '            <a class="nav-link" data-bs-toggle="collapse" href="#adminusers" role="button"'
                + '                 aria-expanded="false" aria-controls="adminusers">         '
                + '              <i class="link-icon" data-feather="users"></i>                                                                                  '
                + '              <span class="link-title">Users</span>                                                                                          '
                + '              <i class="link-arrow" data-feather="chevron-down"></i>                                                                         '
                + '            </a>                                                                                                                             '
                + '            <div class="collapse" id="adminusers">                                                                                               '
                + '              <ul class="nav sub-menu">                                                                                                      '
                + '                <li class="nav-item">                                                                                                        '
                + '                  <a href="/admin/users.htm" class="nav-link">Users</a>                                                                '
                + '                </li>                                                                                                                        '
                + '                <li class="nav-item disabled">                                                                                                        '
                + '                  <a href="/admin/contacts.htm" class="nav-link">Contacts</a>                                                                  '
                + '                </li>                                                                                                                        '
                + '              </ul>                                                                                                                          '
                + '            </div>                                                                                                                           '
                + '          </li>                                                                                                                              '
                + '          <li class="nav-item" data-kind="admin-category" style="display:none">                                                                                                              '
                + '            <a class="nav-link" data-bs-toggle="collapse" href="#adminactions" role="button"'
                + '                 aria-expanded="false" aria-controls="adminactions">         '
                + '              <i class="link-icon" data-feather="zap"></i>                                                                                  '
                + '              <span class="link-title">Actions & Routines</span>                                                                                          '
                + '              <i class="link-arrow" data-feather="chevron-down"></i>                                                                         '
                + '            </a>                                                                                                                             '
                + '            <div class="collapse" id="adminactions">                                                                                               '
                + '              <ul class="nav sub-menu">                                                                                                      '
                + '                <li class="nav-item">                                                                                                        '
                + '                  <a href="/admin/scenarios.htm" class="nav-link">Scenes</a>                                                                '
                + '                </li>                                                                                                                        '
                + '                <li class="nav-item">                                                                                                        '
                + '                  <a href="/admin/routines.htm" class="nav-link">Routines</a>                                                                  '
                + '                </li>                                                                                                                        '
                + '              </ul>                                                                                                                          '
                + '            </div>                                                                                                                           '
                + '          </li>                                                                                                                              '
                + '          <li class="nav-item" data-kind="admin-category" style="display:none">                                                                                                              '
                + '            <a class="nav-link" data-bs-toggle="collapse" href="#adminentities" role="button"'
                + '                 aria-expanded="false" aria-controls="adminactions">         '
                + '              <i class="link-icon" data-feather="smartphone"></i>                                                                                  '
                + '              <span class="link-title">Devices & entities</span>                                                                                          '
                + '              <i class="link-arrow" data-feather="chevron-down"></i>                                                                         '
                + '            </a>                                                                                                                             '
                + '            <div class="collapse" id="adminentities">                                                                                               '
                + '              <ul class="nav sub-menu">                                                                                                      '
                + '                <li class="nav-item">                                                                                                        '
                + '                  <a href="/admin/devices.htm" class="nav-link">Devices</a>                                                                '
                + '                </li>                                                                                                                        '
                + '                <li class="nav-item">                                                                                                        '
                + '                  <a href="/admin/entities.htm" class="nav-link">Entities</a>                                                                  '
                + '                </li>                                                                                                                        '
                + '              </ul>                                                                                                                          '
                + '            </div>                                                                                                                           '
                + '          </li>                                                                                                                              '
                //+ '          <li class="nav-item" data-kind="admin-category" style="display:none">                                                                                                              '
                //+ '            <a href="/admin/devices.htm" class="nav-link" >                                                                                       '
                //+ '              <i class="link-icon" data-feather="smartphone"></i>                                                                                   '
                //+ '              <span class="link-title">Devices</span>                                                                                      '
                //+ '            </a>                                                                                                                             '
                //+ '          </li>                                                                                                                              '

                + '          <li class="nav-item" data-kind="admin-category" style="display:none">                                                                                                              '
                + '            <a class="nav-link" data-bs-toggle="collapse" href="#adminsettings" role="button"'
                + '                 aria-expanded="false" aria-controls="adminsettings">         '
                + '              <i class="link-icon" data-feather="settings"></i>                                                                                  '
                + '              <span class="link-title">Global settings</span>                                                                                          '
                + '              <i class="link-arrow" data-feather="chevron-down"></i>                                                                         '
                + '            </a>                                                                                                                             '
                + '            <div class="collapse" id="adminsettings">                                                                                               '
                + '              <ul class="nav sub-menu">                                                                                                      '
                + '                <li class="nav-item">                                                                                                        '
                + '                  <a href="/admin/settings/units.htm" class="nav-link">Units</a>                                                                '
                + '                </li>                                                                                                                        '
                + '                <li class="nav-item">                                                                                                        '
                + '                  <a href="/admin/settings/recipeconfigs.htm" class="nav-link">Recipe configuration</a>                                                                '
                + '                </li>                                                                                                                        '
                + '              </ul>                                                                                                                          '
                + '            </div>                                                                                                                           '
                + '          </li>                                                                                                                              '

                + '        </ul>                                                                                                                               '
                + '      </div>                                                                                                                                '
                + '    </nav>                                                                                                                                  '
                ;

            var ext = new Array<ExtensionPoint>();

            if (localStorage != null) {
                var tmp = localStorage.getItem("maNoir-menu-extensions");
                if (tmp != null) {
                    var extTmp: Array<ExtensionPoint> = JSON.parse(tmp);
                    if (extTmp != null)
                        ext = extTmp;
                }
            }
            if (ext != null) {
                self.addExtensions(ext);
            }

            setTimeout(() => {
                $.ajax({
                    url: '/v1.0/users/me/is-admin',
                    type: 'GET',
                    headers: {
                        'Accept': 'application/json',
                        'Content-Type': 'application/json'
                    },
                })
                    .done(function (data: boolean) {
                        if (data) {
                            $("*[data-kind='admin-category']").show();
                        }
                    });
            }, 200);
        }

        addExtensions(ext: ExtensionPoint[]) {
            var mnu = $("#maNoirMainMenu");
            var catNonAdmin = mnu.find("*[data-kind='extensions-category']");
            
            for (var i = 0; i < ext.length; i++) {
                var extItem = ext[i];
                if (!extItem.isForAdmin) {
                    catNonAdmin.show();
                    var mnuItem = $(""
                        + '          <li class="nav-item">                                                                                                              '
                        + '            <a href="'
                        + extItem.url
                        + '" class="nav-link">'
                        + '              <i class="link-icon" data-feather="settings"></i>                                                                                   '
                        + '              <span class="link-title">'
                        + extItem.name
                        + ' </span>                                                                                      '
                        + '            </a>                                                                                                                             '
                        + '          </li>                                                                                                                              '
                    );
                    mnuItem.insertAfter(catNonAdmin);
                }
            }
        }

    }

    export class ManoirHeadControl extends HTMLElement {
        connectedCallback() {
            var self = this;
            this.innerHTML = ''
            + '<link rel="preconnect" href="https://fonts.googleapis.com">'
            + '<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>'
            + '<link href="https://fonts.googleapis.com/css2?family=Roboto:wght@300;400;500;700;900&display=swap" rel="stylesheet">'
            + '<link rel="stylesheet" href="/assets/core.css">'
            + '<link rel="stylesheet" href="/assets/style.css">';
            + '<link rel="stylesheet" href="/assets/iconfont.css">';
        }
    }

    export class ManoirHeaderBar extends HTMLElement {
        connectedCallback() {
            var self = this;                                                                                                                                                                                                                                          
            this.innerHTML = ''                                                                                                                                                                                                                                       
                + '	<nav class="navbar">                                                                                                                                                                                                                                      '
                + '		<a href="#" class="sidebar-toggler">                                                                                                                                                                                                                  '
                + '			<i data-feather="menu"></i>                                                                                                                                                                                                                       '
                + '		</a>                                                                                                                                                                                                                                                  '
                + '		<div class="navbar-content">                                                                                                                                                                                                                          '
                + '			<form class="search-form">                                                                                                                                                                                                                        '
                + '				<div class="input-group">                                                                                                                                                                                                                     '
                + '      <div class="input-group-text">                                                                                                                                                                                                                        '
                + '        <i data-feather="terminal"></i>                                                                                                                                                                                                                       '
                + '      </div>                                                                                                                                                                                                                                                '
                + '					<input type="text" class="form-control" id="commandBar" placeholder="Type commands here">                                                                                                                                                     '
                + '				</div>                                                                                                                                                                                                                                        '
                + '			</form>                                                                                                                                                                                                                                           '
                + '			<ul class="navbar-nav">                                                                                                                                                                                                                           '
                //+ '				<li class="nav-item dropdown">                                                                                                                                                                                                                '
                //+ '					<a class="nav-link dropdown-toggle" href="#" id="appsDropdown" role="button" data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false">                                                                                        '
                //+ '						<i data-feather="grid"></i>                                                                                                                                                                                                           '
                //+ '					</a>                                                                                                                                                                                                                                      '
                //+ '					<div class="dropdown-menu p-0" aria-labelledby="appsDropdown">                                                                                                                                                                            '
                //+ '        <div class="px-3 py-2 d-flex align-items-center justify-content-between border-bottom">                                                                                                                                                             '
                //+ '							<p class="mb-0 fw-bold">Web Apps</p>                                                                                                                                                                                              '
                //+ '							<a href="javascript:;" class="text-muted">Edit</a>                                                                                                                                                                                '
                //+ '						</div>                                                                                                                                                                                                                                '
                //+ '        <div class="row g-0 p-1">                                                                                                                                                                                                                           '
                //+ '          <div class="col-3 text-center">                                                                                                                                                                                                                   '
                //+ '            <a href="pages/apps/chat.html" class="dropdown-item d-flex flex-column align-items-center justify-content-center wd-70 ht-70"><i data-feather="message-square" class="icon-lg mb-1"></i><p class="tx-12">Chat</p></a>                           '
                //+ '          </div>                                                                                                                                                                                                                                            '
                //+ '          <div class="col-3 text-center">                                                                                                                                                                                                                   '
                //+ '            <a href="pages/apps/calendar.html" class="dropdown-item d-flex flex-column align-items-center justify-content-center wd-70 ht-70"><i data-feather="calendar" class="icon-lg mb-1"></i><p class="tx-12">Calendar</p></a>                         '
                //+ '          </div>                                                                                                                                                                                                                                            '
                //+ '          <div class="col-3 text-center">                                                                                                                                                                                                                   '
                //+ '            <a href="pages/email/inbox.html" class="dropdown-item d-flex flex-column align-items-center justify-content-center wd-70 ht-70"><i data-feather="mail" class="icon-lg mb-1"></i><p class="tx-12">Email</p></a>                                  '
                //+ '          </div>                                                                                                                                                                                                                                            '
                //+ '          <div class="col-3 text-center">                                                                                                                                                                                                                   '
                //+ '            <a href="pages/general/profile.html" class="dropdown-item d-flex flex-column align-items-center justify-content-center wd-70 ht-70"><i data-feather="instagram" class="icon-lg mb-1"></i><p class="tx-12">Profile</p></a>                       '
                //+ '          </div>                                                                                                                                                                                                                                            '
                //+ '        </div>                                                                                                                                                                                                                                              '
                //+ '						<div class="px-3 py-2 d-flex align-items-center justify-content-center border-top">                                                                                                                                                   '
                //+ '							<a href="javascript:;">View all</a>                                                                                                                                                                                               '
                //+ '						</div>                                                                                                                                                                                                                                '
                //+ '					</div>                                                                                                                                                                                                                                    '
                //+ '				</li>                                                                                                                                                                                                                                         '
                //+ '				<li class="nav-item dropdown">                                                                                                                                                                                                                '
                //+ '					<a class="nav-link dropdown-toggle" href="#" id="notificationDropdown" role="button"                                                                                                                                                      '
                //+ '            data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false">                                                                                                                                                                           '
                //+ '						<i data-feather="bell"></i>                                                                                                                                                                                                           '
                //+ '						<div class="indicator">                                                                                                                                                                                                               '
                //+ '							<div class="circle"></div>                                                                                                                                                                                                        '
                //+ '						</div>                                                                                                                                                                                                                                '
                //+ '					</a>                                                                                                                                                                                                                                      '
                //+ '					<div class="dropdown-menu p-0" aria-labelledby="notificationDropdown">                                                                                                                                                                    '
                //+ '						<div class="px-3 py-2 d-flex align-items-center justify-content-between border-bottom">                                                                                                                                               '
                //+ '							<p>6 New Notifications</p>                                                                                                                                                                                                        '
                //+ '							<a href="javascript:;" class="text-muted">Clear all</a>                                                                                                                                                                           '
                //+ '						</div>                                                                                                                                                                                                                                '
                //+ '        <div class="p-1">                                                                                                                                                                                                                                   '
                //+ '          <a href="javascript:;" class="dropdown-item d-flex align-items-center py-2">                                                                                                                                                                      '
                //+ '            <div class="wd-30 ht-30 d-flex align-items-center justify-content-center bg-primary rounded-circle me-3">                                                                                                                                       '
                //+ '									<i class="icon-sm text-white" data-feather="gift"></i>                                                                                                                                                                    '
                //+ '            </div>                                                                                                                                                                                                                                          '
                //+ '            <div class="flex-grow-1 me-2">                                                                                                                                                                                                                  '
                //+ '									<p>New Order Recieved</p>                                                                                                                                                                                                 '
                //+ '									<p class="tx-12 text-muted">30 min ago</p>                                                                                                                                                                                '
                //+ '            </div>	                                                                                                                                                                                                                                      '
                //+ '          </a>                                                                                                                                                                                                                                              '
                //+ '          <a href="javascript:;" class="dropdown-item d-flex align-items-center py-2">                                                                                                                                                                      '
                //+ '            <div class="wd-30 ht-30 d-flex align-items-center justify-content-center bg-primary rounded-circle me-3">                                                                                                                                       '
                //+ '									<i class="icon-sm text-white" data-feather="alert-circle"></i>                                                                                                                                                            '
                //+ '            </div>                                                                                                                                                                                                                                          '
                //+ '            <div class="flex-grow-1 me-2">                                                                                                                                                                                                                  '
                //+ '									<p>Server Limit Reached!</p>                                                                                                                                                                                              '
                //+ '									<p class="tx-12 text-muted">1 hrs ago</p>                                                                                                                                                                                 '
                //+ '            </div>	                                                                                                                                                                                                                                      '
                //+ '          </a>                                                                                                                                                                                                                                              '
                //+ '          <a href="javascript:;" class="dropdown-item d-flex align-items-center py-2">                                                                                                                                                                      '
                //+ '            <div class="wd-30 ht-30 d-flex align-items-center justify-content-center bg-primary rounded-circle me-3">                                                                                                                                       '
                //+ '              <img class="wd-30 ht-30 rounded-circle" src="https://via.placeholder.com/30x30" alt="userr">                                                                                                                                                  '
                //+ '            </div>                                                                                                                                                                                                                                          '
                //+ '            <div class="flex-grow-1 me-2">                                                                                                                                                                                                                  '
                //+ '									<p>New customer registered</p>                                                                                                                                                                                            '
                //+ '									<p class="tx-12 text-muted">2 sec ago</p>                                                                                                                                                                                 '
                //+ '            </div>	                                                                                                                                                                                                                                      '
                //+ '          </a>                                                                                                                                                                                                                                              '
                //+ '          <a href="javascript:;" class="dropdown-item d-flex align-items-center py-2">                                                                                                                                                                      '
                //+ '            <div class="wd-30 ht-30 d-flex align-items-center justify-content-center bg-primary rounded-circle me-3">                                                                                                                                       '
                //+ '									<i class="icon-sm text-white" data-feather="layers"></i>                                                                                                                                                                  '
                //+ '            </div>                                                                                                                                                                                                                                          '
                //+ '            <div class="flex-grow-1 me-2">                                                                                                                                                                                                                  '
                //+ '									<p>Apps are ready for update</p>                                                                                                                                                                                          '
                //+ '									<p class="tx-12 text-muted">5 hrs ago</p>                                                                                                                                                                                 '
                //+ '            </div>	                                                                                                                                                                                                                                      '
                //+ '          </a>                                                                                                                                                                                                                                              '
                //+ '          <a href="javascript:;" class="dropdown-item d-flex align-items-center py-2">                                                                                                                                                                      '
                //+ '            <div class="wd-30 ht-30 d-flex align-items-center justify-content-center bg-primary rounded-circle me-3">                                                                                                                                       '
                //+ '									<i class="icon-sm text-white" data-feather="download"></i>                                                                                                                                                                '
                //+ '            </div>                                                                                                                                                                                                                                          '
                //+ '            <div class="flex-grow-1 me-2">                                                                                                                                                                                                                  '
                //+ '									<p>Download completed</p>                                                                                                                                                                                                 '
                //+ '									<p class="tx-12 text-muted">6 hrs ago</p>                                                                                                                                                                                 '
                //+ '            </div>	                                                                                                                                                                                                                                      '
                //+ '          </a>                                                                                                                                                                                                                                              '
                //+ '        </div>                                                                                                                                                                                                                                              '
                //+ '						<div class="px-3 py-2 d-flex align-items-center justify-content-center border-top">                                                                                                                                                   '
                //+ '							<a href="javascript:;">View all</a>                                                                                                                                                                                               '
                //+ '						</div>                                                                                                                                                                                                                                '
                //+ '					</div>                                                                                                                                                                                                                                    '
                //+ '				</li>                                                                                                                                                                                                                                         '
                //+ '				<li class="nav-item dropdown">                                                                                                                                                                                                                '
                //+ '					<a class="nav-link dropdown-toggle" href="#" id="profileDropdown" role="button"                                                                                                                                                           '
                //+ '         data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false">                                                                                                                                                                             '
                //+ '						<img class="wd-30 ht-30 rounded-circle" src="https://via.placeholder.com/30x30" alt="profile">                                                                                                                                        '
                //+ '					</a>                                                                                                                                                                                                                                      '
                //+ '					<div class="dropdown-menu p-0" aria-labelledby="profileDropdown">                                                                                                                                                                         '
                //+ '						<div class="d-flex flex-column align-items-center border-bottom px-5 py-3">                                                                                                                                                           '
                //+ '							<div class="mb-3">                                                                                                                                                                                                                '
                //+ '								<img class="wd-80 ht-80 rounded-circle" src="https://via.placeholder.com/80x80" alt="">                                                                                                                                       '
                //+ '							</div>                                                                                                                                                                                                                            '
                //+ '							<div class="text-center">                                                                                                                                                                                                         '
                //+ '								<p class="tx-16 fw-bolder">Amiah Burton</p>                                                                                                                                                                                   '
                //+ '								<p class="tx-12 text-muted">amiahburton@gmail.com</p>                                                                                                                                                                         '
                //+ '							</div>                                                                                                                                                                                                                            '
                //+ '						</div>                                                                                                                                                                                                                                '
                //+ '        <ul class="list-unstyled p-1">                                                                                                                                                                                                                      '
                //+ '          <li class="dropdown-item py-2">                                                                                                                                                                                                                   '
                //+ '            <a href="pages/general/profile.html" class="text-body ms-0">                                                                                                                                                                                    '
                //+ '              <i class="me-2 icon-md" data-feather="user"></i>                                                                                                                                                                                              '
                //+ '              <span>Profile</span>                                                                                                                                                                                                                          '
                //+ '            </a>                                                                                                                                                                                                                                            '
                //+ '          </li>                                                                                                                                                                                                                                             '
                //+ '          <li class="dropdown-item py-2">                                                                                                                                                                                                                   '
                //+ '            <a href="javascript:;" class="text-body ms-0">                                                                                                                                                                                                  '
                //+ '              <i class="me-2 icon-md" data-feather="edit"></i>                                                                                                                                                                                              '
                //+ '              <span>Edit Profile</span>                                                                                                                                                                                                                     '
                //+ '            </a>                                                                                                                                                                                                                                            '
                //+ '          </li>                                                                                                                                                                                                                                             '
                //+ '          <li class="dropdown-item py-2">                                                                                                                                                                                                                   '
                //+ '            <a href="javascript:;" class="text-body ms-0">                                                                                                                                                                                                  '
                //+ '              <i class="me-2 icon-md" data-feather="repeat"></i>                                                                                                                                                                                            '
                //+ '              <span>Switch User</span>                                                                                                                                                                                                                      '
                //+ '            </a>                                                                                                                                                                                                                                            '
                //+ '          </li>                                                                                                                                                                                                                                             '
                //+ '          <li class="dropdown-item py-2">                                                                                                                                                                                                                   '
                //+ '            <a href="javascript:;" class="text-body ms-0">                                                                                                                                                                                                  '
                //+ '              <i class="me-2 icon-md" data-feather="log-out"></i>                                                                                                                                                                                           '
                //+ '              <span>Log Out</span>                                                                                                                                                                                                                          '
                //+ '            </a>                                                                                                                                                                                                                                            '
                //+ '          </li>                                                                                                                                                                                                                                             '
                //+ '        </ul>                                                                                                                                                                                                                                               '
                //+ '					</div>                                                                                                                                                                                                                                    '
                //+ '				</li>                                                                                                                                                                                                                                         '
                + '			</ul>                                                                                                                                                                                                                                             '
                + '		</div>                                                                                                                                                                                                                                                '
                + '	</nav>                                                                                                                                                                                                                                                    '
                ;
        }                                                                                                                                                                                                                                                             


    }

}

customElements.define('manoir-user-menubar', Manoir.User.MainMenuBar);
customElements.define('manoir-head', Manoir.User.ManoirHeadControl);
customElements.define('manoir-user-header', Manoir.User.ManoirHeaderBar);

$(document).ready(() => {
    var host = document.location.hostname;
    if (host == null)
        return;
    if (host.indexOf(".dev.") > 0 || host == "192.168.2.184") {
        $("body").addClass("sidebar-dark");
        $(".dev-tag").show();
    }
});