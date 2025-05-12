using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Tomlyn.Model;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Runtime.InteropServices;

namespace PluginLoader2Web.Data.Configs
{
    public class WebServerCfgs : ITomlMetadataProvider
    {
        public const string CorsPolicyName = "_allowSpecificOrigins";

        public string BindAddress { get; set; } = "*";

#if DEBUG
        public ushort HttpPort { get; set; } = 8080;
        public ushort HttpsPort { get; set; } = 0;
        public string[] AllowedHosts { get; set; } = ["localhost"];
        public string[] CorsAllowedOrigins { get; set; } = [];
#else
        public ushort HttpPort { get; set; } = 80;
        public ushort HttpsPort { get; set; } = 0;
        public string[] AllowedHosts { get; set; } = ["api.example.com"];
        public string[] CorsAllowedOrigins { get; set; } = [];
#endif

        public bool Hsts { get; set; } = false;
        public string SslCertificateFile { get; set; } = "";
        public string SslCertificateKeyFile { get; set; } = "";
        public string SslCertificatePassword { get; set; } = "";

        public WebServerCfgs() { }

        internal void ApplySettings(WebApplicationBuilder webBuilder)
        {
            if (CorsAllowedOrigins != null && CorsAllowedOrigins.Length > 0)
                webBuilder.Services.AddCors(ApplyCorsSettings);
            else
                Log.Warn("No allowed origins specified, CORS requests will fail");

            IWebHostBuilder webHostBuilder = webBuilder.WebHost.UseKestrel(ApplyKestrelSettings);

            if (AllowedHosts != null && AllowedHosts.Length > 0)
                webHostBuilder.ConfigureServices(ApplyHostnameFilter);
        }




        private void ApplyCorsSettings(CorsOptions options)
        {

            string[] hosts = CorsAllowedOrigins.Select(x =>
            {
                if (x.StartsWith("http:", StringComparison.InvariantCultureIgnoreCase) || x.StartsWith("https:", StringComparison.InvariantCultureIgnoreCase))
                    return x;
                return "https://" + x;
            }).ToArray();

            options.AddPolicy(
                name: CorsPolicyName,
                policy =>
                {
                    policy.WithOrigins(hosts);
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                });
        }

        private void ApplyHostnameFilter(IServiceCollection services)
        {
            services.AddHostFiltering(options => options.AllowedHosts = AllowedHosts);
        }

        private void ApplyKestrelSettings(KestrelServerOptions options)
        {
            if (HttpPort == 0 && HttpsPort == 0)
                throw new Exception("No server ports defined");

            if (BindAddress != null && IPAddress.TryParse(BindAddress, out IPAddress? bindAddress))
            {
                if (HttpPort > 0)
                    options.Listen(bindAddress, HttpPort);

                if (HttpsPort > 0)
                {
                    options.Listen(bindAddress, HttpsPort, options =>
                    {
                        if (string.IsNullOrWhiteSpace(SslCertificateFile))
                        {
                            Log.Warn("Https port specified without certificate file");
                            options.UseHttps();
                        }
                        else
                        {
                            options.UseHttps(ReadCertificate());
                        }
                    });
                }

                return;
            }


            if (HttpPort > 0)
                options.ListenAnyIP(HttpPort);

            if (HttpsPort > 0)
            {
                options.ListenAnyIP(HttpsPort, options =>
                {
                    if (string.IsNullOrWhiteSpace(SslCertificateFile))
                    {
                        Log.Warn("Https port specified without certificate file");
                        options.UseHttps();
                    }
                    else
                    {
                        options.UseHttps(ReadCertificate());
                    }
                });
            }
        }


        private X509Certificate2 ReadCertificate()
        {
            bool password = !string.IsNullOrWhiteSpace(SslCertificatePassword);
            string extension = Path.GetExtension(SslCertificateFile);
            if (string.IsNullOrEmpty(extension))
            {
                if (password)
                    return new X509Certificate2(SslCertificateFile, SslCertificatePassword);
                return new X509Certificate2(SslCertificateFile);
            }

            X509Certificate2 cert;
            switch (extension.ToLowerInvariant())
            {
                case ".pem":
                    string? keyFile = null;
                    if (!string.IsNullOrWhiteSpace(SslCertificateKeyFile))
                        keyFile = SslCertificateKeyFile;
                    if (password)
                        cert = X509Certificate2.CreateFromEncryptedPemFile(SslCertificateFile, SslCertificatePassword, keyFile);
                    else
                        cert = X509Certificate2.CreateFromPemFile(SslCertificateFile, keyFile);
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        cert = new X509Certificate2(cert.Export(X509ContentType.Pfx));
                    break;
                case ".pfx":
                case ".p12":
                    if (password)
                        cert = new X509Certificate2(SslCertificateFile, SslCertificatePassword);
                    else
                        cert = new X509Certificate2(SslCertificateFile);
                    break;
                default:
                    throw new IOException("SSL certificate format not supported.");
            }

            if (!cert.Verify())
                Log.Warn("Provided SSL certificate could not be verified");
            return cert;
        }


        [IgnoreDataMember]
        public TomlPropertiesMetadata? PropertiesMetadata { get; set; }
    }
}