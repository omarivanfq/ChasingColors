using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.IO;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Media;

namespace ChasingColors
{
   
    public partial class MainWindow : Window
    {

        struct Obj
        {
            public double dPosX;
            public double dPosY;
            public double dAlto;
            public double dAncho;
        }

        Obj[] puntosObj = new Obj[8];
        Obj manoDerecha, manoIzquierda;
        SolidColorBrush colorIzquierda = Brushes.Blue;
        SolidColorBrush colorDerecha = Brushes.Red;

        int puntajeContador;

        private bool checarColision(Obj ob1, Obj ob2)
        {
            if (ob1.dPosX + ob1.dAncho < ob2.dPosX)    
                return false;
            if (ob1.dPosY + ob1.dAlto < ob2.dPosY)  
                return false;
            if (ob1.dPosY > ob2.dPosY + ob2.dAlto)  
                return false;
            if (ob1.dPosX > ob2.dPosX + ob2.dAncho) 
                return false;
            return true;
        }
        
        private KinectSensor miKinect;  
        DispatcherTimer timerPuntos, timerDisponibles, timerAceleracion;
        TimeSpan timeSpan = new TimeSpan();
        Random random = new Random();
        int timeSpanMS;
        int vidas;

        Queue<int> disponibles;
        Stack<Image> vidasImg;

        /* ----------------------- Área para las variables ------------------------- */
        double dMano_X;            //Representa la coordenada X de la mano derecha
        double dMano_Y;            //Representa la coordenada Y de la mano derecha

        double lMano_X;             //Representa la coordenada X de la mano izquierda
        double lMano_Y;             //Representa la coordenada Y de la mano izquierda

        Point joint_Point = new Point(); //Permite obtener los datos del Joint
        /* ------------------------------------------------------------------------- */

        Ellipse[] puntos = new Ellipse[8];
        int desapareceIndex;

        public MainWindow()
        {
            InitializeComponent();

            timerPuntos = new DispatcherTimer();
            timeSpanMS = 2500;
            timeSpan = new TimeSpan(0, 0, 0, 0, timeSpanMS);
            timerPuntos.Interval = timeSpan;
            timerPuntos.Tick += new EventHandler(Timer_Tick);
            timerPuntos.IsEnabled = true;

            timerAceleracion = new DispatcherTimer();
            timerAceleracion.Interval = new TimeSpan(0, 0, 0, 0, 5000);
            timerAceleracion.Tick += new EventHandler(Timer_Tick_Acelerar);
            timerAceleracion.IsEnabled = true;
            
            disponibles = new Queue<int>();
            setPuntos();
            setVidasImg();
            Kinect_Config();

            for (int i = 0; i < puntos.Length; i++)
            {
                puntosObj[i].dAlto = puntos[i].Height;
                puntosObj[i].dAncho = puntos[i].Width;
            }

            manoDerecha.dPosX = (double)PunteroR.GetValue(Canvas.LeftProperty);
            manoDerecha.dPosY = (double)PunteroR.GetValue(Canvas.TopProperty);
            manoDerecha.dAlto = PunteroR.Height;
            manoDerecha.dAncho = PunteroR.Width;
            manoIzquierda.dPosX = (double)PunteroL.GetValue(Canvas.LeftProperty);
            manoIzquierda.dPosY = (double)PunteroL.GetValue(Canvas.TopProperty);
            manoIzquierda.dAlto = PunteroL.Height;
            manoIzquierda.dAncho = PunteroL.Width;

            vidas = 5;
            puntajeContador = 0;
        }

        private void setVidasImg()
        {
            vidasImg = new Stack<Image>();
            vidasImg.Push(vida1);
            vidasImg.Push(vida2);
            vidasImg.Push(vida3);
            vidasImg.Push(vida4);
            vidasImg.Push(vida5);
        }

        private void Timer_Tick_Acelerar(object sender, EventArgs e)
        {
            if (timeSpanMS - 100 > 100)
                timeSpanMS -= 100;
            timeSpan = new TimeSpan(0, 0, 0, 0, timeSpanMS);
            timerPuntos.Interval = timeSpan;
        }

        private bool colisionaConOtrosPuntos(int puntoIndex)
        {
            for (int i = 0; i < puntosObj.Length; i++)
                if (i != puntoIndex && checarColision(puntosObj[puntoIndex], puntosObj[i]))
                 return true;
            return false;
        }

        private bool colisionaConManos(int puntoIndex)
        {
            if (checarColision(puntosObj[puntoIndex], manoIzquierda)
                 || checarColision(puntosObj[puntoIndex], manoDerecha))
                return true;
            return false;
        }

        private void pierdeVida()
        {
            if (vidas > 0)
            {
                vidasImg.Pop().Opacity = 0;
                vidas--;
            }
            else
            {
                gameover.Opacity = 1;
            }
        }

        private void desaparecePunto(object sender, EventArgs e, int puntoIndex)
        {
            if (puntosObj[puntoIndex].dPosX != -MainCanvas.Width * 1.2) 
                pierdeVida();
            puntos[puntoIndex].SetValue(Canvas.LeftProperty, MainCanvas.Width * 1.2);
            puntosObj[puntoIndex].dPosX = MainCanvas.Width * 1.2;
            puntos[puntoIndex].Opacity = 0;
            disponibles.Enqueue(puntoIndex);
            (sender as DispatcherTimer).Stop();
        }

        private void reacomodarPunto(int puntoIndex, int ms)
        {
            if (puntos[puntoIndex].Opacity == 0)
            {
                puntos[puntoIndex].Opacity = 1;
                do
                {
                    double newX = random.Next(0, (int)(MainCanvas.Width - puntos[puntoIndex].Width));
                    double newY = random.Next(0, (int)(MainCanvas.Height - puntos[puntoIndex].Height));
                    puntos[puntoIndex].SetValue(Canvas.LeftProperty, newX);
                    puntos[puntoIndex].SetValue(Canvas.TopProperty, newY);
                    puntosObj[puntoIndex].dPosX = newX;
                    puntosObj[puntoIndex].dPosY = newY;
                } while (colisionaConOtrosPuntos(puntoIndex) || colisionaConManos(puntoIndex));

                DoubleAnimation animation = new DoubleAnimation(0, TimeSpan.FromSeconds(ms / 1000.0));
                animation.FillBehavior = FillBehavior.Stop;
                puntos[puntoIndex].BeginAnimation(OpacityProperty, animation);

                DispatcherTimer timerDesaparece = new DispatcherTimer();
                timerDesaparece.Interval = new TimeSpan(0, 0, 0, 0, ms - 20);
                timerDesaparece.Tick += (sender, e) => desaparecePunto(sender, e, puntoIndex);
                timerDesaparece.IsEnabled = true;
            }
        }

        private void setPuntos()
        {
            puntos[0] = red1;
            puntos[1] = blue1;
            puntos[2] = red2;
            puntos[3] = blue2;
            puntos[4] = red3;
            puntos[5] = blue3;
            puntos[6] = red4;
            puntos[7] = blue4;
            for (int i = 0; i < puntos.Length; i++)
                disponibles.Enqueue(i);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (disponibles.ToArray().Length > 0)
            {
                int puntoActual = disponibles.Dequeue();
                reacomodarPunto(puntoActual, timeSpanMS * 5);
            }
        }

        /* -- Área para el método que utiliza los datos proporcionados por Kinect -- */
        /// <summary>
        /// Método que realiza las manipulaciones necesarias sobre el Skeleton trazado
        /// </summary>
        private void usarSkeleton(Skeleton skeleton)
        {
            Joint joint1 = skeleton.Joints[JointType.HandRight];
            Joint joint2 = skeleton.Joints[JointType.HandLeft]; //Agregacion de leftHand

            // Si el Joint está listo obtener las coordenadas
            if (joint1.TrackingState == JointTrackingState.Tracked)
            {
                // Obtener coordenadas
                joint_Point = this.SkeletonPointToScreen(joint1.Position);
                dMano_X = joint_Point.X;
                dMano_Y = joint_Point.Y;

                // Modificar coordenadas del indicador que refleja el movimiento (Ellipse rojo)
                PunteroR.SetValue(Canvas.TopProperty, dMano_Y - 12.5);
                PunteroR.SetValue(Canvas.LeftProperty, dMano_X - 12.5);
                manoDerecha.dPosX = dMano_X;
                manoDerecha.dPosY = dMano_Y;

                // Indicar Id de la persona que es trazada
                LID.Content = skeleton.TrackingId;
            }

            if (joint2.TrackingState == JointTrackingState.Tracked)
            {
                // Obtener coordenadas
                joint_Point = this.SkeletonPointToScreen(joint2.Position);
                lMano_X = joint_Point.X;
                lMano_Y = joint_Point.Y;

                // Modificar coordenadas del indicador que refleja el movimiento (Ellipse rojo)
                PunteroL.SetValue(Canvas.TopProperty, lMano_Y - 12.5);
                PunteroL.SetValue(Canvas.LeftProperty, lMano_X - 12.5);
                manoIzquierda.dPosX = lMano_X;
                manoIzquierda.dPosY = lMano_Y;

                // Indicar Id de la persona que es trazada
                LID.Content = skeleton.TrackingId;
            }

            // COLISIONES CHECK
            //Colision MR con Rojo

            for (int i = 0; i < puntos.Length; i++)
            {
                if (checarColision(puntosObj[i], manoDerecha))
                {
                    SystemSounds.Hand.Play();
                    puntosObj[i].dPosX = -MainCanvas.Width * 1.2;
                    puntos[i].SetValue(Canvas.LeftProperty, -MainCanvas.Width * 1.2);
                    puntos[i].Opacity = 0;
                    disponibles.Enqueue(i);
                    if (puntos[i].Fill == colorDerecha)
                    {
                        puntajeContador++;
                    }
                    else
                    {
                        puntajeContador--;
                    }
                    puntaje.Content = "Puntaje: " + puntajeContador;
                }

                if (checarColision(puntosObj[i], manoIzquierda))
                {
                    SystemSounds.Hand.Play();
                    puntosObj[i].dPosX = -MainCanvas.Width * 1.2; ;
                    puntos[i].SetValue(Canvas.LeftProperty, -MainCanvas.Width * 1.2);
                    puntos[i].Opacity = 0;
                    disponibles.Enqueue(i);
                    if (puntos[i].Fill == colorIzquierda)
                    {
                        puntajeContador++;
                    }
                    else
                    {
                        puntajeContador--;
                    }
                    puntaje.Content = "Puntaje: " + puntajeContador;
                }
            }
             
        }

        /* ------------------------------------------------------------------------- */

        /* --------------------------- Métodos Nuevos ------------------------------ */
   
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            DepthImagePoint depthPoint = this.miKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }
        /* ------------------------------------------------------------------------- */
        private void Kinect_Config()
        {
            miKinect = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected);

            if (this.miKinect != null && !this.miKinect.IsRunning)
            {

                /* ------------------- Configuración del Kinect ------------------- */
                // Habilitar el SkeletonStream para permitir el trazo de "Skeleton"
                this.miKinect.SkeletonStream.Enable();

                // Enlistar al evento que se ejecuta cada vez que el Kinect tiene datos listos
                this.miKinect.SkeletonFrameReady += this.Kinect_FrameReady;
                /* ---------------------------------------------------------------- */

                // Enlistar el método que se llama cada vez que hay un cambio en el estado del Kinect
                KinectSensor.KinectSensors.StatusChanged += Kinect_StatusChanged;

                // Iniciar el Kinect
                try
                {
                    this.miKinect.Start();
                }
                catch (IOException)
                {
                    this.miKinect = null;
                }
                LEstatus.Content = "Conectado";
            }
            else
            {
                // Enlistar el método que se llama cada vez que hay un cambio en el estado del Kinect
                KinectSensor.KinectSensors.StatusChanged += Kinect_StatusChanged;
            }
        }
        /// <summary>
        /// Método que adquiere los datos que envia el Kinect, su contenido varía según la tecnología 
        /// que se esté utilizando (Cámara, SkeletonTraking, DepthSensor, etc)
        /// </summary>
        private void Kinect_FrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            // Arreglo que recibe los datos  
            Skeleton[] skeletons = new Skeleton[0];
            Skeleton skeleton;

            // Abrir el frame recibido y copiarlo al arreglo skeletons
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            // Seleccionar el primer Skeleton trazado
            skeleton = (from trackSkeleton in skeletons where trackSkeleton.TrackingState == SkeletonTrackingState.Tracked select trackSkeleton).FirstOrDefault();

            if (skeleton == null)
            {
                LID.Content = "0";
                return;
            }
            LID.Content = skeleton.TrackingId;

            // Enviar el Skelton a usar
            this.usarSkeleton(skeleton);
        }
        /// <summary>
        /// Método que configura del Kinect de acuerdo a su estado(conectado, desconectado, etc),
        /// su contenido varia según la tecnología que se esté utilizando (Cámara, SkeletonTraking, DepthSensor, etc)
        /// </summary>
        private void Kinect_StatusChanged(object sender, StatusChangedEventArgs e)
        {

            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (this.miKinect == null)
                    {
                        this.miKinect = e.Sensor;
                    }

                    if (this.miKinect != null && !this.miKinect.IsRunning)
                    {
                        /* ------------------- Configuración del Kinect ------------------- */
                        // Habilitar el SkeletonStream para permitir el trazo de "Skeleton"
                        this.miKinect.SkeletonStream.Enable();

                        // Enlistar al evento que se ejecuta cada vez que el Kinect tiene datos listos
                        this.miKinect.SkeletonFrameReady += this.Kinect_FrameReady;
                        /* ---------------------------------------------------------------- */

                        // Iniciar el Kinect
                        try
                        {
                            this.miKinect.Start();
                        }
                        catch (IOException)
                        {
                            this.miKinect = null;
                        }
                        LEstatus.Content = "Conectado";
                    }
                    break;
                case KinectStatus.Disconnected:
                    if (this.miKinect == e.Sensor)
                    {
                        /* ------------------- Configuración del Kinect ------------------- */
                        this.miKinect.SkeletonFrameReady -= this.Kinect_FrameReady;
                        /* ---------------------------------------------------------------- */

                        this.miKinect.Stop();
                        this.miKinect = null;
                        LEstatus.Content = "Desconectado";

                    }
                    break;
            }
        }
        /// <summary>
        /// Método que libera los recursos del Kinect cuando se termina la aplicación
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.miKinect != null && this.miKinect.IsRunning)
            {
                /* ------------------- Configuración del Kinect ------------------- */
                this.miKinect.SkeletonFrameReady -= this.Kinect_FrameReady;
                /* ---------------------------------------------------------------- */

                this.miKinect.Stop();
            }
        }
    }
}
