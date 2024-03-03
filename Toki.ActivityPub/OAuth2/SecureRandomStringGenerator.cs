using System.Security.Cryptography;

namespace Toki.ActivityPub.OAuth2;

/// <summary>
/// A secure random string generator for OAuth2 client secret generation.
/// </summary>
public static class SecureRandomStringGenerator
{
    /// <summary>
    /// The supported alphabet.
    /// </summary>
    private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    /// <summary>
    /// Generates a string.
    /// </summary>
    /// <param name="length">The length of the string.</param>
    /// <returns>Said string.</returns>
    public static string Generate(int length = 64) =>
        RandomNumberGenerator.GetString(ALPHABET, length);
}