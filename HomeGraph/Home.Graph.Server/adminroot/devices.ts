///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />


module HomeAutomation.Admin.Devices {

    export interface DeviceStatus {
        id: string;
        deviceName: string;
        deviceKind: string;
        deviceRoles: string[];
        mainStatusInfo: string;
    }


    export interface Device {
        id: string;
        deviceGivenName: string;
        deviceKind: string;
        deviceRoles: string[];
        mainStatusInfo: string;
    }

    export interface IDevicesPageScope extends ng.IScope {
        view: string;
        events: DevicesPage;
        devices: DeviceStatus[];

        currentDevice: string;
    }


    export class DevicesPage {

        connection: signalR.HubConnection;
        scope: IDevicesPageScope;
        static $inject = ["$scope"];

        constructor($scope: IDevicesPageScope) {
            this.scope = $scope;
            var self = this;

            self.scope.view = "devices-list";
            self.scope.events = self;
            self.scope.devices = new Array<DeviceStatus>();
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

            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/1.0/admin")
                .withAutomaticReconnect()
                .build();


            $.ajax({
                url: '/v1.0/devices/find',
                type: 'GET',
                dataType: "json",
                contentType: "application/json"
            })
                .done(function (data: Device[]) {
                    sc.agents = new Array<DeviceStatus>();
                    for (var a of data) {
                        var d: DeviceStatus = {
                            id: a.id,
                            deviceName: a.deviceGivenName,
                            deviceKind: a.deviceKind,
                            deviceRoles: a.deviceRoles,
                            mainStatusInfo: a.mainStatusInfo,
                        };
                        sc.devices.push(d);
                    }
                    sc.$applyAsync();
                })
                .fail(function () {
                });

            this.connection.start().catch(err => document.write(err));
        }
    }
}

var AuthApp = angular.module('DevicesApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('DevicesController', HomeAutomation.Admin.Devices.DevicesPage);
