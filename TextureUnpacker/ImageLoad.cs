using System;
using System.Drawing;

namespace TextureUnpacker
{
    public static class ImageLoad
    {
        public static Bitmap FileToBitmap(string filePath)
        {
            Bitmap bitmap;
            try
            {
                Image img = Image.FromFile(filePath);
                bitmap = new Bitmap(img);
            }
            catch (OutOfMemoryException)
            {
                if (filePath.EndsWith(".tga"))
                {
                    bitmap = Paloma.TargaImage.LoadTargaImage(filePath);
                }
                else
                {
                    throw new InvalidOperationException("图像格式不正确");
                }
            }

            return bitmap;
        }

    }
}


