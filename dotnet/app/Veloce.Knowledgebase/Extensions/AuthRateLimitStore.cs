using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Veloce.Knowledgebase.Extensions;

internal sealed class AuthRateLimitStore(VeloceDbContext dbContext, IConfiguration configuration)
{
    public async Task<bool> AcquireAsync(string scope, string value, int limit, CancellationToken cancellationToken)
    {
        var secret = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");
        var key = Convert.ToHexString(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes($"auth-rate-limit:{scope}:{value}")));

        var affected = await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "AuthRateLimitBuckets" ("Key", "Count", "WindowEndsAtUtc")
            VALUES ({key}, 1, now() + interval '15 minutes')
            ON CONFLICT ("Key") DO UPDATE
            SET "Count" = CASE WHEN "AuthRateLimitBuckets"."WindowEndsAtUtc" <= now() THEN 1 ELSE "AuthRateLimitBuckets"."Count" + 1 END,
                "WindowEndsAtUtc" = CASE WHEN "AuthRateLimitBuckets"."WindowEndsAtUtc" <= now() THEN now() + interval '15 minutes' ELSE "AuthRateLimitBuckets"."WindowEndsAtUtc" END
            WHERE "AuthRateLimitBuckets"."WindowEndsAtUtc" <= now() OR "AuthRateLimitBuckets"."Count" < {limit}
            """, cancellationToken);

        if (Random.Shared.Next(100) == 0)
        {
            await dbContext.AuthRateLimitBuckets
                .Where(bucket => bucket.WindowEndsAtUtc < DateTime.UtcNow.AddHours(-1))
                .ExecuteDeleteAsync(cancellationToken);
        }

        return affected == 1;
    }
}
