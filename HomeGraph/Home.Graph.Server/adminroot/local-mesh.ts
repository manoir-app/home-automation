///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />


module HomeAutomation.Admin.LocalMesh {


    export interface Location {
        Id: string;
        KindLabel: string;
        Coordinates: string;
        Name: string;
        ZonesInfo: string;
        LocationObject: any;
    }

    interface MeshData {
        timeZoneId: string;
        languageId: string;
        countryId: string;

        locationId: string;
    }

    export interface LocalMeshPageScope extends ng.IScope {
        events: LocalMeshPage;
        restarting: boolean;
        restartingPublic: boolean;
        LocalMesh: MeshData;
        AssociatedLocation: Location;
    }


    export class LocalMeshPage {

        connection: signalR.HubConnection;
        scope: LocalMeshPageScope;
        static $inject = ["$scope"];

        constructor($scope: LocalMeshPageScope) {
            this.scope = $scope;
            var self = this;

            self.scope.restarting = false;
            self.scope.events = self;
            self.scope.LocalMesh = null;
            self.scope.AssociatedLocation = null;
            self.init();
        }



       

        public restartPublicProxy(): boolean {
            var sc = this.scope;
            var self = this;

            var msg = { Topic: "gaia.deployments", Action: "Restart", DeploymentName: "home-automation-public" };
            sc.restartingPublic = true;
            sc.$applyAsync();

            $.ajax({
                url: '/v1.0/agents/all/send/gaia.deployments',
                type: 'POST',
                dataType: "json",
                contentType: "application/json",
                data: JSON.stringify(msg),
                cache: false
            })
                .done(function (data) {
                    setTimeout(() => {
                        sc.restartingPublic = false;
                        sc.$applyAsync();
                    }, 5000);
                })

            return false;
        }

        public restartHomeGraph(): boolean {
            var sc = this.scope;
            var self = this;

            var msg = { Topic: "gaia.deployments", Action: "Restart", DeploymentName: "home-automation" };
            sc.restarting = true;
            sc.$applyAsync();

            $.ajax({
                url: '/v1.0/agents/all/send/gaia.deployments',
                type: 'POST',
                dataType: "json",
                contentType: "application/json",
                data: JSON.stringify(msg),
                cache: false
            })
                .done(function (data) {
                    setTimeout(function () {
                        setInterval(function () {
                            $.ajax({
                                url: '/',
                                type: 'GET',
                                cache:false
                            })
                                .fail(function () {
                                    console.log("Non restarted yet");
                                })
                                .done(function (data, status, xhr) {
                                    if (xhr != null) {
                                        if (xhr.status == 200)
                                            document.location.reload(true);
                                        else
                                            console.log("Non restarted yet : " + xhr.status);
                                    }
                                    else
                                        console.log("xhr est vide :(");
                                })
                        }, 500);
                    }, 10000);
                });

            return false;
        }

        public init(): void {
            var sc = this.scope;
            var self = this;

            var sc = this.scope;
            var self = this;
            $.ajax({
                url: '/v1.0/system/mesh/local',
                type: 'GET',
                dataType: "json",
                contentType: "application/json"
            })
                .done(function (data: MeshData) {
                    sc.LocalMesh = data;
                    sc.$applyAsync();

                    $.ajax({
                        url: '/v1.0/locations/' + sc.LocalMesh.locationId,
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json"
                    })
                        .done(function (data: Location) {
                            sc.AssociatedLocation = data;
                            sc.$applyAsync();
                        })
                        .fail(function () {
                        });


                })
                .fail(function () {
                });
           
            
        }
    }
}

var AuthApp = angular.module('LocalMeshApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('LocalMeshController', HomeAutomation.Admin.LocalMesh.LocalMeshPage);


