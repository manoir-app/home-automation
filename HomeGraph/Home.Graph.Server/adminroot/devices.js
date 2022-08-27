///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />
var HomeAutomation;
(function (HomeAutomation) {
    var Admin;
    (function (Admin) {
        var Devices;
        (function (Devices) {
            class DevicesPage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.view = "devices-list";
                    self.scope.events = self;
                    self.scope.devices = new Array();
                    self.scope.currentConfig = null;
                    self.scope.currentDevice = null;
                    self.init();
                }
                switchToView(newView) {
                    var sc = this.scope;
                    sc.view = newView;
                    sc.$applyAsync();
                    return false;
                }
                init() {
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
                        .done(function (data) {
                        sc.agents = new Array();
                        for (var a of data) {
                            var d = {
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
            DevicesPage.$inject = ["$scope"];
            Devices.DevicesPage = DevicesPage;
        })(Devices = Admin.Devices || (Admin.Devices = {}));
    })(Admin = HomeAutomation.Admin || (HomeAutomation.Admin = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('DevicesApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('DevicesController', HomeAutomation.Admin.Devices.DevicesPage);
//# sourceMappingURL=devices.js.map