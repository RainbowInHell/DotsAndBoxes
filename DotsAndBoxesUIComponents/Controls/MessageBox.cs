using System.Windows;

namespace DotsAndBoxesUIComponents;

    public static class MessageBox
    {
        /// <summary>
        /// Displays a message box that has a message and that returns a result.
        /// </summary>
        /// <param name="message">A System.String that specifies the text to display.</param>
        /// <returns>A CustomMsgBox.MsgBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MsgBoxResult Show(string message)
        {
            var msgBox = new CustomMessageBox(message);
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        public static MsgBoxResult Show(string message, MsgBoxButton buttons, MsgBoxImage image)
        {
            var msgBox = new CustomMessageBox(message, buttons, image);
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, a caption; and that returns a result.
        /// </summary>
        /// <param name="message">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the text over the message to display.</param>
        /// <returns>A CustomMsgBox.MsgBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MsgBoxResult Show(string message, string caption)
        {
            var msgBox = new CustomMessageBox(message, caption);
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, a caption, a buttons and that returns a result.
        /// </summary>
        /// <param name="message">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the text over the message to display.</param>
        /// <param name="buttons">A CustomMsgBox.MsgBoxButton that specifies buttons to display.</param>
        /// <returns>A CustomMsgBox.MsgBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MsgBoxResult Show(string message, string caption, MsgBoxButton buttons)
        {
            var msgBox = new CustomMessageBox(message, caption, buttons);
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, a caption, a buttons, an image and that returns a result.
        /// </summary>
        /// <param name="message">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the text over the message to display.</param>
        /// <param name="buttons">A CustomMsgBox.MsgBoxButton that specifies buttons to display.</param>
        /// <param name="image">A CustomMsgBox.MsgBoxImage that specifies image to display.</param>
        /// <returns>A CustomMsgBox.MsgBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MsgBoxResult Show(string message, string caption, MsgBoxButton buttons, MsgBoxImage image)
        {
            var msgBox = new CustomMessageBox(message, caption, buttons, image);
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, a caption, a buttons with custom text, an image and that returns a result.
        /// </summary>
        /// <param name="message">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the text over the message to display.</param>
        /// <param name="buttons">A CustomMsgBox.MsgBoxButton that specifies buttons to display.</param>
        /// <param name="image">A CustomMsgBox.MsgBoxImage that specifies image to display.</param>
        /// <param name="okButtonText">A System.String that specifies the 'OK' button text.</param>
        /// <param name="yesButtonText">A System.String that specifies the 'Yes' button text.</param>
        /// <param name="noButtonText">A System.String that specifies the 'No' button text.</param>
        /// <param name="cancelButtonText">A System.String that specifies the 'Cancel' button text.</param>
        /// <returns>A CustomMsgBox.MsgBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MsgBoxResult Show(string message, string caption, MsgBoxButton buttons, MsgBoxImage image, string okButtonText, string yesButtonText, string noButtonText, string cancelButtonText)
        {
            var msgBox = new CustomMessageBox(message, caption, buttons, image)
            {
                OkButtonText = okButtonText,
                YesButtonText = yesButtonText,
                NoButtonText = noButtonText,
                CancelButtonText = cancelButtonText
            };
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, a caption, a buttons with custom text, a custom buttons, an image and that returns a result.
        /// </summary>
        /// <param name="message">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the text over the message to display.</param>
        /// <param name="buttons">A CustomMsgBox.MsgBoxButton that specifies buttons to display.</param>
        /// <param name="image">A CustomMsgBox.MsgBoxImage that specifies image to display.</param>
        /// <param name="okButtonText">A System.String that specifies the 'OK' button text.</param>
        /// <param name="yesButtonText">A System.String that specifies the 'Yes' button text.</param>
        /// <param name="noButtonText">A System.String that specifies the 'No' button text.</param>
        /// <param name="cancelButtonText">A System.String that specifies the 'Cancel' button text.</param>
        /// <param name="customButton1Text">A System.String that specifies the 1st custom button text.</param>
        /// <returns>A CustomMsgBox.MsgBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MsgBoxResult Show(string message, string caption, MsgBoxButton buttons, MsgBoxImage image, string okButtonText, string yesButtonText, string noButtonText, string cancelButtonText, string customButton1Text)
        {
            var msgBox = new CustomMessageBox(message, caption, buttons, image)
            {
                OkButtonText = okButtonText,
                YesButtonText = yesButtonText,
                NoButtonText = noButtonText,
                CancelButtonText = cancelButtonText,
                CustomButton1Text = customButton1Text,
                custom1Btn =
                {
                    Visibility = Visibility.Visible
                }
            };
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, a caption, a buttons with custom text, a custom buttons, an image and that returns a result.
        /// </summary>
        /// <param name="message">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the text over the message to display.</param>
        /// <param name="buttons">A CustomMsgBox.MsgBoxButton that specifies buttons to display.</param>
        /// <param name="image">A CustomMsgBox.MsgBoxImage that specifies image to display.</param>
        /// <param name="okButtonText">A System.String that specifies the 'OK' button text.</param>
        /// <param name="yesButtonText">A System.String that specifies the 'Yes' button text.</param>
        /// <param name="noButtonText">A System.String that specifies the 'No' button text.</param>
        /// <param name="cancelButtonText">A System.String that specifies the 'Cancel' button text.</param>
        /// <param name="customButton1Text">A System.String that specifies the 1st custom button text.</param>
        /// <param name="customButton2Text">A System.String that specifies the 2nd custom button text.</param>
        /// <returns>A CustomMsgBox.MsgBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MsgBoxResult Show(string message, string caption, MsgBoxButton buttons, MsgBoxImage image, string okButtonText, string yesButtonText, string noButtonText, string cancelButtonText, string customButton1Text, string customButton2Text)
        {
            var msgBox = new CustomMessageBox(message, caption, buttons, image)
            {
                OkButtonText = okButtonText,
                YesButtonText = yesButtonText,
                NoButtonText = noButtonText,
                CancelButtonText = cancelButtonText,
                CustomButton1Text = customButton1Text,
                custom1Btn =
                {
                    Visibility = Visibility.Visible
                },
                CustomButton2Text = customButton2Text,
                custom2Btn =
                {
                    Visibility = Visibility.Visible
                }
            };
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, a caption, a buttons with custom text, a custom buttons, an image and that returns a result.
        /// </summary>
        /// <param name="message">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the text over the message to display.</param>
        /// <param name="buttons">A CustomMsgBox.MsgBoxButton that specifies buttons to display.</param>
        /// <param name="image">A CustomMsgBox.MsgBoxImage that specifies image to display.</param>
        /// <param name="okButtonText">A System.String that specifies the 'OK' button text.</param>
        /// <param name="yesButtonText">A System.String that specifies the 'Yes' button text.</param>
        /// <param name="noButtonText">A System.String that specifies the 'No' button text.</param>
        /// <param name="cancelButtonText">A System.String that specifies the 'Cancel' button text.</param>
        /// <param name="customButton1Text">A System.String that specifies the 1st custom button text.</param>
        /// <param name="customButton2Text">A System.String that specifies the 2nd custom button text.</param>
        /// <param name="customButton3Text">A System.String that specifies the 3rd custom button text.</param>
        /// <returns>A CustomMsgBox.MsgBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MsgBoxResult Show(string message, string caption, MsgBoxButton buttons, MsgBoxImage image, string okButtonText, string yesButtonText, string noButtonText, string cancelButtonText, string customButton1Text, string customButton2Text, string customButton3Text)
        {
            var msgBox = new CustomMessageBox(message, caption, buttons, image)
            {
                OkButtonText = okButtonText,
                YesButtonText = yesButtonText,
                NoButtonText = noButtonText,
                CancelButtonText = cancelButtonText,
                CustomButton1Text = customButton1Text,
                custom1Btn =
                {
                    Visibility = Visibility.Visible
                },
                CustomButton2Text = customButton2Text,
                custom2Btn =
                {
                    Visibility = Visibility.Visible
                },
                CustomButton3Text = customButton3Text,
                custom3Btn =
                {
                    Visibility = Visibility.Visible
                }
            };
            msgBox.ShowDialog();
            return msgBox.Result;
        }





        /// <summary>
        /// Displays a message box that has a message and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="message">A System.String that specifies the text to display.</param>
        /// <returns>A CustomMsgBox.MsgBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MsgBoxResult Show(Window owner, string message)
        {
            var msgBox = new CustomMessageBox(message)
            {
                Owner = owner
            };
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, a caption; and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="message">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the text over the message to display.</param>
        /// <returns>A CustomMsgBox.MsgBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MsgBoxResult Show(Window owner, string message, string caption)
        {
            var msgBox = new CustomMessageBox(message, caption)
            {
                Owner = owner
            };
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, a caption, a buttons and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="message">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the text over the message to display.</param>
        /// <param name="buttons">A CustomMsgBox.MsgBoxButton that specifies buttons to display.</param>
        /// <returns>A CustomMsgBox.MsgBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MsgBoxResult Show(Window owner, string message, string caption, MsgBoxButton buttons)
        {
            var msgBox = new CustomMessageBox(message, caption, buttons)
            {
                Owner = owner
            };
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, a caption, a buttons, an image and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="message">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the text over the message to display.</param>
        /// <param name="buttons">A CustomMsgBox.MsgBoxButton that specifies buttons to display.</param>
        /// <param name="image">A CustomMsgBox.MsgBoxImage that specifies image to display.</param>
        /// <returns>A CustomMsgBox.MsgBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MsgBoxResult Show(Window owner, string message, string caption, MsgBoxButton buttons, MsgBoxImage image)
        {
            var msgBox = new CustomMessageBox(message, caption, buttons, image)
            {
                Owner = owner
            };
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, a caption, a buttons with custom text, an image and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="message">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the text over the message to display.</param>
        /// <param name="buttons">A CustomMsgBox.MsgBoxButton that specifies buttons to display.</param>
        /// <param name="image">A CustomMsgBox.MsgBoxImage that specifies image to display.</param>
        /// <param name="okButtonText">A System.String that specifies the 'OK' button text.</param>
        /// <param name="yesButtonText">A System.String that specifies the 'Yes' button text.</param>
        /// <param name="noButtonText">A System.String that specifies the 'No' button text.</param>
        /// <param name="cancelButtonText">A System.String that specifies the 'Cancel' button text.</param>
        /// <returns>A CustomMsgBox.MsgBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MsgBoxResult Show(Window owner, string message, string caption, MsgBoxButton buttons, MsgBoxImage image, string okButtonText, string yesButtonText, string noButtonText, string cancelButtonText)
        {
            var msgBox = new CustomMessageBox(message, caption, buttons, image)
            {
                Owner = owner,
                OkButtonText = okButtonText,
                YesButtonText = yesButtonText,
                NoButtonText = noButtonText,
                CancelButtonText = cancelButtonText
            };
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, a caption, a buttons with custom text, a custom buttons, an image and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="message">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the text over the message to display.</param>
        /// <param name="buttons">A CustomMsgBox.MsgBoxButton that specifies buttons to display.</param>
        /// <param name="image">A CustomMsgBox.MsgBoxImage that specifies image to display.</param>
        /// <param name="okButtonText">A System.String that specifies the 'OK' button text.</param>
        /// <param name="yesButtonText">A System.String that specifies the 'Yes' button text.</param>
        /// <param name="noButtonText">A System.String that specifies the 'No' button text.</param>
        /// <param name="cancelButtonText">A System.String that specifies the 'Cancel' button text.</param>
        /// <param name="customButton1Text">A System.String that specifies the 1st custom button text.</param>
        /// <returns>A CustomMsgBox.MsgBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MsgBoxResult Show(Window owner, string message, string caption, MsgBoxButton buttons, MsgBoxImage image, string okButtonText, string yesButtonText, string noButtonText, string cancelButtonText, string customButton1Text)
        {
            var msgBox = new CustomMessageBox(message, caption, buttons, image)
            {
                Owner = owner,
                OkButtonText = okButtonText,
                YesButtonText = yesButtonText,
                NoButtonText = noButtonText,
                CancelButtonText = cancelButtonText,
                CustomButton1Text = customButton1Text,
                custom1Btn =
                {
                    Visibility = Visibility.Visible
                }
            };
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, a caption, a buttons with custom text, a custom buttons, an image and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="message">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the text over the message to display.</param>
        /// <param name="buttons">A CustomMsgBox.MsgBoxButton that specifies buttons to display.</param>
        /// <param name="image">A CustomMsgBox.MsgBoxImage that specifies image to display.</param>
        /// <param name="okButtonText">A System.String that specifies the 'OK' button text.</param>
        /// <param name="yesButtonText">A System.String that specifies the 'Yes' button text.</param>
        /// <param name="noButtonText">A System.String that specifies the 'No' button text.</param>
        /// <param name="cancelButtonText">A System.String that specifies the 'Cancel' button text.</param>
        /// <param name="customButton1Text">A System.String that specifies the 1st custom button text.</param>
        /// <param name="customButton2Text">A System.String that specifies the 2nd custom button text.</param>
        /// <returns>A CustomMsgBox.MsgBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MsgBoxResult Show(Window owner, string message, string caption, MsgBoxButton buttons, MsgBoxImage image, string okButtonText, string yesButtonText, string noButtonText, string cancelButtonText, string customButton1Text, string customButton2Text)
        {
            var msgBox = new CustomMessageBox(message, caption, buttons, image)
            {
                Owner = owner,
                OkButtonText = okButtonText,
                YesButtonText = yesButtonText,
                NoButtonText = noButtonText,
                CancelButtonText = cancelButtonText,
                CustomButton1Text = customButton1Text,
                custom1Btn =
                {
                    Visibility = Visibility.Visible
                },
                CustomButton2Text = customButton2Text,
                custom2Btn =
                {
                    Visibility = Visibility.Visible
                }
            };
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, a caption, a buttons with custom text, a custom buttons, an image and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="message">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the text over the message to display.</param>
        /// <param name="buttons">A CustomMsgBox.MsgBoxButton that specifies buttons to display.</param>
        /// <param name="image">A CustomMsgBox.MsgBoxImage that specifies image to display.</param>
        /// <param name="okButtonText">A System.String that specifies the 'OK' button text.</param>
        /// <param name="yesButtonText">A System.String that specifies the 'Yes' button text.</param>
        /// <param name="noButtonText">A System.String that specifies the 'No' button text.</param>
        /// <param name="cancelButtonText">A System.String that specifies the 'Cancel' button text.</param>
        /// <param name="customButton1Text">A System.String that specifies the 1st custom button text.</param>
        /// <param name="customButton2Text">A System.String that specifies the 2nd custom button text.</param>
        /// <param name="customButton3Text">A System.String that specifies the 3rd custom button text.</param>
        /// <returns>A CustomMsgBox.MsgBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MsgBoxResult Show(Window owner, string message, string caption, MsgBoxButton buttons, MsgBoxImage image, string okButtonText, string yesButtonText, string noButtonText, string cancelButtonText, string customButton1Text, string customButton2Text, string customButton3Text)
        {
            var msgBox = new CustomMessageBox(message, caption, buttons, image)
            {
                Owner = owner,
                OkButtonText = okButtonText,
                YesButtonText = yesButtonText,
                NoButtonText = noButtonText,
                CancelButtonText = cancelButtonText,
                CustomButton1Text = customButton1Text,
                custom1Btn =
                {
                    Visibility = Visibility.Visible
                },
                CustomButton2Text = customButton2Text,
                custom2Btn =
                {
                    Visibility = Visibility.Visible
                },
                CustomButton3Text = customButton3Text,
                custom3Btn =
                {
                    Visibility = Visibility.Visible
                }
            };
            msgBox.ShowDialog();
            return msgBox.Result;
        }
    }