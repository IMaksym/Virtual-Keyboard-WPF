using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VK
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Обработчик для нажатия мыши в любом месте окна
            this.MouseDown += MainWindow_MouseDown;
            // Подписываемся на события получения и потери фокуса для всех TextBox и PasswordBox
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler(TextBoxGotFocus));
            EventManager.RegisterClassHandler(typeof(PasswordBox), PasswordBox.GotFocusEvent, new RoutedEventHandler(PasswordBoxGotFocus));
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Скрыть виртуальную клавиатуру, если клик был не по элементу ввода
            if (!(e.OriginalSource is TextBox) && !(e.OriginalSource is PasswordBox))
            {
                VirtualKeyboardPopup.IsOpen = false;
            }
        }
        public void ShowVirtualKeyboard()
        {
            VirtualKeyboardPopup.IsOpen = true;
        }

        public void HideVirtualKeyboard()
        {
            VirtualKeyboardPopup.IsOpen = false;
        }
        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            // Показать виртуальную клавиатуру при фокусе на TextBox
            VirtualKeyboardPopup.IsOpen = true;
        }

        private void PasswordBoxGotFocus(object sender, RoutedEventArgs e)
        {
            // Показать виртуальную клавиатуру при фокусе на PasswordBox
            VirtualKeyboardPopup.IsOpen = true;
        }
    }
}
