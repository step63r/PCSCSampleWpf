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
    /// WriterPage.xamlのViewModelクラス
    /// </summary>
    public partial class WriterPageViewModel : ViewModel
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
        [NotifyCanExecuteChangedFor(nameof(UpdateBinaryCommand))]
        private string selectedReader = string.Empty;

        /// <summary>
        /// 現在のカードステータス
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(UpdateBinaryCommand))]
        private CardStatusEventArgs currentCardStatus = new();

        /// <summary>
        /// 現在のデバイスステータス
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(UpdateBinaryCommand))]
        private StatusChangeEventArgs currentDeviceStatus = new();

        /// <summary>
        /// MSB
        /// </summary>
        [ObservableProperty]
        private int mostSignificantBit = 0x00;

        /// <summary>
        /// LSB
        /// </summary>
        [ObservableProperty]
        private int leastSignificantBit = 0x00;

        /// <summary>
        /// 書き込む対象のバイナリデータ（文字列）
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(UpdateBinaryCommand))]
        private string inputData = string.Empty;

        /// <summary>
        /// 処理結果の文字列
        /// </summary>
        [ObservableProperty]
        private string resultText = string.Empty;
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
        public WriterPageViewModel(ILogger<MainWindowViewModel> logger, IPCSCDeviceService deviceService)
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
        ~WriterPageViewModel()
        {
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }
        #endregion

        #region コマンド
        /// <summary>
        /// バイナリデータを書き込む
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanUpdateBinary))]
        private void UpdateBinary()
        {
            _logger.LogInformation("start");
            ResultText = string.Empty;

            try
            {
                byte[] data = CreateBinaryData(InputData);

                // MIFAREではP1は0x00固定、Leは0x10固定
                var response = _deviceService.UpdateBinary(SelectedReader, Convert.ToByte(MostSignificantBit), Convert.ToByte(LeastSignificantBit), data);
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
        /// UpdateBinaryの実行可否判定
        /// </summary>
        /// <returns>実行可否</returns>
        private bool CanUpdateBinary()
        {
            return CurrentDeviceStatus.NewState.HasFlag(PCSC.SCRState.Present) &&
                !string.IsNullOrEmpty(SelectedReader) &&
                !string.IsNullOrEmpty(InputData);
                
        }
        #endregion

        #region メンバメソッド
        /// <summary>
        /// 書き込む対象のバイナリデータを生成する
        /// </summary>
        /// <param name="input">文字列</param>
        /// <param name="split">分割用のキャラクター</param>
        /// <returns>バイト配列</returns>
        private static byte[] CreateBinaryData(string input, char split = '-')
        {
            List<byte> ret = [];

            foreach (string s in input.Split(split))
            {
                int n = int.Parse(s);
                ret.Add(Convert.ToByte(n));
            }

            return [.. ret];
        }
        #endregion
    }
}
