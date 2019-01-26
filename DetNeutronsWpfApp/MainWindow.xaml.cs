using System;
using System.Collections.Generic;
using System.IO.Ports;
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
        public MainWindow()
        {
            InitializeComponent();
        }
        //Обьвляем порт
        private SerialPort comport = new SerialPort();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            comport.PortName = NamePort.Text;//номер порта
            comport.BaudRate = Convert.ToInt16(SpeedPort.Text);//скорость порта
            comport.Open();//открыть порт
            comport.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);//подписаться на чтение порта
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            comport.Close();//закрыть порт
            comport.DataReceived -= new SerialDataReceivedEventHandler(port_DataReceived);
         

        }
     
        public int[] ConvertStrinMas(string[] str)
        {
            int[] masint = new int[str.Length];
            for(int i=0; i<str.Length; i++)
            {
                masint[i] = Convert.ToInt32(str[i]);

            }
            return masint;
        }
        private void port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            // прочитали строку
           string dataR = comport.ReadExisting();
          
            if(dataR.Contains("end"))
            {
                //ToDo сохраняем в файл строку

                //обрабатываем данные, определяем нейтрон, считаем темп счета
                string[] str = dataR.Split('e');//убираем флаг конца данных 
                int[] Data = ConvertStrinMas(str[0].Split('\t'));//Получаем масив точек
                
                if(Tab.SelectedIndex==1)//если выбрана вкладка "Развертка", то строим график
                {
                   
                    if (Data.Max() >= Convert.ToInt32(TextPorog.Text))//если максимальное значение больше или равно порогу, то строим график
                    {
                       
                        int[] x = new int[Data.Length];//точки по x
                        for(int i=0; i<Data.Length; i++)
                        {
                            x[i] = i;

                        }
                        linegraph.Plot(x, Data); //строим график
                    }

                }


            }

            // Display the text to the user in the terminal
            
        }
    }
}
