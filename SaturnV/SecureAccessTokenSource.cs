using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace SaturnV
{
    public class SecureAccessTokenSource
    {
        private readonly SecureAccessTokenSettings _settings;
        private readonly byte[] _secret;

        public SecureAccessTokenSource(SecureAccessTokenSettings settings)
        {
            if(string.IsNullOrEmpty(settings.Secret))
                throw new ArgumentException($"{nameof(SecureAccessTokenSettings)}.{nameof(SecureAccessTokenSettings.Secret)} cannot be null or empty");
            if (!settings.ValidateTime && !settings.ValidateData)
                throw new ArgumentException(
                    $"{nameof(SecureAccessTokenSettings)}.{nameof(SecureAccessTokenSettings.ValidateTime)} and " +
                    $"{nameof(SecureAccessTokenSettings)}.{nameof(SecureAccessTokenSettings.ValidateData)} are both false, nothing to validate");
            if (settings.ValidateTime && !settings.ValidFor.HasValue)
                throw new ArgumentException($"{nameof(SecureAccessTokenSettings)}.{nameof(SecureAccessTokenSettings.ValidFor)} cannot be null if {nameof(SecureAccessTokenSettings.ValidateTime)} is true");

            _settings = settings;

            _secret = Encoding.UTF8.GetBytes(settings.Secret);
        }

        [PublicAPI]
        public string GetAccessCodeFor([NotNull] byte[] input) => GetAccessCodeFor(input, DateTime.Now);

        private string GetAccessCodeFor([NotNull] byte[] input, DateTime now)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var bytes = Enumerable.Empty<byte>();

            if (_settings.ValidateData)
                bytes = bytes.Concat(GetDataBytes(input));
            if (_settings.ValidateTime)
                bytes = bytes.Concat(GetTimeBytes(now));

            var bytesHash = GetDataBytes(bytes.ToArray());
            return new string(
                    BitConverter.ToUInt32(Truncate(bytesHash), 0)
                        .ToString()
                        .Take(_settings.TokenLength)
                        .ToArray()
                )
                .PadLeft(_settings.TokenLength, '0');
        }


        private static byte[] Truncate(byte[] array)
        {
            if(!array.Any())
                throw new ArgumentException("The hashed bytes array is empty!");

            var offsetArray = new byte[sizeof(int)];
            Array.Copy(array, array.Length - sizeof(int),offsetArray,0,sizeof(int));
            var offset = BitConverter.ToUInt32(offsetArray, 0);

            var selected = new byte[sizeof(int)];
            for (var i = 0; i < selected.Length; ++i)
                selected[i] = array[(offset + i) % array.Length];

            return selected;
        }

        private byte[] GetDataBytes(byte[] data)
        {
            var hmac = new HMACSHA512(_secret);
            return hmac.ComputeHash(data);
        }

        private IEnumerable<byte> GetTimeBytes(DateTime now)
        {
            Debug.Assert(_settings.ValidFor != null, "_settings.ValidFor != null");

            var c = Math.Floor((now.Ticks - (_settings.TimeZero ?? DateTime.MinValue).Ticks)
                                / (double) _settings.ValidFor.Value.Ticks);

            return GetDataBytes(BitConverter.GetBytes(c));
        }

        [PublicAPI]
        public string GetAccessCodeFor(string input) => GetAccessCodeFor(input, DateTime.Now);

        private string GetAccessCodeFor(string input, DateTime now)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException($"{nameof(input)} cannot be null or empty.", nameof(input));

            return GetAccessCodeFor(Encoding.UTF8.GetBytes(input), now);
        }

        [PublicAPI]
        public bool Validate([NotNull] string input, string token)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException($"{nameof(input)} cannot be null or empty.", nameof(input));

            return Validate(Encoding.UTF8.GetBytes(input), token);
        }

        [PublicAPI]
        public bool Validate(byte[] input, string token) => 
            GetAccessCodeFor(input) == token
            || (_settings.EnsureAtLeastValidFor 
            && _settings.ValidateTime 
            && _settings.ValidFor.HasValue 
            && GetAccessCodeFor(input, DateTime.Now - _settings.ValidFor.Value) == token);
    }
}