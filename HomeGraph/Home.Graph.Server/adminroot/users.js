///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />
var HomeAutomation;
(function (HomeAutomation) {
    var Admin;
    (function (Admin) {
        var Users;
        (function (Users) {
            class UsersPage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.events = self;
                    self.scope.MainUsers = new Array();
                    self.scope.OtherUsers = new Array();
                    self.init();
                }
                init() {
                    var sc = this.scope;
                    var self = this;
                    //this.connection = new signalR.HubConnectionBuilder()
                    //    .withUrl("/hubs/1.0/admin")
                    //    .withAutomaticReconnect()
                    //    .build();
                    //this.connection.on("UserStatusChanged", (User: string, status: UserData) => {
                    //    for (var ag of sc.Users) {
                    //        if (ag.id == User) {
                    //            sc.$applyAsync();
                    //            return;
                    //        }
                    //    }
                    //    var ag: UserData = {
                    //        id: User,
                    //        Name:status.Name
                    //    };
                    //    sc.Users.push(ag);
                    //    sc.$applyAsync();
                    //});
                    $.ajax({
                        url: '/v1.0/users/all',
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json"
                    })
                        .done(function (data) {
                        sc.Users = new Array();
                        for (var a of data) {
                            var ag = {
                                id: a.id,
                                name: a.name,
                                isGuest: a.isGuest,
                                isMain: a.isMain,
                                avatar: a.avatar,
                                commonName: a.commonName,
                                firstName: a.firstName
                            };
                            if (a.isMain)
                                sc.MainUsers.push(ag);
                            else if (!a.isGuest)
                                sc.OtherUsers.push(ag);
                        }
                        sc.$applyAsync();
                    })
                        .fail(function () {
                    });
                    //this.connection.start().catch(err => document.write(err));
                }
            }
            UsersPage.$inject = ["$scope"];
            Users.UsersPage = UsersPage;
        })(Users = Admin.Users || (Admin.Users = {}));
    })(Admin = HomeAutomation.Admin || (HomeAutomation.Admin = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('UserApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('UsersController', HomeAutomation.Admin.Users.UsersPage);
//# sourceMappingURL=users.js.map