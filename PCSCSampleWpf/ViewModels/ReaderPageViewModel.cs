using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MinatoProject.PCSCSampleWpf.Services.Interfaces;
using PCSC.Monitoring;
using System.Numerics;
using System.Windows.Threading;

namespace MinatoProject.PCSCSampleWpf.ViewModels
{
    /// <summary>
    /// ReaderPage.xamlのViewModelクラス
    /// </summary>
    public partial class ReaderPageViewModel : ViewModel
    {
        #region プロパティ
        /// <summary>
        /// ページタイトル
        /// </summary>
        [ObservableProperty]
        private string pageTitle = "Reader";

        /// <summary>
        /// デバイス一覧
        /// </summary>
        [ObservableProperty]
        private List<string> readers = [];

        /// <summary>
        /// 選択されたデバイス
        /// </summary>
        [ObservableProperty]
        private string selectedReader = string.Empty;

        /// <summary>
        /// 処理結果の文字列
        /// </summary>
        [ObservableProperty]
        private string resultText = string.Empty;

        /// <summary>
        /// 読取中フラグ
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ReadCardCommand))]
        [NotifyCanExecuteChangedFor(nameof(StopReadCardCommand))]
        private bool isReading = false;
        #endregion

        #region メンバ変数
        /// <summary>
        /// ロガー
        /// </summary>
        private readonly ILogger<MainWindowViewModel> _logger;
        /// <summary>
        /// デバイスサービス
        /// </summary>
        private readonly IPCSCDeviceService _deviceService;
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logger">ロガー</param>
        /// <param name="deviceService">デバイスサービス</param>
        public ReaderPageViewModel(ILogger<MainWindowViewModel> logger, IPCSCDeviceService deviceService)
        {
            _logger = logger;
            _deviceService = deviceService;

            Readers = _deviceService.Readers;
            if (Readers.Count > 0)
            {
                SelectedReader = Readers.First();
            }

            AttachEvents();
        }
        #endregion

        #region コマンド
        [RelayCommand(CanExecute = nameof(CanReadCard))]
        private void ReadCard()
        {
            _logger.LogInformation("start");

            try
            {
                _deviceService.StartMonitor(SelectedReader);

                ResultText = string.Empty;
                IsReading = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
                ResultText = $"{ex.Message}\n-----\n{ex.StackTrace}";
            }

            //try
            //{
            //    using (var context = _contextFactory.Establish(SCardScope.System))
            //    {
            //        using (var reader = context.ConnectReader(SelectedReader, SCardShareMode.Shared, SCardProtocol.Any))
            //        {
            //            var apdu = new CommandApdu(IsoCase.Case2Short, reader.Protocol)
            //            {
            //                CLA = 0xFF,
            //                Instruction = InstructionCode.GetData,
            //                P1 = 0x00,
            //                P2 = 0x00,
            //                Le = 0      // We don't know the ID tag size
            //            };

            //            using (reader.Transaction(SCardReaderDisposition.Leave))
            //            {
            //                var sendPci = SCardPCI.GetPci(reader.Protocol);
            //                var receivePci = new SCardPCI();    // IO returned protocol control information.

            //                var receiveBuffer = new byte[256];
            //                var command = apdu.ToArray();

            //                var bytesReceived = reader.Transmit(
            //                    sendPci,                // Protocol Control Information (T0, T1 or Raw)
            //                    command,                // command APDU
            //                    command.Length,
            //                    receivePci,             // returning Protocol Control Information
            //                    receiveBuffer,
            //                    receiveBuffer.Length);  // data buffer

            //                var responseApdu =
            //                    new ResponseApdu(receiveBuffer, bytesReceived, IsoCase.Case2Short, reader.Protocol);

            //                ResultText = $"SW1: {responseApdu.SW1:X2}" + Environment.NewLine +
            //                    $"SW2: {responseApdu.SW2:X2}" + Environment.NewLine +
            //                    $"Uid: {(responseApdu.HasData ? BitConverter.ToString(responseApdu.GetData()) : "No uid received")}";
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex.Message);
            //}
            //finally
            //{
            //}

            _logger.LogInformation("end");
        }

        /// <summary>
        /// ReadCardコマンドの実行可能状態を取得する
        /// </summary>
        /// <returns>実行可否</returns>
        private bool CanReadCard()
        {
            return !IsReading;
        }

        /// <summary>
        /// 読取りを終了する
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanStopReadCard))]
        private void StopReadCard()
        {
            _logger.LogInformation("start");

            _deviceService.CancelMonitor();
            DetachEvents();
            IsReading = false;

            _logger.LogInformation("end");
        }

        /// <summary>
        /// StopReadCardの実行可能状態を取得する
        /// </summary>
        /// <returns>実行可否</returns>
        private bool CanStopReadCard()
        {
            return IsReading;
        }

        /// <summary>
        /// CardInitializedEventのイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnCardInitialized(object sender, CardStatusEventArgs args)
        {
            if ((args.State & PCSC.SCRState.Present) == PCSC.SCRState.Present)
            {
                _logger.LogInformation(BitConverter.ToString(args.Atr));
                ResultText = $"ATR: {BitConverter.ToString(args.Atr)}";
                System.Windows.Application.Current.Dispatcher.Invoke(StopReadCard);
            }
        }

        /// <summary>
        /// CardInsertedEventのイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCardInserted(object sender, CardStatusEventArgs args)
        {
            _logger.LogInformation(BitConverter.ToString(args.Atr));
            ResultText = $"ATR: {BitConverter.ToString(args.Atr)}";
            System.Windows.Application.Current.Dispatcher.Invoke(StopReadCard);
        }

        /// <summary>
        /// StatusChangeEventのイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnStatusChanged(object sender, StatusChangeEventArgs args)
        {
            if ((args.NewState & PCSC.SCRState.Present) == PCSC.SCRState.Present)
            {
                _logger.LogInformation(BitConverter.ToString(args.Atr));
                ResultText = $"ATR: {BitConverter.ToString(args.Atr)}";
                System.Windows.Application.Current.Dispatcher.Invoke(StopReadCard);
            }
        }
        #endregion

        #region メンバメソッド
        /// <summary>
        /// イベントハンドラをアタッチする
        /// </summary>
        private void AttachEvents()
        {
            _deviceService.OnCardInitialized += OnCardInitialized;
            _deviceService.OnCardInserted += OnCardInserted;
            _deviceService.OnStatusChanged += OnStatusChanged;
        }

        /// <summary>
        /// イベントハンドラをデタッチする
        /// </summary>
        private void DetachEvents()
        {
            _deviceService.OnCardInitialized -= OnCardInitialized;
            _deviceService.OnCardInserted -= OnCardInserted;
            _deviceService.OnStatusChanged -= OnStatusChanged;
        }
        #endregion
    }
}
