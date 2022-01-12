using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.IO.Ports;
using System.Text;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TestWinUi
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        DispatcherTimer timerSendInfos;
        public double TempsTimerSendInfos;
        DispatcherTimer timerChute;
        public double TempsTimerChute;
        SerialPort comPort = new SerialPort();
        public string strToSend;
        public double heightToDelete;
        public bool lastRequete = false;

        public static double Vitesse;

        public string COM;
        public double Compteur { get; set; }
        public double Temperature { get; set; }
        public double Pression { get; set; }
        public double Humidite { get; set; }
        public double Altitude { get; set; }
        public double Surface { get; set; }
        public double Miliseconds { get; set; }

        public MainWindow()
        {
            
            this.InitializeComponent();
            

        }

        private void InitializeTimerSendInfos()
        {
            timerSendInfos = new DispatcherTimer();
            timerChute = new DispatcherTimer();

            timerChute.Interval = TimeSpan.FromMilliseconds(100);
            timerSendInfos.Interval = TimeSpan.FromMilliseconds(Miliseconds);

            timerSendInfos.Tick += TimerSend_Tick;
            timerChute.Tick += TimerChute_Tick;

        }

        private void TimerChute_Tick(object sender, object e)
        {        
            Calculte_Temperature();
            Calculte_Temp(Temperature);
            Calculte_Pression(25, Surface);
            Calculte_Altitude();


            if (Altitude <= 0)
            {
                lastRequete = true;
                writeInSerial();
                Stop();
            }
        }

        private void TimerSend_Tick(object sender, object e)
        {            
            writeInSerial();
            Compteur += 1;
        }



        public void writeInSerial()
        {
            txt_compteur.Text = Compteur.ToString();
            txt_temperature.Text = Temperature.ToString();
            txt_pression.Text = Pression.ToString();
            txt_humidite.Text = Humidite.ToString();
            if (!lastRequete)
                txt_altitude.Text = Altitude.ToString();
            else
            {
                txt_altitude.Text = "0.00";
                Altitude = 0;      
            }
                

            strToSend = $"\x02,{Compteur},{Temperature.ToString("0.00")},{Pression.ToString("0.00")},{Humidite.ToString("00.00")},{Altitude.ToString("0.00")},\x03";
            txtGet.Text = strToSend;
            Console.WriteLine(strToSend);

            try
            {       
                comPort.WriteLine(strToSend);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERREUR = " + ex.ToString());
            }
  
        }

        private double Calculte_Temperature()
        {
            switch (Vitesse)
            {
                case 5:
                    Temperature += 0.05;
                    break;
                case 50:
                    Temperature += 0.5;
                    break;
                case 500:
                    Temperature += 5;
                    break;
            }
               
            //Temperature += 0.1;
            return Temperature;
        }

        private double Calculte_Pression(double force, double surface)
        {
            Pression = force / (surface * 10000);
            return Pression;
        }

        private double Calculte_Altitude()
        {

             Altitude -= Vitesse;
             cnvImg.Height -= heightToDelete;
             return Altitude;    
            
        }

        private double Calculte_Temp(double temp)
        {
            temp = Altitude / Vitesse;
            return temp;
        }

        private void Stop()
        {
            timerSendInfos.Stop();
            timerChute.Stop();
            comPort.Close();
        }

        private void Reset()
        {
            timerSendInfos.Stop();
            timerChute.Stop();
            txt_compteur.Text = "";
            txt_temperature.Text = "";
            txt_pression.Text = "";
            txt_humidite.Text = "";
            txt_altitude.Text = "";
            Compteur = 0;
            Altitude = 0;
            Humidite = 0;
            Pression = 0;
            Temperature = 0;
            Compteur = 0;
            Surface = 0;
            cnvImg.Height = 900;
            btnStart.IsEnabled = false;
            lastRequete = false;
        }



        #region Buttons

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            comPort = new SerialPort(COM, 9600, Parity.None, 8, StopBits.One);
            comPort.Open();
            timerSendInfos.Start();
            timerChute.Start();
            btnSave.IsEnabled = false;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            Altitude = double.Parse(txt_AltitudeDepart.Text);
            Surface = double.Parse(txt_Surface.Text);
            Temperature = double.Parse(txt_Temperature.Text);
            Miliseconds = 1100 - (sld_milliseconds.Value * 100);
            COM = txt_COM.Text;
            if (double.Parse(txt_Temperature.Text) >= 100) 
                Humidite = 100;            
            else
                Humidite = double.Parse(txt_Humidite.Text);

            

            if (rbtn_realiste.IsChecked == true)
            {
                Vitesse = 5;
                
            }
            else if (rbtn_rapide.IsChecked == true)
            {
                Vitesse = 50;               
            }
            else
            {
                Vitesse = 500;               
            }


            heightToDelete = 700 / (Altitude / Vitesse);




            btnStart.IsEnabled = true;
            InitializeTimerSendInfos();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            btnSave.IsEnabled = true;   
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        { 
            cnvImg.Height -= 100;
            Console.WriteLine(cnvImg.Height);
        }



        #endregion

    }
}
