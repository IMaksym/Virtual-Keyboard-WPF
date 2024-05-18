using System;
using System.Diagnostics.Metrics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace VK
{
    public partial class App : Application
    {
        private VirtualKeyboardControl virtualKeyboard;

        private Popup keyboardPopup;
        private DispatcherTimer keyboardDisplayTimer;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SetupVirtualKeyboard();
            RegisterEventHandlers();
            //Mouse.OverrideCursor = Cursors.None;
        }

        private void SetupVirtualKeyboard()
        {
            virtualKeyboard = new VirtualKeyboardControl();
            virtualKeyboard.EnterPressed += VirtualKeyboard_EnterPressed;

            keyboardPopup = new Popup
            {
                Width = SystemParameters.FullPrimaryScreenWidth,
                Height = 480,
                Placement = PlacementMode.Bottom,
                VerticalOffset = SystemParameters.HorizontalScrollBarHeight - (-2000),
                AllowsTransparency = true,
                Focusable = false,
                Child = virtualKeyboard
            };
            keyboardPopup.Closed += (sender, e) =>
            {
                virtualKeyboard.ResetToInitialState();
            };
            keyboardDisplayTimer = new DispatcherTimer();
            keyboardDisplayTimer.Interval = TimeSpan.FromMilliseconds(50);
            keyboardDisplayTimer.Tick += KeyboardDisplayTimer_Tick;
        }

        private void RegisterEventHandlers()
        {
            EventManager.RegisterClassHandler(typeof(Window), UIElement.KeyDownEvent, new KeyEventHandler(Window_KeyDown), true);


            EventManager.RegisterClassHandler(typeof(Window), UIElement.PreviewMouseDownEvent, new MouseButtonEventHandler(Window_MouseDown), true);
            EventManager.RegisterClassHandler(typeof(TextBox), UIElement.PreviewMouseDownEvent, new MouseButtonEventHandler(OnPreviewMouseDown), true);
            EventManager.RegisterClassHandler(typeof(PasswordBox), UIElement.PreviewMouseDownEvent, new MouseButtonEventHandler(OnPreviewMouseDown), true);

            // Регистрация обработчика для события потери фокуса
            EventManager.RegisterClassHandler(typeof(TextBox), UIElement.LostFocusEvent, new RoutedEventHandler(TextBox_LostFocus));
            EventManager.RegisterClassHandler(typeof(PasswordBox), UIElement.LostFocusEvent, new RoutedEventHandler(TextBox_LostFocus));
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Keyboard.ClearFocus();

            if (e.Key == Key.Enter)
            {
                // Снимаем фокус с текущего элемента (текстового поля)

                // Закрываем клавиатуру, если нужно
                if (keyboardPopup.IsOpen)
                {
                    keyboardPopup.IsOpen = false;
                    virtualKeyboard.ResetToInitialState();
                }
            }
        }


        private void VirtualKeyboard_EnterPressed(object sender, EventArgs e)
        {
            // Снимаем фокус с текущего элемента (текстового поля)
            Keyboard.ClearFocus();

            // Закрываем клавиатуру
            if (keyboardPopup.IsOpen)
            {
                keyboardPopup.IsOpen = false;
                virtualKeyboard.ResetToInitialState();
            }
        }


        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            virtualKeyboard.ResetToInitialState();
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var element = sender as UIElement;
            element?.Focus();

            virtualKeyboard.ResetToInitialState();

            keyboardDisplayTimer.Stop();
            keyboardDisplayTimer.Start();
        }

        private void KeyboardDisplayTimer_Tick(object sender, EventArgs e)
        {
            if (!keyboardPopup.IsOpen)
            {
                virtualKeyboard.ResetToInitialState();
                keyboardPopup.IsOpen = true;
            }
            keyboardDisplayTimer.Stop();
        }

        private void OnTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            if (keyboardPopup != null && keyboardPopup.IsOpen)
            {
                virtualKeyboard?.ResetToInitialState();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(e.OriginalSource is TextBox || e.OriginalSource is PasswordBox))
            {
                if (keyboardPopup.IsOpen)
                {
                    keyboardPopup.IsOpen = false;
                    virtualKeyboard.ResetToInitialState();
                }
                Keyboard.ClearFocus();
            }
        }
    }
}
