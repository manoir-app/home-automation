///<reference path="../../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />


module HomeAutomation.Admin.Units {

    export interface IUnitsPageScope extends ng.IScope {
        view: string;
        events: UnitsPage;

        units: Unit[];

        currentUnit: string;
        currentConfig: string;
    }

  
    export interface Unit {
        id: string;
        label: string;
        symbol: string;
        metaType:number,
    }


    export class UnitsPage {

        connection: signalR.HubConnection;
        scope: IUnitsPageScope;
        static $inject = ["$scope"];

        constructor($scope: IUnitsPageScope) {
            this.scope = $scope;
            var self = this;
            self.scope.units = new Array<Unit>();
            self.scope.view = "units-list";
            self.scope.events = self;
            self.init();
        }



        public init(): void {
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
                .done(function (data: Unit[]) {
                    sc.units = data;
                    sc.$applyAsync();
                })
                .fail(function () {
                });

            this.connection.start().catch(err => document.write(err));
        }
    }
}

var AuthApp = angular.module('UnitApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('UnitsController', HomeAutomation.Admin.Units.UnitsPage);
