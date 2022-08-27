///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />

module HomeAutomation.Me.StockPage {

    export interface IStockPageScope extends ng.IScope {
        events: StockPage;
        view: string; 
    }


    export class StockPage {

        connection: signalR.HubConnection;
        scope: IStockPageScope;
        static $inject = ["$scope"];

        constructor($scope: IStockPageScope) {
            this.scope = $scope;
            var self = this;

            self.scope.events = self;
            self.scope.view = "Stocks-list";
            self.init();
        }

        public init(): void {
            var sc = this.scope;
            var self = this;



        }

        public switchToView(newView: string): boolean {
            var sc = this.scope;

            sc.view = newView;

            sc.$applyAsync();
            return false;
        }

    }
}

var AuthApp = angular.module('StockPageApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('StockPageController', HomeAutomation.Me.StockPage.StockPage);
