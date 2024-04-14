using Microsoft.Extensions.Primitives;
using Toki.HTTPSignatures.Models;

namespace Toki.HTTPSignatures.Tests;

[TestClass]
public class ValidationTests
{
    [TestMethod]
    public void ValidationSucceedesOnAkkomaMessage()
    {
        const string prefKey = "-----BEGIN PUBLIC KEY-----\nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAlBaKcsfkEKNAjhmsmQQO\nD9Z8O2ISoiu80W1moVpbi1O3NiNoXVejPVLN5u/xofxhGs8jzf3Dbf4zM4mRSYzm\nYKV62d0yQpj0MoATiEamXaK5W6g6fQTo66BEQyNYkpY6mMUlcRtOQlHo7A5oqhDy\n84CCyHaqi7XaPUIHD+j29Q4RWLX2qDzI5aCNFO1awFsGI6bz2frc/+qg4sbRgqiw\nLS2ZjYaDeLs+JZe5ZYnyHY+HGP/rmbhMBbbDu9mjCI7cV7q7KcSYl4Oc89lfyb9v\nuLi7zbbJ89LZ8oxLV7EwZBPviZ54PB8mNJmQ2GkzgbmmzuQ5xvI7oB+BoGN3a3yb\nXQIDAQAB\n-----END PUBLIC KEY-----\n\n";

        var headers = new Dictionary<string, StringValues>()
        {
            { "content-length", "1763" },
            { "user-agent", "Akkoma 3.10.4-0-gebfb617; https://miku.place <mikuplace@proton.me>" },
            { "connection", "close" },
            { "content-type", "application/activity+json" },
            { "date", "Mon, 19 Feb 2024 17:56:13 GMT" },
            {
                "signature",
                "keyId=\"https://miku.place/users/pref_test#main-key\",algorithm=\"rsa-sha256\",headers=\"(request-target) content-length date digest host\",signature=\"NxlW4faRXqdz0mX+8Vsrp4RdQpkIPlBNuUUTzk/0eaNfq3M6F+C4CAah5HeRzvhkM1wjbcjJy42C95tqJuVUbA64tNBITqyhzya1Q2IA5iEWlsU+hCo47uOxf9ZfLFSyoT/7hGUEzII/iybulr2MbQYc0vf8W3ASJs2Qbr8nXkMnGurmzcuQkLP1ymTjuUd5mPdCxuQ1tsP6IMhtbN1KDYvW8XjYUjTTrOIcfCmkr5v40H6yebuiH5LYB/nn/0g1bYBDZOnsbdC8/T98AAVQeNtdLPmKNiFLOtJhpralASBdS160J9NtQUbDi2l/dNFbuUMX2WOscEuzuZMlx3qWcA==\""
            },
            { "digest", "SHA-256=SLI2RFzJ01FcQzXPNvxO44jM22VPG3VW7xA/5AiE/vA=" },
            { "host", "snac.frieren.icu" }
        };
        
        var request = new HttpRequest(
            "POST",
            "/shikabane/inbox",
            headers,
            "");

        var signature = Signature.FromHttpRequest(request);
        Assert.IsNotNull(signature);
        
        var validator = new HttpSignatureValidator();
        Assert.IsTrue(validator.Validate(signature, prefKey));
    }
}