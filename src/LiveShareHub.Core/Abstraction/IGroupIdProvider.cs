using System;
using System.Text;

namespace LiveShareHub.Core.Abstraction
{
    public interface IGroupIdProvider
    {
        string GenerateGroupId();
        string GenerateGroupOwnerPassword(string groupId);
        string GenerateGroupClientPassword(string groupId);

        bool VerifyGroupOwnerPassword(string groupId, string password);
        bool VerifyGroupClientPassword(string groupId, string password);
    }
}
