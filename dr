using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Windows.Media.Animation;
namespace DatReciver
{
    public partial class MainWindow : Window
    {
        private Thread thread;
        private List<byte[]> bytes = new List<byte[]>();
        private int headerLenth = 12;
        private UdpClient udpServer;
        private int serverPort = 10002;
        private bool fileIsEnded = false;
        IPEndPoint address = new IPEndPoint(IPAddress.Any, 10001);
        public MainWindow()
        {
            InitializeComponent();
            Closing += OnWindowsClosing;
            ResizeMode = ResizeMode.NoResize;
        }
        private void acceptData()
        {
            closeUdpServer();
            try
            {
                udpServer = new UdpClient(serverPort);
                while (!fileIsEnded)
                {
                    byte[] reciveBytes = udpServer.Receive(ref address);
                    byte[] sendingData = new byte[reciveBytes.Length - headerLenth];
                    int v = 0;
                    for (int i = headerLenth; i < reciveBytes.Length; i++)
                    {
                        sendingData[v] = reciveBytes[i];
                        v++;
                    }
                    bytes.Add(sendingData);
                }
            }
            catch (Exception)
            {
                //
            }
        }
        private void StartAcceptDataButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                serverPort = Convert.ToInt32(inputServerPort.Text);
                headerLenth = Convert.ToInt32(inputHeaderLenth.Text);
                fileIsEnded = false;
                if (serverPort < 0 || serverPort > 65535)
                {
                    MessageBox.Show("Порты должны варироваться от 0 до 65535", "Ошибка");
                }
                else if (headerLenth < 0)
                {
                    MessageBox.Show("Длина заголовка должна быть больше 0", "Ошибка");
                }
                else
                {
                    thread = new Thread(new ThreadStart(acceptData));
                    fileIsEnded = false;
                    thread.Start();
                    status.Text = "Статус приёма: On";
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Неверный формат порта или длины заголовка", "Ошибка");
            }
        }

        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            fileIsEnded = true;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Data files(*.dat*)|*.dat*|Text files(*.txt*.txt)|*.txt|All files(*.*)|*.*";
            saveFileDialog.ShowDialog();
            saveFileDialog.FileName += ".dat";
            try
            {
                FileStream fileStream = (FileStream)saveFileDialog.OpenFile();
                foreach (byte[] b in bytes)
                {
                    fileStream.Write(b, 0, b.Length);
                }
                bytes.Clear();
                status.Text = "Статус приёма: Off";
                MessageBox.Show($"Файл сохранён под именем:\n {saveFileDialog.FileName}", "Файл успешно сохранён");
                fileStream.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Файл пуст", "Ошибка");
            }
            closeUdpServer();
        }
        private void OnWindowsClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            fileIsEnded = false;
            closeUdpServer();
        }
        private void closeUdpServer()
        {
            if (udpServer != null)
            {
                udpServer.Close();
            }
        }

    }
}
