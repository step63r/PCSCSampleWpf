using Microsoft.Extensions.Logging;
using MinatoProject.PCSCSampleWpf.Services.Interfaces;
using PCSC;
using PCSC.Monitoring;

namespace MinatoProject.PCSCSampleWpf.Services
{
    /// <summary>
    /// PCSCデバイスサービス
    /// </summary>
    public sealed class PCSCDeviceService : IPCSCDeviceService
    {
        #region イベントハンドラ
        /// <inheritdoc />
        public event CardInsertedEvent? OnCardInserted;
        /// <inheritdoc />
        public event CardRemovedEvent? OnCardRemoved;
        /// <inheritdoc />
        public event CardInitializedEvent? OnCardInitialized;
        /// <inheritdoc />
        public event StatusChangeEvent? OnStatusChanged;
        /// <inheritdoc />
        public event MonitorExceptionEvent? OnMonitorExceptionRaised;
        #endregion

        #region プロパティ
        /// <inheritdoc />
        public List<string> Readers
        {
            get
            {
                using (var context = _contextFactory.Establish(SCardScope.System))
                {
                    return [.. context.GetReaders()];
                }
            }
        }
        #endregion

        #region メンバ変数
        /// <summary>
        /// ロガー
        /// </summary>
        private readonly ILogger<PCSCDeviceService> _logger;
        /// <summary>
        /// IContextFactory
        /// </summary>
        private readonly IContextFactory _contextFactory = ContextFactory.Instance;
        /// <summary>
        /// ISCardMonitor
        /// </summary>
        private readonly ISCardMonitor _monitor;
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logger">ロガー</param>
        public PCSCDeviceService(ILogger<PCSCDeviceService> logger)
        {
            _logger = logger;

            _monitor = MonitorFactory.Instance.Create(SCardScope.System);
        }
        #endregion

        #region メソッド
        /// <inheritdoc />
        public void StartMonitor(string readerName)
        {
            _logger.LogInformation("start");

            if (!_monitor.Monitoring)
            {
                AttachToAllEvents(_monitor);
                _monitor.Start(readerName);
            }
            else
            {
                _logger.LogWarning("Monitor is already started.");
            }

            _logger.LogInformation("end");
        }

        /// <inheritdoc />
        public void CancelMonitor()
        {
            _logger.LogInformation("start");

            if (_monitor.Monitoring)
            {
                _monitor.Cancel();
                DetachFromAllEvents(_monitor);
            }
            else
            {
                _logger.LogWarning("Monitor is already stopped.");
            }
            
            _logger.LogInformation("end");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_monitor.Monitoring)
            {
                CancelMonitor();
            }
            _logger.LogInformation("end");
        }

        /// <summary>
        /// すべてのイベントにアタッチする
        /// </summary>
        /// <param name="monitor">デバイスモニター</param>
        private void AttachToAllEvents(ISCardMonitor monitor)
        {
            _logger.LogInformation("start");
            _monitor.CardInserted += OnCardInserted;
            _monitor.CardRemoved += OnCardRemoved;
            _monitor.Initialized += OnCardInitialized;
            _monitor.StatusChanged += OnStatusChanged;
            _monitor.MonitorException += OnMonitorExceptionRaised;
            _logger.LogInformation("end");
        }

        /// <summary>
        /// すべてのイベントからデタッチする
        /// </summary>
        /// <param name="monitor">デバイスモニター</param>
        private void DetachFromAllEvents(ISCardMonitor monitor)
        {
            _logger.LogInformation("start");
            _monitor.CardInserted -= OnCardInserted;
            _monitor.CardRemoved -= OnCardRemoved;
            _monitor.Initialized -= OnCardInitialized;
            _monitor.StatusChanged -= OnStatusChanged;
            _monitor.MonitorException -= OnMonitorExceptionRaised;
            _logger.LogInformation("end");
        }
        #endregion
    }
}
