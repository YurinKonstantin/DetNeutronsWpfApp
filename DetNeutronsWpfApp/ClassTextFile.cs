using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DetNeutronsWpfApp
{
  static  class ClassTextFile
    {
       static string nameFile;
        static FileStream file1;
        static StreamWriter fnew;
        static  public void CreatFileData(string name)//создаем новый файл или открываем этот если есть
        {
            try
            {
                nameFile = name;

                //File.Create(nameFile);
               // file = new StreamWriter(nameFile+".txt");
               file1 = new FileStream(nameFile + ".txt", FileMode.Append);
               fnew = new StreamWriter(file1, Encoding.GetEncoding(1251));


            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при открытии файла"+"\t"+ex.ToString());
            }
            // }
        }
        static public async void WriteFileData(string text)//пишим строку в файл
        {
            try
            {

                await fnew.WriteLineAsync(text);




            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка записи текста");
            }
        }
        static public void CloseFileData()//закрываем файл
        {
            try
            {

                fnew.Close();
                file1.Close();




            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибказакрытия файла");
            }
            // }
        }
    }
}
