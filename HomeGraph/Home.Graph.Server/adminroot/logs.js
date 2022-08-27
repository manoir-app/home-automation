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
        var Logs;
        (function (Logs) {
            class LogsPage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.view = "Logs-list";
                    self.scope.events = self;
                    self.scope.Logs = new Array();
                    self.scope.currentConfig = null;
                    self.scope.currentDevice = null;
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
                    setInterval(() => self.refresh(), 5000);
                    self.refresh();
                }
                refresh() {
                    var sc = this.scope;
                    var self = this;
                    $.ajax({
                        url: '/v1.0/system/mesh/local/logs',
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json"
                    })
                        .done(function (data) {
                        sc.Logs = data;
                        sc.$applyAsync();
                    })
                        .fail(function () {
                    });
                }
            }
            LogsPage.$inject = ["$scope"];
            Logs.LogsPage = LogsPage;
        })(Logs = Admin.Logs || (Admin.Logs = {}));
    })(Admin = HomeAutomation.Admin || (HomeAutomation.Admin = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('LogsApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('LogsController', HomeAutomation.Admin.Logs.LogsPage);
//# sourceMappingURL=logs.js.map