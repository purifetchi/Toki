using Microsoft.Extensions.DependencyInjection;

namespace Toki.HTTPSignatures.Tests;

[TestClass]
public class SigningTests
{
    /// <summary>
    /// The service provider.
    /// </summary>
    private readonly IServiceProvider _provider;

    public SigningTests()
    {
        var services = new ServiceCollection();
        services.AddHttpClient()
            .AddTransient<SignedHttpClient>();

        _provider = services.BuildServiceProvider();
    }
    
    [TestMethod]
    public void SignsProperly()
    {
        const string privateKey = "-----BEGIN PRIVATE KEY-----\nMIIJQgIBADANBgkqhkiG9w0BAQEFAASCCSwwggkoAgEAAoICAQDHfKNCkIys+682\nmEthLKn1/VEqbjcan59hXtWrgyNRk7kP72ol0Qjugmsp1nLut9kjf3msGmM3w0hU\nT481+FG19hHczXd2lJ5ExIzxsfGibKJj1II3nGPs+k3ivcourgoq7AAlJZ/2W2/X\nI/9YuPxLudmbQQJsidwA9FK3bKAjx2owsBHZdf//BCASA1KZzZZ1eGkAuAWRXZe+\nWxRcB61DMTWv40F+EqtgyQMfCJ7Kshicg/kBB3GT0Lf/v13meSlvHw9ilbCs9YzB\nBB+rL0C90dnodXSAbkDg1asbkFrHOIxQgxM2jG5l+e37lssgQFpV/6W8wHYjj2O2\ncJAPRmvkuhertZe9qzhU1pFqXFaGIaE+FEJNeDNNV0Wnl8+UD2Q+7zPVozmboSr9\nj1oT4jQ1Rrzv346rxiH+uB2crdSqnu0KovmTQmJ4LHlOowbWLdnAwKIV6KXFeOUY\n+MdJNO+aFNZFThAhdOh6VqCo3Qc0/L6rqmrG7UyC/b6AECaCOyzrCwXNKjl83wkn\n23iSz6ydSvXf3fa2q569QP0yEltaazXUafI1+OY5/tWwLspQxkujBGGOK2dVRexf\nga5FNx0tJ+I+xrtw27dFszVoIktpmItzOvYoxBqEeGjUCGn+WOn9nY2UJ1PVqs9n\n1uq5F5bAsf9LsAh5hgex4ITGQfs1VwIDAQABAoICACM5SsqSI4TX3BRTx9ucu1YI\nKLDJTUmSLd5PlIL4YOAiqWa6649J3b3foXR9vUWRiPHPrHhLIIHUJY1djvgEpMjw\nbi8CyLlTfK6/1fNHbFc1v2bJO0T+hFZvzBjhUjXz3S+/BDeK8Tfa5WCjpo7P+xyT\n8GyNihQIM6Snf4OL1qr6pzF5pW00fMNWwyLrG0a1GoAbhs7tr23/jK+7/VhSsrFO\nmyA3jHTqbwBj/f0Y1JEN6XKZivo3ikKDdDngXIA5nKtWXK0XqV6g7P2X8M+VRGVA\nDK4TL0MONL5+e/wd/Sl4Pq5otpfHLCctfmwSMEO9mTXeMjGl+jZFN2IbdhR8Zsho\nN5pJfWkgL/Bx++X8oyGCKkWUhgwjWG1TQlJQMJXjNQW4Z3EJfKphVA1I9ePyg7Zd\nl2KMr8Mzj1d3HFkb/W7hthyITBCXqZOqo5j5wsVT1LWGx9xWtwygjAoXhszIBGc7\n5ARlS8ALY/nl5PEZM5rJzLD/VtV4+FQP+BIfOfTH1X7ICOOQVeyTs2w4YSfGT7IK\nzD18FHbuXRh24QMq7+eOWjRH6fQOIy+HfZ4xySVp44vInOski75gqQ9QZEp2XHeu\nNDZx3usPJsi9yOYAPqgNF95e3JgQog43FyG+vkBTDeQaNV3RbnLxNWoMjsAPClBI\nhTHrRn1pKYtuSMZxAp6hAoIBAQD4rVBI3bo6DJUocg77Qr/2ot5tD8WNypz9nEpd\n4YaxJ4nV9fr2YUOR+1u5LYCiRw+gKUYBZGVlmqDSq4zIyF5J53WcbAwI/jUvd+56\nDSHs5e1TLca9xyQ+unpL4/wC76lgsQrdDrUNEmANTrjmvQPflXOlTUHZeDyQ8OZD\nYSvOoNLxxKSDD4Kys+3BtHdrtgd4kKBR8pSDEzbLYnR6GAiHdeJbRoUIc4wrne0J\nYilIMPXcBtjgQxsRh6uncRyB8ErNF+LU1cATBQK6KRjgceEDA3W2JSP1U4gU5x5A\nuTruURl2oKUQ2tUdDQL/NUNMVNLF/KCLn+IWOgXO3EihGnXbAoIBAQDNXH9Tx36V\nhDlmLEbtIZ6VoNiObbc6CXWZpbOJVEzR1Bek8NYkRKhzvqlsn9tcEtL5AqBPNBmH\nJHUGuMKizTrrKpwNLSS9z/fbIVQvFjfHrUUPLrABpag6EDEL5+VsYGkK+FuYjp97\nqFM7c/CIuWxe65F7bfBUvZlVQxGKquEMIcKsJ4F4JKXOiLrEySXBa3jUpU7v4KvJ\nEHZa2RSUvnEqPKhMebH/xf/Lq7wUhRuoBuf/5ixHib+jSHmYJWF1kzipSHE5iyUw\nuBqykv7LhljOKo6ra7jbS77c+RBaDVgMv9hheRnEK+tpjyzJAHxNdiczTNtycny4\nefKv9hR2pR01AoIBAFc+7yjxadHN8lLjWXA75f+Q+rqGywfJKzUrLUgsxMXImmpP\nx8HDNCK2bmLUnQnSJqBJer8oSw7PplbjSxxyd+oz46aCneJV1bEWwbteeiWUMaR6\nLf0NLiE36YpQrW0WSnWbB7Ww4EJ6zOo+UU7ax84csBsxtMWoko9DhXXkVKtE711V\nYWZ9/ZvW1racKz3F+m8JRsYZdpNaHWJT44umfO+Ro13kqu3hJC91U+Fz6Ank641L\ndbGQuaF32PiBDcBk3sl/9Nw5Ng82NdO8cXblXU1iXF0QQMJkkRzMTWfl1NAh8e4V\nxHPaYuSzWPbS6A6Sv/QogucZrAeiScFetbiYAPsCggEBAIOV9jwvguu5CrJuQpAl\nT6EerEQvBw2YDBOEelLPgl2c4f2dGAmrOKjZjWP91ifcI/TMev3lK0pN0PKappR8\nnnkbF1zWBUnenTl61J9LRDiczLhauQqFJBL4VwUC9R2JdSVDWCWblZM9mv9oXcKt\n1UPI+7I0Ep+p0Tsxu2a38XW9mCiJM8tQ6DE7qTj4jkmuXIEc30STGF/APaYenLJm\niMqAjXJbM/Po4euWQfCADeWPb06oJM099V5hzQ/xO86Do/XMZkhx4UWjYe/0gnDU\nx8hqunoajxttIIIk0RZyoG2i67ohR6JMZg6AYDj3J67Wu22CnMZrl4D5GsknC5uu\nVtUCggEAXU+4R3o+9wyRHGcwBjXS0LqYoPGTog8SN5r+XGGu7rGpz1dTmpDhM/hi\nfR97tZvQjoWDd1HAqvaAUMpjjcxSy7vtqevpJcksspXBNB/PBuvUOga7Yn0YQugX\nPrzaI6UEwOS60QCCjxCtJ0qY/rUkzTICr4mimf4CINzJQ2rthJcXTDuHI/AE1pmu\npLqbp78W9pbRx2sqFLjkCAM8mt4RuZCa6XRUPKavgakNTOKNj+qM3QTCg2woJdDU\nmqfgTh9XwfLr9R31C9VsaJPkFxWNggq5oWZ0TBcYcHI5Y+d0V4B3mYStigrsUnto\nwdQTianlq77hVxuN9g6rcMjJjc3OJA==\n-----END PRIVATE KEY-----";
        const string keyId = "https://keys-for-sale.example.com/key#public";
        const string truth = "keyId=\"https://keys-for-sale.example.com/key#public\",algorithm=\"rsa-sha256\",headers=\"(request-target) date digest host\",signature=\"jJ1rJqKLNdT2IQ8CPACE6NRT3mrEn5K/b1Uee04Xt4gWSU/+YNrCfjVXVqLsJAoVkbGFPX6UT4vj4t+O9YAm7qtg6SeCduESt2PE/rRSNTBEXdyTLJo9hyu53/cl6EsKBYC/xVdpQZFrXu4inVzrt+6VEbQ3yjAIZyX5M5I7TW9QFs81jb6NPD+SQLilp2gFCabpgqwzqp4bfL9Gy8jDu08eZRFG7pZ77Ml+FiwtFqwHWzm9BtBVHDK7J0M6JPMwnzELC2ZkoJas6VIUfwzSXRRgVWBMSSK2iwqpp8JEyVsyhJpQXTi12zm7G2717R0DOe3qDg58GITHQO3XxkqtL+h12px8GAiaMxbJfluVb5urzT+hEqPsxN0hzzj9lTebY/CLUfWDJFum6OIlOaMSeF/pVXiybIv2rqCcCxYsiqIjagw+cYSWW+HRbsjypqdiI+WoKnuC3slLWnX/5x5B8ytGM1OSJZ8RepyybQwTdCJ07ufm8TtGRA1hF5nN/f4Tq8JszGiWQVmrr40Tug/jE0Vhf1htf/fN8iut6swSt8hbngqUDhmUfLzn5krh5H/g/3Ik7bZa57xP0ytIUNXZ556HKThq5cJsaEoU3XvAa1kHEQG7qEoRQzeHRmrOl0M9mE0gv+bo5d4DQVPBgjr+8zwVxNmFoSKnBBuzsAWv6hU=\"";
        
        var client = _provider.GetService<SignedHttpClient>();

        var signature = client!
            .WithKey(keyId, privateKey)
            .AddHeaderToSign("Date", "Mon, 19 Feb 2024 17:56:13 GMT")
            .AddHeaderToSign("Digest", "SHA-256=SLI2RFzJ01FcQzXPNvxO44jM22VPG3VW7xA/5AiE/vA=")
            .AddHeaderToSign("Host", "snac.frieren.icu")
            .ConstructSignature("POST", "/shikabane/inbox");
        
        Assert.AreEqual(truth, signature);
    }
}