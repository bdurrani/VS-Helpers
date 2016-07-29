using System.Windows.Forms;

namespace BD.VSHelpers.Options
{
    public partial class OptionsPageControl : UserControl
    {
        private readonly OptionPageGrid _optionsPage;

        public OptionsPageControl(OptionPageGrid optionsPage)
        {
            _optionsPage = optionsPage;

            InitializeComponent();
            var wpf = new OptionsPageControlWPF();
            wpf.InitializeComponent();
            wpf.DataContext = _optionsPage;
            wpfElementHost.Child = wpf;
        }

        public OptionPageGrid OptionsPage
        {
            get
            {
                return _optionsPage;
            }
        }
    } 
}
