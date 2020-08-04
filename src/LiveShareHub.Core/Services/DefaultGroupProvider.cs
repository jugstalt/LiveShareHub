using LiveShareHub.Core.Abstraction;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LiveShareHub.Core.Services
{
    public class DefaultGroupProvider : IGroupIdProvider
    {
        private readonly DefaultGroupProviderOptions _options;

        public DefaultGroupProvider(IOptionsMonitor<DefaultGroupProviderOptions> optionsMonitor)
        {
            _options = optionsMonitor.CurrentValue;
        }

        public string GenerateGroupClientPassword(string groupId)
        {
            return groupId;
        }

        public string GenerateGroupId()
        {
            return Guid.NewGuid().ToString();
        }

        public string GenerateGroupOwnerPassword(string groupId)
        {
            return groupId;
        }

        public bool VerifyGroupClientPassword(string groupId, string password)
        {
            return groupId == password;
        }

        public bool VerifyGroupOwnerPassword(string groupId, string password)
        {
            return groupId == password;
        }
    }
}
