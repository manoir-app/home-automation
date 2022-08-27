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
        var Agents;
        (function (Agents) {
            class AgentsPage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.view = "agents-list";
                    self.scope.events = self;
                    self.scope.agents = new Array();
                    self.scope.currentConfig = null;
                    self.scope.currentAgent = null;
                    self.init();
                }
                restart(ag) {
                    $.ajax({
                        url: '/v1.0/agents/' + ag.agentName.toLocaleLowerCase() + '/restart',
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json"
                    })
                        .done(function (result) {
                    });
                }
                switchToView(newView) {
                    var sc = this.scope;
                    sc.view = newView;
                    sc.$applyAsync();
                    return false;
                }
                saveConfig(agent, config) {
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
                switchToConfigPage(ag) {
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
                init() {
                    var sc = this.scope;
                    var self = this;
                    this.connection = new signalR.HubConnectionBuilder()
                        .withUrl("/hubs/1.0/admin")
                        .withAutomaticReconnect()
                        .build();
                    this.connection.on("agentStatusChanged", (agent, status) => {
                        for (var ag of sc.agents) {
                            if (ag.agentName == agent) {
                                ag.kind = status.kind;
                                ag.messageDate = status.messageDate;
                                ag.message = status.message;
                                sc.$applyAsync();
                                return;
                            }
                        }
                        var ag = {
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
                        .done(function (data) {
                        sc.agents = new Array();
                        for (var a of data) {
                            var ag = {
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
            AgentsPage.$inject = ["$scope"];
            Agents.AgentsPage = AgentsPage;
        })(Agents = Admin.Agents || (Admin.Agents = {}));
    })(Admin = HomeAutomation.Admin || (HomeAutomation.Admin = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('AgentApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('AgentsController', HomeAutomation.Admin.Agents.AgentsPage);
//# sourceMappingURL=agents.js.map