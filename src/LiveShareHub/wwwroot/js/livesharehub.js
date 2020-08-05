﻿var livesharehub = new function () {
    "use strict"

    var src = document.getElementById('livesharehub-script') ?
        document.getElementById('livesharehub-script').src.toLowerCase() :
        null;
    src = src ? src.substring(0, src.lastIndexOf('/js/')) : '';

    this.baseUrl = src;
    this.signalRHubUrl = function () {
        return this.baseUrl + '/signalrhub'
    };

    var _const = {
        onReceiveMessage: "ReceiveMessage",
        onClientJoinedGroup: "ClientJoinedGroup",
        onClientLeftGroup: "ClientLeftGroup",
        onReceiveClientInfo: "ReceiveClientInfo",
        onClientRequestsGroupPassword: "ClientRequestsGroupPassword",
        onReceiveGroupClientPassword: "ReceiveGroupClientPassword",
        onJoinGroupDenied: "JoinGroupDenied"
    };

    var _currentGroupIndex = -1;
    var _groups = [];
    var _clients = [];

    this.getGroupId = function () {
        if (_currentGroupIndex < 0 || _currentGroupIndex >= _groups.length)
            return null;

        return _groups[_currentGroupIndex].groupId;
    };
    this.addGroup = function (group) {
        if (group && group.groupId && !_getGroup(group.groupId)) {
            _groups.push(group);
            _currentGroupIndex = _groups.length - 1;
            return true;
        }
        return false;
    };
    this.isOwner = function (groupId) {
        var group = _getGroup(groupId);
        return (group != null && typeof group.groupOwnerPassword === 'string');
    };
    var _setGroupId = function (groupId) {
        for (var i in _groups) {
            if (_groups[i].groupId == groupId)
                return i;
        }

        _groups.push({ groupId: groupId });
        _currentGroupIndex = _groups.length - 1;
    };
    var _getGroup = function (groupId) {
        for (var i in _groups) {
            if (_groups[i].groupId == groupId)
                return _groups[i];
        }

        return null;
    }

    this.requestGroup = function (callback) {
        var xhttp = new XMLHttpRequest();
        xhttp.onreadystatechange = function () {
            if (this.readyState == 4 && this.status == 200) {
                livesharehub.addGroup(JSON.parse(this.responseText));

                callback();
            }
        };
        xhttp.open("GET", this.baseUrl + '/hubgroup', true);
        xhttp.send();
    };

    var _clientKey = function (groupId, connectionId, clientId) { return groupId + ":" + connectionId + ":" + clientId };
    var _clientIdFromConnectionId = function (connectionId) {
        for (var c in _clients) {
            if (_clients[c].connectionId === connectionId) {
                return _clients[c].clientId;
            }
        }
    }
    var _connection;
    var _options;

    this.initHubClient = function (connection, options) {
        _connection = connection;
        _options = options;

        if (options.onReceiveMessage) {
            _connection.on(_const.onReceiveMessage, function (groupId, connectionId, message) {
                options.onReceiveMessage({
                    groupId: groupId,
                    connectoinId: connectionId,
                    message: message,
                    clientId: _clientIdFromConnectionId(connectionId)
                });
            });
        }
        if (options.onClientJoinedGroup) {
            _connection.on(_const.onClientJoinedGroup, function (groupId, connectionId, clientId) {
                if (!_clients[_clientKey(groupId, connectionId, clientId)]) {
                    var client = {
                        groupId: groupId,
                        connectionId: connectionId,
                        clientId: clientId
                    };
                    _clients[_clientKey(groupId, connectionId, clientId)] = client;

                    var currentClientId = typeof options.clientId === "string" ?
                        options.clientId :
                        options.clientId();

                    console.log('my-clientid', currentClientId);

                    _connection.invoke("SendClientInfo", groupId, currentClientId, false, connectionId)
                        .then(function () {
                            console.log("sdkfjasölfaslöf");
                        })
                        .catch(function (err) {
                            return console.error(err.toString());
                        });

                    options.onClientJoinedGroup(client);
                }
            });
        }
        if (options.onClientLeftGroup) {
            _connection.on(_const.onClientLeftGroup, function (groupId, connectionId, clientId) {
                if (_clients[_clientKey(groupId, connectionId, clientId)]) {
                    var client = _clients[_clientKey(groupId, connectionId, clientId)];

                    options.onClientLeftGroup(client);

                    _clients[_clientKey(groupId, connectionId, clientId)] = null;
                }
            });
        }
        if (options.onReceiveClientInfo) {
            _connection.on(_const.onReceiveClientInfo, function (groupId, connectionId, clientId, isOwner) {
                if (!_clients[_clientKey(groupId, connectionId, clientId)]) {
                    var client = {
                        groupId: groupId,
                        connectionId: connectionId,
                        clientId: clientId
                    };
                    _clients[_clientKey(groupId, connectionId, clientId)] = client;

                    options.onReceiveClientInfo(client);
                }
            });
        }

        _connection.on(_const.onClientRequestsGroupPassword, function (groupId, connectionId, clientId) {
            var group = _getGroup(groupId);
            if (group && group.groupOwnerPassword && group.groupClientPassword) {
                if (options.onConfirmJoinGroup) {
                    options.onConfirmJoinGroup({
                        groupId: groupId,
                        connectionId: connectionId,
                        clientId: clientId
                    },
                        function () {
                            _connection.invoke("SendGroupClientPassword", groupId, connectionId, group.groupOwnerPassword, group.groupClientPassword);
                        },
                        function () {
                            _connection.invoke("DenyClientRequestJoinGroup", groupId, connectionId);
                            if (options.onDeniedGroup)
                                options.onDeniedGroup({ groupId: groupId });
                        });
                } else {
                    _connection.invoke("SendClientGroupPassword", groupId, connectionId, group.groupOwnerPassword, group.groupClientPassword);
                }
            }
        });

        _connection.on(_const.onReceiveGroupClientPassword, function (groupId, connectionId, clientPassword) {
            _join(groupId, options.clientId(), clientPassword, function () {
                //if (options.onJoinedGroup)
                //    options.onJoinedGroup({
                //        groupId: groupId
                //    });
            });
        });
    };

    this.emitMessage = function (message, onEmited) {
        if (_connection) {
            _connection.invoke("EmitMessage", this.getGroupId(), message)
                .then(function () {
                    if (onEmited) {
                        onEmited(message);
                    }
                })
                .catch(function (err) {
                    return console.error(err.toString());
                });
        }
    };

    this.requestJoin = function (groupId, clientId) {
        if (_connection) {
            var group = _getGroup(groupId);
            if (group && group.groupClientPassword) {
                _join(groupId, clientId, group.groupClientPassword);
            } else {
                _connection.invoke("RequestJoinGroup", groupId, clientId);
            }
        }
    };

    var _join = function (groupId, clientId, clientPassword) {
        if (_connection) {
            _connection.invoke("JoinGroup", groupId, clientId, clientPassword)
                .then(function () {
                    _setGroupId(groupId);
                    if (_options.onJoinedGroup)
                        _options.onJoinedGroup({ groupId: groupId });
                })
                .catch(function (err) {
                    return console.error(err.toString());
                });
        }
    };

    this.leave = function (groupId, clientId, onLeft) {
        if (_connection) {
            _connection.invoke("LeaveGroup", groupId, clientId)
                .then(function () {

                    var groups = [];
                    _currentGroupIndex = -1;

                    for (var i in _groups) {
                        if (_groups[i].groupId !== groupId) {
                            groups.push(_goups[i]);
                        }
                    }
                    _groups = groups;

                    var clients = [];

                    for (var c in _clients) {
                        if (c.indexOf(groupId + ":") != null) {
                            clients.push(_clients[c]);
                        }
                    }

                    _clients = clients;

                    if (onLeft) {
                        onLeft();
                    }
                })
                .catch(function (err) {
                    return console.error(err.toString());
                });
        }
    };
};