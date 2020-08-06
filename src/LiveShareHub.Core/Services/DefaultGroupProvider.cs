using LiveShareHub.Core.Abstraction;
using LiveShareHub.Core.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LiveShareHub.Core.Services
{
    public class DefaultGroupProvider : IGroupIdProvider
    {
        //private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IDistributedCache _distributedCache;
        private readonly DefaultGroupProviderOptions _options;

        public DefaultGroupProvider(
                //IDataProtectionProvider dataProtectionProvider,
                IDistributedCache distributedCache,
                IOptionsMonitor<DefaultGroupProviderOptions> optionsMonitor)
        {
            //_dataProtectionProvider = dataProtectionProvider;
            _distributedCache = distributedCache;
            _options = optionsMonitor.CurrentValue;
        }

        public string GenerateGroupClientPassword(string groupId)
        {
            return StaticEncrypt(Hash(groupId), _options.EncryptionPassword);
        }

        public string GenerateGroupId()
        {
            //return Encrypt(Guid.NewGuid().ToString());

            return Guid.NewGuid().ToString();
        }

        public string GenerateGroupOwnerPassword(string groupId)
        {
            return StaticEncrypt(Hash(Hash(groupId)), _options.EncryptionPassword);
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
                if (String.IsNullOrEmpty(await _distributedCache.GetStringAsync(simpleGroupId.RemoveNonNumericChars())))
                {
                    await _distributedCache.SetStringAsync(simpleGroupId.RemoveNonNumericChars(), groupId);
                    return simpleGroupId;
                }
            }
        }

        async public Task<string> UnsimplyGroupId(string simpleGroupId)
        {
            string groupId = await _distributedCache.GetStringAsync(simpleGroupId.RemoveNonNumericChars());

            if (String.IsNullOrEmpty(groupId))
            {
                throw new ArgumentException("Invalid simpleGroupId");
            }

            return groupId;
        }

        #region Crypto Helper

        //private string Encrypt(string input)
        //{
        //    var protector = _dataProtectionProvider.CreateProtector(_options.EncryptionPassword);
        //    return protector.Protect(input);
        //}

        //private string Decrypt(string cipherText)
        //{
        //    var protector = _dataProtectionProvider.CreateProtector(_options.EncryptionPassword);
        //    return protector.Unprotect(cipherText);
        //}

        private static byte[] _static_iv = new byte[8] { 12, 122, 112, 8, 49, 47, 14, 191 };

        private string StaticEncrypt(string text, string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(StaticPassword24(password));
            byte[] inputbuffer = Encoding.UTF8.GetBytes(StaticPassword24(text));

            SymmetricAlgorithm algorithm = System.Security.Cryptography.TripleDES.Create();
            ICryptoTransform transform = algorithm.CreateEncryptor(passwordBytes, _static_iv);

            List<byte> outputBuffer = new List<byte>(transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length));

            string result = "0x" + string.Concat(outputBuffer.ToArray().Select(b => b.ToString("X2")));

            return result;
        }

        private string StaticDecrypt(string input, string password)
        {
            var inputbuffer = StringToByteArray(input);

            byte[] passwordBytes = Encoding.UTF8.GetBytes(StaticPassword24(password));

            SymmetricAlgorithm algorithm = TripleDES.Create();
            ICryptoTransform transform = algorithm.CreateDecryptor(passwordBytes, _static_iv);
            byte[] bytesDecrypted = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);

            string result = Encoding.UTF8.GetString(bytesDecrypted);

            if (result == "#string.emtpy#")
                return String.Empty;

            return result;
        }

        private string StaticPassword24(string password)
        {
            if (String.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Invalid password");
            }
            while (password.Length < 24)
            {
                password += password;
            }

            return password.Substring(0, 24);
        }

        private byte[] StringToByteArray(String hex)
        {
            if (hex.StartsWith("0x"))
                hex = hex.Substring(2, hex.Length - 2);

            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            return bytes;
        }

        private string Hash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
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
                if (i > 0 && i % 3 == 0)
                    sb.Append("-");

                sb.Append(random.Next(9));
            }

            return sb.ToString();
        }

        #endregion
    }
}
