///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />


module HomeAutomation.Admin.Routines {

    export interface RoutineData {
        imageUrl: string;
        label: string;
        details: string;
    }

    export interface IRoutinesPageScope extends ng.IScope {
        view: string;
        events: RoutinesPage;
        Routines: RoutineData[];
    }


    export class RoutinesPage {

        connection: signalR.HubConnection;
        scope: IRoutinesPageScope;
        static $inject = ["$scope"];

        constructor($scope: IRoutinesPageScope) {
            this.scope = $scope;
            var self = this;

            self.scope.view = "Routines-list";
            self.scope.events = self;
            self.scope.Routines = new Array<RoutineData>();
            self.scope.currentConfig = null;
            self.scope.currentDevice = null;
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
            self.refresh();
        }

        public refresh(): void {

            var sc = this.scope;
            var self = this;
            $.ajax({
                url: '/v1.0/system/mesh/local/triggers',
                type: 'GET',
                dataType: "json",
                contentType: "application/json"
            })
                .done(function (data) {

                    var items = new Array<RoutineData>();

                    for (var i = 0; i < data.length; i++) {
                        var tr: RoutineData = {
                            imageUrl: "https://www.manoir.app/resources/trigger_" + data[i].kind + ".png",
                            label: data[i].label,
                            details: ''
                        };
                        items.push(tr);
                    }

                    sc.Routines = items;
                    sc.$applyAsync();
                })
                .fail(function () {
                });
        }
    }
}

var AuthApp = angular.module('RoutinesApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('RoutinesController', HomeAutomation.Admin.Routines.RoutinesPage);
