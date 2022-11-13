///<reference path="../scripts/typings/angularjs/angular.d.ts" />
///<reference path="../scripts/typings/angularjs/angular-sanitize.d.ts" />
///<reference path="../scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../scripts/typings/signalr/index.d.ts" />
var Manoir;
(function (Manoir) {
    class MainMenuBar extends HTMLElement {
        connectedCallback() {
            var self = this;
            this.innerHTML = '<section id="manoirMainMenu" class="manoir-main-menu-content"><button class="manoir-main-menu-close"><img src="/assets/imgs/streamlinehq-close-interface-essential.svg" /></button><ul></ul></section>';
            $("#manoirMainMenu .manoir-main-menu-close").click(function (e) {
                e.preventDefault();
                $("body").removeClass("menu-opened");
                return false;
            });
            setTimeout(function () { self.refreshData(); }, 100);
        }
        refreshData() {
            var nav = $("#manoirMainMenu ul");
            nav.empty();
            nav.append($("<li class='app'><a href='/devicehome/'><img class='icon' src='/assets/imgs/streamlinehq-bird-house-pets-animals.svg' /><span class='title'>Accueil</span><span class='desc'>/devicehome/</span></li>"));
            nav.append($("<li class='separator'>Home Automation</li>"));
            nav.append($("<li class='app'><a href='/app/homautomation'><img class='icon' src='/assets/imgs/streamlinehq-smart-light-connect-lamps-lights-fire.svg' /><span class='title'>Domotique</span><span class='desc'>/app/homeautomation/</span></li>"));
            nav.append($("<li class='app'><a href='/app/security/'><img class='icon' src='/assets/imgs/streamlinehq-lock-1-interface-essential.svg' /><span class='title'>Présence</span><span class='desc'>/app/security/</span></li>"));
            nav.append($("<li class='separator'>Outils</li>"));
            nav.append($("<li class='app'><a href='/app/welcomescreen/'><img class='icon' src='/assets/imgs/streamlinehq-layout-dashboard-interface-essential.svg' /><span class='title'>Welcome</span><span class='desc'>/app/welcomescreen/</span></li>"));
        }
    }
    Manoir.MainMenuBar = MainMenuBar;
    class MenuButton extends HTMLElement {
        connectedCallback() {
            var self = this;
            $("body").addClass("with-menu");
            this.innerHTML = `<button id="manoirMainButton" class="manoir-main-menu"></button>`;
            $("#manoirMainButton").click(function (e) {
                e.preventDefault();
                $("body").toggleClass("menu-opened");
                return false;
            });
        }
    }
    Manoir.MenuButton = MenuButton;
    class Header extends HTMLElement {
        connectedCallback() {
            var greetings = "Bonjour <strong>tout le monde</strong> !";
            var dateMessage = "- chargement en cours -";
            var found = false;
            var self = this;
            if (localStorage != null) {
                var it = localStorage.getItem("manoir-global-greetings");
                if (it != null) {
                    var t = JSON.parse(it);
                    if (t != null) {
                        if (t.date != null) {
                            var now = new Date();
                            var diff = now.getTime() - t.date;
                            var min = diff / 60;
                            if (min > 60) {
                                setTimeout(function () { self.refreshData(); }, 100);
                            }
                        }
                        if (t.content != null) {
                            found = true;
                            greetings = t.content;
                        }
                        if (t.dateMessage != null)
                            dateMessage = t.dateMessage;
                    }
                }
            }
            this.innerHTML = `<header class='manoir-header'><h1 id="manoirGreetings">${greetings}</h1><p class="date">${dateMessage}</p></header>`;
            if (!found)
                setTimeout(function () { self.refreshData(); }, 100);
            setInterval(function () { self.refreshData(); }, 60000);
        }
        refreshData() {
            var ctl = this;
            var loc = document.location.hostname;
            if (loc == "localhost")
                return;
            $.ajax({
                url: '/v1.0/system/mesh/local/interactions/greetings/general/?convertTo=html',
                type: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
            })
                .done(function (data) {
                console.log(data);
                if (data.response == "OK") {
                    var greets = null;
                    var datemsg = null;
                    data.items.forEach(item => {
                        if (item.contentKind == 0) {
                            greets = item;
                        }
                        if (item.contentKind == 2) {
                            datemsg = item;
                        }
                    });
                    if (greets != null && localStorage != null) {
                        var save = {
                            date: new Date().getTime(),
                            dateMessage: datemsg == null ? null : datemsg.content,
                            content: greets.content
                        };
                        localStorage.setItem("manoir-global-greetings", JSON.stringify(save));
                        ctl.innerHTML = `<header class='manoir-header'><h1 id="manoirGreetings">${greets.content}</h1><p class="date">${datemsg == null ? "" : datemsg.content}</p></header>`;
                    }
                }
            })
                .fail(function () {
            });
        }
    }
    Manoir.Header = Header;
})(Manoir || (Manoir = {}));
(function (Manoir) {
    var Common;
    (function (Common) {
        class ManoirAppPage {
            constructor() {
                this.sysconnection = new signalR.HubConnectionBuilder()
                    .withUrl("/hubs/1.0/system")
                    .withAutomaticReconnect()
                    .build();
                this.sysconnection.on("changeAppOnDevice", this.changeAppOnDevice);
                this.sysconnection.start().catch(err => console.error(err));
            }
            checkLogin(autoRedirect = true) {
                var deviceIdentifier = angular.fromJson(localStorage.getItem("deviceToken"));
                if (deviceIdentifier != null) {
                    return true;
                }
                else {
                    if (location.pathname != '/devicehome.html')
                        window.location.replace('/devicehome.html');
                    return false;
                }
            }
            changeAppOnDevice(changeType, app) {
                if (app.url != null) {
                    if (typeof manoirDeviceApp != "undefined" && manoirDeviceApp != null) {
                        manoirDeviceApp.setApplication(app.url);
                    }
                    document.location = app.url;
                }
            }
        }
        Common.ManoirAppPage = ManoirAppPage;
    })(Common = Manoir.Common || (Manoir.Common = {}));
})(Manoir || (Manoir = {}));
customElements.define('manoir-header', Manoir.Header);
customElements.define('manoir-menu-button', Manoir.MenuButton);
customElements.define('manoir-menu-bar', Manoir.MainMenuBar);
$(document).ready(() => {
    $("body").keydown(e => {
        switch (e.keyCode) {
            case 27:
                if ($("body").hasClass("menu-opened")) {
                    $("body").removeClass("menu-opened");
                    e.preventDefault();
                    return false;
                }
        }
        return true;
    });
});
//# sourceMappingURL=manoir.js.map