namespace DotsAndBoxesUIComponents;

public static class MessageBox
{
    public static MsgBoxResult Show(string message, MsgBoxButton buttons, MsgBoxImage image)
    {
        var msgBox = new CustomMessageBox(message, ResolveCaptionByImageType(image), buttons, image);
        msgBox.ShowDialog();
        return msgBox.Result;
    }

    private static string ResolveCaptionByImageType(MsgBoxImage image)
    {
        return image switch
        {
            MsgBoxImage.None => string.Empty,
            MsgBoxImage.Information => "Информация!",
            MsgBoxImage.Question => string.Empty,
            MsgBoxImage.Warning => "Предупреждение!",
            MsgBoxImage.Error => "Ошибка!",
            _ => throw new ArgumentOutOfRangeException(nameof(image), image, null)
        };
    }
}