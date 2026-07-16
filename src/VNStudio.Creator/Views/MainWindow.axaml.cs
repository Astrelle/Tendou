using System;
using Avalonia.Controls;
using VNStudio.Creator.ViewModels;

namespace VNStudio.Creator.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is MainViewModel viewModel)
        {
            viewModel.StorageProvider = this.StorageProvider;
        }
    }
}