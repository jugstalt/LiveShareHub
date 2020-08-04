"use strict";

var connection = new signalR.HubConnectionBuilder()
    .withUrl(livesharehub.signalRHubUrl())
    .build();

var hubGroup = null;

livesharehub.initHubClient(
    connection,
    {
        onReceiveMessage: function (result) {
            $("<p>")
                .html(result.clientId + ":<br/><code>" + result.message + "<code>")
                .appendTo("#receivedMessages")
        },
        onClientJoinedGroup: function (result) {
            console.log('onClientJoinedGroup', result);
            $("<li>")
                .addClass(result.connectionId)
                .text(result.clientId)
                .appendTo($("#connectedUsers"));
        },
        onClientLeftGroup: function (result) {
            $("li." + result.connectionId).remove();
        },
        onReceiveClientInfo: function (result) {
            console.log('onReceiveClientInfo', result);
            $("<li>")
                .addClass(result.connectionId)
                .text(result.clientId)
                .appendTo($("#connectedUsers"));
        },
        onJoinedGroup: function (result) {
            $("#groupIdText").val(livesharehub.getGroupId());
            $("#joinGroupButton").css('display', 'none');
            $("#leaveGroupButton").css('display', 'block');
        },
        onDeniedGroup: function (result) {

        },
        onConfirmJoinGroup(result, onAllow, onDeny) {
            if (confirm("Client " + result.cliendId + " zutritt erteilen?")) {
                onAllow();
            } else {
                onDeny();
            }
        },
        clientId: function () {
            return $("#userNameText").val();
        }
    }
);

connection.start().then(function () {
    console.log('connection started...');
}).catch(function (err) {
    return console.error(err.toString());
});

$("#emitButton").click(function (event) {
    var message = document.getElementById("jsonMessage").value;
    livesharehub.emitMessage(message, function (emitedMessage) {
        $("<p>")
            .html("me:<br/><code>" + emitedMessage + "<code>")
            .appendTo("#receivedMessages")
    });
});

$("#requestGroupButton").click(function (event) {
    livesharehub.requestGroup(function () {
        console.log('groupId', livesharehub.getGroupId());
        livesharehub.requestJoin(livesharehub.getGroupId(), $("#userNameText").val());
    });
});

$("#joinGroupButton").click(function (event) {
    if (!$('#groupIdText').val())
        return;

    livesharehub.requestJoin($('#groupIdText').val(), $('#userNameText').val());
});

$("#leaveGroupButton").click(function (event) {

    if (livesharehub.getGroupId()) {
        livesharehub.leave(livesharehub.getGroupId(), $("#userNameText").val(), function () {
            $("#joinGroupButton").css('display', 'block');
            $("#leaveGroupButton").css('display', 'none');
            $("#groupIdText").val("");
        });
    }
});



