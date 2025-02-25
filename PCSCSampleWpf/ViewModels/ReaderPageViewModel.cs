using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using MinatoProject.PCSCSampleWpf.Common;
using MinatoProject.PCSCSampleWpf.Services.Interfaces;
using PCSC.Monitoring;

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
        /// 選択されたデバイス
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ReadAllBinariesCommand))]
        private string selectedReader = string.Empty;

        /// <summary>
        /// 現在のカードステータス
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ReadAllBinariesCommand))]
        private CardStatusEventArgs currentCardStatus = new();

        /// <summary>
        /// 現在のデバイスステータス
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ReadAllBinariesCommand))]
        private StatusChangeEventArgs currentDeviceStatus = new();

        /// <summary>
        /// 処理結果の文字列
        /// </summary>
        [ObservableProperty]
        private string resultText = string.Empty;

        /// <summary>
        /// Offset
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ReadAllBinariesCommand))]
        private int offset = 0;

        /// <summary>
        /// Size
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ReadAllBinariesCommand))]
        private int size = 0;

        /// <summary>
        /// All bytes
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ReadAllBinariesCommand))]
        private int allBytes = 0;
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
            
            // 現在選択されているデバイスをMainWindowViewModelに要求
            SelectedReader = WeakReferenceMessenger.Default.Send<SelectedReaderRequestMessage>();

            // メッセンジャーの登録
            WeakReferenceMessenger.Default.Register<StatusChangeEventMessage>(this, (r, m) =>
            {
                CurrentDeviceStatus = m.Value;
            });

            WeakReferenceMessenger.Default.Register<CardStatusEventMessage>(this, (r, m) =>
            {
                CurrentCardStatus = m.Value;
            });

            WeakReferenceMessenger.Default.Register<SelectedReaderChangedMessage>(this, (r, m) =>
            {
                SelectedReader = m.Value;
            });
        }
        #endregion

        #region ファイナライザ
        /// <summary>
        /// ファイナライザ
        /// </summary>
        ~ReaderPageViewModel()
        {
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }
        #endregion

        #region コマンド
        /// <summary>
        /// すべてのバイナリデータを読み込む
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanReadAllBinaries))]
        private void ReadAllBinaries()
        {
            _logger.LogInformation("start");
            ResultText = string.Empty;

            try
            {
                var responses = _deviceService.ReadAllBinaries(SelectedReader, Offset, Size, AllBytes);

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
        /// ReadAllBinariesの実行可否判定
        /// </summary>
        /// <returns>実行可否</returns>
        private bool CanReadAllBinaries()
        {
            return CurrentDeviceStatus.NewState.HasFlag(PCSC.SCRState.Present) &&
                !string.IsNullOrEmpty(SelectedReader) &&
                Offset > 0 && Size > 0 && AllBytes > 0;
        }
        #endregion
    }
}
