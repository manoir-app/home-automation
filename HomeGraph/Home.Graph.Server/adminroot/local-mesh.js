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
        var LocalMesh;
        (function (LocalMesh) {
            class LocalMeshPage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.restarting = false;
                    self.scope.events = self;
                    self.scope.LocalMesh = null;
                    self.scope.AssociatedLocation = null;
                    self.init();
                }
                restartPublicProxy() {
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
                    });
                    return false;
                }
                restartHomeGraph() {
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
                                    cache: false
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
                                });
                            }, 500);
                        }, 10000);
                    });
                    return false;
                }
                init() {
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
                        .done(function (data) {
                        sc.LocalMesh = data;
                        sc.$applyAsync();
                        $.ajax({
                            url: '/v1.0/locations/' + sc.LocalMesh.locationId,
                            type: 'GET',
                            dataType: "json",
                            contentType: "application/json"
                        })
                            .done(function (data) {
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
            LocalMeshPage.$inject = ["$scope"];
            LocalMesh.LocalMeshPage = LocalMeshPage;
        })(LocalMesh = Admin.LocalMesh || (Admin.LocalMesh = {}));
    })(Admin = HomeAutomation.Admin || (HomeAutomation.Admin = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('LocalMeshApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('LocalMeshController', HomeAutomation.Admin.LocalMesh.LocalMeshPage);
//# sourceMappingURL=local-mesh.js.map