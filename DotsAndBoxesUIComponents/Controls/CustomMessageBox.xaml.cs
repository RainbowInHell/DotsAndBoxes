﻿using System.Windows;

namespace DotsAndBoxesUIComponents;

public enum MsgBoxResult
{
    None,
    OK,
    Yes,
    No,
    Cancel,
    CustomResult1,
    CustomResult2,
    CustomResult3
}

public enum MsgBoxButton
{
    None,
    OK,
    OKCancel,
    YesNo,
    YesNoCancel
}

public enum MsgBoxImage
{
    None,
    Information,
    Question,
    Warning,
    Error
}

public partial class CustomMessageBox
{
    public MsgBoxResult Result { get; private set; }

    internal string Caption
    {
        get => captionTxtBlock.Text;
        set => captionTxtBlock.Text = value;
    }

    internal string Message
    {
        get => messageTxtBlock.Text;
        set => messageTxtBlock.Text = value;
    }

    internal string OkButtonText
    {
        get => okBtn.Content.ToString();
        set => okBtn.Content = value;
    }

    internal string YesButtonText
    {
        get => yesBtn.Content.ToString();
        set => yesBtn.Content = value;
    }

    internal string NoButtonText
    {
        get => noBtn.Content.ToString();
        set => noBtn.Content = value;
    }

    internal string CancelButtonText
    {
        get => cancelBtn.Content.ToString();
        set => cancelBtn.Content = value;
    }

    internal string CustomButton1Text
    {
        get => custom1Btn.Content.ToString();
        set => custom1Btn.Content = value;
    }

    internal string CustomButton2Text
    {
        get => custom2Btn.Content.ToString();
        set => custom2Btn.Content = value;
    }

    internal string CustomButton3Text
    {
        get => custom3Btn.Content.ToString();
        set => custom3Btn.Content = value;
    }

    public CustomMessageBox(string message)
    {
        InitializeComponent();
        Message = message;
        messageTxtBlock.Visibility = Visibility.Visible;
        SetUpButtons(MsgBoxButton.OK);
    }

    public CustomMessageBox(string message, MsgBoxButton buttons, MsgBoxImage image)
    {
        InitializeComponent();
        Message = message;
        captionTxtBlock.Visibility = Visibility.Visible;
        messageTxtBlock.Visibility = Visibility.Visible;
        SetUpButtons(buttons);
        SetUpImage(image);
    }

    public CustomMessageBox(string message, string caption)
    {
        InitializeComponent();
        Message = message;
        Caption = caption;
        captionTxtBlock.Visibility = Visibility.Visible;
        messageTxtBlock.Visibility = Visibility.Visible;
        SetUpButtons(MsgBoxButton.OK);
    }

    public CustomMessageBox(string message, string caption, MsgBoxButton buttons)
    {
        InitializeComponent();
        Message = message;
        Caption = caption;
        captionTxtBlock.Visibility = Visibility.Visible;
        messageTxtBlock.Visibility = Visibility.Visible;
        SetUpButtons(buttons);
    }

    public CustomMessageBox(string message, string caption, MsgBoxButton buttons, MsgBoxImage image)
    {
        InitializeComponent();
        Message = message;
        Caption = caption;
        captionTxtBlock.Visibility = Visibility.Visible;
        messageTxtBlock.Visibility = Visibility.Visible;
        SetUpButtons(buttons);
        SetUpImage(image);
    }
    
    private void SetUpButtons(MsgBoxButton buttons)
    {
        switch (buttons)
        {
            case MsgBoxButton.None:
            {
                break;
            }
            case MsgBoxButton.OKCancel:
            {
                okBtn.Visibility = Visibility.Visible;
                cancelBtn.Visibility = Visibility.Visible;
                okBtn.Focus();
                break;
            }
            case MsgBoxButton.YesNoCancel:
            {
                yesBtn.Visibility = Visibility.Visible;
                noBtn.Visibility = Visibility.Visible;
                cancelBtn.Visibility = Visibility.Visible;
                yesBtn.Focus();
                break;
            }
            case MsgBoxButton.YesNo:
            {
                yesBtn.Visibility = Visibility.Visible;
                noBtn.Visibility = Visibility.Visible;
                yesBtn.Focus();
                break;
            }
            case MsgBoxButton.OK:
            default:
            {
                okBtn.Visibility = Visibility.Visible;
                okBtn.Focus();
                break;
            }
        }
    }

    private void SetUpImage(MsgBoxImage image)
    {            
        switch (image)
        {
            case MsgBoxImage.None:
            {
                break;
            }
            case MsgBoxImage.Question:
            {
                questionImage.Visibility = Visibility.Visible;
                break;
            }
            case MsgBoxImage.Information:
            {
                informationImage.Visibility = Visibility.Visible;
                break;
            }
            case MsgBoxImage.Warning:
            {
                warningImage.Visibility = Visibility.Visible;
                break;
            }
            case MsgBoxImage.Error:
            {
                errorImage.Visibility = Visibility.Visible;
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(image), image, null);
            }
        }
    }

    private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void custom1Btn_Click(object sender, RoutedEventArgs e)
    {
        Result = MsgBoxResult.CustomResult1;
        Close();
    }

    private void custom2Btn_Click(object sender, RoutedEventArgs e)
    {
        Result = MsgBoxResult.CustomResult2;
        Close();
    }

    private void custom3Btn_Click(object sender, RoutedEventArgs e)
    {
        Result = MsgBoxResult.CustomResult3;
        Close();
    }

    private void okBtn_Click(object sender, RoutedEventArgs e)
    {
        Result = MsgBoxResult.OK;
        Close();
    }

    private void yesBtn_Click(object sender, RoutedEventArgs e)
    {
        Result = MsgBoxResult.Yes;
        Close();
    }

    private void noBtn_Click(object sender, RoutedEventArgs e)
    {
        Result = MsgBoxResult.No;
        Close();
    }

    private void cancelBtn_Click(object sender, RoutedEventArgs e)
    {
        Result = MsgBoxResult.Cancel;
        Close();
    }

    private void closeBtn_Click(object sender, RoutedEventArgs e)
    {
        Result = MsgBoxResult.None;
        Close();
    }
}