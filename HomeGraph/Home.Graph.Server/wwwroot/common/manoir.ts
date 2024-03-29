﻿///<reference path="../scripts/typings/angularjs/angular.d.ts" />
///<reference path="../scripts/typings/angularjs/angular-sanitize.d.ts" />
///<reference path="../scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../scripts/typings/signalr/index.d.ts" />


declare var manoirDeviceApp: Manoir.IManoirDeviceApp;

namespace Manoir {

    interface GreetingsSave {
        content: string;
        dateMessage: string;
        date: number;
    }

    interface GreetingsResponse {
        items: Array<GreetingsResponseItem>;
        response: string;
    }
    interface GreetingsResponseItem {
        content: string;
        contentKind: number;
        contentUrl: string;
    }

    export class MainMenuBar extends HTMLElement {
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

    export class MenuButton extends HTMLElement {
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

    export class Header extends HTMLElement {

        appconnection: signalR.HubConnection;

        constructor() {
            super();

            var self = this;

            this.appconnection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/1.0/appanddevices")
                .withAutomaticReconnect()
                .build();

            this.appconnection.on("notifyUserChange", (changetype: string, user: any) => {
                self.notifyUserChange(changetype, user);
            });

            this.appconnection.start().catch(err => console.error(err));
        }

        private notifyUserChange(changeType: string, user: any): void {

            if (changeType != null && changeType.toLocaleLowerCase() == "presence") {
                this.refreshData();
            }

        }

        connectedCallback() {
            var greetings = "Bonjour <strong>tout le monde</strong> !";
            var dateMessage = "- chargement en cours -";
            var found = false;
            var self = this;
            if (localStorage != null) {
                var it = localStorage.getItem("manoir-global-greetings");
                if (it != null) {
                    var t: GreetingsSave = JSON.parse(it);
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
                .done(function (data: GreetingsResponse) {
                    console.log(data);

                    if (data.response == "OK") {
                        var greets: GreetingsResponseItem = null;
                        var datemsg: GreetingsResponseItem = null;
                        data.items.forEach(item => {
                            if (item.contentKind == 0) {
                                greets = item;
                            }
                            if (item.contentKind == 2) {
                                datemsg = item;
                            }
                        });
                        if (greets != null && localStorage != null) {
                            var save: GreetingsSave = {
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


    export interface IManoirDeviceApp {
        setApplication(url: string): void;
        saveDeviceId(deviceId: string): void;
        getDeviceId(): string;
    }
}

namespace Manoir.Common {
    export abstract class ManoirAppPage {
        sysconnection: signalR.HubConnection;

        // notifyUserChange

        constructor() {
            this.sysconnection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/1.0/system")
                .withAutomaticReconnect()
                .build();

            this.sysconnection.on("changeAppOnDevice", this.changeAppOnDevice);
            this.sysconnection.on("forceRefresh", this.forceRefresh);

            this.sysconnection.start().catch(err => console.error(err));


            

        }

     

        public checkLogin(autoRedirect: boolean = true): boolean {
            var deviceIdentifier = angular.fromJson(localStorage.getItem("deviceToken"));
            if (deviceIdentifier != null) {
                return true;
            } else {
                if (location.pathname != '/devicehome.html')
                    window.location.replace('/devicehome.html');

                return false;
            }
        }

        private static isCurrentDevice(deviceId: string): boolean {
            var deviceIdentifier = angular.fromJson(localStorage.getItem("deviceToken"));
            if (deviceIdentifier == null || deviceIdentifier.device == null)
                return false;
            return deviceIdentifier.device.id == deviceId;
        }

        private changeAppOnDevice(changeDeviceId: string, app: any): void {

            if (!ManoirAppPage.isCurrentDevice(changeDeviceId))
                return;

            if (app.url != null) {
                if (typeof manoirDeviceApp != "undefined" && manoirDeviceApp != null) {
                    manoirDeviceApp.setApplication(app.url);
                }

                document.location = app.url;
            }
        }

        private forceRefresh(changeDeviceId: string): void {

            if (!ManoirAppPage.isCurrentDevice(changeDeviceId))
                return;

            (document.location as any).reload(true);
        }
    }
}

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
})