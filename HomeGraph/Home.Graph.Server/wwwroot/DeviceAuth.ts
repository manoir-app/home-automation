///<reference path="scripts/typings/jquery.d.ts" />
///<reference path="scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="scripts/typings/angularjs/angular.d.ts" />
///<reference path="HomeGraphTools.ts" />

module HomeAutomation.DeviceAuth {

    export interface IDefaultPageScope extends ng.IScope {
        events: DefaultPage;
        deviceIdentifier: User;
    }


    export class LoginFromDevice {
        public user: User;
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
            self.deviceIdentifier = angular.fromJson(localStorage.getItem("deviceToken"));
            if (self.deviceIdentifier != null) {
                if (location.pathname != '/devicehome/') {
                    window.location.replace('/devicehome/');
                    }
            } else {
                if (location.pathname != '/devicehome.html')
                    window.location.replace('/devicehome.html');
            }
        }

        public Login() {
            var email = $("#email").val();
            var pwd = $("#password").val();
            var devname = $("#iternalName").val();
            var type = $("#deviceType").val();

            var self = this.scope;
            $('#pwdError').hide();

            var user = {
                login: email,
                pwd: pwd,
                deviceInternalName: devname,
                deviceKind : type
            };

            $.ajax({
                url: '/v1.0/users/login/device?addCookie=true',
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
                    localStorage.setItem("deviceToken", angular.toJson(data));
                    window.location.replace('/devicehome/');
                })
                .fail(function () {
                    $('#pwdError').show();
                });
        }
    }
}

var DeviceAuthApp = angular.module('DeviceAuthApp', ['ui.select2', 'ngAnimate']);
DeviceAuthApp.controller('DeviceAuthController', HomeAutomation.DeviceAuth.DefaultPage);
