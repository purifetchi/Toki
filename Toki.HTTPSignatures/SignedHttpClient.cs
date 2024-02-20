using System.Security.Cryptography;
using System.Text;

namespace Toki.HTTPSignatures;

/// <summary>
/// A signed HTTP client.
/// </summary>
/// <param name="httpClientFactory">The HTTP client factory.</param>
public class SignedHttpClient(IHttpClientFactory httpClientFactory)
{
    /// <summary>
    /// The client we're operating on.
    /// </summary>
    private readonly HttpClient _client = httpClientFactory.CreateClient();

    /// <summary>
    /// The request message to be sent.
    /// </summary>
    private readonly HttpRequestMessage _requestMessage = new();

    /// <summary>
    /// The list of headers to sign.
    /// </summary>
    private readonly List<string> _signedHeaders = new();

    /// <summary>
    /// The PEM of the public key.
    /// </summary>
    private string? _privatePem;

    /// <summary>
    /// The key's id.
    /// </summary>
    private string? _id;

    /// <summary>
    /// The body of the message.
    /// </summary>
    private string? _body;
    
    /// <summary>
    /// Adds a header to sign.
    /// </summary>
    /// <param name="headerName">The name of the header.</param>
    /// <param name="headerValue">The value of the header.</param>
    /// <returns>Ourselves.</returns>
    public SignedHttpClient AddHeaderToSign(string headerName, string headerValue)
    {
        _requestMessage.Headers.Add(headerName, headerValue);
        _signedHeaders.Add(headerName.ToLowerInvariant());
        return this;
    }
    
    /// <summary>
    /// Adds a header to sign.
    /// </summary>
    /// <param name="headerName">The name of the header.</param>
    /// <returns>Ourselves.</returns>
    public SignedHttpClient AddHeaderToSign(string headerName)
    {
        _signedHeaders.Add(headerName.ToLowerInvariant());
        return this;
    }

    /// <summary>
    /// Sets a key for this signed request.
    /// </summary>
    /// <param name="id">The ID.</param>
    /// <param name="pem">The private PEM.</param>
    /// <returns>Ourselves.</returns>
    public SignedHttpClient WithKey(string id, string pem)
    {
        _id = id;
        _privatePem = pem;

        return this;
    }

    /// <summary>
    /// The body of the message.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <returns>Ourselves.</returns>
    public SignedHttpClient WithBody(string body)
    {
        _body = body;
        return this;
    }

    /// <summary>
    /// POSTs an url.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <returns>The response.</returns>
    public Task<HttpResponseMessage> Post(string url)
    {
        return Send(url, HttpMethod.Post);
    }
    
    /// <summary>
    /// GETs an url.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <returns>The response.</returns>
    public Task<HttpResponseMessage> Get(string url)
    {
        return Send(url, HttpMethod.Get);
    }

    /// <summary>
    /// Sends the message.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <param name="method">The method.</param>
    /// <returns>The response.</returns>
    private async Task<HttpResponseMessage> Send(string url, HttpMethod method)
    {
        if (_body is not null)
        {
            // If we also want to sign the digest, add it to the headers list.
            if (_signedHeaders.Contains("digest"))
            {
                _requestMessage.Headers.Add("Digest",
                    DigestMessage());
            }
            
            _requestMessage.Content = new StringContent(_body);
        }
        
        _requestMessage.RequestUri = new(url);
        _requestMessage.Method = method;
        
        // Set up some already known headers.
        _requestMessage.Headers.Host = _requestMessage.RequestUri.Host;
        _requestMessage.Headers.Add("Signature", 
            ConstructSignature(method.ToString(), _requestMessage.RequestUri.AbsolutePath));
        
        return await _client.SendAsync(_requestMessage);
    }

    /// <summary>
    /// Constructs a signature.
    /// </summary>
    /// <returns>The signature.</returns>
    public string ConstructSignature(string method, string path)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(_privatePem);
        
        _signedHeaders.Insert(0, "(request-target)");
        var message = _signedHeaders.Aggregate(
            string.Empty, (acc, header) => header switch
            {
                "(request-target)" => acc + $"(request-target): {method.ToLowerInvariant()} {path}\n",
                _ => acc + $"{header}: {_requestMessage.Headers.GetValues(header).First()}\n"
            }).TrimEnd();
        
        var signature = rsa.SignData(
            Encoding.UTF8.GetBytes(message),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
                    
        return $"""
                keyId="{_id}",algorithm="rsa-sha256",headers="{string.Join(' ', _signedHeaders)}",signature="{Convert.ToBase64String(signature)}"
                """;
    }

    /// <summary>
    /// Digests a message.
    /// </summary>
    /// <returns>The message.</returns>
    public string DigestMessage()
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(_body!));
        return $"SHA-256={Convert.ToBase64String(hash)}";
    }
}