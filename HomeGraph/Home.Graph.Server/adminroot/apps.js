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
        var Apps;
        (function (Apps) {
            class AppsPage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.view = "apps-list";
                    self.scope.events = self;
                    self.scope.CurrentApp = null;
                    self.scope.Apps = new Array();
                    self.init();
                }
                restartExtension(ext) {
                    $.ajax({
                        url: '/v1.0/system/mesh/local/extensions/' + ext.id + '/restart',
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json"
                    })
                        .done(function (data) {
                    });
                    return false;
                }
                installExtension(ext) {
                    var self = this;
                    $.ajax({
                        url: '/v1.0/system/mesh/local/extensions/' + ext.id + '/install',
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json"
                    })
                        .done(function (data) {
                        self.init();
                    });
                    return false;
                }
                uninstallExtension(ext) {
                    var self = this;
                    $.ajax({
                        url: '/v1.0/system/mesh/local/extensions/' + ext.id + '/uninstall',
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json"
                    })
                        .done(function (data) {
                        self.init();
                    });
                    return false;
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
                    sc.Apps = new Array();
                    sc.Extensions = new Array();
                    //$.ajax({
                    //    url: '/v1.0/Apps',
                    //    type: 'GET',
                    //    dataType: "json",
                    //    contentType: "application/json"
                    //})
                    //    .done(function (data) {
                    //        sc.Apps = new Array<App>();
                    //        for (var a of data) {
                    //            var ag: App = {
                    //            };
                    //            sc.Apps.push(ag);
                    //        }
                    //        sc.view = "Apps-list";
                    //        sc.$applyAsync();
                    //    })
                    //    .fail(function () {
                    //    });
                    $.ajax({
                        url: '/v1.0/system/mesh/local/extensions',
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json"
                    })
                        .done(function (data) {
                        sc.Extensions = new Array();
                        for (var a of data) {
                            sc.Extensions.push(a);
                        }
                        sc.$applyAsync();
                    })
                        .fail(function () {
                    });
                }
            }
            AppsPage.$inject = ["$scope"];
            Apps.AppsPage = AppsPage;
        })(Apps = Admin.Apps || (Admin.Apps = {}));
    })(Admin = HomeAutomation.Admin || (HomeAutomation.Admin = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('AppsApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('AppsController', HomeAutomation.Admin.Apps.AppsPage);
angular.module('AppsApp').directive('convertToNumber', function () {
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
angular.module('AppsApp').directive('convertToFloat', function () {
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
//# sourceMappingURL=apps.js.map