using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final
{
    public class Repository
    {
        public readonly static List<Paciente> Pacientes = new List<Paciente>();

        public static void GuardarPaciente(Paciente paciente)
        {
            Pacientes.Add(paciente);
        }

        public static byte[] ConvertImgToBinary(Image Img)
        {
            var bmp = new Bitmap(Img);
            var MyStream = new MemoryStream();
            bmp.Save(MyStream, ImageFormat.Bmp);

            return MyStream.ToArray();
        }

        public static Image ConvertByteToImg(byte [] bytes)
        {
            var ms = new MemoryStream(bytes);
            var image = Image.FromStream(ms);
            ms.Close();

            return image;

        }
    }
}
