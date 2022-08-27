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
        var Locations;
        (function (Locations) {
            class LocationsPage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.view = "locations-list";
                    self.scope.events = self;
                    self.scope.CurrentLocation = null;
                    self.scope.Locations = new Array();
                    self.scope.CurrentRoom = null;
                    self.scope.RoomInModal = null;
                    self.init();
                }
                switchToView(newView) {
                    var sc = this.scope;
                    sc.view = newView;
                    sc.$applyAsync();
                    return false;
                }
                switchToNew() {
                    var sc = this.scope;
                    var newLoc = {
                        id: null, name: "Test", kind: 0,
                        coordinates: {
                            latitude: 50,
                            longitude: 5
                        }
                    };
                    sc.CurrentLocation = newLoc;
                    sc.view = "locations-edit";
                    sc.$applyAsync();
                    return false;
                }
                switchToEdit(loc) {
                    var sc = this.scope;
                    $.ajax({
                        url: '/v1.0/locations/' + loc.Id,
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json"
                    })
                        .done(function (data) {
                        sc.CurrentLocation = data;
                        sc.view = "locations-edit";
                        sc.$applyAsync();
                    });
                    return false;
                }
                switchBackToMain() {
                    var sc = this.scope;
                    sc.CurrentRoom = null;
                    sc.CurrentLocation = null;
                    sc.view = "locations-list";
                    sc.$applyAsync();
                }
                setRoomForModal(roomToShow) {
                    var sc = this.scope;
                    sc.RoomInModal = {
                        room: roomToShow,
                        isLoading: true,
                        storageUnits: new Array()
                    };
                    this.loadRoomDetails(sc.RoomInModal);
                    sc.$applyAsync();
                }
                switchToRoomEdit(roomToEdit) {
                    var sc = this.scope;
                    sc.view = "room-edit";
                    sc.CurrentRoom = {
                        room: roomToEdit,
                        isLoading: true,
                        storageUnits: new Array()
                    };
                    this.loadRoomDetails(sc.CurrentRoom);
                    sc.$applyAsync();
                    return false;
                }
                loadRoomDetails(room) {
                    var sc = this.scope;
                    $.ajax({
                        url: '/v1.0/storage/storageunits?roomId=' + room.room.id,
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json"
                    })
                        .done(function (data) {
                        room.storageUnits = data;
                        room.isLoading = false;
                        sc.$applyAsync();
                    });
                }
                saveRoom() {
                }
                saveLocation() {
                    var sc = this.scope;
                    var self = this;
                    $.ajax({
                        url: '/v1.0/locations',
                        type: 'POST',
                        dataType: "json",
                        contentType: "application/json",
                        data: JSON.stringify(sc.CurrentLocation),
                        cache: false
                    })
                        .done(function (data) {
                        sc.CurrentLocation = null;
                        self.init();
                    });
                    return false;
                }
                init() {
                    var sc = this.scope;
                    var self = this;
                    $.ajax({
                        url: '/v1.0/locations',
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json",
                        cache: false
                    })
                        .done(function (data) {
                        sc.Locations = new Array();
                        for (var a of data) {
                            var ag = {
                                Id: a.id,
                                Name: a.name,
                                KindLabel: "",
                                Coordinates: "",
                                ZonesInfo: "",
                                LocationObject: a
                            };
                            switch (a.kind) {
                                case 0:
                                    ag.KindLabel = "Home";
                                    break;
                                case 1:
                                    ag.KindLabel = "Work";
                                    break;
                                case 2:
                                    ag.KindLabel = "Family";
                                    break;
                                case 3:
                                    ag.KindLabel = "Friends";
                                    break;
                            }
                            if (a.coordinates != null) {
                                ag.Coordinates = a.coordinates.latitude.toString() + "," + a.coordinates.longitude.toString();
                            }
                            sc.Locations.push(ag);
                        }
                        sc.view = "locations-list";
                        sc.$applyAsync();
                    })
                        .fail(function () {
                    });
                }
            }
            LocationsPage.$inject = ["$scope"];
            Locations.LocationsPage = LocationsPage;
        })(Locations = Admin.Locations || (Admin.Locations = {}));
    })(Admin = HomeAutomation.Admin || (HomeAutomation.Admin = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('LocationApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('LocationsController', HomeAutomation.Admin.Locations.LocationsPage);
angular.module('LocationApp').directive('convertToNumber', function () {
    return {
        require: 'ngModel',
        link: function (scope, element, attrs, ngModel) {
            ngModel.$parsers.push(function (val) {
                return parseInt(val, 10);
            });
            ngModel.$formatters.push(function (val) {
                return '' + val;
            });
        }
    };
});
angular.module('LocationApp').directive('convertToFloat', function () {
    return {
        require: 'ngModel',
        link: function (scope, element, attrs, ngModel) {
            ngModel.$parsers.push(function (val) {
                return parseFloat(val);
            });
            ngModel.$formatters.push(function (val) {
                return '' + val;
            });
        }
    };
});
//# sourceMappingURL=locations.js.map