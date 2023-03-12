///<reference path="../../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />


module HomeAutomation.Admin.RecipeConfig {

    export interface IRecipeConfigPageScope extends ng.IScope {
        view: string;
        events: RecipeConfigPage;

        categories: RecipeCategory[];
        cuisines: RecipeCuisine[];

        currentCategory: RecipeCategory;
        currentCuisine: RecipeCuisine;
    }


    export interface RecipeCategory {
        id: string;
        name: string;
    }

    export interface RecipeCuisine {
        id: string;
        name: string;
    }


    export class RecipeConfigPage {

        connection: signalR.HubConnection;
        scope: IRecipeConfigPageScope;
        static $inject = ["$scope"];

        constructor($scope: IRecipeConfigPageScope) {
            this.scope = $scope;
            var self = this;
            self.scope.categories = new Array<RecipeCategory>();
            self.scope.cuisines = new Array<RecipeCuisine>();
            self.scope.view = "RecipeConfig-list";
            self.scope.events = self;
            self.init();
        }



        public init(): void {
            var sc = this.scope;
            var self = this;

            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/1.0/admin")
                .withAutomaticReconnect()
                .build();

            this.refresh(sc);
            this.connection.start().catch(err => document.write(err));
        }

        public refresh(sc: IRecipeConfigPageScope): void {
            $.ajax({
                url: '/v1.0/meals/recipes/cuisines/',
                type: 'GET',
                dataType: "json",
                contentType: "application/json"
            })
                .done(function (data: RecipeCuisine[]) {
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
                .done(function (data: RecipeCategory[]) {
                    sc.categories = data;
                    sc.$applyAsync();
                })
                .fail(function () {
                });

        }

        public switchToNewCategory(): void {
            this.scope.currentCategory = {
                id: "",
                name: "New Category"
            };
            this.scope.view = "RecipeConfig-editCategory";
            this.scope.$applyAsync();
        }

        public switchBackToMain(): void {
            this.scope.view = "RecipeConfig-list";
            this.scope.$applyAsync();
        }

        public switchToNewCuisine(): void {
            this.scope.currentCuisine = {
                id: "",
                name: "New Cuisine"
            };
            this.scope.view = "RecipeConfig-editCuisine";
            this.scope.$applyAsync();

        }

        public editCategory(cat: RecipeCategory): void {
            this.scope.currentCategory = cat;
            this.scope.view = "RecipeConfig-editCategory";
            this.scope.$applyAsync();
        }

        public editCuisine(cuisine: RecipeCuisine): void {
            this.scope.currentCuisine = cuisine;
            this.scope.view = "RecipeConfig-editCuisine";
            this.scope.$applyAsync();
        }

        public saveCuisineInEdit() {
            var self = this;
            $.ajax({
                url: '/v1.0/meals/recipes/cuisines/',
                type: 'POST',
                dataType: "json",
                contentType: "application/json",
                data: JSON.stringify(self.scope.currentCuisine),
                cache: false
            })
                .done(function (data: RecipeCuisine) {
                    self.scope.view = "RecipeConfig-list";
                    self.scope.$applyAsync();
                    self.refresh(self.scope);
                })
                .fail(function () {
                });
        }

        public saveCategoryInEdit() {
            var self = this;
            $.ajax({
                url: '/v1.0/meals/recipes/categories/',
                type: 'POST',
                dataType: "json",
                contentType: "application/json",
                data: JSON.stringify(self.scope.currentCategory),
                cache: false
            })
                .done(function (data: RecipeCategory) {
                    self.scope.view = "RecipeConfig-list";
                    self.scope.$applyAsync();
                    self.refresh(self.scope);
                })
                .fail(function () {
                });
        }

    }
}

var AuthApp = angular.module('RecipeConfigApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('RecipeConfigController', HomeAutomation.Admin.RecipeConfig.RecipeConfigPage);
