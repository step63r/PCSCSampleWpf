using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MinatoProject.PCSCSampleWpf.Common;
using MinatoProject.PCSCSampleWpf.Services.Interfaces;
using PCSC;
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
        /// デバイスのステータス
        /// </summary>
        [ObservableProperty]
        private string deviceStatus = string.Empty;

        /// <summary>
        /// デバイスのATR
        /// </summary>
        [ObservableProperty]
        private string deviceATR = string.Empty;

        /// <summary>
        /// カードのUID
        /// </summary>
        [ObservableProperty]
        private string cardUid = string.Empty;

        /// <summary>
        /// カード名称
        /// </summary>
        [ObservableProperty]
        private string cardName = string.Empty;

        /// <summary>
        /// カード種別
        /// </summary>
        [ObservableProperty]
        private string cardType = string.Empty;
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
        }
        #endregion

        #region 抽象メソッドの実装
        /// <inheritdoc />
        public override void OnNavigatedTo()
        {
            _logger.LogInformation("start");

            if (!string.IsNullOrEmpty(SelectedReader))
            {
                AttachEvents();
                _deviceService.StartMonitor(SelectedReader);
            }

            _logger.LogInformation("end");
        }

        /// <inheritdoc />
        public override void OnNavigatedFrom()
        {
            _logger.LogInformation("start");

            _deviceService.CancelMonitor();
            DetachEvents();

            _logger.LogInformation("end");
        }
        #endregion

        #region コマンド
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
        /// すべてのバイナリデータを読み込む
        /// </summary>
        [RelayCommand]
        private void ReadAllBinaries()
        {
            _logger.LogInformation("start");
            ResultText = string.Empty;

            try
            {
                var responses = _deviceService.ReadAllBinaries(SelectedReader, 4, 0x10, 540);

                int i = 0;
                foreach (var response in responses)
                {
                    ResultText += $"[SW1=0x{response.SW1:X2}, SW2=0x{response.SW2:X2}] [0x{i++ * 4:X2}] {(response.HasData ? BitConverter.ToString(response.GetData()) : "NO DATA")}\n";
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
        /// CardInsertedEventのイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCardInserted(object sender, CardStatusEventArgs args)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var status = PCSCUtils.SeparatePCSCState(args.State);
                DeviceStatus = string.Join(", ", [.. status.Select(x => x.ToString())]);

                if (args.Atr != null && args.Atr.Length > 0)
                {
                    DeviceATR = BitConverter.ToString(args.Atr);
                }
            });

            if ((args.State & SCRState.Present) == SCRState.Present)
            {
                UpdateCardInfo();
            }
        }

        /// <summary>
        /// CardRemovedEvent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnCardRemoved(object sender, CardStatusEventArgs args)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var status = PCSCUtils.SeparatePCSCState(args.State);
                DeviceStatus = string.Join(", ", [.. status.Select(x => x.ToString())]);

                if (args.Atr != null && args.Atr.Length > 0)
                {
                    DeviceATR = BitConverter.ToString(args.Atr);
                }

                CardUid = string.Empty;
                CardName = string.Empty;
                CardType = string.Empty;
            });
        }

        /// <summary>
        /// CardInitializedEventのイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnCardInitialized(object sender, CardStatusEventArgs args)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var status = PCSCUtils.SeparatePCSCState(args.State);
                DeviceStatus = string.Join(", ", [.. status.Select(x => x.ToString())]);

                if (args.Atr != null && args.Atr.Length > 0)
                {
                    DeviceATR = BitConverter.ToString(args.Atr);
                }
            });

            if ((args.State & SCRState.Present) == SCRState.Present)
            {
                UpdateCardInfo();
            }
        }

        /// <summary>
        /// StatusChangeEventのイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnStatusChanged(object sender, StatusChangeEventArgs args)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var status = PCSCUtils.SeparatePCSCState(args.NewState);
                DeviceStatus = string.Join(", ", [.. status.Select(x => x.ToString())]);

                if (args.Atr != null && args.Atr.Length > 0)
                {
                    DeviceATR = BitConverter.ToString(args.Atr);
                }
            });

            if ((args.NewState & SCRState.Present) == SCRState.Present)
            {
                UpdateCardInfo();
            }
        }
        #endregion

        #region メンバメソッド
        /// <summary>
        /// イベントハンドラをアタッチする
        /// </summary>
        private void AttachEvents()
        {
            _deviceService.OnCardInserted += OnCardInserted;
            _deviceService.OnCardRemoved += OnCardRemoved;
            _deviceService.OnCardInitialized += OnCardInitialized;
            _deviceService.OnStatusChanged += OnStatusChanged;
        }

        /// <summary>
        /// イベントハンドラをデタッチする
        /// </summary>
        private void DetachEvents()
        {
            _deviceService.OnCardInserted -= OnCardInserted;
            _deviceService.OnCardRemoved -= OnCardRemoved;
            _deviceService.OnCardInitialized -= OnCardInitialized;
            _deviceService.OnStatusChanged -= OnStatusChanged;
        }

        /// <summary>
        /// カード情報を更新する
        /// </summary>
        private void UpdateCardInfo()
        {
            string cardUid = string.Empty;
            string cardName = string.Empty;
            string cardType = string.Empty;

            try
            {
                // カードのUID
                var response1 = _deviceService.GetData(SelectedReader, ApduInsGetData.GetUid);
                cardUid = response1.HasData ? BitConverter.ToString(response1.GetData()) : "No uid received";

                // カード名称
                var response2 = _deviceService.GetData(SelectedReader, ApduInsGetData.GetCardName);
                cardName = response2.HasData ? Encoding.GetEncoding(AnsiCodePage).GetString(response2.GetData()) : "No card name received";

                // カード種別名称
                var response3 = _deviceService.GetData(SelectedReader, ApduInsGetData.GetCardTypeName);
                cardType = response3.HasData ? Encoding.GetEncoding(AnsiCodePage).GetString(response3.GetData()) : "No card type name received";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                CardUid = cardUid;
                CardName = cardName;
                CardType = cardType;
            });
        }
        #endregion
    }
}
