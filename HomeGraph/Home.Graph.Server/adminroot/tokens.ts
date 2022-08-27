///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />


module HomeAutomation.Admin.Tokens {

   
    export interface Token {
        tokenType: string;
        user: string;
    }

    export interface ITokensPageScope extends ng.IScope {
        view: string;
        events: TokensPage;
        SystemTokens: Token[];
    }


    export class TokensPage {

        connection: signalR.HubConnection;
        scope: ITokensPageScope;
        static $inject = ["$scope"];

        constructor($scope: ITokensPageScope) {
            this.scope = $scope;
            var self = this;

            self.scope.view = "systemtokens-list";
            self.scope.events = self;
            self.scope.SystemTokens = new Array<Token>();
            self.init();
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

           
            $.ajax({
                url: '/v1.0/security/tokens/self/foradmin',
                type: 'GET',
                dataType: "json",
                contentType: "application/json"
            })
                .done(function (data) {
                    sc.Tokens = new Array<Token>();
                    for (var a of data) {
                        var ag: Token = {
                            user : a.user,
                            tokenType: a.tokenType
                        };
                        if (a.user.toLocaleLowerCase() == "system")
                            sc.SystemTokens.push(ag);

                    }
                    sc.$applyAsync();
                })
                .fail(function () {
                });

        }
    }
}

var AuthApp = angular.module('TokenApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('TokensController', HomeAutomation.Admin.Tokens.TokensPage);
