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
        var AdminHome;
        (function (AdminHome) {
            class AdminHomePage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.view = "AdminHome-list";
                    self.scope.events = self;
                    self.init();
                }
                switchToView(newView) {
                    var sc = this.scope;
                    sc.view = newView;
                    sc.$applyAsync();
                    return false;
                }
                init() {
                    var sc = this.scope;
                    var self = this;
                    this.connection = new signalR.HubConnectionBuilder()
                        .withUrl("/hubs/1.0/admin")
                        .withAutomaticReconnect()
                        .build();
                    this.connection.start().catch(err => document.write(err));
                }
            }
            AdminHomePage.$inject = ["$scope"];
            AdminHome.AdminHomePage = AdminHomePage;
        })(AdminHome = Admin.AdminHome || (Admin.AdminHome = {}));
    })(Admin = HomeAutomation.Admin || (HomeAutomation.Admin = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('AdminHomeApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('AdminHomeController', HomeAutomation.Admin.AdminHome.AdminHomePage);
//# sourceMappingURL=index.js.map