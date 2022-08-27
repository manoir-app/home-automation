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
        var Extensions;
        (function (Extensions) {
            class ExtensionsPage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.view = "extensions-list";
                    self.scope.events = self;
                    self.scope.Extensions = new Array();
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
                    sc.Extensions = new Array();
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
            ExtensionsPage.$inject = ["$scope"];
            Extensions.ExtensionsPage = ExtensionsPage;
        })(Extensions = Admin.Extensions || (Admin.Extensions = {}));
    })(Admin = HomeAutomation.Admin || (HomeAutomation.Admin = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('ExtensionsApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('ExtensionsController', HomeAutomation.Admin.Extensions.ExtensionsPage);
angular.module('ExtensionsApp').directive('convertToNumber', function () {
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
angular.module('ExtensionsApp').directive('convertToFloat', function () {
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
//# sourceMappingURL=extensions.js.map