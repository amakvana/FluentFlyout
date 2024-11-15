﻿using MicaWPF.Controls;
using MicaWPF.Core.Extensions;
using System.Windows;
using Windows.UI;
using FluentFlyoutWPF.Classes;
using FluentFlyout.Properties;


namespace FluentFlyoutWPF.Windows;

/// <summary>
/// Interaction logic for LockWindow.xaml
/// </summary>
public partial class LockWindow : MicaWindow
{
    private CancellationTokenSource cts;
    MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

    public LockWindow()
    {
        WindowHelper.SetNoActivate(this);
        InitializeComponent();
        WindowHelper.SetTopmost(this);
        CustomWindowChrome.CaptionHeight = 0;
        CustomWindowChrome.UseAeroCaptionButtons = false;
        CustomWindowChrome.GlassFrameThickness = new Thickness(0);

        WindowStartupLocation = WindowStartupLocation.Manual;
        Left = SystemParameters.WorkArea.Width / 2 - Width / 2;
        cts = new CancellationTokenSource();
        mainWindow.OpenAnimation(this, true);
    }

    private void setStatus(string key, bool isOn)
    {
        Dispatcher.Invoke(() =>
        {
            this.EnableBackdrop();

            LockTextBlock.Text = key + " is " + (isOn ? "on" : "off");
            if (Settings.Default.LockKeysBoldUI) LockTextBlock.FontWeight = FontWeights.Medium;
            else LockTextBlock.FontWeight = FontWeights.Normal;

            if (isOn)
            {
                LockIndicatorRectangle.Opacity = 1;
                if (Settings.Default.LockKeysBoldUI) LockSymbol.Symbol = Wpf.Ui.Controls.SymbolRegular.LockClosed24;
                else LockSymbol.Symbol = Wpf.Ui.Controls.SymbolRegular.LockClosed20;
            }
            else
            {
                LockIndicatorRectangle.Opacity = 0.2;
                if (Settings.Default.LockKeysBoldUI) LockSymbol.Symbol = Wpf.Ui.Controls.SymbolRegular.LockOpen24;
                else LockSymbol.Symbol = Wpf.Ui.Controls.SymbolRegular.LockOpen20;
            }
        });
    }

    public async void ShowLockFlyout(string key, bool isOn)
    {
        setStatus(key, isOn);

        if (Visibility == Visibility.Hidden)
        {
            mainWindow.OpenAnimation(this, true);
        }
        cts.Cancel();
        cts = new CancellationTokenSource();
        var token = cts.Token;
        Visibility = Visibility.Visible;

        try
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(Settings.Default.LockKeysDuration, token);
                mainWindow.CloseAnimation(this, true);
                await Task.Delay(mainWindow.getDuration());
                Hide();
                break;
            }
        }
        catch (TaskCanceledException)
        {
            // do nothing
        }
    }
}