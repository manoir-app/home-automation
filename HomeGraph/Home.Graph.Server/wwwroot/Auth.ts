///<reference path="scripts/typings/jquery.d.ts" />
///<reference path="scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="scripts/typings/angularjs/angular.d.ts" />
///<reference path="HomeGraphTools.ts" />

module HomeAutomation.Auth {

    export interface IDefaultPageScope extends ng.IScope {
        events: DefaultPage;
        loggedInUser: User;
        menuOuvert: boolean;
    }

    export class User {
        public id: string
        public name: string
        public firstName: string
        public mainEmail: string
        public role: string
    }

    export class DefaultPage {
        scope: IDefaultPageScope;
        static $inject = ["$scope"];
        constructor($scope: IDefaultPageScope) {
            this.scope = $scope;
            var self = this;
            self.scope.events = self;
            self.init();
        }

        private init() {
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
            } else {
                if (location.pathname != '/login.html')
                    window.location.replace('/login.html');
            }
        }

        public Login() {
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
                .done(function (data: User) {
                    console.log(data);
                    sessionStorage.setItem("loggedInUser", angular.toJson(data));
                    window.location.replace('/me/');
                })
                .fail(function () {
                    $('#pwdError').show();
                });
        }

        public Logout() {
            var self = this;
            $.ajax({
                url: '/v1.0/users/logout',
                type: 'GET',
                dataType: "json",
                contentType: "application/json"
            })
                .done(function (data: any) {
                    sessionStorage.removeItem("loggedInUser");
                    window.location.replace('/login.html');
                })
                .fail(function () {
                });
        }

    }
}

var AuthApp = angular.module('AuthApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('AuthController', HomeAutomation.Auth.DefaultPage);
