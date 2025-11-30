using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlyrUI.Toasts;
using ITFCreator.ITF;
using SkiaSharp;

namespace ITFCreator.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private ITFReader? _reader;
    private MemoryStream? _stream;
    public ToastManager ToastManager { get; set; } = new();
    
    [ObservableProperty] private string? _ITFPath = "E:\\Games\\The Legend of Heroes Trails from Zero\\data\\system\\fontdat\\font.itf";
    [ObservableProperty] private string? _ITFInfo;
    [ObservableProperty] private string? _charSearchResult;
    [ObservableProperty] private string? _searchText;
    [ObservableProperty] private bool _ITFLoaded;
    //bitmap
    [ObservableProperty] private Bitmap? _bitmap;
    [ObservableProperty] private double _bitmapSize;
    [ObservableProperty] private double _bitmapX;
    [ObservableProperty] private double _bitmapY;
    [ObservableProperty] private double _bitmapWidth;
    [ObservableProperty] private double _bitmapHeight;

    [ObservableProperty]private SKFontStyleWeight _fontWeight = SKFontStyleWeight.Normal;
    [ObservableProperty]private double _fontSize = 72;
    [ObservableProperty]private float _baselineOffset = 0;
    [ObservableProperty] private string? _fontName = "霞鹜臻楷 GB";
    
    [ObservableProperty] private string? _unicodeRange ="0x20-0xFFFF";
    public SKFontStyleWeight[] FontWeights =>
    [
        SKFontStyleWeight.Thin ,
        SKFontStyleWeight.ExtraLight ,
        SKFontStyleWeight.Light ,
        SKFontStyleWeight.Normal ,
        SKFontStyleWeight.Medium ,
        SKFontStyleWeight.SemiBold , 
        SKFontStyleWeight.Bold ,
        SKFontStyleWeight.ExtraBold ,
        SKFontStyleWeight.Black ,
        SKFontStyleWeight.ExtraBlack
    ];

    [RelayCommand]
    private void OpenITFFile()
    {
        
        if (!File.Exists(ITFPath) || Path.GetExtension(ITFPath) != ".itf") 
        {
            ToastManager.CreateToast()
                .WithTitle("ITFCreator")
                .WithContent("文件不符。")
                .WithDismissStyle(TimeSpan.FromSeconds(5))
                .Queue();
            return;
        }

        var bytes = File.ReadAllBytes(ITFPath);
        LoadITF(bytes);
    }

    private void LoadITF(byte[] bytes)
    {
        _reader?.Dispose();
        _stream = new MemoryStream(bytes);
        _reader = new ITFReader(_stream);
        ITFInfo = $"分辨率: {_reader.Header.Resolution}, 字符数: {_reader.Header.ActualNodeCount}, 总节点数: {_reader.Header.TotalNodeCount}";
        BitmapSize = 100;
        ToastManager.CreateToast()
            .WithTitle("ITFCreator")
            .WithContent("ITF文件已载入。")
            .WithDismissStyle(TimeSpan.FromSeconds(5))
            .Queue();
        ITFLoaded = true;
    }
    
    [RelayCommand]
    private void SearchChar()
    {
        if(string.IsNullOrEmpty(SearchText)|| _reader == null) return;
        var code = SearchText.First();
        if (SearchText.StartsWith("0x"))
        {
            code = (char)Convert.ToInt32(SearchText.Replace("0x", ""), 16);
        }
        var index = _reader.Search(code);
        ITFChar? ch = null;
        if (index == -1 || (ch = _reader.GetChar(index)) == null || ch.Data is null) 
        {
            CharSearchResult = "搜索失败";
            Bitmap?.Dispose();
            Bitmap = null;
            BitmapX = 0;
            BitmapY = 0;
            BitmapWidth = 0;
            BitmapHeight = 0;
            return;
        }
        CharSearchResult =
            $"{code} : Code: 0x{(int)code:X}, Index: 0x{index:X}, Node Offset: 0x{Marshal.SizeOf<ITFHeader>() + index * 8:X}, {ch}";
        Bitmap?.Dispose();
        Bitmap = _reader.CharDataToBitmap(ch.Data,ch.PixelWidth,ch.PixelHeight,255,0,255);
        BitmapX = ch.Left;
        BitmapY = ch.Top;
        BitmapWidth = ch.Width;
        BitmapHeight = ch.PixelHeight;
    }

    [RelayCommand]
    private void CreateITFFile()
    {
        if (string.IsNullOrEmpty(FontName) || string.IsNullOrEmpty(UnicodeRange))
            return;
        
        using var ms = new MemoryStream();
        using var writer = new ITFWriter(ms);
        var res = writer.Build(UnicodeRange, FontName, (int)FontSize, FontWeight,BaselineOffset);
        if (!res)
        {
            ToastManager.CreateToast()
                .WithTitle("ITFCreator")
                .WithContent("ITF创建失败。")
                .WithDismissStyle(TimeSpan.FromSeconds(5))
                .Queue();
            return;
        }
        ToastManager.CreateToast()
            .WithTitle("ITFCreator")
            .WithContent($"ITF创建成功。")
            .WithDismissStyle(TimeSpan.FromSeconds(5))
            .Queue();
        ms.Seek(0, SeekOrigin.Begin);
        if(!Directory.Exists("output"))
            Directory.CreateDirectory("output");
        File.WriteAllBytes("output\\font.itf", ms.ToArray());
    }

    [RelayCommand]
    private void OpenOutputDirectory()
    {
        if(!Directory.Exists("output"))
            Directory.CreateDirectory("output");
        Process.Start("explorer",Path.Combine(Environment.CurrentDirectory,"output"));
    }
}