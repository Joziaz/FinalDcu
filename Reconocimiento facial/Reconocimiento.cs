using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Data.OleDb;
using System.Speech.Synthesis;
using System.Media;
using System.Runtime.InteropServices;

namespace Final
{
    public partial class Reconocimiento : Form
    {
        #region Dlls para poder hacer el movimiento del Form
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        Rectangle sizeGripRectangle;
        bool inSizeDrag = false;
        const int GRIP_SIZE = 15;

        int w = 0;
        int h = 0;
        #endregion

        public int heigth, width;

        public Reconocimiento()
        {
            InitializeComponent();
            buscarPorSelect.SelectedIndex = 0;
        }

        private void SetGripRectangle()
        {
            sizeGripRectangle = new Rectangle(
                       this.Width - GRIP_SIZE,
                       this.Height - GRIP_SIZE, GRIP_SIZE, GRIP_SIZE);
        }

        private bool IsInSizeGrip(Point tmp)
        {
            if (tmp.X >= sizeGripRectangle.X
              && tmp.X <= this.Width
              && tmp.Y >= sizeGripRectangle.Y
              && tmp.Y <= this.Height
                )
                return true;
            else
                return false;
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
               
        private void btn_minimize_Click_1(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btn_close_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            if (txtBuscar.Text == string.Empty)
            {
                MessageBox.Show("El campo de busqueda esta vacio");
                return;
            }

            Buscar();
        }

        private void txtBuscar_MouseEnter(object sender, EventArgs e)
        {

        }

        private void txtBuscar_Enter(object sender, EventArgs e)
        {

        }

        private void btn_maximize_Click(object sender, EventArgs e)
        {
            StateWin();
        }

        private void imageBoxFrameGrabber_Click(object sender, EventArgs e)
        {

        }

        private void StateWin()
        {

            if (this.btn_maximize.Text == "1")
            {
                this.btn_maximize.Text = "2";
                this.Location = new Point(0, 0);
                this.Size = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
            }
            else if (this.btn_maximize.Text == "2")
            {
                this.btn_maximize.Text = "1";
                this.Size = new Size(width, heigth);
                this.StartPosition = FormStartPosition.CenterScreen;
            }
        }

        private void menuStrip1_MouseDown(object sender, MouseEventArgs e)
        {
                //para poder arrastrar el formulario sin bordes
                ReleaseCapture();
                SendMessage(this.Handle, 0x112, 0xf012, 0);
                w = this.Width;
                h = this.Height;
        }
        private void Buscar()
        {
            Paciente paciente;

            if(buscarPorSelect.Text == "Nombre")
                paciente = Repository.Pacientes.FirstOrDefault(x => x.Nombre == txtBuscar.Text);
            else
                paciente = Repository.Pacientes.FirstOrDefault(x => x.Cedula == txtBuscar.Text);

            if (paciente == null)
            {
                MessageBox.Show("No existe un paciente con ese nombre");
                return;
            }

            txtNombre.Text = paciente.Nombre;
            txtInformacion.Text = paciente.Informacion;
            txtCedula.Text = paciente.Cedula;
            imagen.Image = Repository.ConvertByteToImg(paciente.Foto);
        }

        private void label7_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("documentacion.pdf");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Registrar r = new Registrar();
            r.ShowDialog();
        }
    }
}
