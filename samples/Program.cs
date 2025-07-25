﻿using System;
using System.Device.I2c;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.IO;
using Iot.Device.Arduino;
using Iot.Device.Graphics;
using Iot.Device.Graphics.SkiaSharpAdapter;
using Iot.Device.Ssd13xx;

/// <summary>
/// Demo entry point
/// </summary>
public class Program
{
    Ssd1306 SSD1306 { get; set; }
    /// <summary>
    /// Entry point
    /// </summary>
    public static int Main(string[] args)
    {
        Console.WriteLine("Hello Ssd1306 Sample!");
        SkiaSharpAdapter.Register();

        Program program = new Program();
        program.Run(args);

        return 0;
    }

    private void Run(string[] args)
    {
        ArduinoBoard board = null;
        Ssd13xx device;
        Console.WriteLine("Using direct I2C protocol");

        I2cDevice i2cDevice = null;

        if (args.Any(x => x == "--arduino"))
        {
            board = new ArduinoBoard("COM4", 115200);
            I2cConnectionSettings connectionSettings = new(0, 0x3C);
            i2cDevice = board.CreateI2cDevice(connectionSettings);
        }
        else
        {
            I2cConnectionSettings connectionSettings = new(1, 0x3C);
            i2cDevice = I2cDevice.Create(connectionSettings);
        }

        if (args.Any(x => x == "--1327"))
        {
            var device1 = new Ssd1327(i2cDevice);
            device = device1;
        }
        else
        {
            Console.WriteLine("Tappin' the SSD1306...");
            try
            {
                SSD1306 = new Ssd1306(i2cDevice, 128, 32);
                device = SSD1306;
            }
            catch { Exception ex; }
        }

        SSD1306.ClearScreen();
        DisplayImages(SSD1306);
        DisplayClock(SSD1306);
        SSD1306.ClearScreen();
        //SendMessage(device, "Hello .NET IoT!!!");
        SSD1306.Dispose();
        board.Dispose();
    }

    private void DisplayImages(GraphicDisplay ssd1306)
    {
        Console.WriteLine("Display Images");
        foreach (var image_name in Directory.GetFiles("images", "*.bmp").OrderBy(f => f))
        {
            using BitmapImage image = BitmapImage.CreateFromFile(image_name);
            ssd1306.DrawBitmap(image);
            Thread.Sleep(1000);
        }
    }

    private void DisplayClock(GraphicDisplay ssd1306)
    {
        Console.WriteLine("Display clock");
        var fontSize = 25;
        var font = "DejaVu Sans";
        var y = 0;

        while (!Console.KeyAvailable)
        {
            using (var image = BitmapImage.CreateBitmap(128, 32, PixelFormat.Format32bppArgb))
            {
                image.Clear(Color.Black);
                var g = image.GetDrawingApi();
                g.DrawText(DateTime.Now.ToString("HH:mm:ss"), font, fontSize, Color.White, new Point(0, y));
                ssd1306.DrawBitmap(image);

                y++;
                if (y >= image.Height)
                {
                    y = 0;
                }

                Thread.Sleep(100);
            }
        }

        Console.ReadKey(true);
    }
}
