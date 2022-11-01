using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;
using Microsoft.Extensions.Configuration;

namespace Lab2.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;
        public IList<KeyValuePair<string, string>> Secrets { get; } = new List<KeyValuePair<string, string>>();

        public IndexModel(IConfiguration configuration, ILogger<IndexModel> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void OnGet()
        {
            var options = new SecretClientOptions()
            {
                Retry =
                {
                    Delay= TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(16),
                    MaxRetries = 5,
                    Mode = RetryMode.Exponential
                }
            };

            var keyVaultUri = _configuration["KeyVault"];
            if (string.IsNullOrWhiteSpace(keyVaultUri))
            {
                throw new ArgumentException("KeyVault URI not set in configuration");
            }

            var client = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential(), options);

            foreach (var secretProperties in client.GetPropertiesOfSecrets())
            {
                var secret = client.GetSecret(secretProperties.Name);

                Secrets.Add(new KeyValuePair<string, string>(secret.Value.Name, secret.Value.Value));
            }
        }
    }
}
