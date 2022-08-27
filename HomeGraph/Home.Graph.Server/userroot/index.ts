///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />


module HomeAutomation.Me.HomePage {

    export interface IHomePageScope extends ng.IScope {
        events: HomePage;
        presentUsers: Array<UserPresent>;
    }

    export interface UserPresent {
        name: string;
        firstName: string;
        commonName: string;
        urlAvatarSmall: string;
    }

    export class HomePage {

        connection: signalR.HubConnection;
        scope: IHomePageScope ;
        static $inject = ["$scope"];

        constructor($scope: IHomePageScope ) {
            this.scope = $scope;
            var self = this;

            self.scope.events = self;
            self.scope.presentUsers = new Array<UserPresent>();
            self.init();
        }

        public init(): void {
            var sc = this.scope;
            var self = this;

            $.ajax({
                url: '/v1.0/users/presence/mesh/local/all',
                type: 'GET',
                dataType: "json",
                contentType: "application/json"
            })
                .done(function (data) {
                    sc.presentUsers = new Array<UserPresent>();
                    for (var i = 0; i < data.length; i++) {
                        sc.presentUsers.push({
                            name : data[i].name,
                            firstName: data[i].firstName,
                            commonName: data[i].commonName,
                            urlAvatarSmall: data[i].avatar != null ? data[i].avatar.urlSquareSmall: null
                        });
                    }
                    sc.$applyAsync();
                })
                .fail(function () {
                });

        }
    }
}

var AuthApp = angular.module('HomePageApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('HomePageController', HomeAutomation.Me.HomePage.HomePage);
