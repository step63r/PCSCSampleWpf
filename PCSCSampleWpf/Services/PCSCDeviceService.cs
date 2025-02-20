using Microsoft.Extensions.Logging;
using MinatoProject.PCSCSampleWpf.Common;
using MinatoProject.PCSCSampleWpf.Services.Interfaces;
using PCSC;
using PCSC.Iso7816;
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
        }

        /// <inheritdoc />
        public ReaderStatus GetStatus(string readerName)
        {
            _logger.LogInformation("start");

            using (var context = _contextFactory.Establish(SCardScope.System))
            {
                using (var reader = context.ConnectReader(readerName, SCardShareMode.Direct, SCardProtocol.Unset))
                {
                    _logger.LogInformation("end");
                    return reader.GetStatus();
                }
            }
        }

        /// <inheritdoc />
        public ResponseApdu GetData(string readerName, ApduInsGetData P1)
        {
            _logger.LogInformation("start");

            using var context = _contextFactory.Establish(SCardScope.System);
            using var reader = context.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any);

            var apdu = new CommandApdu(IsoCase.Case2Short, reader.Protocol)
            {
                CLA = 0xFF,
                Instruction = InstructionCode.GetData,
                P1 = (byte)P1,
                P2 = 0x00,
                Le = 0,
            };

            var responseApdu = SendCommandApdu(reader, apdu);

            _logger.LogInformation("end");
            return responseApdu;
        }

        /// <summary>
        /// APDUコマンド送信用の内部メソッド
        /// </summary>
        /// <param name="reader">カードリーダー</param>
        /// <param name="apdu">APDUコマンドオブジェクト</param>
        /// <returns>APDUレスポンスオブジェクト</returns>
        private ResponseApdu SendCommandApdu(ICardReader reader, CommandApdu apdu)
        {
            _logger.LogInformation("start");

            using (reader.Transaction(SCardReaderDisposition.Leave))
            {
                var sendPci = SCardPCI.GetPci(reader.Protocol);
                var receivePci = new SCardPCI();    // IO returned protocol control information.

                var receiveBuffer = new byte[256];
                var command = apdu.ToArray();

                var bytesReceived = reader.Transmit(
                    sendPci,                // Protocol Control Information (T0, T1 or Raw)
                    command,                // command APDU
                    command.Length,
                    receivePci,             // returning Protocol Control Information
                    receiveBuffer,
                    receiveBuffer.Length);  // data buffer

                var responseApdu =
                    new ResponseApdu(receiveBuffer, bytesReceived, IsoCase.Case2Short, reader.Protocol);

                _logger.LogInformation("end");
                return responseApdu;
            }
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
