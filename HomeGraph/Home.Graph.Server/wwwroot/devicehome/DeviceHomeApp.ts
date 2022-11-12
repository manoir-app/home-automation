///<reference path="../scripts/typings/angularjs/angular.d.ts" />
///<reference path="../common/manoir.ts" />
///<reference path="../scripts/typings/angularjs/angular-sanitize.d.ts" />
///<reference path="../scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../scripts/typings/signalr/index.d.ts" />

module Manoir.DeviceHomeApp {

    interface IDefaultPageScope extends ng.IScope {
        Loading: boolean;
        isLoggedIn: boolean;
    }

    export class DefaultPage extends Manoir.Common.ManoirAppPage {
        connection: signalR.HubConnection;

        scope: IDefaultPageScope;
        $timeout: ng.ITimeoutService;
        http: any;
        constructor($scope: IDefaultPageScope, $http: any, $timeout: ng.ITimeoutService) {
            super();
            this.scope = $scope;
            this.http = $http;
            this.$timeout = $timeout;
            this.scope.Loading = true;
            let self = this;
            this.init();
        }

        private init() {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/1.0/appanddevices")
                .withAutomaticReconnect()
                .build();

            this.connection.on("notifyMeshChange", this.onMeshChange);

            this.connection.start().catch(err => console.error(err));

            this.scope.isLoggedIn = super.checkLogin(true);
        }

        private onMeshChange(changeType: string, mesh: any) : void {
            console.log(mesh);
        }

       

        public RefreshData(): void {
        }

    }
}

var theApp = angular.module('DeviceHomeApp', []);

theApp.controller('DefaultPage', Manoir.DeviceHomeApp.DefaultPage);
theApp.filter('trustAsHtml', function ($sce) {
    return function (html) {
        return $sce.trustAsHtml(html);
    }
});

