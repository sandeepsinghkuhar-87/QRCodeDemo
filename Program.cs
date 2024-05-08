using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using ZXing;
using ZXing.Common;
using static System.Net.Mime.MediaTypeNames;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to QR Code Generator and Scanner!");

        while (true)
        {
            Console.WriteLine("\nChoose an option:");
            Console.WriteLine("1. Generate QR code");
            Console.WriteLine("2. Scan QR code");
            Console.WriteLine("3. Exit");

            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                switch (choice)
                {
                    case 1:
                        GenerateQRCode();
                        break;
                    case 2:
                        ScanQRCode();
                        break;
                    case 3:
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please choose a valid option.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a number.");
            }
        }
    }

    static void GenerateQRCode()
    {
        Console.WriteLine("\nEnter the text payload for the QR code:");
        string textPayload = Console.ReadLine();

        // Generate a random file name using Guid
        string fileName = Guid.NewGuid().ToString() + ".png";

        var barcodeWriter = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new EncodingOptions
            {
                Height = 300,
                Width = 300
            }
        };

        var pixelData = barcodeWriter.Write(textPayload);

        // Create a Bitmap from pixel data
        using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
        {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            try
            {
                // Copy pixel data to Bitmap
                Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }

            // Save the Bitmap to a PNG file
            bitmap.Save(fileName, ImageFormat.Png);
        }

        Console.WriteLine($"QR code generated successfully and saved as '{fileName}'.");
    }

    static void ScanQRCode()
    {
        Console.WriteLine("\nEnter the path to the PNG file containing the QR code:");
        string imagePath = Console.ReadLine();

        if (File.Exists(imagePath))
        {
            var barcodeReader = new BarcodeReader<Bitmap>((bitmap) =>
            {
                // Convert Bitmap to byte array
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int bytes = Math.Abs(bitmapData.Stride) * bitmap.Height;
                byte[] rgbValues = new byte[bytes];
                Marshal.Copy(bitmapData.Scan0, rgbValues, 0, bytes);
                bitmap.UnlockBits(bitmapData);

                // Create RGBLuminanceSource from byte array
                return new RGBLuminanceSource(rgbValues, bitmap.Width, bitmap.Height, RGBLuminanceSource.BitmapFormat.RGB32);
            });

            var bitmap = (Bitmap)System.Drawing.Image.FromFile(imagePath);
            var result = barcodeReader.Decode(bitmap);

            if (result != null)
            {
                Console.WriteLine($"QR code content: {result.Text}");
            }
            else
            {
                Console.WriteLine("No QR code found in the provided image.");
            }
        }
        else
        {
            Console.WriteLine("File not found. Please provide a valid file path.");
        }
    }
}
