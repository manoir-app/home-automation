///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />
var HomeAutomation;
(function (HomeAutomation) {
    var Me;
    (function (Me) {
        var StockPage;
        (function (StockPage_1) {
            class StockPage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.events = self;
                    self.scope.view = "Stocks-list";
                    self.init();
                }
                init() {
                    var sc = this.scope;
                    var self = this;
                }
                switchToView(newView) {
                    var sc = this.scope;
                    sc.view = newView;
                    sc.$applyAsync();
                    return false;
                }
            }
            StockPage.$inject = ["$scope"];
            StockPage_1.StockPage = StockPage;
        })(StockPage = Me.StockPage || (Me.StockPage = {}));
    })(Me = HomeAutomation.Me || (HomeAutomation.Me = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('StockPageApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('StockPageController', HomeAutomation.Me.StockPage.StockPage);
//# sourceMappingURL=stocks.js.map