///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />

module HomeAutomation.Me.HomeAutoPage {

    export interface IHomeAutoPageScope extends ng.IScope {
        events: HomeAutoPage;
        view: string;
        zonesForSearch: Array<HomeAutoZone>;
        roomsForDevices: Array<HomeAutoRoomForDevices>;

        currentCategory: string;
        currentZone: string;

    }

    export interface HomeAutoZone {
        id: string;
        name: string;
        rooms: Array<HomeAutoRoom>;
    }

    export interface HomeAutoRoom {
        id: string;
        name: string;
        roomKind: number;
    }

    export interface HomeAutoRoomForDevices {
        id: string;
        name: string;
        roomKind: number;
        devices: Array<HomeAutoDevice>;

    }

    export interface DeviceInfo {
        id: string;
        deviceGivenName: string;
        devicePlatform: string;
        deviceKind: string;
        deviceRoles: string[];
        datas: DeviceData[];
    }

    export interface DeviceData {
        name: string;
        value: string;
    }

    export interface HomeAutoDevice extends DeviceInfo {
        mainRole: string;

        mainStatus: string;
        mainStatusInt: number;
    }

    export interface DeviceOperation {
        deviceName: string;
        role: string;
        elementName: string;
        value: string;
    }

    export interface Category {
        id: string;
        label: string;
    }

    export class HomeAutoPage {

        connection: signalR.HubConnection;
        scope: IHomeAutoPageScope;
        static $inject = ["$scope"];

        constructor($scope: IHomeAutoPageScope) {
            this.scope = $scope;
            var self = this;

            self.scope.events = self;
            self.scope.view = "devices-list";
            self.init();
        }

        public init(): void {
            var sc = this.scope;
            var self = this;



            $.ajax({
                url: '/v1.0/system/mesh/local/location',
                type: 'GET',
                dataType: "json",
                contentType: "application/json",
            })
                .done(function (data) {
                    sc.zonesForSearch = data.zones;

                    self.refreshHomeAutos();
                    setTimeout(() => self.refreshHomeAutos(), 10000);

                    sc.$applyAsync();
                });


        }

        public switchToView(newView: string): boolean {
            var sc = this.scope;

            sc.view = newView;

            sc.$applyAsync();
            return false;
        }

        public switchBackToMain() {
            var sc = this.scope;
            sc.currentHomeAuto = null;
            sc.currentHomeAutoImages = null;
            sc.view = "devices-list";
            sc.$applyAsync();
        }

        public refreshHomeAutos(): void {
            var sc = this.scope;
            var self = this;
            if (sc.currentCategory == null || sc.currentCategory == '') {

                $.ajax({
                    url: '/v1.0/devices/find?kind=homeautomation' ,
                    type: 'GET',
                    dataType: "json",
                    contentType: "application/json",
                })
                    .done(function (data : DeviceInfo[]) {
                        self.makeRoomDeviceTree(sc, data);
                        sc.$applyAsync();
                    });
            }
            else {

                $.ajax({
                    url: 'v1.0/devices/find?kind=homeautomation',
                    type: 'GET',
                    dataType: "json",
                    contentType: "application/json",
                })
                    .done(function (data) {
                        self.makeRoomDeviceTree(sc, data);
                        sc.$applyAsync();

                    });
            }
        }


        public makeRoomDeviceTree(sc: IHomeAutoPageScope, data: DeviceInfo[]) {
            var rooms = new Array<HomeAutoRoomForDevices>();
            var self = this;

            rooms.push({
                id: "",
                name: "unknwon",
                roomKind: 0,
                devices: new Array<HomeAutoDevice>()
            });

            data.forEach((item, idx) => {

                var inroom = self.findRoom(rooms, sc);
                var dev: HomeAutoDevice = {
                    id: item.id,
                    deviceGivenName: item.deviceGivenName,
                    mainRole: "unknown",
                    deviceKind: item.deviceKind,
                    devicePlatform: item.devicePlatform,
                    deviceRoles: item.deviceRoles,
                    datas: item.datas,
                    mainStatus: null,
                    mainStatusInt: null
                };
                dev.mainRole = self.identifyMainRole(dev, sc);
                if (dev.mainRole == "main:bridge")
                    return;

                self.fillMainStatus(dev, sc);
                inroom.devices.push(dev);
            });

            sc.roomsForDevices = rooms;
        }

        private findRoom(rooms: Array<HomeAutoRoomForDevices>, sc: IHomeAutoPageScope): HomeAutoRoomForDevices {
            return rooms[0];
        }

        private identifyMainRole(device: HomeAutoDevice, sc: IHomeAutoPageScope): string {
            if (device.deviceRoles != null) {
                for (var i = 0; i < device.deviceRoles.length; i++) {
                    if (device.deviceRoles[i].startsWith("main:"))
                        return device.deviceRoles[i];
                }
            }
            return "unknown";
        }

        private fillMainStatus(device: HomeAutoDevice, sc: IHomeAutoPageScope): string {
            if (device.deviceRoles != null) {
                for (var i = 0; i < device.deviceRoles.length; i++) {
                    if (device.deviceRoles[i] == ("switch")) {
                        for (var j = 0; j < device.datas.length; j++) {
                            if (device.mainRole == "main:shutters" && device.datas[j].name == "open") {
                                device.mainStatus = device.datas[j].value;
                                device.mainStatusInt = parseInt(device.mainStatus);
                                if (isNaN(device.mainStatusInt)) {
                                    device.mainStatusInt = null;
                                } else {
                                    if (device.mainStatusInt > 5) device.mainStatus = "on";
                                    else device.mainStatus = "off";
                                }
                            }
                            else if (device.datas[i].name == "on") {
                                device.mainStatus = device.datas[j].value;
                            }
                            else if (device.mainRole == "main:light" && device.datas[i].name == "brightness") {
                                device.mainStatusInt = parseInt(device.datas[j].value);
                            }
                        }
                    }
                }
            }
            return "unknown";
        }


        public isCurrentCategory(catId: string): boolean {
            var sc = this.scope;
            if (sc.currentCategory == null && catId == "") // cas particulier du "tous"
                return true;
            return sc.currentCategory == catId;
        }

        public selectCategory(catId: string): void {
            var sc = this.scope;
            sc.currentPage = 1;
            if (catId == '')
                sc.currentCategory = null;
            else
                sc.currentCategory = catId;
            sc.$applyAsync();

            this.refreshHomeAutos();
        }

        public isCurrentZone(zoneId: string): boolean {
            var sc = this.scope;
            if (sc.currentZone == null && zoneId == "") // cas particulier du "tous"
                return true;
            return sc.currentZone== zoneId;
        }

        public selectZone(zoneId: string): void {
            var sc = this.scope;
            sc.currentPage = 1;
            if (zoneId == '')
                sc.currentZone = null;
            else
                sc.currentZone = zoneId;
            sc.$applyAsync();

            this.refreshHomeAutos();
        }


        public swicthOff(dev: HomeAutoDevice): boolean {
            console.log("off pour " + dev.id);
            var ops = new Array<DeviceOperation>();
            ops.push({
                deviceName: dev.id,
                elementName: "on",
                role: "switch",
                value:"off"
            });

            this.sendHomeAutomationMessage(dev,ops)
            return false;
        }
        public swicthOn(dev: HomeAutoDevice): boolean {
            console.log("off pour " + dev.id);

            var ops = new Array<DeviceOperation>();
            ops.push({
                deviceName: dev.id,
                elementName: "on",
                role: "switch",
                value: "on"
            });
            this.sendHomeAutomationMessage(dev, ops)

            return false;
        }

        private sendHomeAutomationMessage(dev: DeviceInfo, operations:DeviceOperation[]): boolean {

            var sc = this.scope;
            var self = this;

            var data = {
                topic: dev.deviceKind + "." + dev.devicePlatform,
                operations: operations
            };

            $.ajax({
                url: '/v1.0/agents/all/send/' + data.topic,
                type: 'POST',
                dataType: "json",
                contentType: "application/json",
                data: JSON.stringify(data)
            })
                .done(function (data: DeviceInfo[]) {
                    self.refreshHomeAutos();
                });

            return false;
        }

    }
}

var AuthApp = angular.module('HomeAutoPageApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('HomeAutoPageController', HomeAutomation.Me.HomeAutoPage.HomeAutoPage);
