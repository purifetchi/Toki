using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Toki.ActivityPub.Persistence.DatabaseContexts;
using Toki.HTTPSignatures;

namespace Toki.Admin.Commands.FixKeys;

public class FixKeysHandler(
    TokiDatabaseContext db)
{
    public async Task Handle(
        FixKeysOptions opts)
    {
        var keys = await db.Keypairs
            .Include(keypair => keypair.Owner)
            .Where(keypair => !keypair.Owner!.IsRemote)
            .ToListAsync();

        foreach (var key in keys)
        {
            using (var rsa = RSA.Create())
            {
                rsa.ImportFromPem(key.PublicKey);
                key.PublicKey = rsa.ExportSubjectPublicKeyInfoPem();
            }
            
            Console.WriteLine($"Verifying the key still works fine...");
            using var rsa2 = RSA.Create();
            using var rsa3 = RSA.Create();
            
            rsa2.ImportFromPem(key.PublicKey);
            rsa3.ImportFromPem(key.PrivateKey);

            var data = Encoding.ASCII.GetBytes("testing");
            
            var signature = rsa3.SignData(
                data,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            var b = rsa2.VerifyData(
                data,
                signature,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            if (!b)
            {
                Console.WriteLine("ERROR WHILE VERIFYING DATA!!!");
                continue;
            }
            
            Console.WriteLine("Verification passed, updating.");

            db.Keypairs.Update(key);
        }

        await db.SaveChangesAsync();
    }
}