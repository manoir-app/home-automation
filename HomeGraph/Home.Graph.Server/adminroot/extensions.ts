///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />


module HomeAutomation.Admin.Extensions {


    export interface App {
        id: string;
    }

    export interface Extension {
        id: string;
        title: string;
        dockerImageName: string;
        isInstalled: boolean;
    }

    export interface IExtensionsPageScope extends ng.IScope {
        view: string;
        events: ExtensionsPage;
        Extensions: Extension[];
    }


    export class ExtensionsPage {

        connection: signalR.HubConnection;
        scope: IExtensionsPageScope;
        static $inject = ["$scope"];

        constructor($scope: IExtensionsPageScope) {
            this.scope = $scope;
            var self = this;

            self.scope.view = "extensions-list";
            self.scope.events = self;
            self.scope.Extensions = new Array<Extension>();
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
            
            sc.Extensions = new Array<Extension>();

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

var AuthApp = angular.module('ExtensionsApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('ExtensionsController', HomeAutomation.Admin.Extensions.ExtensionsPage);


angular.module('ExtensionsApp').directive('convertToNumber', function () {
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

angular.module('ExtensionsApp').directive('convertToFloat', function () {
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