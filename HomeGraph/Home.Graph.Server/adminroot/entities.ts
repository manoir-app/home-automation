///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />


module HomeAutomation.Admin.Entites {

   
    export interface IEntitesPageScope extends ng.IScope {
        view: string;
        events: EntitesPage;
    }


    export class EntitesPage {

        connection: signalR.HubConnection;
        scope: IEntitesPageScope;
        static $inject = ["$scope"];

        constructor($scope: IEntitesPageScope) {
            this.scope = $scope;
            var self = this;

            self.scope.view = "systemEntites-list";
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

           
            //$.ajax({
            //    url: '/v1.0/security/Entites/self/foradmin',
            //    type: 'GET',
            //    dataType: "json",
            //    contentType: "application/json"
            //})
            //    .done(function (data) {
            //        sc.Entites = new Array<Token>();
            //        for (var a of data) {
            //            var ag: Token = {
            //                user : a.user,
            //                tokenType: a.tokenType
            //            };
            //            if (a.user.toLocaleLowerCase() == "system")
            //                sc.SystemEntites.push(ag);

            //        }
            //        sc.$applyAsync();
            //    })
            //    .fail(function () {
            //    });

        }
    }
}

var AuthApp = angular.module('EntitesApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('EntitesController', HomeAutomation.Admin.Entites.EntitesPage);
