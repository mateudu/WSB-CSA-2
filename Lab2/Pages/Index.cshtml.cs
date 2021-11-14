using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;

namespace Lab2.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public IList<KeyValuePair<string, string>> Secrets { get; } = new List<KeyValuePair<string, string>>();

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
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
            var client = new SecretClient(new Uri(Environment.GetEnvironmentVariable("KeyVault") ?? throw new InvalidOperationException("KeyVault URI not set in configuration")), new DefaultAzureCredential(), options);

            foreach (var secretProperties in client.GetPropertiesOfSecrets())
            {
                var secret = client.GetSecret(secretProperties.Name);

                Secrets.Add(new KeyValuePair<string, string>(secret.Value.Name, secret.Value.Value));
            }
        }
    }
}
