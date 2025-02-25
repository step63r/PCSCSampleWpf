using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using MinatoProject.PCSCSampleWpf.Common;
using MinatoProject.PCSCSampleWpf.Services.Interfaces;
using PCSC.Monitoring;
using PCSC;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using Wpf.Ui;
using Wpf.Ui.Controls;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;

namespace MinatoProject.PCSCSampleWpf.ViewModels
{
    /// <summary>
    /// MainWindow.xamlのViewModelクラス
    /// </summary>
    public partial class MainWindowViewModel : ViewModel
    {
        #region プロパティ
        /// <summary>
        /// アプリケーションタイトル
        /// </summary>
        [ObservableProperty]
        private string applicationTitle = "PCSC Sample WPF";

        /// <summary>
        /// ナビゲーション領域に表示する要素
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<object> _navigationItems = [];

        /// <summary>
        /// トレイ
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = [];

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
        /// 初期化済みフラグ
        /// </summary>
        private bool _isInitialized = false;

        /// <summary>
        /// ロガー
        /// </summary>
        private readonly ILogger<MainWindowViewModel> _logger;

        /// <summary>
        /// デバイスサービス
        /// </summary>
        private readonly IPCSCDeviceService _deviceService;

        /// <summary>
        /// スナックバーサービス
        /// </summary>
        private readonly ISnackbarService _snackbarService;
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
        public MainWindowViewModel(ILogger<MainWindowViewModel> logger, INavigationService navigationService, IPCSCDeviceService deviceService, ISnackbarService snackbarService)
        {
            _logger = logger;
            _deviceService = deviceService;
            _snackbarService = snackbarService;

            if (!_isInitialized)
            {
                InitializeViewModel();
            }
        }
        #endregion

        #region ファイナライザ
        /// <summary>
        /// ファイナライザ
        /// </summary>
        ~MainWindowViewModel()
        {
            _deviceService.CancelMonitor();
            DetachEvents();
        }
        #endregion

        #region メソッド
        /// <summary>
        /// ViewModelの初期化処理
        /// </summary>
        private void InitializeViewModel()
        {
            _logger.LogInformation("start");

            NavigationItems =
            [
                new NavigationViewItem()
                {
                    Content = "Reader",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.ScanText24 },
                    TargetPageType = typeof(Views.Pages.ReaderPage),
                },
                new NavigationViewItem()
                {
                    Content = "Writer",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.DrawText24 },
                    TargetPageType = typeof(Views.Pages.WriterPage),
                },
            ];

            Readers = _deviceService.Readers;
            if (Readers.Count > 0)
            {
                SelectedReader = Readers.First();
            }

            WeakReferenceMessenger.Default.Register<MainWindowViewModel, SelectedReaderRequestMessage>(this, (r, m) =>
            {
                m.Reply(SelectedReader);
            });

            // .NETでASCII、UTF-7、UTF-8、UTF-16、UTF-32以外のエンコードを使うために必要
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (!string.IsNullOrEmpty(SelectedReader))
            {
                AttachEvents();
                _deviceService.StartMonitor(SelectedReader);
            }

            _isInitialized = true;

            _logger.LogInformation("end");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        partial void OnSelectedReaderChanged(string? oldValue, string newValue)
        {
            if (!string.IsNullOrEmpty(oldValue) && oldValue != newValue)
            {
                WeakReferenceMessenger.Default.Send(new SelectedReaderChangedMessage(newValue));
            }
        }

        /// <summary>
        /// CardInsertedEventのイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCardInserted(object sender, CardStatusEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var status = PCSCUtils.SeparatePCSCState(args.State);
                DeviceStatus = string.Join(", ", [.. status.Select(x => x.ToString())]);

                if (args.Atr != null && args.Atr.Length > 0)
                {
                    DeviceATR = BitConverter.ToString(args.Atr);
                }

                if ((args.State & SCRState.Present) == SCRState.Present)
                {
                    UpdateCardInfo();
                }

                _snackbarService.Show(
                    "Card Inserted",
                    $"{CardName} ({CardType})",
                    ControlAppearance.Info,
                    new SymbolIcon { Symbol = SymbolRegular.Info12 },
                    TimeSpan.FromSeconds(5));
                WeakReferenceMessenger.Default.Send(new CardStatusEventMessage(args));
            });
        }

        /// <summary>
        /// CardRemovedEvent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnCardRemoved(object sender, CardStatusEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(() =>
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

                _snackbarService.Show(
                    "Card Removed",
                    $"No description",
                    ControlAppearance.Info,
                    new SymbolIcon { Symbol = SymbolRegular.Info12 },
                    TimeSpan.FromSeconds(5));
                WeakReferenceMessenger.Default.Send(new CardStatusEventMessage(args));
            });
        }

        /// <summary>
        /// CardInitializedEventのイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnCardInitialized(object sender, CardStatusEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var status = PCSCUtils.SeparatePCSCState(args.State);
                DeviceStatus = string.Join(", ", [.. status.Select(x => x.ToString())]);

                if (args.Atr != null && args.Atr.Length > 0)
                {
                    DeviceATR = BitConverter.ToString(args.Atr);
                }

                if ((args.State & SCRState.Present) == SCRState.Present)
                {
                    UpdateCardInfo();
                }

                _snackbarService.Show(
                    "Card Initialized",
                    $"No description",
                    ControlAppearance.Info,
                    new SymbolIcon { Symbol = SymbolRegular.Info12 },
                    TimeSpan.FromSeconds(5));
                WeakReferenceMessenger.Default.Send(new CardStatusEventMessage(args));
            });
        }

        /// <summary>
        /// StatusChangeEventのイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnStatusChanged(object sender, StatusChangeEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var status = PCSCUtils.SeparatePCSCState(args.NewState);
                DeviceStatus = string.Join(", ", [.. status.Select(x => x.ToString())]);

                if (args.Atr != null && args.Atr.Length > 0)
                {
                    DeviceATR = BitConverter.ToString(args.Atr);
                }

                if ((args.NewState & SCRState.Present) == SCRState.Present)
                {
                    UpdateCardInfo();
                }

                //_snackbarService.Show(
                //    "Device Status Changed",
                //    $"{CardName} ({CardType})",
                //    ControlAppearance.Info,
                //    new SymbolIcon { Symbol = SymbolRegular.Info12 },
                //    TimeSpan.FromSeconds(5));
                WeakReferenceMessenger.Default.Send(new StatusChangeEventMessage(args));
            });
        }

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

            Application.Current.Dispatcher.Invoke(() =>
            {
                CardUid = cardUid;
                CardName = cardName;
                CardType = cardType;
            });
        }
        #endregion
    }
}
