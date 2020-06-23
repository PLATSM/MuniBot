using System;
using System.IO;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;

namespace MuniBot.Common
{
    public class Funciones
    {
        public static string GetSHA256(string str)
        {
            SHA256 sha256 = SHA256Managed.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = null;
            StringBuilder sb = new StringBuilder();
            stream = sha256.ComputeHash(encoding.GetBytes(str));
            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
            return sb.ToString();
        }
        public static byte[] GetBytes(string str)
        {
            //byte[] bytes = Encoding.ASCII.GetBytes(str);
            //byte[] bytes = Convert.FromBase64String(str);

            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        static string GetString(byte[] bytes)
        {
            //string str = Encoding.ASCII.GetString(bytes);
            //string str = Convert.ToBase64String(bytes);

            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public void StringToImage(byte[] bytes)
        {
            var base64 = Convert.ToBase64String(bytes);
            var imgSrc = String.Format("data:image/gif;base64,{0}", base64);

            //string mimeType = /* Get mime type somehow (e.g. "image/png") */;
            //string base64 = Convert.ToBase64String(yourImageBytes);
            //return string.Format("data:{0};base64,{1}", mimeType, base64);
        }
        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }
        public Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        public byte[] ImageToByteArray(System.Drawing.Image images)
        {
            using (var _memorystream = new MemoryStream())
            {
                images.Save(_memorystream, images.RawFormat);
                return _memorystream.ToArray();
            }
        }
        public void SaveImage(string base64String, string filepath)
        {
            // image convert to base64string is base64String 
            //File path is which path to save the image.
            var bytess = Convert.FromBase64String(base64String);
            using (var imageFile = new FileStream(filepath, FileMode.Create))
            {
                imageFile.Write(bytess, 0, bytess.Length);
                imageFile.Flush();
            }
        }
        //public Image byteArrayToImage(byte[] byteArrayIn)
        //{
        //    try
        //    {
        //        MemoryStream ms = new MemoryStream(byteArrayIn, 0, byteArrayIn.Length);
        //        ms.Write(byteArrayIn, 0, byteArrayIn.Length);
        //        var returnImage = Image.FromStream(ms, true);//Exception occurs here 
        //    }
        //    catch { }
        //    return returnImage;

        //    //MemoryStream ms = new MemoryStream(byteArrayIn, 0, byteArrayIn.Length);
        //    //ms.Position = 0; // this is important 
        //    //returnImage = Image.FromStream(ms, true);
        //}

        public Image ConvertByteArrayToImage(byte[] byteArrayIn)
        {
            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                return Image.FromStream(ms);
            }
        }
        public byte[] ConvertImageToByteArray(Image image, string extension)
        {
            using (var memoryStream = new MemoryStream())
            {
                switch (extension)
                {
                    case ".jpeg":
                    case ".jpg":
                        image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case ".png":
                        image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    case ".gif":
                        image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                }
                return memoryStream.ToArray();
            }
        }
    }


}
