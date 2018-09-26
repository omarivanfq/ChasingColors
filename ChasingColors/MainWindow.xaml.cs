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
    /// <summary>
    /// Capítulo: Reflejar el movimiento con imágenes
    /// Ejemplo: Obtener la posición de la mano derecha (De cualquier persona, no se selecciona cual)
    /// Descripción: 
    ///              Este sencillo ejemplo muestra una ventana con un círculo del cual, su movimiento, refleja el 
    ///              movimiento de la mano derecha. Conforme se mueve la mano se mueve el círculo.
    /// </summary>
    public partial class MainWindow : Window
    {

        struct Obj
        {
            public double dPosX;
            public double dPosY;
            public double dAlto;
            public double dAncho;
        }

        Obj O1, O2, O3, O4, O5, O6, O7, O8;
        Obj MR, ML;

        int Puntaje = 0;

        private bool checarColision(Obj ob1, Obj ob2)
        {
            if (ob1.dPosX + ob1.dAncho < ob2.dPosX)     //Colisión	por	la	izquierda	de	ob2
                return false;
            if (ob1.dPosY + ob1.dAlto < ob2.dPosY)      //Colisión	por	arriba	de	ob2
                return false;
            if (ob1.dPosY > ob2.dPosY + ob2.dAlto)      //Colisión	por	abajo	ob2
                return false;
            if (ob1.dPosX > ob2.dPosX + ob2.dAncho) //Colisión	por	la	derecha	ob2
                return false;
            return true;
        }


        private KinectSensor miKinect;  //Representa el Kinect conectado
        DispatcherTimer timerPuntos;
        TimeSpan timeSpan = new TimeSpan();
        //  DoubleAnimation[] animations = new DoubleAnimation[6];
        Random random = new Random();
        int timeSpanMS;
        /* ----------------------- Área para las variables ------------------------- */
        double dMano_X;            //Representa la coordenada X de la mano derecha
        double dMano_Y;            //Representa la coordenada Y de la mano derecha

        double lMano_X;             //Representa la coordenada X de la mano izquierda
        double lMano_Y;             //Representa la coordenada Y de la mano izquierda

        Point joint_Point = new Point(); //Permite obtener los datos del Joint
        /* ------------------------------------------------------------------------- */

        Ellipse[] puntos = new Ellipse[8];
        int puntoActual;

        public MainWindow()
        {
            InitializeComponent();

            initAnimations();

            timerPuntos = new DispatcherTimer();
            timeSpanMS = 2500;
            timeSpan = new TimeSpan(0, 0, 0, 0, timeSpanMS);
            timerPuntos.Interval = timeSpan;
            timerPuntos.Tick += new EventHandler(Timer_Tick);
            timerPuntos.IsEnabled = true;

            puntoActual = 0;
            setPuntos();
            Kinect_Config();

            O1.dAlto = puntos[0].Height;
            O1.dAncho = puntos[0].Width;

            O2.dAlto = puntos[1].Height;
            O2.dAncho = puntos[1].Width;

            O3.dAlto = puntos[2].Height;
            O3.dAncho = puntos[2].Width;

            O4.dAlto = puntos[3].Height;
            O4.dAncho = puntos[3].Width;

            O5.dAlto = puntos[4].Height;
            O5.dAncho = puntos[4].Width;

            O6.dAlto = puntos[5].Height;
            O6.dAncho = puntos[5].Width;

            O7.dAlto = puntos[6].Height;
            O7.dAncho = puntos[6].Width;

            O8.dAlto = puntos[7].Height;
            O8.dAncho = puntos[7].Width;

            MR.dAlto = PunteroR.Height;
            MR.dAncho = PunteroR.Width;

            ML.dAlto = PunteroL.Height;
            ML.dAncho = PunteroL.Width;

        }

        private void reaparecerPunto(Ellipse punto)
        {
            DoubleAnimation animation2 = new DoubleAnimation(1, TimeSpan.FromSeconds(0.001));
            punto.BeginAnimation(Ellipse.OpacityProperty, animation2);
        }

        private void initAnimations()
        {
            /*  animations[0] = new DoubleAnimation();
              animations[1] = new DoubleAnimation();
              animations[2] = new DoubleAnimation();
              animations[3] = new DoubleAnimation();
              animations[4] = new DoubleAnimation();
              animations[5] = new DoubleAnimation();*/
        }

        private void reacomodarPunto(int puntoIndex, int ms)
        {
            //DoubleAnimation animation = new DoubleAnimation(0, TimeSpan.FromSeconds(ms / 1000.0));
            //punto.BeginAnimation(Ellipse.OpacityProperty, animation);

            Ellipse punto = puntos[puntoIndex];

            punto.Opacity = 1;//SetValue(Canvas.OpacityProperty, );

            double newX = random.Next(0, (int)(MainCanvas.Width - punto.Width));
            double newY = random.Next(0, (int)(MainCanvas.Height - punto.Height));
            punto.SetValue(Canvas.LeftProperty, newX);
            punto.SetValue(Canvas.TopProperty, newY);

            switch (puntoIndex)
            {
                case 0:
                    O1.dPosX = newX;
                    O1.dPosY = newY;
                    break;
                case 1:
                    O2.dPosX = newX;
                    O2.dPosY = newY;
                    break;
                case 2:
                    O3.dPosX = newX;
                    O3.dPosY = newY;
                    break;
                case 3:
                    O4.dPosX = newX;
                    O4.dPosY = newY;
                    break;
                case 4:
                    O5.dPosX = newX;
                    O5.dPosY = newY;
                    break;
                case 5:
                    O6.dPosX = newX;
                    O6.dPosY = newY;
                    break;
                case 6:
                    O7.dPosX = newX;
                    O7.dPosY = newY;
                    break;
                case 7:
                    O8.dPosX = newX;
                    O8.dPosY = newY;
                    break;
            }

            if (punto.Opacity == 1)
            {
                DoubleAnimation animation = new DoubleAnimation(0, TimeSpan.FromSeconds(ms / 1000.0));
                //  animations[puntoIndex] = new DoubleAnimation(0, TimeSpan.FromSeconds(ms / 1000.0));
                animation.FillBehavior = FillBehavior.Stop;
                punto.BeginAnimation(Ellipse.OpacityProperty, animation);
            }
            //  
            punto.Opacity = 1;//SetValue(Canvas.OpacityProperty, );


        }

        private void Timer_Tick_Reaparece(object sender, EventArgs e)
        {
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

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            reacomodarPunto(puntoActual, timeSpanMS * 2);
            puntoActual++;
            if (puntoActual >= puntos.Length)
            {
                puntoActual = 0;
            }
            if (timeSpanMS - 50 > 0)
            {
                timeSpanMS -= 100;
            }
            timeSpan = new TimeSpan(0, 0, 0, 0, timeSpanMS);
            timerPuntos.Interval = timeSpan;
            //DoubleAnimation animation = new DoubleAnimation(0, TimeSpan.FromSeconds(4));
            //puntos[puntoActual].BeginAnimation(Ellipse.OpacityProperty, animation);
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
                MR.dPosX = dMano_X;
                MR.dPosY = dMano_Y;

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
                ML.dPosX = lMano_X;
                ML.dPosY = lMano_Y;

                // Indicar Id de la persona que es trazada
                LID.Content = skeleton.TrackingId;
            }

            // COLISIONES CHECK
            //Colision MR con Rojo
            if (checarColision(O1, MR))
            {
                SystemSounds.Hand.Play();       //Si	hay	colisión	emite	un	sonido
                O1.dPosX = 1000.00;
                puntos[0].SetValue(Canvas.LeftProperty, 1000.00);
            }
            if (checarColision(O3, MR))
            {
                SystemSounds.Hand.Play();       //Si	hay	colisión	emite	un	sonido
                O3.dPosX = 1000.00;
                puntos[2].SetValue(Canvas.LeftProperty, 1000.00);
            }
            if (checarColision(O5, MR))
            {
                SystemSounds.Hand.Play();       //Si	hay	colisión	emite	un	sonido
                O5.dPosX = 1000.00;
                puntos[4].SetValue(Canvas.LeftProperty, 1000.00);
            }
            if (checarColision(O7, MR))
            {
                SystemSounds.Hand.Play();       //Si	hay	colisión	emite	un	sonido
                O7.dPosX = 1000.00;
                puntos[6].SetValue(Canvas.LeftProperty, 1000.00);
            } 
            //Colision con azules, pierde pts
            if (checarColision(O2, MR))
            {
                SystemSounds.Hand.Play();       //Si	hay	colisión	emite	un	sonido
                O2.dPosX = 1000.00;
                puntos[1].SetValue(Canvas.LeftProperty, 1000.00);
            }
            if (checarColision(O4, MR))
            {
                SystemSounds.Hand.Play();       //Si	hay	colisión	emite	un	sonido
                O4.dPosX = 1000.00;
                puntos[3].SetValue(Canvas.LeftProperty, 1000.00);
            }
            if (checarColision(O6, MR))
            {
                SystemSounds.Hand.Play();       //Si	hay	colisión	emite	un	sonido
                O6.dPosX = 1000.00;
                puntos[5].SetValue(Canvas.LeftProperty, 1000.00);
            }
            if (checarColision(O8, MR))
            {
                SystemSounds.Hand.Play();       //Si	hay	colisión	emite	un	sonido
                O8.dPosX = 1000.00;
                puntos[7].SetValue(Canvas.LeftProperty, 1000.00);


            //Colisiones Mano izquierda

            if (checarColision(O1, ML))
            {
                SystemSounds.Hand.Play();       //Si	hay	colisión	emite	un	sonido
                O1.dPosX = 1000.00;
                puntos[0].SetValue(Canvas.LeftProperty, 1000.00);
            }
            if (checarColision(O3, ML))
            {
                SystemSounds.Hand.Play();       //Si	hay	colisión	emite	un	sonido
                O3.dPosX = 1000.00;
                puntos[2].SetValue(Canvas.LeftProperty, 1000.00);
            }
            if (checarColision(O5, ML))
            {
                SystemSounds.Hand.Play();       //Si	hay	colisión	emite	un	sonido
                O5.dPosX = 1000.00;
                puntos[4].SetValue(Canvas.LeftProperty, 1000.00);
            }
            if (checarColision(O7, ML))
            {
                SystemSounds.Hand.Play();       //Si	hay	colisión	emite	un	sonido
                O7.dPosX = 1000.00;
                puntos[6].SetValue(Canvas.LeftProperty, 1000.00);
            }
            //Colision con azules, pierde pts
            if (checarColision(O2, ML))
            {
                SystemSounds.Hand.Play();       //Si	hay	colisión	emite	un	sonido
                O2.dPosX = 1000.00;
                puntos[1].SetValue(Canvas.LeftProperty, 1000.00);
            }
            if (checarColision(O4, ML))
            {
                SystemSounds.Hand.Play();       //Si	hay	colisión	emite	un	sonido
                O4.dPosX = 1000.00;
                puntos[3].SetValue(Canvas.LeftProperty, 1000.00);
            }
            if (checarColision(O6, ML))
            {
                SystemSounds.Hand.Play();       //Si	hay	colisión	emite	un	sonido
                O6.dPosX = 1000.00;
                puntos[5].SetValue(Canvas.LeftProperty, 1000.00);
            }
            if (checarColision(O8, ML))
            {
                SystemSounds.Hand.Play();       //Si	hay	colisión	emite	un	sonido
                O8.dPosX = 1000.00;
                puntos[7].SetValue(Canvas.LeftProperty, 1000.00);
            }


        }
        /* ------------------------------------------------------------------------- */

        /* --------------------------- Métodos Nuevos ------------------------------ */

        /// <summary>
        /// Metodo que convierte un "SkeletonPoint" a "DepthSpace", esto nos permite poder representar las coordenadas de los Joints
        /// en nuestra ventana en las dimensiones deseadas.
        /// </summary>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convertertir un punto a "Depth Space" en una resolución de 640x480
            DepthImagePoint depthPoint = this.miKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }
        /* ------------------------------------------------------------------------- */

        /// <summary>
        /// Método que realiza las configuraciones necesarias en el Kinect 
        /// así también inicia el Kinect para el envío de datos
        /// </summary>
        private void Kinect_Config()
        {
            // Buscamos el Kinect conectado con la propiedad KinectSensors, al descubrir el primero con el estado Connected
            // se asigna a la variable miKinect que lo representará (KinectSensor miKinect)
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
