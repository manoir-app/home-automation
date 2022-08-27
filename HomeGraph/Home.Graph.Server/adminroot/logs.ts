///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />


module HomeAutomation.Admin.Logs {

    export interface LogData {
        source: string;
        sourceId: string;
        imageUrl: string;
        message: string;
    }

    export interface ILogsPageScope extends ng.IScope {
        view: string;
        events: LogsPage;
        Logs: LogData[];
    }


    export class LogsPage {

        connection: signalR.HubConnection;
        scope: ILogsPageScope;
        static $inject = ["$scope"];

        constructor($scope: ILogsPageScope) {
            this.scope = $scope;
            var self = this;

            self.scope.view = "Logs-list";
            self.scope.events = self;
            self.scope.Logs = new Array<LogData>();
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

            setInterval(() => self.refresh(), 5000);
            self.refresh();
        }

        public refresh(): void {

            var sc = this.scope;
            var self = this;
            $.ajax({
                url: '/v1.0/system/mesh/local/logs',
                type: 'GET',
                dataType: "json",
                contentType: "application/json"
            })
                .done(function (data: LogData[]) {
                    sc.Logs = data;
                    sc.$applyAsync();
                })
                .fail(function () {
                });
        }
    }
}

var AuthApp = angular.module('LogsApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('LogsController', HomeAutomation.Admin.Logs.LogsPage);
