///<reference path="scripts/typings/jquery.d.ts" />
///<reference path="scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="scripts/typings/angularjs/angular.d.ts" />
///<reference path="HomeGraphTools.ts" />
var HomeAutomation;
(function (HomeAutomation) {
    var Auth;
    (function (Auth) {
        class User {
        }
        Auth.User = User;
        class DefaultPage {
            constructor($scope) {
                this.scope = $scope;
                var self = this;
                self.scope.events = self;
                self.init();
            }
            init() {
                var self = this.scope;
                var methode = this;
                self.menuOuvert = false;
                self.loggedInUser = angular.fromJson(sessionStorage.getItem("loggedInUser"));
                console.log(self.loggedInUser);
                console.log(location.pathname);
                if (self.loggedInUser != null) {
                    if (self.loggedInUser.role == "admin") {
                        if (location.pathname != '/me/') {
                            window.location.replace('/me/');
                        }
                    }
                }
                else {
                    if (location.pathname != '/login.html')
                        window.location.replace('/login.html');
                }
            }
            Login() {
                var email = $("#email").val();
                var pwd = $("#password").val();
                var self = this.scope;
                $('#pwdError').hide();
                var user = {
                    login: email,
                    pwd: pwd
                };
                $.ajax({
                    url: '/v1.0/users/login',
                    type: 'POST',
                    headers: {
                        'Accept': 'application/json',
                        'Content-Type': 'application/json'
                    },
                    data: JSON.stringify(user),
                    dataType: "json",
                    contentType: "application/json"
                })
                    .done(function (data) {
                    console.log(data);
                    sessionStorage.setItem("loggedInUser", angular.toJson(data));
                    window.location.replace('/me/');
                })
                    .fail(function () {
                    $('#pwdError').show();
                });
            }
            Logout() {
                var self = this;
                $.ajax({
                    url: '/v1.0/users/logout',
                    type: 'GET',
                    dataType: "json",
                    contentType: "application/json"
                })
                    .done(function (data) {
                    sessionStorage.removeItem("loggedInUser");
                    window.location.replace('/login.html');
                })
                    .fail(function () {
                });
            }
        }
        DefaultPage.$inject = ["$scope"];
        Auth.DefaultPage = DefaultPage;
    })(Auth = HomeAutomation.Auth || (HomeAutomation.Auth = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('AuthApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('AuthController', HomeAutomation.Auth.DefaultPage);
//# sourceMappingURL=Auth.js.map