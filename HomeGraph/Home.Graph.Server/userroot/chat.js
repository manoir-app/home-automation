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
        var ChatPage;
        (function (ChatPage_1) {
            class ChatPage {
                constructor($scope) {
                    this.scope = $scope;
                    var self = this;
                    self.scope.events = self;
                    self.scope.channels = new Array();
                    self.scope.messages = new Map();
                    self.scope.currentChannel = null;
                    self.scope.input = "";
                    self.init();
                }
                clearCurrentChannel() {
                    var sc = this.scope;
                    var self = this;
                    sc.currentChannel = null;
                    sc.currentChatMessages = null;
                    return false;
                }
                sendMessage() {
                    var sc = this.scope;
                    var self = this;
                    if (sc.currentChannel == null)
                        return;
                    var chann = sc.currentChannel;
                    var msg = this.convertToMessage();
                    $.ajax({
                        url: '/v1.0/chat/messages/' + chann.id,
                        type: 'POST',
                        dataType: "json",
                        contentType: "application/json",
                        data: JSON.stringify(msg)
                    })
                        .done(function (message) {
                        chann.lastMessage = message.messageContent;
                        self.updateMessageStruct(message);
                        var tmp = sc.messages.get(chann.id);
                        tmp.push(message);
                        sc.input = "";
                        sc.$applyAsync();
                        self.scrollAfterTimeout();
                    });
                }
                convertToMessage() {
                    var sc = this.scope;
                    var self = this;
                    return sc.input;
                }
                scrollAfterTimeout() {
                    setTimeout(() => {
                        var t = $(".chat-content .chat-body");
                        var size = t.height() + 50;
                        t.scrollTop(size);
                    }, 200);
                }
                setCurrentChannel(c) {
                    var sc = this.scope;
                    var self = this;
                    sc.currentChannel = c;
                    if (c != null) {
                        sc.currentChatMessages = sc.messages.get(c.id);
                    }
                    sc.$applyAsync();
                    self.scrollAfterTimeout();
                    return false;
                }
                init() {
                    var sc = this.scope;
                    var self = this;
                    $.ajax({
                        url: '/v1.0/users/me/identity',
                        type: 'GET',
                        dataType: "json",
                        contentType: "application/json",
                    })
                        .done(function (userId) {
                        sc.myUsername = userId.id;
                        sc.$applyAsync();
                    }).then(() => {
                        $.ajax({
                            url: '/v1.0/users/all',
                            type: 'GET',
                            dataType: "json",
                            contentType: "application/json"
                        })
                            .done(function (data) {
                            sc.allUsers = data;
                            sc.$applyAsync();
                        });
                        $.ajax({
                            url: '/v1.0/chat/channels',
                            type: 'GET',
                            dataType: "json",
                            contentType: "application/json"
                        })
                            .done(function (data) {
                            for (var i = 0; i < data.length; i++) {
                                var found = false;
                                var dt = data[i];
                                for (var j = 0; j < sc.channels.length; j++) {
                                    var insc = sc.channels[j];
                                    if (insc.id == dt.id) {
                                        found: true;
                                    }
                                }
                                if (!found) {
                                    sc.channels.push({
                                        id: dt.id,
                                        name: dt.name,
                                        userIds: dt.userIds,
                                        privacyLevel: dt.privacyLevel,
                                        lastMessage: null,
                                        nbUnread: 0
                                    });
                                    sc.messages.set(dt.id, new Array());
                                }
                            }
                            for (var j = sc.channels.length - 1; j >= 0; j--) {
                                var insc = sc.channels[j];
                                var found = false;
                                for (var i = 0; i < data.length; i++) {
                                    var dt = data[i];
                                    if (insc.id == dt.id) {
                                        found = true;
                                    }
                                }
                                if (!found) {
                                    sc.channels.splice(j, 1);
                                    sc.messages.delete(insc.id);
                                }
                            }
                            self.refreshMessages();
                            sc.$applyAsync();
                        })
                            .fail(function () {
                        });
                    });
                }
                isFromMe(msg) {
                    return msg != null && msg.fromUserId == this.scope.myUsername;
                }
                getImageForUser(userId) {
                    var self = this;
                    var sc = this.scope;
                    if (sc.allUsers == null)
                        return null;
                    for (var i = 0; i < sc.allUsers.length; i++) {
                        var u = sc.allUsers[i];
                        if (u.id == userId && u.avatar != null)
                            return u.avatar.urlSquareSmall;
                    }
                    return null;
                }
                updateMessageStruct(message) {
                    var self = this;
                    if (message == null)
                        return;
                    message.fromMe = self.isFromMe(message);
                }
                refreshMessages() {
                    var sc = this.scope;
                    var self = this;
                    sc.channels.forEach((channel, index) => {
                        $.ajax({
                            url: '/v1.0/chat/messages/' + channel.id,
                            type: 'GET',
                            dataType: "json",
                            contentType: "application/json"
                        }).done(function (data) {
                            data.forEach((message, msgIdx) => { self.updateMessageStruct(message); });
                            if (data != null && data.length > 0)
                                channel.lastMessage = data[data.length - 1].messageContent;
                            sc.messages.set(channel.id, data);
                            if (sc.currentChannel != null && sc.currentChannel.id == channel.id) {
                                sc.currentChatMessages = data;
                            }
                            else {
                            }
                            sc.$applyAsync();
                        });
                    });
                }
            }
            ChatPage.$inject = ["$scope"];
            ChatPage_1.ChatPage = ChatPage;
        })(ChatPage = Me.ChatPage || (Me.ChatPage = {}));
    })(Me = HomeAutomation.Me || (HomeAutomation.Me = {}));
})(HomeAutomation || (HomeAutomation = {}));
var AuthApp = angular.module('ChatPageApp', ['ui.select2', 'ngAnimate']);
AuthApp.controller('ChatPageController', HomeAutomation.Me.ChatPage.ChatPage);
//# sourceMappingURL=chat.js.map