using System.Windows.Controls;

namespace MinatoProject.PCSCSampleWpf.Views.Pages
{
    /// <summary>
    /// ReaderPage.xaml の相互作用ロジック
    /// </summary>
    public partial class ReaderPage : Page
    {
        /// <summary>
        /// デザイナー用コンストラクタ
        /// </summary>
        public ReaderPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ViewModelの引数付きコンストラクタ
        /// </summary>
        /// <param name="viewModel">ViewModelのインスタンス</param>
        public ReaderPage(ViewModels.ReaderPageViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
