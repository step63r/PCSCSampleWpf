using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MinatoProject.PCSCSampleWpf.Common;
using MinatoProject.PCSCSampleWpf.Services.Interfaces;
using PCSC.Monitoring;
using System.Globalization;
using System.Text;

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

        #region 定数値
        /// <summary>
        /// システムのANSIコードページ
        /// </summary>
        private static readonly int AnsiCodePage = CultureInfo.CurrentCulture.TextInfo.ANSICodePage;
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

            // .NETでASCII、UTF-7、UTF-8、UTF-16、UTF-32以外のエンコードを使うために必要
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

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
        /// デバイスステータスを取得する
        /// </summary>
        [RelayCommand]
        private void GetStatus()
        {
            _logger.LogInformation("start");

            try
            {
                var status = _deviceService.GetStatus(SelectedReader);
                ResultText = $"Protocol: {status.Protocol}" + Environment.NewLine +
                    $"State: {status.State}" + Environment.NewLine +
                    $"ATR: {BitConverter.ToString(status.GetAtr())}";
            }
            catch (Exception ex)
            {
                ResultText = $"[ERROR]\n{ex.Message}\n-----\n{ex.StackTrace}";
                _logger.LogError(ex.Message);
            }

            _logger.LogInformation("end");
        }

        /// <summary>
        /// カード情報を取得する
        /// </summary>
        [RelayCommand]
        private void GetCardInfo()
        {
            _logger.LogInformation("start");
            ResultText = string.Empty;

            try
            {
                // カードのUID
                var response1 = _deviceService.GetData(SelectedReader, ApduInsGetData.GetUid);
                ResultText += $"[SW1=0x{response1.SW1:X2}, SW2=0x{response1.SW2:X2}] UID: {(response1.HasData ? BitConverter.ToString(response1.GetData()) : "No uid received")}\n";

                // カード名称
                var response2 = _deviceService.GetData(SelectedReader, ApduInsGetData.GetCardName);
                ResultText += $"[SW1=0x{response2.SW1:X2}, SW2=0x{response2.SW2:X2}] Card Name: {(response2.HasData ? Encoding.GetEncoding(AnsiCodePage).GetString(response2.GetData()) : "No card name received")}\n";

                // カード種別名称
                var response3 = _deviceService.GetData(SelectedReader, ApduInsGetData.GetCardTypeName);
                ResultText += $"[SW1=0x{response3.SW1:X2}, SW2=0x{response3.SW2:X2}] Card Type: {(response3.HasData ? Encoding.GetEncoding(AnsiCodePage).GetString(response3.GetData()) : "No card type name received")} \n";
            }
            catch (Exception ex)
            {
                ResultText = $"[ERROR]\n{ex.Message}\n-----\n{ex.StackTrace}";
                _logger.LogError(ex.Message);
            }

            _logger.LogInformation("end");
        }

        /// <summary>
        /// バイナリデータを読み込む
        /// </summary>
        [RelayCommand]
        private void ReadBinary()
        {
            _logger.LogInformation("start");
            ResultText = string.Empty;

            try
            {
                // MIFAREではP1は0x00固定、Leは0x10固定
                for (int i = 0; i < 34; i++)
                {
                    var response = _deviceService.ReadBinary(SelectedReader, 0x00, (byte)(i * 4), 0x10);
                    ResultText += $"[SW1=0x{response.SW1:X2}, SW2=0x{response.SW2:X2}] [0x{i * 4:X2}] {(response.HasData ? BitConverter.ToString(response.GetData()) : "NO DATA")}\n";
                }
            }
            catch (Exception ex)
            {
                ResultText = $"[ERROR]\n{ex.Message}\n-----\n{ex.StackTrace}";
                _logger.LogError(ex.Message);
            }

            _logger.LogInformation("end");
        }

        /// <summary>
        /// バイナリデータを書き込む
        /// </summary>
        [RelayCommand]
        private void UpdateBinary()
        {
            _logger.LogInformation("start");
            ResultText = string.Empty;

            try
            {
                // MIFAREではP1は0x00固定、Leは0x10固定
                var response = _deviceService.UpdateBinary(SelectedReader, 0x00, 0x09, [0x05, 0x06, 0x07, 0x08]);
                ResultText = $"[SW1={response.SW1:X2}, SW2=0x{response.SW2:X2}]";
            }
            catch (Exception ex)
            {
                ResultText = $"[ERROR]\n{ex.Message}\n-----\n{ex.StackTrace}";
                _logger.LogError(ex.Message);
            }

            _logger.LogInformation("end");
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
