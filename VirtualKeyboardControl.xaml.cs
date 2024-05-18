            using System;
            using System.Collections.Generic;
            using System.Windows;
            using System.Windows.Controls;
            using System.Windows.Input;
            using System.Windows.Media;
            using System.Windows.Threading;

namespace VK
    {
        public partial class VirtualKeyboardControl : UserControl
        {
            private DispatcherTimer backspaceTimer;
            private bool isBackspaceHeld;

            private bool isCapsLock = false;
            private bool isNumericMode = false;
            private bool isSymbolMode = false;

            public event EventHandler CloseRequested;
            public event EventHandler EnterPressed;


            public VirtualKeyboardControl()
            {
                InitializeComponent();
                UpdateKeyboard();
                InitializeBackspaceTimer();
                Enter.Click += Enter_Click;
            }

            public void ResetToInitialState()
            {
                isCapsLock = false;
                isNumericMode = false;
                isSymbolMode = false;
                UpdateKeyboard();
            }

            private void Button_Click(object sender, RoutedEventArgs e)
            {
                var button = sender as System.Windows.Controls.Button;
                InsertText(button.Content.ToString());
            }

            private void InsertText(string text)
            {
                var focusedControl = Keyboard.FocusedElement;

                switch (focusedControl)
                {
                    case TextBox textBox:
                        int selectionStart = textBox.SelectionStart;
                        textBox.Text = textBox.Text.Insert(selectionStart, text);
                        textBox.SelectionStart = selectionStart + text.Length;
                        textBox.Focus();
                        break;
                    case PasswordBox passwordBox:
                        passwordBox.Password += text;
                        passwordBox.GetType().GetMethod("Select", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.Invoke(passwordBox, new object[] { passwordBox.Password.Length, 0 });
                        passwordBox.Focus();
                        break;
                }
            }

            private void InitializeBackspaceTimer()
            {
                backspaceTimer = new DispatcherTimer();
                backspaceTimer.Interval = TimeSpan.FromMilliseconds(100);
                backspaceTimer.Tick += HandleBackspaceTimerTick;
                isBackspaceHeld = false;
            }

            private void HandleBackspaceTimerTick(object sender, EventArgs e)
            {
                if (isBackspaceHeld)
                {
                    ButtonBackspace_Click(this, null);
                }
                else
                {
                    backspaceTimer.Stop();
                }
            }

            private void ButtonBackspace_Click(object sender, RoutedEventArgs e)
            {
                DeleteSymbolFromFocusedControl();
            }

            private void DeleteSymbolFromFocusedControl()
            {
                var focusedControl = Keyboard.FocusedElement;
                switch (focusedControl)
                {
                    case TextBox textBox when textBox.SelectionStart > 0:
                        int selectionStart = textBox.SelectionStart;
                        textBox.Text = textBox.Text.Remove(selectionStart - 1, 1);
                        textBox.SelectionStart = selectionStart - 1;
                        break;
                    case PasswordBox passwordBox when passwordBox.Password.Length > 0:
                        passwordBox.Password = passwordBox.Password.Remove(passwordBox.Password.Length - 1);
                        passwordBox.GetType().GetMethod("Select", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.Invoke(passwordBox, new object[] { passwordBox.Password.Length, 0 });
                        passwordBox.Focus();
                        break;
                }
            }

            private void ButtonBackspace_MouseDown(object sender, MouseButtonEventArgs e)
            {
                isBackspaceHeld = true;
                backspaceTimer.Start();
                DeleteSymbolFromFocusedControl();
            }

            private void ButtonBackspace_MouseUp(object sender, MouseButtonEventArgs e)
            {
                if (isBackspaceHeld)
                {
                    isBackspaceHeld = false;
                    backspaceTimer.Stop();
                }

                Application.Current.MainWindow.PreviewMouseUp -= Window_PreviewMouseUp;
            }

            private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
            {
                isBackspaceHeld = false;
                backspaceTimer.Stop();
            }

            private void Button_Space_Click(object sender, RoutedEventArgs e)
            {
                InsertText(" ");
            }

            private void CapsLock_Click(object sender, RoutedEventArgs e)
            {
                isCapsLock = !isCapsLock;
                ShowLetterKeys();
            }

            private void Button123_Click(object sender, RoutedEventArgs e)
            {
                ToggleModes();
            }

            private void Enter_Click(object sender, RoutedEventArgs e)
            {
                var button = sender as Button;
                button.Focus();
                EnterPressed?.Invoke(this, EventArgs.Empty);
            }



            private void UserControl_Loaded(object sender, RoutedEventArgs e)
            {
                UpdateKeyboard();
            }

            private readonly List<string> numericKeys = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            private readonly List<string> symbolKeys = new List<string> { "!", "@", "#", "$", "%", "^", "&", "*", "(", ")" };

            private void UpdateKeyboard()
            {
                HideAllKeys();

                if (isSymbolMode)
                {
                    ShowSymbolKeys();
                    button123.Content = "ABC";
                }
                else
                {
                    ShowLetterKeys();
                    button123.Content = "123";
                }
            }

            private void HideAllKeys()
            {
                foreach (Button key in FindVisualChildren<Button>(this))
                {
                    if (!key.Name.StartsWith("buttonSpace") &&
                        !key.Name.StartsWith("buttonBackspace") &&
                        !key.Name.StartsWith("button1") &&
                        !key.Name.StartsWith("button2") &&
                        !key.Name.StartsWith("button3") &&
                        !key.Name.StartsWith("button4") &&
                        !key.Name.StartsWith("button5") &&
                        !key.Name.StartsWith("button6") &&
                        !key.Name.StartsWith("button7") &&
                        !key.Name.StartsWith("button8") &&
                        !key.Name.StartsWith("button9") &&
                        !key.Name.StartsWith("Enter") &&
                        !key.Name.StartsWith("button0"))
                    {
                        key.Visibility = Visibility.Collapsed;
                    }
                }
            }

            private void ShowNumericKeys()
            {
                for (int i = 0; i < numericKeys.Count; i++)
                {
                    var buttonName = $"button{i}";
                    var btn = (Button)this.FindName(buttonName);
                    if (btn != null)
                    {
                        btn.Visibility = Visibility.Visible;
                    }
                }
            }

            private void ShowLetterKeys()
            {
                var letters = "QWERTYUIOPASDFGHJKLZXCVBNM";
                for (int i = 0; i < letters.Length; i++)
                {
                    var btn = (Button)this.FindName($"button{letters[i]}");
                    btn.Content = isCapsLock ? letters[i].ToString() : letters[i].ToString().ToLower();
                    btn.Visibility = Visibility.Visible;
                }
                CapsLockButton.Visibility = Visibility.Visible;
            }

            private void ShowSymbolKeys()
            {
                buttonQ.Content = "-";
                buttonQ.Visibility = Visibility.Visible;
                buttonW.Content = "_";
                buttonW.Visibility = Visibility.Visible;
                buttonE.Content = ":";
                buttonE.Visibility = Visibility.Visible;
                buttonR.Content = ";";
                buttonR.Visibility = Visibility.Visible;
                buttonT.Content = "(";
                buttonT.Visibility = Visibility.Visible;
                buttonY.Content = ")";
                buttonY.Visibility = Visibility.Visible;
                buttonU.Content = "@";
                buttonU.Visibility = Visibility.Visible;
                buttonI.Content = "&";
                buttonI.Visibility = Visibility.Visible;
                buttonO.Content = "+";
                buttonO.Visibility = Visibility.Visible;
                buttonP.Content = "=";
                buttonP.Visibility = Visibility.Visible;

                buttonA.Content = "!";
                buttonA.Visibility = Visibility.Visible;
                buttonS.Content = "/";
                buttonS.Visibility = Visibility.Visible;
                buttonD.Content = "[";
                buttonD.Visibility = Visibility.Visible;
                buttonF.Content = "]";
                buttonF.Visibility = Visibility.Visible;
                buttonG.Content = "{";
                buttonG.Visibility = Visibility.Visible;
                buttonH.Content = "}";
                buttonH.Visibility = Visibility.Visible;
                buttonJ.Content = "?";
                buttonJ.Visibility = Visibility.Visible;
                buttonZ.Content = "`";
                buttonK.Visibility = Visibility.Visible;
                buttonL.Content = "$";
                buttonL.Visibility = Visibility.Visible;

                buttonK.Content = "\"";

                buttonZ.Visibility = Visibility.Visible;
                buttonX.Content = "<";
                buttonX.Visibility = Visibility.Visible;
                buttonC.Content = ">";
                buttonC.Visibility = Visibility.Visible;
                buttonV.Content = "/";
                buttonV.Visibility = Visibility.Visible;
                buttonB.Content = "\\";
                buttonB.Visibility = Visibility.Visible;
                buttonN.Content = ",";
                buttonN.Visibility = Visibility.Visible;
                buttonM.Content = ".";
                buttonM.Visibility = Visibility.Visible;

                CapsLockButton.Visibility = Visibility.Collapsed;

                button123.Content = "ABC";
                button123.Visibility = Visibility.Visible;

                buttonBackspace.Visibility = Visibility.Visible;

                ShowNumericKeys();
            }

            private void ToggleModes()
            {
                isSymbolMode = !isSymbolMode;

                isCapsLock = false;
                UpdateKeyboard();
            }

            public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
            {
                if (depObj != null)
                {
                    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                    {
                        DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                        if (child is T)
                        {
                            yield return (T)child;
                        }

                        foreach (T childOfChild in FindVisualChildren<T>(child))
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }
    }

  
