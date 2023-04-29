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
        var Integrations;
        (function (Integrations) {
            class IntegrationsPage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.view = "systemIntegrations-list";
                    self.scope.events = self;
                    self.scope.installed = new Array();
                    self.scope.inProgress = new Array();
                    self.scope.notInstalled = new Array();
                    self.scope.all = new Array();
                    self.scope.isSaving = false;
                    self.init();
                }
                switchToView(newView) {
                    var sc = this.scope;
                    sc.view = newView;
                    sc.editedIntegration = null;
                    sc.editedIntegrationConfigurationData = null;
                    sc.$applyAsync();
                    return false;
                }
                switchToConfigPage(it) {
                    var sc = this.scope;
                    var self = this;
                    sc.view = "config-page";
                    sc.editedIntegration = it;
                    sc.editedIntegrationConfigurationData = null;
                    $.ajax({
                        url: '/v1.0/system/mesh/local/integrations/' + it.id + "/config/" + it.instanceId,
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json"
                    })
                        .done(function (data) {
                        if (data != null && data.currentInstance != null && data.configurationCard != null) {
                            sc.editedIntegrationConfigurationData = data.currentInstance;
                            if (data.configurationCardFormat == null)
                                data.configurationCardFormat = "adaptivecard+json";
                            switch (data.configurationCardFormat) {
                                case "adaptivecard+json":
                                    self.SetupAdaptiveCard(data.configurationCard, data.currentInstance);
                                    break;
                            }
                        }
                        sc.$applyAsync();
                    })
                        .fail(function () {
                    });
                    sc.$applyAsync();
                    return false;
                }
                SetupAdaptiveCard(cardData, configdata) {
                    var sc = this.scope;
                    var self = this;
                    var template = new ACData.Template(JSON.parse(cardData));
                    var cardPayload = template.expand({
                        $root: configdata
                    });
                    //var cardPayload = JSON.parse(cardData);
                    var adaptiveCard = new AdaptiveCards.AdaptiveCard();
                    adaptiveCard.hostConfig = new AdaptiveCards.HostConfig({
                        fontFamily: "Segoe UI, Helvetica Neue, sans-serif"
                        // More host config options
                    });
                    adaptiveCard.onExecuteAction = (action) => {
                        if (action instanceof AdaptiveCards.SubmitAction) {
                            self.parseActionData(action, sc.editedIntegrationConfigurationData);
                            $.ajax({
                                url: '/v1.0/system/mesh/local/integrations/' + sc.editedIntegration.id + "/config/" + sc.editedIntegration.instanceId,
                                type: 'POST',
                                dataType: "json",
                                contentType: "application/json",
                                data: JSON.stringify(sc.editedIntegrationConfigurationData.settings)
                            })
                                .done(function (data) {
                                if (data != null && data.currentInstance != null && data.configurationCard != null) {
                                    sc.editedIntegrationConfigurationData = data.currentInstance;
                                    if (data.configurationCardFormat == null)
                                        data.configurationCardFormat = "adaptivecard+json";
                                    switch (data.configurationCardFormat) {
                                        case "adaptivecard+json":
                                            self.SetupAdaptiveCard(data.configurationCard, data.currentInstance);
                                            break;
                                    }
                                }
                                $("#divSaveOkConfig").fadeIn();
                                setTimeout(() => { $("#divSaveOkConfig").fadeOut(); }, 2500);
                                sc.$applyAsync();
                            })
                                .fail(function () {
                                $("#divSaveKoConfig").fadeIn();
                                setTimeout(() => { $("#divSaveKoConfig").fadeOut(); }, 2500);
                            });
                        }
                    };
                    adaptiveCard.parse(cardPayload);
                    var renderedCard = adaptiveCard.render();
                    var div = document.getElementById("adaptiveCardCanvas");
                    while (div.hasChildNodes())
                        div.removeChild(div.firstChild);
                    div.appendChild(renderedCard);
                }
                parseActionData(action, editedIntegrationConfigurationData) {
                    if (action.data != null) {
                        for (var k in action.data) {
                            if (k.startsWith("settings_")) {
                                var settName = k.substr(9);
                                editedIntegrationConfigurationData.settings[settName] = action.data[k];
                            }
                        }
                    }
                }
                init() {
                    var sc = this.scope;
                    var self = this;
                    var withHidden = false;
                    $.ajax({
                        url: '/v1.0/system/mesh/local/integrations' + (withHidden ? "?includeHidden=true" : ""),
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json"
                    })
                        .done(function (data) {
                        self.parseIntegrations(data);
                        sc.$applyAsync();
                    })
                        .fail(function () {
                    });
                }
                parseIntegrations(data) {
                    var sc = this.scope;
                    var self = this;
                    var arr = new Array();
                    var arrEnCours = new Array();
                    var arrNot = new Array();
                    for (var i = 0; i < data.length; i++) {
                        var int = data[i];
                        if (int.instances != null && int.instances.length > 0) {
                            for (var j = 0; j < int.instances.length; j++) {
                                var toAdd = {
                                    id: int.id,
                                    agentId: int.agentId,
                                    label: int.label,
                                    hidden: int.hidden,
                                    instanceId: int.instances[j].id,
                                    instanceLabel: int.instances[j].label,
                                    isSetup: int.instances[j].isSetup
                                };
                                if (int.instances[j].isSetup)
                                    arr.push(toAdd);
                                else
                                    arrEnCours.push(toAdd);
                            }
                        }
                        else {
                            arrNot.push(int);
                        }
                    }
                    arr.sort((a, b) => b.label.toLocaleLowerCase().localeCompare(a.label.toLocaleLowerCase()));
                    arrEnCours.sort((a, b) => b.label.toLocaleLowerCase().localeCompare(a.label.toLocaleLowerCase()));
                    arrNot.sort((a, b) => b.label.toLocaleLowerCase().localeCompare(a.label.toLocaleLowerCase()));
                    sc.notInstalled = arrNot;
                    sc.inProgress = arrEnCours;
                    sc.installed = arr;
                    sc.all = data;
                }
                saveConfig() {
                }
            }
            IntegrationsPage.$inject = ["$scope"];
            Integrations.IntegrationsPage = IntegrationsPage;
        })(Integrations = Admin.Integrations || (Admin.Integrations = {}));
    })(Admin = HomeAutomation.Admin || (HomeAutomation.Admin = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('IntegrationsApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('IntegrationsController', HomeAutomation.Admin.Integrations.IntegrationsPage);
//# sourceMappingURL=integrations.js.map