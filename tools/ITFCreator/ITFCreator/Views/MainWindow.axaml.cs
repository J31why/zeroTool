using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using FlyrUI.Controls;

namespace ITFCreator.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ITFPathBox_DragHandler(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            var fi = e.DataTransfer.TryGetFile()?.Path.LocalPath;
            if (!string.IsNullOrWhiteSpace(fi))
                ((TextBox?)sender)?.SetValue(TextBox.TextProperty, fi);
        }
        else if (e.DataTransfer.Contains(DataFormat.Text))
        {
            var fi = e.DataTransfer.TryGetText();
            if (!string.IsNullOrWhiteSpace(fi))
                ((TextBox?)sender)?.SetValue(TextBox.TextProperty, fi);
        }
    }
}