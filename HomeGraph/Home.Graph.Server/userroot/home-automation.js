///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />
var HomeAutomation;
(function (HomeAutomation) {
    var Me;
    (function (Me) {
        var HomeAutoPage;
        (function (HomeAutoPage_1) {
            class HomeAutoPage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.events = self;
                    self.scope.view = "devices-list";
                    self.init();
                }
                init() {
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
                switchToView(newView) {
                    var sc = this.scope;
                    sc.view = newView;
                    sc.$applyAsync();
                    return false;
                }
                switchBackToMain() {
                    var sc = this.scope;
                    sc.currentHomeAuto = null;
                    sc.currentHomeAutoImages = null;
                    sc.view = "devices-list";
                    sc.$applyAsync();
                }
                refreshHomeAutos() {
                    var sc = this.scope;
                    var self = this;
                    if (sc.currentCategory == null || sc.currentCategory == '') {
                        $.ajax({
                            url: '/v1.0/devices/find?kind=homeautomation',
                            type: 'GET',
                            dataType: "json",
                            contentType: "application/json",
                        })
                            .done(function (data) {
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
                makeRoomDeviceTree(sc, data) {
                    var rooms = new Array();
                    var self = this;
                    rooms.push({
                        id: "",
                        name: "unknwon",
                        roomKind: 0,
                        devices: new Array()
                    });
                    data.forEach((item, idx) => {
                        var inroom = self.findRoom(rooms, sc);
                        var dev = {
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
                findRoom(rooms, sc) {
                    return rooms[0];
                }
                identifyMainRole(device, sc) {
                    if (device.deviceRoles != null) {
                        for (var i = 0; i < device.deviceRoles.length; i++) {
                            if (device.deviceRoles[i].startsWith("main:"))
                                return device.deviceRoles[i];
                        }
                    }
                    return "unknown";
                }
                fillMainStatus(device, sc) {
                    if (device.deviceRoles != null) {
                        for (var i = 0; i < device.deviceRoles.length; i++) {
                            if (device.deviceRoles[i] == ("switch")) {
                                for (var j = 0; j < device.datas.length; j++) {
                                    if (device.mainRole == "main:shutters" && device.datas[j].name == "open") {
                                        device.mainStatus = device.datas[j].value;
                                        device.mainStatusInt = parseInt(device.mainStatus);
                                        if (isNaN(device.mainStatusInt)) {
                                            device.mainStatusInt = null;
                                        }
                                        else {
                                            if (device.mainStatusInt > 5)
                                                device.mainStatus = "on";
                                            else
                                                device.mainStatus = "off";
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
                isCurrentCategory(catId) {
                    var sc = this.scope;
                    if (sc.currentCategory == null && catId == "") // cas particulier du "tous"
                        return true;
                    return sc.currentCategory == catId;
                }
                selectCategory(catId) {
                    var sc = this.scope;
                    sc.currentPage = 1;
                    if (catId == '')
                        sc.currentCategory = null;
                    else
                        sc.currentCategory = catId;
                    sc.$applyAsync();
                    this.refreshHomeAutos();
                }
                isCurrentZone(zoneId) {
                    var sc = this.scope;
                    if (sc.currentZone == null && zoneId == "") // cas particulier du "tous"
                        return true;
                    return sc.currentZone == zoneId;
                }
                selectZone(zoneId) {
                    var sc = this.scope;
                    sc.currentPage = 1;
                    if (zoneId == '')
                        sc.currentZone = null;
                    else
                        sc.currentZone = zoneId;
                    sc.$applyAsync();
                    this.refreshHomeAutos();
                }
                swicthOff(dev) {
                    console.log("off pour " + dev.id);
                    var ops = new Array();
                    ops.push({
                        deviceName: dev.id,
                        elementName: "on",
                        role: "switch",
                        value: "off"
                    });
                    this.sendHomeAutomationMessage(dev, ops);
                    return false;
                }
                swicthOn(dev) {
                    console.log("off pour " + dev.id);
                    var ops = new Array();
                    ops.push({
                        deviceName: dev.id,
                        elementName: "on",
                        role: "switch",
                        value: "on"
                    });
                    this.sendHomeAutomationMessage(dev, ops);
                    return false;
                }
                sendHomeAutomationMessage(dev, operations) {
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
                        .done(function (data) {
                        self.refreshHomeAutos();
                    });
                    return false;
                }
            }
            HomeAutoPage.$inject = ["$scope"];
            HomeAutoPage_1.HomeAutoPage = HomeAutoPage;
        })(HomeAutoPage = Me.HomeAutoPage || (Me.HomeAutoPage = {}));
    })(Me = HomeAutomation.Me || (HomeAutomation.Me = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('HomeAutoPageApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('HomeAutoPageController', HomeAutomation.Me.HomeAutoPage.HomeAutoPage);
//# sourceMappingURL=home-automation.js.map