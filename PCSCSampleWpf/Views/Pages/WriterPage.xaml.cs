using System.Windows.Controls;

namespace MinatoProject.PCSCSampleWpf.Views.Pages
{
    /// <summary>
    /// WriterPage.xaml の相互作用ロジック
    /// </summary>
    public partial class WriterPage : Page
    {
        /// <summary>
        /// デザイナー用コンストラクタ
        /// </summary>
        public WriterPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ViewModelの引数付きコンストラクタ
        /// </summary>
        /// <param name="viewModel">ViewModelのインスタンス</param>
        public WriterPage(ViewModels.WriterPageViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
