using System.Diagnostics;
using System.Security.Cryptography;

namespace Cart;

/// <summary>
/// Utility functions.
/// </summary>
internal static class Utils
{
    /// <summary>
    /// Returns cryptographic random float between +/-1.
    /// </summary>
    /// <returns></returns>
    internal static double GetRandomValuePlusMinus1()
    {
        byte[] randomBytes = new byte[8];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        ulong randomInt = BitConverter.ToUInt64(randomBytes, 0);

        // large int between  0 and Uint64.MaxValue turned into a float 0..1 -> -0.5..0.5 -> -1..1
        float result = (((float)randomInt / (float)UInt64.MaxValue) - 0.5f) * 2f;

        return result;
    }
}
