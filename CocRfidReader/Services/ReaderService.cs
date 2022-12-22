using Impinj.OctaneSdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CocRfidReader.Services
{
    public class ReaderService
    {
        private string? _readerhostname;
        private uint _readTime;
        private int _connectTimeout;
        private IConfiguration configuration;
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

        public ReaderService(IConfiguration configuration, ILogger<ReaderService>? logger = null)
        {
            _logger = logger;
            _readerhostname = configuration.GetValue<string>("readerIP");
            _connectTimeout = configuration.GetValue<int>("readerTimeOut");
            _readTime = configuration.GetValue<uint>("readerReadTime");
            this.configuration = configuration;
        }

        private void ConnectToReader()
        {
            try
            {
                _reader ??= new ImpinjReader();

                _reader.ConnectTimeout = _connectTimeout;
                _logger?.LogDebug("Attempting connection to reader {0}", _readerhostname);
                _reader.Connect(_readerhostname);
                
            }
            catch(OctaneSdkException e)
            {
                _logger?.LogError(e, "Failed to connect to IP:{Address}", new { Address = _readerhostname });
                throw;
            }
            
        }

        private void ConfigureReader()
        {
            try
            {
                var settings = Reader.QueryDefaultSettings();

                settings.Session = configuration.GetValue<ushort>("session", 2);
                settings.SearchMode = SearchMode.SingleTarget;

                settings.AutoStart.Mode = AutoStartMode.None;
                settings.AutoStop.Mode = AutoStopMode.None;

                settings.Keepalives.Enabled = true;
                settings.Keepalives.PeriodInMs = 5000;
                settings.SearchMode = SearchMode.SingleTarget;

                settings.Keepalives.EnableLinkMonitorMode = true;
                settings.Keepalives.LinkDownThreshold = 5;
                var rfMode = configuration.GetValue("rfmode", 1000u);
                settings.RfMode = rfMode;

                var antennas = configuration.GetValue("enabledAntennas", 1);

                for (ushort i = 1; i <= antennas; i++)
                {
                    var antenna = settings.Antennas.GetAntenna(i);
                    antenna.IsEnabled = true;
                    antenna.MaxTxPower = true;
                    antenna.TxPowerInDbm = configuration.GetValue($"antena{i}Power", 20.0);
                }

                var features = Reader.QueryFeatureSet();

                Reader.ApplySettings(settings);

                Reader.KeepaliveReceived += Reader_KeepaliveReceived;
                Reader.ConnectionLost += Reader_ConnectionLost;
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Error while attempting to configure the reader.");
                throw;
            }
        }

        private void Reader_ConnectionLost(ImpinjReader reader)
        {
            _logger?.LogCritical("Connection lost : {0}", _readerhostname);
            
            reader.Disconnect();

            ConnectToReader();
        }

        private void Reader_KeepaliveReceived(ImpinjReader reader)
        {
            _logger?.LogInformation("Keepalive received from {0}", reader.Address);
        }
    }
}
