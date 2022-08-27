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
        var Tokens;
        (function (Tokens) {
            class TokensPage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.view = "systemtokens-list";
                    self.scope.events = self;
                    self.scope.SystemTokens = new Array();
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
                    $.ajax({
                        url: '/v1.0/security/tokens/self/foradmin',
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json"
                    })
                        .done(function (data) {
                        sc.Tokens = new Array();
                        for (var a of data) {
                            var ag = {
                                user: a.user,
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
            TokensPage.$inject = ["$scope"];
            Tokens.TokensPage = TokensPage;
        })(Tokens = Admin.Tokens || (Admin.Tokens = {}));
    })(Admin = HomeAutomation.Admin || (HomeAutomation.Admin = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('TokenApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('TokensController', HomeAutomation.Admin.Tokens.TokensPage);
//# sourceMappingURL=tokens.js.map