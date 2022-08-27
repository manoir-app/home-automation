///<reference path="../../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />
var HomeAutomation;
(function (HomeAutomation) {
    var Admin;
    (function (Admin) {
        var Units;
        (function (Units) {
            class UnitsPage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.units = new Array();
                    self.scope.view = "units-list";
                    self.scope.events = self;
                    self.init();
                }
                init() {
                    var sc = this.scope;
                    var self = this;
                    this.connection = new signalR.HubConnectionBuilder()
                        .withUrl("/hubs/1.0/admin")
                        .withAutomaticReconnect()
                        .build();
                    $.ajax({
                        url: '/v1.0/settings/units/',
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json"
                    })
                        .done(function (data) {
                        sc.units = data;
                        sc.$applyAsync();
                    })
                        .fail(function () {
                    });
                    this.connection.start().catch(err => document.write(err));
                }
            }
            UnitsPage.$inject = ["$scope"];
            Units.UnitsPage = UnitsPage;
        })(Units = Admin.Units || (Admin.Units = {}));
    })(Admin = HomeAutomation.Admin || (HomeAutomation.Admin = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('UnitApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('UnitsController', HomeAutomation.Admin.Units.UnitsPage);
//# sourceMappingURL=units.js.map