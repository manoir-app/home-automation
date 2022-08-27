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
        var Routines;
        (function (Routines) {
            class RoutinesPage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.view = "Routines-list";
                    self.scope.events = self;
                    self.scope.Routines = new Array();
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
                    self.refresh();
                }
                refresh() {
                    var sc = this.scope;
                    var self = this;
                    $.ajax({
                        url: '/v1.0/system/mesh/local/triggers',
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json"
                    })
                        .done(function (data) {
                        var items = new Array();
                        for (var i = 0; i < data.length; i++) {
                            var tr = {
                                imageUrl: "https://www.manoir.app/resources/trigger_" + data[i].kind + ".png",
                                label: data[i].label,
                                details: ''
                            };
                            items.push(tr);
                        }
                        sc.Routines = items;
                        sc.$applyAsync();
                    })
                        .fail(function () {
                    });
                }
            }
            RoutinesPage.$inject = ["$scope"];
            Routines.RoutinesPage = RoutinesPage;
        })(Routines = Admin.Routines || (Admin.Routines = {}));
    })(Admin = HomeAutomation.Admin || (HomeAutomation.Admin = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('RoutinesApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('RoutinesController', HomeAutomation.Admin.Routines.RoutinesPage);
//# sourceMappingURL=routines.js.map