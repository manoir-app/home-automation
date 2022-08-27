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
        var HomePage;
        (function (HomePage_1) {
            class HomePage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.events = self;
                    self.scope.presentUsers = new Array();
                    self.init();
                }
                init() {
                    var sc = this.scope;
                    var self = this;
                    $.ajax({
                        url: '/v1.0/users/presence/mesh/local/all',
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json"
                    })
                        .done(function (data) {
                        sc.presentUsers = new Array();
                        for (var i = 0; i < data.length; i++) {
                            sc.presentUsers.push({
                                name: data[i].name,
                                firstName: data[i].firstName,
                                commonName: data[i].commonName,
                                urlAvatarSmall: data[i].avatar != null ? data[i].avatar.urlSquareSmall : null
                            });
                        }
                        sc.$applyAsync();
                    })
                        .fail(function () {
                    });
                }
            }
            HomePage.$inject = ["$scope"];
            HomePage_1.HomePage = HomePage;
        })(HomePage = Me.HomePage || (Me.HomePage = {}));
    })(Me = HomeAutomation.Me || (HomeAutomation.Me = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('HomePageApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('HomePageController', HomeAutomation.Me.HomePage.HomePage);
//# sourceMappingURL=index.js.map