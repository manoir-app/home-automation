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
                    self.scope.groups = null;
                    self.init();
                }
                init() {
                    var sc = this.scope;
                    var self = this;
                    self.refreshHomeAutos();
                    setInterval(() => self.refreshHomeAutos(), 15000);
                }
                refreshHomeAutos() {
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
                            .done(function (data) {
                            sc.groups.forEach((group) => group.scenes = null);
                            for (var i = 0; i < data.length; i++) {
                                var item = data[i];
                                sc.groups.forEach((group) => {
                                    if (item.groupId == group.id) {
                                        if (group.scenes == null)
                                            group.scenes = new Array();
                                        group.scenes.push(item);
                                    }
                                });
                            }
                            sc.$applyAsync();
                        });
                    });
                }
                isActive(scene) {
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
                invokeScene(scene) {
                    var sc = this.scope;
                    var self = this;
                    $.ajax({
                        url: '/v1.0/homeautomation/scenes/execute/' + scene.id,
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json",
                    })
                        .done(function (data) {
                        setTimeout(() => self.refreshHomeAutos(), 3000);
                    });
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