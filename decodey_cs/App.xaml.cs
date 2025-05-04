using System;
using Microsoft.Maui.Controls;

namespace Decodey;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}