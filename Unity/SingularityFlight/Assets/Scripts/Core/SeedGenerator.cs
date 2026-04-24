using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// SeedGenerator
/// Purpose: Create stable daily seeds and deterministic random streams for procedural systems.
/// Responsibilities:
/// - Build a seed from UTC date using SHA256.
/// - Provide a deterministic random wrapper.
/// </summary>
public static class SeedGenerator
{
    public static int GenerateSeedFromUtcDate(DateTime utcDate)
    {
        string dateToken = utcDate.ToString("yyyyMMdd");
        byte[] dateBytes = Encoding.UTF8.GetBytes(dateToken);
        using SHA256 sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(dateBytes);

        int rawSeed = BitConverter.ToInt32(hash, 0);
        return rawSeed & int.MaxValue;
    }

    public static int GetDailySeedUtc()
    {
        return GenerateSeedFromUtcDate(DateTime.UtcNow.Date);
    }

    public static DeterministicRandom CreateDeterministicRandom(int seed)
    {
        return new DeterministicRandom(seed);
    }

    public sealed class DeterministicRandom
    {
        private readonly Random random;

        public DeterministicRandom(int seed)
        {
            random = new Random(seed);
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            return random.Next(minInclusive, maxExclusive);
        }

        public float NextFloat(float minInclusive, float maxInclusive)
        {
            double value01 = random.NextDouble();
            return (float)(minInclusive + ((maxInclusive - minInclusive) * value01));
        }
    }
}
