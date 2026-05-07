using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace CourseWork.Helpers;

public static class MessageBox
{
    public enum MessageBoxButtons { OK, YesNo }
    public enum MessageBoxResult { OK, Yes, No }

    public static async Task<MessageBoxResult> ShowAsync(
        Window owner, 
        string message, 
        string title, 
        MessageBoxButtons buttons = MessageBoxButtons.OK)
    {
        var window = new Window
        {
            Title = title,
            Width = 400,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 15,
                Children =
                {
                    new TextBlock 
                    { 
                        Text = message, 
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Margin = new Avalonia.Thickness(0, 20, 0, 0)
        };

        MessageBoxResult result = MessageBoxResult.OK;

        if (buttons == MessageBoxButtons.YesNo)
        {
            var btnNo = new Button 
            { 
                Content = "Нет", 
                Width = 80,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#6C757D")),
                Foreground = Avalonia.Media.Brushes.White
            };
            btnNo.Click += (s, e) => { result = MessageBoxResult.No; window.Close(); };
            buttonPanel.Children.Add(btnNo);

            var btnYes = new Button 
            { 
                Content = "Да", 
                Width = 80,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0F4B5E")),
                Foreground = Avalonia.Media.Brushes.White
            };
            btnYes.Click += (s, e) => { result = MessageBoxResult.Yes; window.Close(); };
            buttonPanel.Children.Add(btnYes);
        }
        else
        {
            var btnOk = new Button 
            { 
                Content = "OK", 
                Width = 80,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0F4B5E")),
                Foreground = Avalonia.Media.Brushes.White
            };
            btnOk.Click += (s, e) => { result = MessageBoxResult.OK; window.Close(); };
            buttonPanel.Children.Add(btnOk);
        }

        ((StackPanel)window.Content).Children.Add(buttonPanel);

        await window.ShowDialog(owner);
        return result;
    }
}