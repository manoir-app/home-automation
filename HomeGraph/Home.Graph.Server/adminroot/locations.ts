///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />


module HomeAutomation.Admin.Locations {


    export interface Location {
        Id: string;
        KindLabel: string;
        Coordinates: string;
        Name: string;
        ZonesInfo: string;
        LocationObject: any;
    }


    export interface Room {
        id: string;

    }

    export interface RoomInEditMode {
        room: Room;
        isLoading: boolean;
        storageUnits: StorageUnit[];
    }

    export interface StorageUnit {
        id: string;
        label: string;
    }


    export interface ILocationsPageScope extends ng.IScope {
        view: string;
        events: LocationsPage;
        Locations: Location[];
        CurrentLocation: any;
        CurrentRoom: RoomInEditMode;
        RoomInModal: RoomInEditMode;
    }


    export class LocationsPage {

        connection: signalR.HubConnection;
        scope: ILocationsPageScope;
        static $inject = ["$scope"];

        constructor($scope: ILocationsPageScope) {
            this.scope = $scope;
            var self = this;

            self.scope.view = "locations-list";
            self.scope.events = self;
            self.scope.CurrentLocation = null;
            self.scope.Locations = new Array<Location>();
            self.scope.CurrentRoom = null;
            self.scope.RoomInModal = null;
            self.init();
        }



        public switchToView(newView: string): boolean {
            var sc = this.scope;

            sc.view = newView;

            sc.$applyAsync();
            return false;
        }

        public switchToNew(): boolean {
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

        public switchToEdit(loc: Location): boolean {
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

        public switchBackToMain() {
            var sc = this.scope;

            sc.CurrentRoom = null;
            sc.CurrentLocation = null;
            sc.view = "locations-list";
            sc.$applyAsync();
        }

        public setRoomForModal(roomToShow: Room) {
            var sc = this.scope;
            sc.RoomInModal = {
                room: roomToShow,
                isLoading: true,
                storageUnits: new Array<StorageUnit>()
            };
            this.loadRoomDetails(sc.RoomInModal);
            sc.$applyAsync();
        }

        public switchToRoomEdit(roomToEdit: Room) {
            var sc = this.scope;
            sc.view = "room-edit";
            sc.CurrentRoom = {
                room: roomToEdit,
                isLoading: true,
                storageUnits: new Array<StorageUnit>()

            };
            this.loadRoomDetails(sc.CurrentRoom);
            sc.$applyAsync();

            
            return false;
        }

        private loadRoomDetails(room: RoomInEditMode) {
            var sc = this.scope;

            $.ajax({
                url: '/v1.0/storage/storageunits?roomId=' + room.room.id,
                type: 'GET',
                dataType: "json",
                contentType: "application/json"
            })
                .done(function (data : Array<StorageUnit>) {
                    room.storageUnits = data;
                    room.isLoading = false;
                    sc.$applyAsync();
                });
        }
        public saveRoom() {

        }

        public saveLocation(): boolean {
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

        public init(): void {
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
                    sc.Locations = new Array<Location>();
                    for (var a of data) {
                        var ag: Location = {
                            Id: a.id,
                            Name: a.name,
                            KindLabel: "",
                            Coordinates: "",
                            ZonesInfo: "",
                            LocationObject: a
                        };

                        switch (a.kind) {
                            case 0: ag.KindLabel = "Home"; break;
                            case 1: ag.KindLabel = "Work"; break;
                            case 2: ag.KindLabel = "Family"; break;
                            case 3: ag.KindLabel = "Friends"; break;
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
}

var AuthApp = angular.module('LocationApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('LocationsController', HomeAutomation.Admin.Locations.LocationsPage);


angular.module('LocationApp').directive('convertToNumber', function () {
    return {
        require: 'ngModel',
        link: function (scope, element, attrs, ngModel) {
            (<any>ngModel).$parsers.push(function (val) {
                return parseInt(val, 10);
            });
            (<any>ngModel).$formatters.push(function (val) {
                return '' + val;
            });
        }
    };
});

angular.module('LocationApp').directive('convertToFloat', function () {
    return {
        require: 'ngModel',
        link: function (scope, element, attrs, ngModel) {
            (<any>ngModel).$parsers.push(function (val) {
                return parseFloat(val);
            });
            (<any>ngModel).$formatters.push(function (val) {
                return '' + val;
            });
        }
    };
});