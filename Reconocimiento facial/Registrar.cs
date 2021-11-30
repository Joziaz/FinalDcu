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
using System.Runtime.InteropServices;

namespace Final
{
    public partial class Registrar : Form
    {
        #region Dlls para poder hacer el movimiento del Form
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);
        
        int w = 0;
        int h = 0;

        Rectangle sizeGripRectangle;
        bool inSizeDrag = false;
        const int GRIP_SIZE = 25; 
        #endregion

        public int heigth, width;

        public string[] Labels;
        int con = 0,ini=0,fin;
        //DECLARANDO TODAS LAS VARIABLES, vectores y  haarcascades
        Image<Bgr, Byte> currentFrame;
        Capture grabber;         
        HaarCascade face;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        Image<Bgr, byte> result, TrainedFace = null;
        Image<Bgr, byte> color = null;
        List<Image<Bgr, byte>> trainingImages = new List<Image<Bgr, byte>>();
        List<string> labels = new List<string>();
        List<string> labels1 = new List<string>();
        List<string> NamePersons = new List<string>();
        int ContTrain, NumLabels, t;
        string name;
        
        public Registrar()
        {
            InitializeComponent();
            heigth = this.Height; width = this.Width;
            //GARGAMOS LA DETECCION DE LAS CARAS POR  haarcascades 
            face = new HaarCascade("haarcascade_frontalface_default.xml");
            try
            {                
                foreach (var paciente in Repository.Pacientes)
                {
                    var bmp = new Bitmap(Repository.ConvertByteToImg(paciente.Foto));
                    trainingImages.Add(new Image<Bgr, byte>(bmp));//cargo la foto con ese nombre
                }

                grabber = new Capture();
                grabber.QueryFrame();

                //Inicia el evento FrameGraber
                Application.Idle += new EventHandler(FrameGrabber);
            }
            catch (Exception e)
            {//Si la variable NumLabels es 0 me presenta el msj
                MessageBox.Show(e + " No hay ningún rostro en la Base de Datos, por favor añadir por lo menos una cara", "Cragar caras en tu Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
       
        void FrameGrabber(object sender, EventArgs e)
        {
            NamePersons.Add("");
            try
            {

                //Obtener la secuencia del dispositivo de captura
                try
                {
                    currentFrame = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                }
                catch (Exception)
                {                    
                    imageBoxFrameGrabber.Image = null;
                }

                //Convertir a escala de grises
                color = currentFrame.Convert<Bgr, Byte>();

                //Detector de Rostros
                MCvAvgComp[][] facesDetected = color.DetectHaarCascade(face, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));

                //Accion para cada elemento detectado
                foreach (MCvAvgComp f in facesDetected[0])
                {
                    t = t + 1;
                    result = currentFrame.Copy(f.rect).Convert<Bgr, byte>().Resize(640, 480, INTER.CV_INTER_CUBIC);
                    //Dibujar el cuadro para el rostro
                    currentFrame.Draw(f.rect, new Bgr(Color.DarkGreen), 2);

                    NamePersons[t - 1] = name;
                    NamePersons.Add("");
                    //Establecer el nùmero de rostros detectados
                    //lblNadie.Text = name;

                }
                t = 0;
                
                //Mostrar los rostros procesados y reconocidos
                imageBoxFrameGrabber.Image = currentFrame;
                name = "";
                //Borrar la lista de nombres            
                NamePersons.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btn_minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            Application.Idle -= new EventHandler(FrameGrabber);
            grabber.Dispose();
            this.Close();
        }

        private void menuStrip1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
            w = this.Width;
            h = this.Height;
        }

        private void btn_agregar_Click(object sender, EventArgs e)
        {
            try
            {
                ContTrain = ContTrain + 1;

                color = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                MCvAvgComp[][] facesDetected = color.DetectHaarCascade(face, 1.2, 10,Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,new Size(20, 20));

                foreach (MCvAvgComp f in facesDetected[0])
                {
                    TrainedFace = currentFrame.Copy(f.rect).Convert<Bgr, byte>();
                    break;
                }

                TrainedFace = result.Resize(250, 250, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                trainingImages.Add(TrainedFace);
                labels.Add(txt_nombre.Text);

                imageBox2.Image = TrainedFace;
                var imageBytes= Repository.ConvertImgToBinary(imageBox2.Image.Bitmap);
                var paciente = new Paciente()
                {
                    Cedula = txtCedula.Text,
                    Nombre = txt_nombre.Text,
                    Informacion = txt_informacion.Text,
                    Foto = imageBytes
                };
                Repository.GuardarPaciente(paciente);
                MessageBox.Show("Agregado correctamente", "Capturado", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
              
        private void button3_Click(object sender, EventArgs e)
        {
            imageBox2.Image = null;
            this.txt_informacion.Clear();
            this.txt_nombre.Clear();
        }

        private void txtCedula_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Application.Idle -= new EventHandler(FrameGrabber);//Detenemos el evento de captura
                grabber.Dispose();//Dejamos de usar la clase para capturar usar los dispositivos
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Resgistrar_Load(object sender, EventArgs e)
        {
            #region[Metodo deredimension de formulario sin borde]

            SetGripRectangle();
            this.Paint += (o, ea) => { ControlPaint.DrawSizeGrip(ea.Graphics, this.BackColor, sizeGripRectangle); };

            this.MouseUp += delegate { inSizeDrag = false; };
            this.MouseDown += (o, ea) =>
            {
                if (IsInSizeGrip(ea.Location))
                    inSizeDrag = true;
            };
            this.MouseMove += (o, ea) =>
            {
                if (inSizeDrag)
                {
                    this.Width = ea.Location.X + GRIP_SIZE / 2;
                    this.Height = ea.Location.Y + GRIP_SIZE / 2;
                    SetGripRectangle();
                    this.Invalidate();
                }
            };
            #endregion
        }

        private void btn_maximize_Click(object sender, EventArgs e)
        {
            StateWin();
        }

        private void StateWin()
        {
            
            if (this.btn_maximize.Text == "1")
            {
                this.btn_maximize.Text = "2";
                this.Location = new Point(0, 0);
                this.Size = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
            }
            else if(this.btn_maximize.Text == "2")
            {
                this.btn_maximize.Text = "1";
                this.Size = new Size(width, heigth);
                this.StartPosition = FormStartPosition.CenterScreen;
            }
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
    }
}
