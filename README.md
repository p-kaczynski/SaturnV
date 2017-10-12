# SaturnV
Library that provides token generation/validation against a time-window and input data using modified HOTP algorithm

## Usage

Create an instance of `SecureAccessTokenSource` and pass a `SecureAccessTokenSettings` object to it:

```
 public class SecureAccessTokenSettings
    {
        public string Secret { get; [PublicAPI]set; }
        public bool ValidateTime { get; [PublicAPI]set; }
        public TimeSpan? ValidFor { get; [PublicAPI]set; }
        public DateTime? TimeZero { get; [PublicAPI]set; }
        public bool ValidateData { get; [PublicAPI]set; }
        public int TokenLength { get; [PublicAPI]set; } = 8;
    }
```

Then call `GetAccessCodeFor(string input)` or `GetAccessCodeFor(byte[] input)` to retrieve a token for provided text/data, and validate using `Validate(string input, string token)` or `Validate(byte[] input, string token)` methods.

## Settings
* `string Secret` - A secret key used to hash information. This is currently not store in a protected way, but should be kept confidential
* `bool ValidateTime` - Whether to take time passage into consideration when validating a token. Note, that changing that setting _will_ invalidate all existing tokens.
* `TimeSpan? ValidFor` - *Required* if `ValidateTime` is _true_. TimeSpan for which the tokens should be valid
* `DateTime? TimeZero` - _Optional_, determines the agreed time to start counting time from. Defaults to `DateTime.MinValue`.
* `bool ValidateData` - Whether to take the input data into consideration when validating a token. Note, that changing that setting _will_ invalidate all existing tokens.
* `int TokenLength` -  _Optional_, sets the string length of generated numerical token. Defaults to `8`.

Either `ValidateTime` or `ValidateData` must be _true_.
