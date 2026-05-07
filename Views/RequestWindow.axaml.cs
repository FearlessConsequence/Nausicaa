using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CourseWork.Views;

public partial class RequestReasonWindow : Window
{
    private string? _result;

    public RequestReasonWindow()
    {
        InitializeComponent();
        
        btn_submit.Click += OnSubmit;
        btn_cancel.Click += (s, e) => Close();
    }

    private void OnSubmit(object? sender, RoutedEventArgs e)
    {
        _result = txt_reason.Text?.Trim();
        if (string.IsNullOrWhiteSpace(_result))
        {
            var errorText = new TextBlock { Text = "Пожалуйста, укажите причину", Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#DC3545")), FontSize = 11 };
        }
        else
        {
            Close(_result);
        }
    }
}