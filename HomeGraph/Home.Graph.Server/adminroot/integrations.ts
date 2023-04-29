///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />

declare var AdaptiveCards: any;
declare var ACData: any;


module HomeAutomation.Admin.Integrations {


    export interface IIntegrationsPageScope extends ng.IScope {
        view: string;
        events: IntegrationsPage;

        isSaving: boolean;

        installed: Array<InstalledIntegration>;
        inProgress: Array<InstalledIntegration>;
        notInstalled: Array<Integration>;
        all: Array<Integration>;

        editedIntegration: InstalledIntegration;
    }

    export interface Integration {
        id: string;
        agentId: string;
        hidden: boolean;
        label: string;
        image: string;
        description: string;
        category: string;
        canInstallMultipleTimes: boolean;
        instances: Array<IntegrationInstance>;
    }
    export interface IntegrationInstance {
        id: string;
        label: string;
        isSetup: boolean;
    }

    export interface InstalledIntegration {
        id: string;
        agentId: string;
        hidden: boolean;
        label: string;
        instanceId: string;
        instanceLabel: string;
        isSetup: boolean;
    }

    export interface IntegrationConfigurationData {
        integration: Integration,
        currentInstance: IntegrationInstance,
        configurationCardFormat: string;
        configurationCard: string;
        isFinalStep: boolean;
    }

    export class IntegrationsPage {

        connection: signalR.HubConnection;
        scope: IIntegrationsPageScope;
        static $inject = ["$scope"];

        constructor($scope: IIntegrationsPageScope) {
            this.scope = $scope;
            var self = this;

            self.scope.view = "systemIntegrations-list";
            self.scope.events = self;
            self.scope.installed = new Array<InstalledIntegration>();
            self.scope.inProgress = new Array<InstalledIntegration>();
            self.scope.notInstalled = new Array<Integration>();
            self.scope.all = new Array<Integration>();
            self.scope.isSaving = false;
            self.init();
        }



        public switchToView(newView: string): boolean {
            var sc = this.scope;

            sc.view = newView;

            sc.$applyAsync();
            return false;
        }

        public switchToConfigPage(it: InstalledIntegration): boolean {
            var sc = this.scope;
            var self = this;
            sc.view = "config-page";

            sc.editedIntegration = it;

            $.ajax({
                url: '/v1.0/system/mesh/local/integrations/' + it.id + "/config/" + it.instanceId,
                type: 'GET',
                dataType: "json",
                contentType: "application/json"
            })
                .done(function (data: IntegrationConfigurationData) {
                    if (data != null && data.currentInstance != null && data.configurationCard != null) {
                        if (data.configurationCardFormat == null) data.configurationCardFormat = "adaptivecard+json";
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

        public SetupAdaptiveCard(cardData: string, configdata: IntegrationInstance) {

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

            adaptiveCard.parse(cardPayload);
            var renderedCard = adaptiveCard.render();
            var div = document.getElementById("adaptiveCardCanvas");
            while (div.hasChildNodes())
                div.removeChild(div.firstChild);
            div.appendChild(renderedCard);
        }


        public init(): void {
            var sc = this.scope;
            var self = this;

            var withHidden = false;

            $.ajax({
                url: '/v1.0/system/mesh/local/integrations' + (withHidden ?"?includeHidden=true":""),
                type: 'GET',
                dataType: "json",
                contentType: "application/json"
            })
                .done(function (data: Array<Integration>) {
                    self.parseIntegrations(data);
                    sc.$applyAsync();
                })
                .fail(function () {
                });

        }

        parseIntegrations(data: Integration[]) {
            var sc = this.scope;
            var self = this;

            var arr = new Array<InstalledIntegration>();
            var arrEnCours = new Array<InstalledIntegration>();
            var arrNot  = new Array<Integration>();
            for (var i = 0; i < data.length; i++) {
                var int = data[i];
                if (int.instances != null && int.instances.length > 0) {
                    for (var j = 0; j < int.instances.length; j++) {
                        var toAdd: InstalledIntegration =  {
                            id: int.id,
                            agentId:int.agentId,
                            label : int.label,
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


        public saveConfig() {

        }
    }
}

var AuthApp = angular.module('IntegrationsApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('IntegrationsController', HomeAutomation.Admin.Integrations.IntegrationsPage);
