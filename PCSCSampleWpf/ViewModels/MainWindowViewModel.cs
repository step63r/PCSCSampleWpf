using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using Wpf.Ui;
using Wpf.Ui.Controls;

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
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel(ILogger<MainWindowViewModel> logger, INavigationService navigationService)
        {
            _logger = logger;

            if (!_isInitialized)
            {
                InitializeViewModel();
            }
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

            _isInitialized = true;

            _logger.LogInformation("end");
        }
        #endregion
    }
}
