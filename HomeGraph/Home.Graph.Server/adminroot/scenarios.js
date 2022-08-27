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
        var Scenarios;
        (function (Scenarios) {
            class ScenariosPage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.view = "systemScenarios-list";
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
                    //$.ajax({
                    //    url: '/v1.0/security/Scenarios/self/foradmin',
                    //    type: 'GET',
                    //    dataType: "json",
                    //    contentType: "application/json"
                    //})
                    //    .done(function (data) {
                    //        sc.Scenarios = new Array<Token>();
                    //        for (var a of data) {
                    //            var ag: Token = {
                    //                user : a.user,
                    //                tokenType: a.tokenType
                    //            };
                    //            if (a.user.toLocaleLowerCase() == "system")
                    //                sc.SystemScenarios.push(ag);
                    //        }
                    //        sc.$applyAsync();
                    //    })
                    //    .fail(function () {
                    //    });
                }
            }
            ScenariosPage.$inject = ["$scope"];
            Scenarios.ScenariosPage = ScenariosPage;
        })(Scenarios = Admin.Scenarios || (Admin.Scenarios = {}));
    })(Admin = HomeAutomation.Admin || (HomeAutomation.Admin = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('ScenariosApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('ScenariosController', HomeAutomation.Admin.Scenarios.ScenariosPage);
//# sourceMappingURL=scenarios.js.map