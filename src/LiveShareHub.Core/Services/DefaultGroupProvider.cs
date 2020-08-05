using LiveShareHub.Core.Abstraction;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LiveShareHub.Core.Services
{
    public class DefaultGroupProvider : IGroupIdProvider
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IDistributedCache _distributedCache;
        private readonly DefaultGroupProviderOptions _options;

        public DefaultGroupProvider(
                IDataProtectionProvider dataProtectionProvider,
                IDistributedCache distributedCache,
                IOptionsMonitor<DefaultGroupProviderOptions> optionsMonitor)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _distributedCache = distributedCache;
            _options = optionsMonitor.CurrentValue;
        }

        public string GenerateGroupClientPassword(string groupId)
        {
            return Encrypt(Hash(groupId));
        }

        public string GenerateGroupId()
        {
            //return Encrypt(Guid.NewGuid().ToString());

            return Guid.NewGuid().ToString();
        }

        public string GenerateGroupOwnerPassword(string groupId)
        {
            return Encrypt(Hash(Hash(groupId)));
        }

        public bool VerifyGroupId(string groupId)
        {
            try
            {
                //var guid = new Guid(Decrypt(groupId));
                var guid = new Guid(groupId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyGroupClientPassword(string groupId, string password)
        {
            return GenerateGroupClientPassword(groupId) == password;
        }

        public bool VerifyGroupOwnerPassword(string groupId, string password)
        {
            return GenerateGroupOwnerPassword(groupId) == password;
        }

        async public Task<string> SimplyGroupId(string groupId)
        {
            while (true)
            {
                var simpleGroupId = Simplfy(groupId);
                if (String.IsNullOrEmpty(await _distributedCache.GetStringAsync(simpleGroupId)))
                {
                    await _distributedCache.SetStringAsync(simpleGroupId, groupId);
                    return simpleGroupId;
                }
            }
        }

        async public Task<string> UnsimplyGroupId(string simpleGroupId)
        {
            string groupId = await _distributedCache.GetStringAsync(simpleGroupId);

            if (String.IsNullOrEmpty(groupId))
            {
                throw new ArgumentException("Invalid simpleGroupId");
            }

            return groupId;
        }

        #region Crypto Helper

        private string Encrypt(string input)
        {
            var protector = _dataProtectionProvider.CreateProtector(_options.EncryptionPassword);
            return protector.Protect(input);
        }

        private string Decrypt(string cipherText)
        {
            var protector = _dataProtectionProvider.CreateProtector(_options.EncryptionPassword);
            return protector.Unprotect(cipherText);
        }

        private string Hash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes("hello world"));
                var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                return hash;
            }
        }

        #endregion

        #region Simplify

        private string Simplfy(string groupId)
        {
            var random = new Random();
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 9; i++)
            {
                sb.Append(random.Next(9));
            }

            return sb.ToString();
        }

        #endregion
    }
}
