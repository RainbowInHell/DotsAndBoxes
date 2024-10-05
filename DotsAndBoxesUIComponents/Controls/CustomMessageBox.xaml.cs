using System.Windows;
using System.Windows.Media;

namespace DotsAndBoxesUIComponents;

public enum MessageType
{
    Info,
    Confirmation,
    Success,
    Warning,
    Error
}

public enum MessageButtons
{
    OkCancel,
    YesNo,
    Ok
}

public partial class CustomMessageBox : Window
{
    public CustomMessageBox(string message, MessageType type, MessageButtons buttons)
    {
        InitializeComponent();
        txtMessage.Content = message;

        switch (type)
        {
            case MessageType.Info:
                txtTitle.Text = "Информация";
                break;
            case MessageType.Confirmation:
                txtTitle.Text = "Подтверждение";
                break;
            case MessageType.Success:
            {
                var defaultColor = "#4527a0";
                var bkColor = (Color)ColorConverter.ConvertFromString(defaultColor);
                ChangeBackgroundThemeColor(Colors.Green);
                txtTitle.Text = "Успех";
            }
                break;
            case MessageType.Warning:
                txtTitle.Text = "Предупреждение";
                break;
            case MessageType.Error:
            {
                var defaultColor = "#F44336";
                var bkColor = (Color)ColorConverter.ConvertFromString(defaultColor);
                ChangeBackgroundThemeColor(bkColor);
                ChangeBackgroundThemeColor(Colors.Red);
                txtTitle.Text = "Ошибка";
            }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        switch (buttons)
        {
            case MessageButtons.OkCancel:
                btnYes.Visibility = Visibility.Collapsed;
                btnNo.Visibility = Visibility.Collapsed;
                break;
            case MessageButtons.YesNo:
                btnOk.Visibility = Visibility.Collapsed;
                btnCancel.Visibility = Visibility.Collapsed;
                break;
            case MessageButtons.Ok:
                btnOk.Visibility = Visibility.Visible;
                btnCancel.Visibility = Visibility.Collapsed;
                btnYes.Visibility = Visibility.Collapsed;
                btnNo.Visibility = Visibility.Collapsed;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(buttons), buttons, null);
        }
    }

    private void ChangeBackgroundThemeColor(Color newColor)
    {
        cardHeader.Background = new SolidColorBrush(newColor);
        btnClose.Foreground = new SolidColorBrush(newColor);
        btnYes.Background = new SolidColorBrush(newColor);
        btnNo.Background = new SolidColorBrush(newColor);

        btnOk.Background = new SolidColorBrush(newColor);
        btnCancel.Background = new SolidColorBrush(newColor);
    }

    private void btnYes_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void btnNo_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}