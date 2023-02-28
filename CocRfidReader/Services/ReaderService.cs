using Impinj.OctaneSdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CocRfidReader.Services
{
    public class ReaderService
    {
        private ConfigurationService configuration;
        private ILogger<ReaderService>? _logger;
        private ImpinjReader? _reader;

        public ImpinjReader Reader 
        { 
            get 
            {
                if (_reader == null || !_reader.IsConnected)
                {
                    ConnectToReader();
                    ConfigureReader();
                    return _reader;
                }
                
                return _reader != null ? _reader : throw new FieldAccessException();
            } 
        }

        public ReaderService(ConfigurationService configuration, ILogger<ReaderService>? logger = null)
        {
            _logger = logger;
            
            this.configuration = configuration;
        }

        public ImpinjReader Connect()
        {
            if (_reader == null || !_reader.IsConnected)
            {
                ConnectToReader();
                ConfigureReader();
                return _reader;
            }
            return _reader;
        }

        private void ConnectToReader()
        {
            var hostName = configuration.GetSettings().ReaderIP;
            try
            {
                _reader ??= new ImpinjReader();
                _reader.ConnectTimeout = configuration
                    .GetSettings()
                    .ReaderTimeOut
                    .GetValueOrDefault(6000);

                _logger?
                    .LogDebug($"Attempting connection to reader {hostName}");
                _reader.Connect(hostName);
            }
            catch(OctaneSdkException e)
            {
                _logger?.LogError
                    (
                        e, 
                        "Failed to connect to IP:{Address}",
                        new { Address = hostName }
                    );
                throw;
            }
        }

        public void Disconnect()
        {
            _reader?.Disconnect();
        }

        private void ConfigureReader()
        {
            try
            {
                var settings = Reader.QueryDefaultSettings();

                settings.Session = (ushort)configuration.GetSettings().Session.GetValueOrDefault(2);
                settings.SearchMode = SearchMode.SingleTarget;

                settings.AutoStart.Mode = AutoStartMode.None;
                settings.AutoStop.Mode = AutoStopMode.None;

                settings.Keepalives.Enabled = true;
                settings.Keepalives.PeriodInMs = 5000;
                settings.SearchMode = SearchMode.SingleTarget;

                settings.Keepalives.EnableLinkMonitorMode = true;
                settings.Keepalives.LinkDownThreshold = 5;
                var rfMode = (uint)configuration.GetSettings().RfMode.GetValueOrDefault(1000);
                settings.RfMode = rfMode;

                var antennas = configuration.GetSettings().EnabledAntennas.GetValueOrDefault(1);

                for (ushort i = 1; i <= antennas; i++)
                {
                    var antenna = settings.Antennas.GetAntenna(i);
                    antenna.IsEnabled = true;
                    antenna.MaxTxPower = true;
                    antenna.TxPowerInDbm = configuration.GetSettings().GetAntennaPower(i);
                }

                var features = Reader.QueryFeatureSet();

                Reader.ApplySettings(settings);

                Reader.KeepaliveReceived += Reader_KeepaliveReceived;
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Error while attempting to configure the reader.");
                throw;
            }
        }

        private void Reader_KeepaliveReceived(ImpinjReader reader)
        {
            _logger?.LogInformation("Keepalive received from {0}", reader.Address);
        }
    }
}
