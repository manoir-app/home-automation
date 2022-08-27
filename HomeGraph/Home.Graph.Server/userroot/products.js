///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />
var HomeAutomation;
(function (HomeAutomation) {
    var Me;
    (function (Me) {
        var ProductPage;
        (function (ProductPage_1) {
            class ProductPage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.events = self;
                    self.scope.categories = new Array();
                    self.scope.currentCategory = null;
                    self.scope.currentPage = 1;
                    self.scope.view = "products-list";
                    self.scope.pages = new Array();
                    self.scope.products = new Array();
                    self.scope.units = new Array();
                    self.scope.types = new Array();
                    self.scope.storageUnits = new Array();
                    self.scope.isSaving = false;
                    self.init();
                }
                init() {
                    var sc = this.scope;
                    var self = this;
                    $.ajax({
                        url: '/v1.0/products/types',
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json",
                    })
                        .done(function (data) {
                        sc.types = data;
                        sc.$applyAsync();
                    });
                    $.ajax({
                        url: '/v1.0/storage/storageunits',
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json",
                    })
                        .done(function (data) {
                        sc.storageUnits = data;
                        sc.$applyAsync();
                    });
                    $.ajax({
                        url: '/v1.0/settings/units/',
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json",
                    })
                        .done(function (data) {
                        var dt = new Array();
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
                        .done(function (data) {
                        self.makeCategoryTree(sc, data);
                        sc.$applyAsync();
                    }).then(() => {
                        self.refreshProducts();
                    });
                }
                switchToView(newView) {
                    var sc = this.scope;
                    sc.view = newView;
                    sc.$applyAsync();
                    return false;
                }
                editProduct(prd) {
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
                switchToNew() {
                    var sc = this.scope;
                    var newPrd = {
                        id: null, label: "",
                        genericName: "",
                        images: new Map(),
                        unitId: null, unitExplanation: null,
                        productTypeId: null,
                        categories: new Array(),
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
                        packagings: new Array(),
                        mainCategoryId: null
                    };
                    newPrd.unitId = sc.units.length > 0 ? sc.units[0].id : null;
                    newPrd.productTypeId = sc.types.length > 0 ? sc.types[0].id : null;
                    sc.currentProduct = newPrd;
                    sc.currentProductImages = new Array();
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
                switchBackToMain() {
                    var sc = this.scope;
                    sc.currentProduct = null;
                    sc.currentProductImages = null;
                    sc.view = "products-list";
                    sc.$applyAsync();
                }
                saveProduct() {
                    var sc = this.scope;
                    var self = this;
                    sc.isSaving = true;
                    sc.$applyAsync();
                    var prd = sc.currentProduct;
                    prd.categories = new Array();
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
                        .done(function (data) {
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
                newPackaging(prd) {
                    var pack = {
                        ean: null,
                        label: null,
                        id: null,
                        packQuantity: null,
                        images: new Map(),
                        originId: null,
                        originKind: 0,
                        productPackagingType: 1,
                        standardExpirationDuration: null
                    };
                    prd.packagings.push(pack);
                }
                refreshProducts() {
                    var sc = this.scope;
                    var self = this;
                    if (sc.currentCategory == null || sc.currentCategory == '') {
                        $.ajax({
                            url: '/v1.0/products/find/all?page=' + (sc.currentPage - 1),
                            type: 'GET',
                            dataType: "json",
                            contentType: "application/json",
                        })
                            .done(function (data) {
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
                            .done(function (data) {
                            self.makeProductResult(sc, data);
                            sc.$applyAsync();
                        });
                    }
                }
                isCurrentCategory(catId) {
                    var sc = this.scope;
                    if (sc.currentCategory == null && catId == "") // cas particulier du "tous les articles"
                        return true;
                    return sc.currentCategory == catId;
                }
                isCurrentPage(pageId) {
                    var sc = this.scope;
                    return sc.currentPage == parseInt(pageId);
                }
                setCurrentPage(pageId) {
                    var sc = this.scope;
                    sc.currentPage == parseInt(pageId);
                    sc.$applyAsync();
                    this.refreshProducts();
                }
                selectCategory(catId) {
                    var sc = this.scope;
                    sc.currentPage = 1;
                    if (catId == '')
                        sc.currentCategory = null;
                    else
                        sc.currentCategory = catId;
                    sc.$applyAsync();
                    this.refreshProducts();
                }
                getImage(prd) {
                    if (prd.images == null)
                        return '/me/imgs/empty-40x40.png';
                    var t = prd.images.get("main");
                    if (t == null)
                        return '/me/imgs/empty-40x40.png';
                    if (t.smallImageUrls == null || t.smallImageUrls.length == 0)
                        return '/me/imgs/empty-40x40.png';
                    return t.smallImageUrls[0];
                }
                showProduct(prd) {
                    var sc = this.scope;
                    sc.productInModal = prd;
                }
                makeCategoryTree(sc, data) {
                    sc.categories = new Array();
                    data.forEach((parent, index) => {
                        var childs = data.filter((item, index) => item.parentId != null && item.parentId == parent.id);
                        parent.subcategories = childs;
                        if (parent.parentId == null)
                            sc.categories.push(parent);
                    });
                }
                makeProductResult(sc, data) {
                    sc.products = data.items;
                    if (data.totalResults > 250) {
                        sc.pages = new Array();
                        var i = 0;
                        for (i = 0; i < (data.totalResults / 250); i++) {
                            sc.pages.push((i + 1).toString());
                        }
                        if (data.totalResults % 250 != 0)
                            sc.pages.push((i + 1).toString());
                    }
                    else {
                        sc.pages = new Array();
                    }
                }
            }
            ProductPage.$inject = ["$scope"];
            ProductPage_1.ProductPage = ProductPage;
        })(ProductPage = Me.ProductPage || (Me.ProductPage = {}));
    })(Me = HomeAutomation.Me || (HomeAutomation.Me = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('ProductPageApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('ProductPageController', HomeAutomation.Me.ProductPage.ProductPage);
//# sourceMappingURL=products.js.map