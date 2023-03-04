///<reference path="../../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />
var HomeAutomation;
(function (HomeAutomation) {
    var Admin;
    (function (Admin) {
        var RecipeConfig;
        (function (RecipeConfig) {
            class RecipeConfigPage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.categories = new Array();
                    self.scope.cuisines = new Array();
                    self.scope.view = "RecipeConfig-list";
                    self.scope.events = self;
                    self.init();
                }
                init() {
                    var sc = this.scope;
                    var self = this;
                    this.connection = new signalR.HubConnectionBuilder()
                        .withUrl("/hubs/1.0/admin")
                        .withAutomaticReconnect()
                        .build();
                    this.refresh(sc);
                    this.connection.start().catch(err => document.write(err));
                }
                refresh(sc) {
                    $.ajax({
                        url: '/v1.0/meals/recipes/cuisines/',
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json"
                    })
                        .done(function (data) {
                        sc.cuisines = data;
                        sc.$applyAsync();
                    })
                        .fail(function () {
                    });
                    $.ajax({
                        url: '/v1.0/meals/recipes/categories/',
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json"
                    })
                        .done(function (data) {
                        sc.categories = data;
                        sc.$applyAsync();
                    })
                        .fail(function () {
                    });
                }
                switchToNewCategory() {
                    this.scope.currentCategory = {
                        id: "",
                        name: "New Category"
                    };
                    this.scope.view = "RecipeConfig-editCategory";
                    this.scope.$applyAsync();
                }
                switchBackToMain() {
                    this.scope.view = "RecipeConfig-list";
                    this.scope.$applyAsync();
                }
                switchToNewCuisine() {
                    this.scope.currentCuisine = {
                        id: "",
                        name: "New Cuisine"
                    };
                    this.scope.view = "RecipeConfig-editCuisine";
                    this.scope.$applyAsync();
                }
                editCategory(cat) {
                    this.scope.currentCategory = cat;
                    this.scope.view = "RecipeConfig-editCategory";
                    this.scope.$applyAsync();
                }
                editCuisine(cuisine) {
                    this.scope.currentCuisine = cuisine;
                    this.scope.view = "RecipeConfig-editCuisine";
                    this.scope.$applyAsync();
                }
                saveCuisineInEdit() {
                    var self = this;
                    $.ajax({
                        url: '/v1.0/meals/recipes/cuisines/',
                        type: 'POST',
                        dataType: "json",
                        contentType: "application/json",
                        data: JSON.stringify(self.scope.currentCuisine),
                        cache: false
                    })
                        .done(function (data) {
                        self.scope.view = "RecipeConfig-list";
                        self.scope.$applyAsync();
                        self.refresh(self.scope);
                    })
                        .fail(function () {
                    });
                }
                saveCategoryInEdit() {
                    var self = this;
                    $.ajax({
                        url: '/v1.0/meals/recipes/categories/',
                        type: 'POST',
                        dataType: "json",
                        contentType: "application/json",
                        data: JSON.stringify(self.scope.currentCategory),
                        cache: false
                    })
                        .done(function (data) {
                        self.scope.view = "RecipeConfig-list";
                        self.scope.$applyAsync();
                        self.refresh(self.scope);
                    })
                        .fail(function () {
                    });
                }
            }
            RecipeConfigPage.$inject = ["$scope"];
            RecipeConfig.RecipeConfigPage = RecipeConfigPage;
        })(RecipeConfig = Admin.RecipeConfig || (Admin.RecipeConfig = {}));
    })(Admin = HomeAutomation.Admin || (HomeAutomation.Admin = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('RecipeConfigApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('RecipeConfigController', HomeAutomation.Admin.RecipeConfig.RecipeConfigPage);
//# sourceMappingURL=recipeconfigs.js.map