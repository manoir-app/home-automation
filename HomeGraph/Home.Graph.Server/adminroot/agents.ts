///<reference path="../wwwroot/scripts/typings/jquery.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-animate.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-route.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular-cookies.d.ts" />
///<reference path="../wwwroot/scripts/typings/angularjs/angular.d.ts" />
///<reference path="../node_modules/@microsoft/signalr/dist/esm/index.d.ts" />


module HomeAutomation.Admin.Agents {

    export interface AgentStatus {
        agentName: string;
        kind: number;
        messageDate: Date;
        message: string;
        roles: string[];
        canRestart: boolean;
    }

    export interface CurrentStatus {
        kind: number;
        messageDate: Date;
        message: string;
        format: number;
        options?: any;
    }

    export interface Agent {
        id: string;
        agentName: string;
        agentMesh: string;
        agentMachineName: string;
        roles: string[];
        lastPing: Date;
        currentStatus: CurrentStatus;
    }

    export interface IAgentsPageScope extends ng.IScope {
        view: string;
        events: AgentsPage;
        agents: AgentStatus[];

        currentAgent: string;
        currentConfig: string;
    }


    export class AgentsPage {

        connection: signalR.HubConnection;
        scope: IAgentsPageScope;
        static $inject = ["$scope"];

        constructor($scope: IAgentsPageScope) {
            this.scope = $scope;
            var self = this;

            self.scope.view = "agents-list";
            self.scope.events = self;
            self.scope.agents = new Array<AgentStatus>();
            self.scope.currentConfig = null;
            self.scope.currentAgent = null;
            self.init();
        }

        public restart(ag: Agent) {
            $.ajax({
                url: '/v1.0/agents/' + ag.agentName.toLocaleLowerCase() + '/restart',
                type: 'GET',
                dataType: "json",
                contentType: "application/json"
            })
                .done(function (result: boolean) {

                });
        }

        public switchToView(newView: string): boolean {
            var sc = this.scope;

            sc.view = newView;

            sc.$applyAsync();
            return false;
        }

        public saveConfig(agent: string, config:string) {
            var sc = this.scope;
            var self = this;

            $.ajax({
                url: '/v1.0/agents/' + agent + "/configuration",
                type: 'POST',
                dataType: "json",
                contentType: "application/json",
                data: JSON.stringify(config)
            })
                .done(function (data) {
                    sc.view = "agents-list";
                    sc.currentAgent = null;
                    sc.currentConfig = "";
                    sc.$applyAsync();
                });


            return false;
        }

        public switchToConfigPage(ag: AgentStatus): boolean {
            var sc = this.scope;
            var self = this;

            sc.view = "agents-config";
            sc.currentAgent = ag.agentName;
            sc.currentConfig = "";
            sc.$applyAsync();

            $.ajax({
                url: '/v1.0/agents/' + ag.agentName,
                type: 'GET',
                dataType: "json",
                contentType: "application/json"
            })
            .done(function (data) {
                sc.currentConfig = data.configurationData;
                if (sc.currentConfig == undefined || sc.currentConfig == null)
                    sc.currentConfig = "";
                sc.$applyAsync();
            });

            return false;
        }

        public init(): void {
            var sc = this.scope;
            var self = this;

            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/1.0/admin")
                .withAutomaticReconnect()
                .build();

            this.connection.on("agentStatusChanged", (agent: string, status: AgentStatus) => {
                for (var ag of sc.agents) {
                    if (ag.agentName == agent) {
                        ag.kind = status.kind;
                        ag.messageDate = status.messageDate;
                        ag.message = status.message;
                        sc.$applyAsync();
                        return;
                    }
                }

                var ag: AgentStatus = {
                    agentName: agent,
                    kind: status.kind,
                    message: status.message,
                    messageDate: status.messageDate,
                    roles: [],
                    canRestart: true
                };
                if (ag.agentName.toLocaleLowerCase() == "gaia")
                    ag.canRestart = false;
                sc.agents.push(ag);
                sc.$applyAsync();
            });

            $.ajax({
                url: '/v1.0/system/mesh/local/agents/',
                type: 'GET',
                dataType: "json",
                contentType: "application/json"
            })
                .done(function (data: Agent[]) {
                    sc.agents = new Array<AgentStatus>();
                    for (var a of data) {
                        var ag: AgentStatus = {
                            agentName: a.agentName,
                            kind: a.currentStatus.kind,
                            message: a.currentStatus.message,
                            messageDate: a.currentStatus.messageDate,
                            roles: a.roles,
                            canRestart: true
                        };
                        if (ag.agentName.toLocaleLowerCase() == "gaia")
                            ag.canRestart = false;
                        sc.agents.push(ag);
                    }
                    sc.$applyAsync();
                })
                .fail(function () {
                });

            this.connection.start().catch(err => document.write(err));
        }
    }
}

var AuthApp = angular.module('AgentApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('AgentsController', HomeAutomation.Admin.Agents.AgentsPage);
