///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />

module HomeAutomation.Me.ProductPage {

    export interface IProductPageScope extends ng.IScope {
        events: ProductPage;
        view: string;
        categories: Array<Category>;
        currentCategory: string;
        currentPage: number;
        products: Array<Product>;
        pages: Array<string>;
        types: Array<ProductType>;
        storageUnits: Array<StorageUnit>;
        units: Array<Unit>;

        /* la partie édition */
        currentProduct: Product;
        currentProductImages: Array<string>;
        isSaving: boolean;

        /* et l'affichage */
        productInModal: Product;
    }

    export interface ProductImages {
        bigImageUrls: Array<string>;
        smallImageUrls: Array<string>;
        iconUrls: Array<string>;
    }

    export interface Category {
        id: string;
        label: string;
        parentId: string;
        subcategories: Array<Category>;
    }

    export interface Product {
        id: string;
        label: string;
        genericName: string;


        categories: Array<string>;
        productTypeId: string;
        images: Map<string, ProductImages>;
        packagings: Array<ProductPackaging>;
        consumptionInfo: ProductConsumption;
        storageInfo: ProductStorageInfo;
        unitId: string;
        unitExplanation: string;

        // pour édition
        mainCategoryId: string;
    }

    export interface ProductPackaging {
        id: string;
        label: string;
        originKind: number;
        originId: string;
        ean: string;
        packQuantity: number;
        standardExpirationDuration: number;
        images: Map<string, ProductImages>;
        productPackagingType: number;
    }

    export interface ProductStorageInfo {
        standardExpirationDuration: number;
        standardExpirationTolerance: number;
        defaultStorageIdWhenNew: string;
        defaultStorageSubElementIdWhenNew: string;
        defaultStorageCommentWhenNew: string;
        defaultStorageIdWhenOpened: string;
        defaultStorageSubElementIdWhenOpened: string;
        defaultStorageCommentWhenOpened: string;
    }

    export interface ProductConsumption {
        autoDecrementAmount: number;
        autoDecrementPeriod: string;
        lastAutoDecrementDate: Date;
    }


    interface ProductSearchResult {
        totalResults: number;
        items: Array<Product>;
    }
    export interface ProductType {
        id: string;
        label: string;
        metaType: number;
    }
    export interface StorageUnit {
        id: string;
        roomId: string;
        label: string;
        subElements: Array<StorageUnitSubElement>;
    }
    export interface StorageUnitSubElement {
        id: string;
        label: string;

    }
    export interface Unit {
        id: string;
        label: string;
        symbol: string;
        metaType: number,
    }

    export class ProductPage {

        connection: signalR.HubConnection;
        scope: IProductPageScope;
        static $inject = ["$scope"];

        constructor($scope: IProductPageScope) {
            this.scope = $scope;
            var self = this;

            self.scope.events = self;
            self.scope.categories = new Array<Category>();
            self.scope.currentCategory = null;
            self.scope.currentPage = 1;
            self.scope.view = "products-list";
            self.scope.pages = new Array<string>();
            self.scope.products = new Array<Product>();
            self.scope.units = new Array<Unit>();
            self.scope.types = new Array<ProductType>();
            self.scope.storageUnits = new Array<StorageUnit>();
            self.scope.isSaving = false;
            self.init();
        }

        public init(): void {
            var sc = this.scope;
            var self = this;



            $.ajax({
                url: '/v1.0/products/types',
                type: 'GET',
                dataType: "json",
                contentType: "application/json",
            })
                .done(function (data: ProductType[]) {
                    sc.types = data;
                    sc.$applyAsync();
                });

            $.ajax({
                url: '/v1.0/storage/storageunits',
                type: 'GET',
                dataType: "json",
                contentType: "application/json",
            })
                .done(function (data: StorageUnit[]) {
                    sc.storageUnits = data;
                    sc.$applyAsync();
                });

            $.ajax({
                url: '/v1.0/settings/units/',
                type: 'GET',
                dataType: "json",
                contentType: "application/json",
            })
                .done(function (data: Unit[]) {
                    var dt = new Array<Unit>();
                    for (var i = 0; i < data.length; i++) {
                        if (data[i].metaType < 3) // on mass, volume & discrete
                            dt.push(data[i]);
                    }
                    sc.units = dt;
                    sc.$applyAsync();
                });

            $.ajax({
                url: '/v1.0/products/categories',
                type: 'GET',
                dataType: "json",
                contentType: "application/json",
            })
                .done(function (data: Category[]) {

                    self.makeCategoryTree(sc, data);

                    sc.$applyAsync();
                }).then(() => {
                    self.refreshProducts();
                });

        }

        public switchToView(newView: string): boolean {
            var sc = this.scope;

            sc.view = newView;

            sc.$applyAsync();
            return false;
        }

        public editProduct(prd: Product) {
            var sc = this.scope;
            sc.currentProduct = prd;
            for (var i = 0; i < sc.categories.length; i++) {
                var vat = sc.categories[i];
                for (var j = 0; j < vat.subcategories.length; j++) {
                    if (prd.categories.indexOf(vat.subcategories[j].id) >= 0) {
                        sc.currentProduct.mainCategoryId = vat.subcategories[j].id;
                    }
                }
            }
            sc.view = "product-edit";

            sc.$applyAsync();
            return false;
        }

        public switchToNew(): boolean {
            var sc = this.scope;

            var newPrd: Product = {
                id: null, label: "",
                genericName: "",
                images: new Map<string, ProductImages>(),
                unitId: null, unitExplanation: null,
                productTypeId: null,
                categories:new Array<string>(),
                storageInfo: {
                    defaultStorageCommentWhenNew: null,
                    defaultStorageCommentWhenOpened: null,
                    defaultStorageIdWhenNew: null,
                    defaultStorageIdWhenOpened: null,
                    defaultStorageSubElementIdWhenNew: null,
                    defaultStorageSubElementIdWhenOpened: null,
                    standardExpirationTolerance: null,
                    standardExpirationDuration: null,
                },
                consumptionInfo: {
                    autoDecrementAmount: null, autoDecrementPeriod: null, lastAutoDecrementDate: null,
                },
                packagings: new Array<ProductPackaging>(),
                mainCategoryId: null
            };
            newPrd.unitId = sc.units.length > 0 ? sc.units[0].id : null;
            newPrd.productTypeId = sc.types.length > 0 ? sc.types[0].id : null;
            
            sc.currentProduct = newPrd;
            sc.currentProductImages = new Array<string>();

            if (sc.categories != null && sc.categories.length > 0
                && sc.categories[0].subcategories != null && sc.categories[0].subcategories.length > 0) {

                newPrd.mainCategoryId = sc.categories[0].subcategories[0].id;
            }
            else
                newPrd.mainCategoryId = "";

            sc.view = "product-edit";

            sc.$applyAsync();

            return false;
        }

        public switchBackToMain() {
            var sc = this.scope;
            sc.currentProduct = null;
            sc.currentProductImages = null;
            sc.view = "products-list";
            sc.$applyAsync();
        }

        public saveProduct() {
            var sc = this.scope;
            var self = this;

            sc.isSaving = true;
            sc.$applyAsync();

            var prd = sc.currentProduct;
            prd.categories = new Array<string>();
            prd.categories.push(sc.currentProduct.mainCategoryId);

            
            if (prd.storageInfo.defaultStorageSubElementIdWhenNew == "") {
                    prd.storageInfo.defaultStorageSubElementIdWhenNew = null;
                    prd.storageInfo.defaultStorageIdWhenNew = null;
            }
            if (prd.storageInfo.defaultStorageSubElementIdWhenNew != null) {
                for (var i = 0; i < sc.storageUnits.length; i++) {
                    var su = sc.storageUnits[i];
                    if (prd.storageInfo.defaultStorageSubElementIdWhenNew == su.id) {
                        prd.storageInfo.defaultStorageSubElementIdWhenNew = null;
                        prd.storageInfo.defaultStorageIdWhenNew = su.id;
                    }
                    else {
                        var exists = su.subElements.filter((item, idx) => item.id == prd.storageInfo.defaultStorageSubElementIdWhenNew);
                        if (exists != null && exists.length > 0)
                            prd.storageInfo.defaultStorageIdWhenNew = su.id;
                    }
                }
            }
            if (prd.storageInfo.defaultStorageSubElementIdWhenOpened == "") {
                prd.storageInfo.defaultStorageSubElementIdWhenOpened = null;
                prd.storageInfo.defaultStorageIdWhenOpened = null;
            }
            if (prd.storageInfo.defaultStorageSubElementIdWhenOpened != null) {
                for (var i = 0; i < sc.storageUnits.length; i++) {
                    var su = sc.storageUnits[i];
                    if (prd.storageInfo.defaultStorageSubElementIdWhenOpened == su.id) {
                        prd.storageInfo.defaultStorageSubElementIdWhenOpened = null;
                        prd.storageInfo.defaultStorageIdWhenOpened = su.id;
                    }
                    else {
                        var exists = su.subElements.filter((item, idx) => item.id == prd.storageInfo.defaultStorageSubElementIdWhenOpened);
                        if (exists != null && exists.length > 0)
                            prd.storageInfo.defaultStorageIdWhenOpened = su.id;
                    }
                }
            }

            $.ajax({
                url: '/v1.0/products/',
                type: 'POST',
                contentType: "application/json",
                data: JSON.stringify(sc.currentProduct)
            })
                .done(function (data: ProductSearchResult) {
                    sc.isSaving = false;
                    self.switchBackToMain();
                    self.refreshProducts();
                    sc.$applyAsync();
                }).fail((err) => {
                    sc.isSaving = false;
                    self.switchBackToMain();
                    sc.$applyAsync();
                });

            self.switchBackToMain();
        }

        public newPackaging(prd: Product) {
            var pack: ProductPackaging = {
                ean: null,
                label: null,
                id: null,
                packQuantity: null,
                images: new Map<string, ProductImages>(),
                originId: null,
                originKind: 0,
                productPackagingType:1,
                standardExpirationDuration:null
            };
            prd.packagings.push(pack);
        }

        public refreshProducts(): void {
            var sc = this.scope;
            var self = this;
            if (sc.currentCategory == null || sc.currentCategory == '') {

                $.ajax({
                    url: '/v1.0/products/find/all?page=' + (sc.currentPage - 1),
                    type: 'GET',
                    dataType: "json",
                    contentType: "application/json",
                })
                    .done(function (data: ProductSearchResult) {
                        self.makeProductResult(sc, data);
                        sc.$applyAsync();
                    });
            }
            else {

                $.ajax({
                    url: '/v1.0/products/find/category/' + sc.currentCategory + "?page=" + (sc.currentPage - 1),
                    type: 'GET',
                    dataType: "json",
                    contentType: "application/json",
                })
                    .done(function (data: ProductSearchResult) {
                        self.makeProductResult(sc, data);
                        sc.$applyAsync();

                    });
            }
        }

        public isCurrentCategory(catId: string): boolean {
            var sc = this.scope;
            if (sc.currentCategory == null && catId == "") // cas particulier du "tous les articles"
                return true;
            return sc.currentCategory == catId;
        }

        public isCurrentPage(pageId: string): boolean {
            var sc = this.scope;
            return sc.currentPage == parseInt(pageId);
        }

        public setCurrentPage(pageId: string) {
            var sc = this.scope;
            sc.currentPage == parseInt(pageId);
            sc.$applyAsync();

            this.refreshProducts();
        }


        public selectCategory(catId: string): void {
            var sc = this.scope;
            sc.currentPage = 1;
            if (catId == '')
                sc.currentCategory = null;
            else
                sc.currentCategory = catId;
            sc.$applyAsync();

            this.refreshProducts();
        }


        public getImage(prd: Product) {
            if (prd.images == null)
                return '/me/imgs/empty-40x40.png';
            var t = prd.images.get("main");
            if (t == null)
                return '/me/imgs/empty-40x40.png';
            if (t.smallImageUrls == null || t.smallImageUrls.length == 0)
                return '/me/imgs/empty-40x40.png';
            return t.smallImageUrls[0];
        }

        public showProduct(prd: Product) {
            var sc = this.scope;
            sc.productInModal = prd;
        }

        private makeCategoryTree(sc: IProductPageScope, data: Category[]): void {
            sc.categories = new Array<Category>();
            data.forEach((parent, index) => {
                var childs = data.filter((item, index) => item.parentId != null && item.parentId == parent.id);
                parent.subcategories = childs;
                if (parent.parentId == null)
                    sc.categories.push(parent);
            })
        }
        private makeProductResult(sc: IProductPageScope, data: ProductSearchResult): void {
            sc.products = data.items;
            if (data.totalResults > 250) {
                sc.pages = new Array<string>();
                var i = 0;
                for (i = 0; i < (data.totalResults / 250); i++) {
                    sc.pages.push((i + 1).toString());
                }
                if (data.totalResults % 250 != 0)
                    sc.pages.push((i + 1).toString());
            }
            else {
                sc.pages = new Array<string>();
            }
        }
    }
}

var AuthApp = angular.module('ProductPageApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('ProductPageController', HomeAutomation.Me.ProductPage.ProductPage);
