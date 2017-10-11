using System;
using JetBrains.Annotations;

namespace SaturnV
{
    public class SecureAccessTokenSettings
    {
        public string Secret { get; [PublicAPI]set; }
        public bool ValidateTime { get; [PublicAPI]set; }
        public TimeSpan? ValidFor { get; [PublicAPI]set; }
        public DateTime? TimeZero { get; [PublicAPI]set; }
        public bool ValidateData { get; [PublicAPI]set; }
        public int TokenLength { get; [PublicAPI]set; } = 8;
    }
}