using LiveShareHub.Core.Abstraction;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace LiveShareHub.Hubs
{
    public class SignalRHub : Hub
    {
        private readonly IGroupIdProvider _groupIdProvider;

        public SignalRHub(IGroupIdProvider groupIdProvider)
        {
            _groupIdProvider = groupIdProvider;
        }

        async public Task JoinGroup(string groupId, string clientId, string clientPassword)
        {
            if (_groupIdProvider.VerifyGroupId(groupId) &&
                _groupIdProvider.VerifyGroupClientPassword(groupId, clientPassword))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
                await Clients.OthersInGroup(groupId).SendAsync("ClientJoinedGroup", groupId, Context.ConnectionId, clientId);
            }
        }

        #region Request Join Group

        async public Task RequestJoinGroup(string groupId, string clientId)
        {
            await Clients.OthersInGroup(groupId).SendAsync("ClientRequestsGroupPassword", groupId, Context.ConnectionId, clientId);
        }

        async public Task SendGroupClientPassword(string groupId, string connectionId, string ownerPassword, string clientPassword)
        {
            if (_groupIdProvider.VerifyGroupOwnerPassword(groupId, ownerPassword))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveGroupClientPassword", groupId, Context.ConnectionId, clientPassword);
            }
        }

        async public Task DenyClientRequestJsonGroup(string groupId, string connectionId)
        {
            await Clients.Client(connectionId).SendAsync("JoinGroupDenied", groupId);
        }

        #endregion

        async public Task LeaveGroup(string groupId, string clientId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
            await Clients.OthersInGroup(groupId).SendAsync("ClientLeftGroup", groupId, Context.ConnectionId, clientId);
        }

        async public Task RemoveClient(string groupId, string connectionId, string clientId, string ownerPassword)
        {
            if (_groupIdProvider.VerifyGroupOwnerPassword(groupId, ownerPassword))
            {
                await Clients.Groups(groupId).SendAsync("ClientLeftGroup", groupId, connectionId, clientId);
                await Groups.RemoveFromGroupAsync(connectionId, groupId);
            }
        }

        async public Task RemoveGroup(string groupId, string ownerPassword)
        {
            if (_groupIdProvider.VerifyGroupOwnerPassword(groupId, ownerPassword))
            {
                await Clients.Groups(groupId).SendAsync("RemoveGroup", groupId);
            }
        }

        async public Task SendClientInfo(string groupId, string clientId, bool isOwner = false, string toConnectionId = "")
        {
            if (!String.IsNullOrEmpty(toConnectionId))
            {
                await Clients.Client(toConnectionId).SendAsync("ReceiveClientInfo", groupId, Context.ConnectionId, clientId, isOwner);
            }
            else
            {
                await Clients.OthersInGroup(groupId).SendAsync("ReceiveClientInfo", groupId, Context.ConnectionId, clientId, isOwner);
            }
        }

        public async Task EmitMessage(string groupId, string messageString)
        {
            await Clients.OthersInGroup(groupId).SendAsync("ReceiveMessage", groupId, Context.ConnectionId, messageString);
        }
    }
}
