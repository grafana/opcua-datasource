using System;
using System.Collections.Generic;
using System.Text;

namespace plugin_dotnet
{
    internal static class ApplicationConfigurationFactory
    {
        internal static Opc.Ua.ApplicationConfiguration CreateApplicationConfiguration()
        {
            Opc.Ua.CertificateValidator certificateValidator = new Opc.Ua.CertificateValidator();
            certificateValidator.CertificateValidation += (sender, eventArgs) =>
            {
                if (Opc.Ua.ServiceResult.IsGood(eventArgs.Error))
                    eventArgs.Accept = true;
                else if (eventArgs.Error.StatusCode.Code == Opc.Ua.StatusCodes.BadCertificateUntrusted)
                    eventArgs.Accept = true;
                else
                    throw new Exception(string.Format("Failed to validate certificate with error code {0}: {1}", eventArgs.Error.Code, eventArgs.Error.AdditionalInfo));
            };

            Opc.Ua.SecurityConfiguration securityConfigurationcv = new Opc.Ua.SecurityConfiguration
            {
                AutoAcceptUntrustedCertificates = true,
                RejectSHA1SignedCertificates = false,
                MinimumCertificateKeySize = 1024,
            };
            certificateValidator.Update(securityConfigurationcv);

            return new Opc.Ua.ApplicationConfiguration
            {
                ApplicationName = "Grafana",
                ApplicationType = Opc.Ua.ApplicationType.Client,
                CertificateValidator = certificateValidator,
                ServerConfiguration = new Opc.Ua.ServerConfiguration
                {
                    MaxSubscriptionCount = 100000,
                    MaxMessageQueueSize = 1000000,
                    MaxNotificationQueueSize = 1000000,
                    MaxPublishRequestCount = 10000000,
                },

                SecurityConfiguration = new Opc.Ua.SecurityConfiguration
                {
                    AutoAcceptUntrustedCertificates = true,
                    RejectSHA1SignedCertificates = false,
                    MinimumCertificateKeySize = 1024,
                },

                TransportQuotas = new Opc.Ua.TransportQuotas
                {
                    OperationTimeout = 6000000,
                    MaxStringLength = int.MaxValue,
                    MaxByteStringLength = int.MaxValue,
                    MaxArrayLength = 65535,
                    MaxMessageSize = 419430400,
                    MaxBufferSize = 65535,
                    ChannelLifetime = -1,
                    SecurityTokenLifetime = -1
                },
                ClientConfiguration = new Opc.Ua.ClientConfiguration
                {
                    DefaultSessionTimeout = -1,
                    MinSubscriptionLifetime = -1,
                },
                DisableHiResClock = true
            };
        }

    }
}
