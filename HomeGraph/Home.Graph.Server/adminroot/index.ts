///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />


module HomeAutomation.Admin.AdminHome {

    export interface IAdminHomePageScope extends ng.IScope {
        view: string;
        events: AdminHomePage;
    }

    export class AdminHomePage {

        connection: signalR.HubConnection;
        scope: IAdminHomePageScope;
        static $inject = ["$scope"];

        constructor($scope: IAdminHomePageScope) {
            this.scope = $scope;
            var self = this;

            self.scope.view = "AdminHome-list";
            self.scope.events = self;
            self.init();
        }

        public switchToView(newView: string): boolean {
            var sc = this.scope;

            sc.view = newView;

            sc.$applyAsync();
            return false;
        }

        public init(): void {
            var sc = this.scope;
            var self = this;

            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/1.0/admin")
                .withAutomaticReconnect()
                .build();

            this.connection.start().catch(err => document.write(err));
        }
    }
}

var AuthApp = angular.module('AdminHomeApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('AdminHomeController', HomeAutomation.Admin.AdminHome.AdminHomePage);
