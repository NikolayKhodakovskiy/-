using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Linq;
using System.IO;
using System.Windows.Media.Imaging;

namespace EncodingImages
{
    internal class CustomImage
    {
        private Bitmap image;
        private string projectPath;
        private byte[] dataBytes;
        private byte bitshift;

        public CustomImage(Bitmap source, string data, string path, string imageName, string bitPosition)
        {
            image = source;
            dataBytes = Encoding.Unicode.GetBytes(data);
            projectPath = path + @"EncodedImages\" + imageName;
            bitshift = Convert.ToByte(bitPosition);
        }

        public bool IsMessageLengthExceeded()
        {
            return ((image.Width - 1) * image.Height / 3) < dataBytes.Length;
        }

        public Bitmap EncodeTextIntoImage()
        {
            BitArray bits = new BitArray(dataBytes);
            byte bitMask = (byte)(1 << bitshift);
            int bit = bits.Length;
            int counter = 0;
            int x = 1;
            SetPixelColor(0, 0, 1, ReturnBit(bitshift, 4), ReturnBit(bitshift, 2), ReturnBit(bitshift, 1));
            for (; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if (counter == 2)
                    {
                        SetPixelColor(x, y, bitMask, bits[--bit], bits[--bit], false);
                        counter = 0;
                    }
                    else
                    {
                        SetPixelColor(x, y, bitMask, bits[--bit], bits[--bit], bits[--bit]);
                        counter++;
                    }
                    if (bit == 0)
                    {
                        Color endMessagePixel = image.GetPixel(x, y);
                        image.SetPixel(x, y, Color.FromArgb(endMessagePixel.R, endMessagePixel.G, EncodeBitPosition(endMessagePixel.B, true, bitMask)));
                        goto EndLoop;
                    }
                }
            }
            EndLoop:
            image.Save(projectPath);
            return image;
        }

        void SetPixelColor(int x, int y, byte bitMask, params bool[] bits)
        {
            Color pixel = image.GetPixel(x, y);
            byte red = EncodeBitPosition(pixel.R, bits[0], bitMask);
            byte green = EncodeBitPosition(pixel.G, bits[1], bitMask);
            byte blue = EncodeBitPosition(pixel.B, bits[2], bitMask);
            image.SetPixel(x, y, Color.FromArgb(red, green, blue));
        }

        private byte EncodeBitPosition(byte color, bool bit, byte bitMask)
        {
            return (byte)( bit ? (color | bitMask) : (color & (~bitMask)) );
        }

        private static byte GetEncodedBitPosition(Bitmap image)
        {
            string bitShift = "";
            Color pixel = image.GetPixel(0, 0);
            bitShift += Convert.ToByte(ReturnBit(pixel.R, 1));
            bitShift += Convert.ToByte(ReturnBit(pixel.G, 1));
            bitShift += Convert.ToByte(ReturnBit(pixel.B, 1));
            return Convert.ToByte(bitShift, 2);
        }

        public static string DecodeTextFromImage(string imagePath)
        {
            int bit = 0;
            string decodedChar = "";
            string initialMessage = "";
            Bitmap image = new Bitmap(GetImageBitmap(imagePath));
            byte bitMask = (byte)(1 << GetEncodedBitPosition(image));
            for (int x = 1; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixel = image.GetPixel(x, y);
                    decodedChar += Convert.ToByte(ReturnBit(pixel.R, bitMask));
                    decodedChar += Convert.ToByte(ReturnBit(pixel.G, bitMask));
                    byte flaggedBit = Convert.ToByte(ReturnBit(pixel.B, bitMask));
                    if (bit == 2)
                    {
                        bit = 0;
                        if (flaggedBit == 1) goto EndLoop;
                    }
                    else
                    {
                        decodedChar += flaggedBit;
                        bit += 1;
                    }
                    if (decodedChar.Length == 16)
                    {
                        initialMessage += (char)Convert.ToInt16(decodedChar, 2);
                        decodedChar = "";
                    }
                }
            }
            EndLoop:
            if (decodedChar != string.Empty) initialMessage += (char)Convert.ToInt16(decodedChar, 2);

            char[] temp = initialMessage.ToCharArray();
            Array.Reverse(temp);
            return new string(temp);
        }

        private static bool ReturnBit(byte number, byte bitMask)
        {
            return (number & bitMask) != 0;
        }

        public static bool IsExtensionSupported(string extension)
        {
            string[] extensions = { "bmp", "jpg", "jpeg", "png", "tiff" };
            return extensions.Any(ext => ext == extension);
        }

        public static Bitmap GetImageBitmap(string path)
        {
            using (var temp = new Bitmap(path))
            {
                return new Bitmap(temp);
            }
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            stream.Seek(0, SeekOrigin.Begin);
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }
    }
}
