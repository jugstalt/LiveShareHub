using System;
using System.Text;
using System.Threading.Tasks;

namespace LiveShareHub.Core.Abstraction
{
    public interface IGroupIdProvider
    {
        string GenerateGroupId();
        string GenerateGroupOwnerPassword(string groupId);
        string GenerateGroupClientPassword(string groupId);

        bool VerifyGroupId(string groupId);
        bool VerifyGroupOwnerPassword(string groupId, string password);
        bool VerifyGroupClientPassword(string groupId, string password);

        Task<string> SimplyGroupId(string groupId);
        Task<string> UnsimplyGroupId(string simpleGroupId);
    }
}
