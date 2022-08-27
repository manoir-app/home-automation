///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />


module HomeAutomation.Admin.Apps {


    export interface App {
        id: string;
    }

    export interface Extension {
        id: string;
        title: string;
        dockerImageName: string;
        isInstalled: boolean;
    }

    export interface IAppsPageScope extends ng.IScope {
        view: string;
        events: AppsPage;
        Apps: App[];
        Extensions: Extension[];
    }


    export class AppsPage {

        connection: signalR.HubConnection;
        scope: IAppsPageScope;
        static $inject = ["$scope"];

        constructor($scope: IAppsPageScope) {
            this.scope = $scope;
            var self = this;

            self.scope.view = "apps-list";
            self.scope.events = self;
            self.scope.CurrentApp = null;
            self.scope.Apps = new Array<App>();
            self.init();
        }

        public restartExtension(ext: Extension): boolean {

            $.ajax({
                url: '/v1.0/system/mesh/local/extensions/' + ext.id + '/restart',
                type: 'GET',
                dataType: "json",
                contentType: "application/json"
            })
                .done(function (data : boolean) {
                    
                });

            return false;
        }

        public installExtension(ext: Extension): boolean {
            var self = this;

            $.ajax({
                url: '/v1.0/system/mesh/local/extensions/' + ext.id + '/install',
                type: 'GET',
                dataType: "json",
                contentType: "application/json"
            })
                .done(function (data: boolean) {
                    self.init();
                });

            return false;
        }

        public uninstallExtension(ext: Extension): boolean {

            var self = this;

            $.ajax({
                url: '/v1.0/system/mesh/local/extensions/' + ext.id + '/uninstall',
                type: 'GET',
                dataType: "json",
                contentType: "application/json"
            })
                .done(function (data: boolean) {
                    self.init();
                });

            return false;
        }

        public switchToView(newView: string): boolean {
            var sc = this.scope;

            sc.view = newView;

            sc.$applyAsync();
            return false;
        }

        public init(): void {
            var sc = this.scope;
            var self = this;
            
            sc.Apps = new Array<App>();
            sc.Extensions = new Array<Extension>();

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
                .done(function (data: Extension[]) {
                    sc.Extensions = new Array<Extension>();
                    for (var a of data) {
                        sc.Extensions.push(a);
                    }
                    sc.$applyAsync();
                })
                .fail(function () {
                });

        }
    }
}

var AuthApp = angular.module('AppsApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('AppsController', HomeAutomation.Admin.Apps.AppsPage);


angular.module('AppsApp').directive('convertToNumber', function () {
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

angular.module('AppsApp').directive('convertToFloat', function () {
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