using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Should;
using Xunit;

namespace SaturnV.Tests
{
    public class SecureAccessTokenTests
    {
        private const int TokenLength = 8;

        private static readonly Random Random = new Random();
        private readonly SecureAccessTokenSource _source = new SecureAccessTokenSource(new SecureAccessTokenSettings
        {
            ValidateTime = true,
            ValidateData = true,
            ValidFor = new TimeSpan(1,2,3,4,5),
            Secret = "Lubię placki i długie spacery po plaży.",
            TokenLength = TokenLength
        });

        [Theory]
        [MemberData(nameof(RandomStringProvider), 10)]
        private void GetAndValidateTokens(string input)
        {
            var token = _source.GetAccessCodeFor(input);
            token.ShouldNotBeNull();
            token.Length.ShouldEqual(TokenLength);

            _source.Validate(input, token).ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(RandomStringProvider), 10)]
        private void GetAndValidateTokens_DataChanged(string input)
        {
            var token = _source.GetAccessCodeFor(input);
            token.ShouldNotBeNull();
            token.Length.ShouldEqual(TokenLength);

            input = Mutate(input);
            _source.Validate(input, token).ShouldBeFalse();
        }

        [Theory]
        [MemberData(nameof(RandomStringProvider), 10)]
        private void GetAndValidateTokens_TimeChanged(string input)
        {
            var shortTimeSource = new SecureAccessTokenSource(new SecureAccessTokenSettings
            {
                ValidateTime = true,
                ValidateData = true,
                ValidFor = new TimeSpan(0,0,0,0, 500),
                Secret = "Lubię placki i długie spacery po plaży.",
                TokenLength = TokenLength
            });
            var token = shortTimeSource.GetAccessCodeFor(input);
            token.ShouldNotBeNull();
            token.Length.ShouldEqual(TokenLength);
            // wait for a second
            Thread.Sleep(1000);

            shortTimeSource.Validate(input, token).ShouldBeFalse();
        }

        private static string Mutate(string input)
        {
            var change = Random.Next(1, input.Length);
            var arr = input.ToCharArray();
            for (var i = 0; i < change; ++i)
                arr[Random.Next() % arr.Length] = GetRandomChar();

            return new string(arr);
        }

        public static IEnumerable<object[]> RandomStringProvider(int howMany) =>
            Enumerable.Range(0, howMany).Select(_ => new object[]
                {new string(Enumerable.Range(0, Random.Next(1, 255)).Select(__ => GetRandomChar()).ToArray())});

        private static char GetRandomChar()
        {
            char c;
            while (char.IsControl(c = (char)Random.Next(char.MinValue, char.MaxValue + 1)))
            {
            }
            return c;
        }
    }
}