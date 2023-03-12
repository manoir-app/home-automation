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

        groups: Array<SceneGroup>;
    }

    export interface SceneGroup {
        id: string;
        label: string;
        order: number;
        sceneIsExclusive: number;
        clearGroupSceneId: string;
        currentActiveScenes: Array<string>;
        scenes: Array<Scene>;
    }

    export interface Scene {
        id: string;
        groupId: string;
        label: string;
        iconUrl: number;
        bannerUrl: number;
        privacyLevel: number;
    }


    export class HomeAutoPage {

        connection: signalR.HubConnection;
        scope: IHomeAutoPageScope;
        static $inject = ["$scope"];

        constructor($scope: IHomeAutoPageScope) {
            this.scope = $scope;
            var self = this;

            self.scope.events = self;
            self.scope.groups = null;
            self.init();
        }

        public init(): void {
            var sc = this.scope;
            var self = this;

            self.refreshHomeAutos();
            setInterval(() => self.refreshHomeAutos(), 15000);




        }


        public refreshHomeAutos(): void {
            var sc = this.scope;
            var self = this;
            $.ajax({
                url: '/v1.0/homeautomation/scenes/groups',
                type: 'GET',
                dataType: "json",
                contentType: "application/json",
            })
                .done(function (data) {
                    sc.groups = data;


                    $.ajax({
                        url: '/v1.0/homeautomation/scenes/scenes',
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json",
                    })
                        .done(function (data: Scene[]) {
                            sc.groups.forEach((group) => group.scenes = null);

                            for (var i = 0; i < data.length; i++) {
                                var item = data[i];
                                sc.groups.forEach((group) => {
                                    if (item.groupId == group.id) {
                                        if (group.scenes == null)
                                            group.scenes = new Array<Scene>();
                                        group.scenes.push(item);
                                    }
                                })
                            }

                            sc.$applyAsync();
                        });
                });

        }

        public isActive(scene: Scene): boolean {
            var sc = this.scope;
            var found = false;
            sc.groups.forEach((group) => {
                if (group.currentActiveScenes != null) {
                    group.currentActiveScenes.forEach((id) => {
                        if (id == scene.id)
                            found = true;
                    });
                }
            });
            return found;
        }

        public invokeScene(scene: Scene) {
            var sc = this.scope;
            var self = this;
            $.ajax({
                url: '/v1.0/homeautomation/scenes/execute/' + scene.id,
                type: 'GET',
                dataType: "json",
                contentType: "application/json",
            })
                .done(function (data: Scene[]) {
                    setTimeout(() => self.refreshHomeAutos(), 3000);
                });
        }


    }
}

var AuthApp = angular.module('HomeAutoPageApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('HomeAutoPageController', HomeAutomation.Me.HomeAutoPage.HomeAutoPage);
