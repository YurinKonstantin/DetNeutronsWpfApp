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
        public MainWindow()
        {
            InitializeComponent();
        }
        //Обьвляем порт
        private SerialPort comport = new SerialPort();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            str_bt.IsEnabled = false;
            comport.PortName = "COM" + NamePort.Text;//номер порта
            comport.BaudRate = Convert.ToInt16(SpeedPort.Text);//скорость порта
            comport.Open();//открыть порт
            comport.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);//подписаться на чтение порта
            Start_time = DateTime.Now;
            lock (KEY_lock)
            {
                KEY_lock = "1";
            }
            Task.Run(()=>Update_Scr());
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
        private void port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            // прочитали строку
            dataR += comport.ReadLine();
            //  MessageBox.Show(dataR);

            if (dataR.Contains("end"))
            {
                //ToDo сохраняем в файл строку

                //обрабатываем данные, определяем нейтрон, считаем темп счета
                string[] str = dataR.Split('e');//убираем флаг конца данных 
                int[] Data = ConvertStrinMas(str[0].Split('\t'));//Получаем масив точек

                Tab.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => //использую инвок для вызова перерисовки из не родного потока
                {
                    if (Data.Max() >= Convert.ToInt32(TextPorog.Text))//если максимальное значение больше или равно порогу, то строим график
                    {
                        _Total_Count++;
                        _Max_Ampl = Data.Max() - 170;

                        if (Tab.SelectedIndex == 1)//если выбрана вкладка "Развертка", то строим график
                        {
                            try
                            {
                                int[] x = new int[Data.Length];//точки по x
                                for (int i = 0; i < Data.Length; i++)
                                {
                                    x[i] = i;

                                }

                                linegraph.Plot(x, Data); //строим график

                            }
                            catch (Exception ex) { MessageBox.Show("ошибка " + ex.Message); }
                        }
                    }

                    Count_total.Content = _Total_Count.ToString();
                    Max_Ampl.Content = _Max_Ampl.ToString();

                }));


                dataR = String.Empty;

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
            }
        }
    }
}
