using System.Windows.Input;
using MealPlanner.UI.Mobile.Views.Controls.Resources;

namespace MealPlanner.UI.Mobile.Views.Controls;

public partial class PasswordEntry : ContentView
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(PasswordEntry), default(string), BindingMode.TwoWay);

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(PasswordEntry));

    public static readonly BindableProperty ReturnTypeProperty = BindableProperty.Create(
        nameof(ReturnType), typeof(ReturnType), typeof(PasswordEntry), ReturnType.Default);

    public static readonly BindableProperty ReturnCommandProperty = BindableProperty.Create(
        nameof(ReturnCommand), typeof(ICommand), typeof(PasswordEntry));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public ReturnType ReturnType
    {
        get => (ReturnType)GetValue(ReturnTypeProperty);
        set => SetValue(ReturnTypeProperty, value);
    }

    public ICommand ReturnCommand
    {
        get => (ICommand)GetValue(ReturnCommandProperty);
        set => SetValue(ReturnCommandProperty, value);
    }

    public PasswordEntry()
    {
        InitializeComponent();
        UpdateToggleState();
    }

    private void OnToggleClicked(object sender, EventArgs e)
    {
        PasswordField.IsPassword = !PasswordField.IsPassword;
        UpdateToggleState();
    }

    private void UpdateToggleState()
    {
        var isPasswordHidden = PasswordField.IsPassword;
        ToggleButton.Text = isPasswordHidden ? "\U0001F441" : "\U0001F648";

        var description = isPasswordHidden
            ? PasswordEntryStrings.ShowPasswordAction
            : PasswordEntryStrings.HidePasswordAction;
        SemanticProperties.SetDescription(ToggleButton, description);
        ToolTipProperties.SetText(ToggleButton, description);
    }
}
