using InteractiveDataDisplay.WPF;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace DetNeutronsWpfApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// 
    /// конец данных "end"
    /// разделение точек табом ("\t")
    /// </summary>
    public partial class MainWindow : Window
    {
        int _Total_Count = 0;
        int _Max_Ampl;
        int _Temp_count;
        DateTime Start_time;
        double[] yBar;
        public MainWindow()
        {
            InitializeComponent();
            Task.Run(() => Update_Scr());
        }
        //Обьвляем порт
        private SerialPort comport = new SerialPort();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                str_bt.IsEnabled = false;
                yBar = new double[255];
                comport.PortName = "COM" + NamePort.Text;//номер порта
                comport.BaudRate = Convert.ToInt16(SpeedPort.Text);//скорость порта
                comport.Open();//открыть порт
                comport.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);//подписаться на чтение порта
                Start_time = DateTime.Now;
                ClassTextFile.CreatFileData(PathText.Text + Start_time.Year.ToString() + "_" + Start_time.Month.ToString() + "_" + Start_time.Day.ToString() + "_" + Start_time.Hour.ToString() + "_" + Start_time.Minute.ToString());
                lock (KEY_lock)
                {
                    KEY_lock = "1";
                }
                Task.Run(() => Update_Scr());
            }
            catch (Exception exp) { MessageBox.Show(exp.Message); }
        }
        static String KEY_lock = "1";
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            str_bt.IsEnabled = true;
            comport.Close();//закрыть порт
            comport.DataReceived -= new SerialDataReceivedEventHandler(port_DataReceived);
            lock (KEY_lock)
            {
                KEY_lock = "0";
            }
            ClassTextFile.CloseFileData();

        }

        public int[] ConvertStrinMas(string[] str)
        {
            int[] masint = new int[str.Length - 1];
            for (int i = 0; i < str.Length - 1; i++)
            {
                Console.WriteLine("string = " + str[i]);
                try
                {
                    masint[i] = Convert.ToInt32(str[i]);
                }
                catch (Exception e)// если мусор, а не число, то пишу нуль
                {
                    masint[i] = 170;
                }

            }
            return masint;
        }
        string dataR;
        //var lg1 = new LineGraph();
        double t = 100;
        private void port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            byte[] buf = new byte[100];
            // прочитали строку
            int countByte= comport.Read(buf, 0, 100);
                //  MessageBox.Show(dataR);

                if (countByte>0)
                {
               // MessageBox.Show("FFF");
                //ToDo сохраняем в файл строку
                try
                {


                    //обрабатываем данные, определяем нейтрон, считаем темп счета
                 
            
                    _Max_Ampl = buf.Max() - 170;
                    _Total_Count++;
                    Tab.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => //использую инвок для вызова перерисовки из не родного потока
                    {
                        if (buf.Max() >= Convert.ToInt32(TextPorog.Text))//если максимальное значение больше или равно порогу, то строим график
                        {

                            // linegraph.Children.Clear();

                            if (Tab.SelectedIndex == 1)//если выбрана вкладка "Развертка", то строим график
                            {
                                try
                                {
                                    int[] x = new int[buf.Length];//точки по x
                                    for (int i = 0; i < buf.Length; i++)
                                    {
                                        x[i] = i;

                                    }


                                    linegraph.Plot(x, buf); //строим график

                                    /*  double[] y1 = new double[x.Length];
                                      ClassFilterSig.Dacc = Data[0];
                                      ClassFilterSig.Dout= Data[0];
                                      for (int i = 0; i < x.Length; i++)
                                      {



                                          y1[i] = ClassFilterSig.Filtr(Data[i], t);




                                      }
                                     // var lg1 = new LineGraph();

                                    //  linegraph.Children.Add(lg1);

                                    //  lg1.Stroke = new SolidColorBrush(Color.FromArgb(255, 225, 0, 255));
                                     // lg1.Description = String.Format("SigIntegral");
                                    //  lg1.StrokeThickness = 2;
                                   //   lg1.Plot(x, y1);
                                   */

                                }
                                catch (Exception ex) { MessageBox.Show("ошибка " + ex.Message); }
                            }
                        }



                        ClassTextFile.WriteFileData(DateTime.Now.ToString() + "\t" + _Max_Ampl.ToString());

                    }));
                    Count_total.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { Count_total.Content = _Total_Count.ToString(); }));
                    Max_Ampl.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { Max_Ampl.Content = _Max_Ampl.ToString(); }));
                    yBar[buf.Max()]++;
                    barChart.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { barChart.PlotBars(yBar); }));

                    dataR = String.Empty;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                }

                // Display the text to the user in the terminal
         
        }
        public void Update_Scr()
        {
            while (true)
            {
                Tab.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    Temp_Lab.Content = ((int)Math.Round(_Total_Count / ((DateTime.Now - Start_time).TotalMinutes))).ToString();
                }));
                Thread.Sleep(1000);
                lock (KEY_lock) { if (KEY_lock == "0") break; }
                labTime.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { labTime.Content = DateTime.Now.ToString(); }));
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

            // Start_time = DateTime.Now;
            //ClassTextFile.CreatFileData(PathText.Text + Start_time.Year.ToString() + "_" + Start_time.Month.ToString() + "_" + Start_time.Day.ToString() + "_" + Start_time.Hour.ToString() + "_" + Start_time.Minute.ToString());
            linegraph.Children.Clear();
            var x = Enumerable.Range(0, 10).Select(i => i).ToArray();
            // var y = x.Select(v => Math.Abs(v) < 1e-10 ? 1 : Math.Sin(v)/v).ToArray();
            var y = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                if (i > 5 && i < 10)
                    y[i] = 30;
                barChart.PlotBars(y);
            }

            var lg = new LineGraph();
            linegraph.Children.Add(lg);

            lg.Stroke = new SolidColorBrush(Color.FromArgb(255, 36, 0, 255));
            lg.Description = String.Format("Sig");
            lg.StrokeThickness = 2;
            lg.Plot(x, y);

            double[] y1 = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                double t = 100;


                y1[i] = ClassFilterSig.Filtr(y[i], t);




            }
            var lg1 = new LineGraph();

            linegraph.Children.Add(lg1);

            lg1.Stroke = new SolidColorBrush(Color.FromArgb(255, 225, 0, 255));
            lg1.Description = String.Format("SigIntegral");
            lg1.StrokeThickness = 2;
            lg1.Plot(x, y1);
            //barChart.PlotBars(y);
          


        }
    }
}
