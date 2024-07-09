using System;
using System.Windows;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Text;
namespace Generator1
{
    public partial class MainWindow : Window
    {
        private FileStream fileStream;
        private string path = String.Empty;

        private UdpClient udpClient;
        private Thread reciveThread;
        private Thread sendThread;

        private int countOfBytes;
        private int pt = 16;
        private int sendingDelay = 1;
        private int sendingType = 0;

        private const string errorTitle = "Ошибка";
        private string error = "Непредвиденная ошибка";
        private string connectionError = "Неудалось установить соединение";

        private bool isRunned = false;
        private bool isConnected = false;
        private bool deviceBufferIsOverflow = false;
        private static IPEndPoint cringe = new IPEndPoint(IPAddress.Any, 0);
        private IPEndPoint serverAddress;
        public MainWindow()
        {
            Closing += OnWindowsClosing;
            ResizeMode = ResizeMode.NoResize;
        }
        private void reciveSignal()
        {
            while (isRunned)
            {
                try
                {
                    var recivedBytes = udpClient.ReceiveAsync(); byte checkedByte = recivedBytes.Result.Buffer[32];
                    if (checkedByte <= 53 && checkedByte >= 51)
                    {
                        deviceBufferIsOverflow = true;

                    }
                    else if (checkedByte >= 48 && checkedByte <= 50)
                    {
                        deviceBufferIsOverflow = false;
                    }
                }
                catch (Exception ex)
                {
                    //
                }
            }
        }
        private void sendData()
        {
            int bytesRead = 0;
            int sequenceNumber = 0;
            bool fileIsNotSended = true;
            fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            while (isRunned && (fileIsNotSended || sendingType == 1))
            {
                try
                {
                    bool check = fileStream.Length > (fileStream.Position + countOfBytes);
                    if (check && !deviceBufferIsOverflow)
                    {
                        byte[] buffer = new byte[countOfBytes];
                        fileStream.Position = bytesRead;
                        fileStream.Read(buffer, 0, countOfBytes);
                        RtpHeader rtpHeader = new RtpHeader(pt);
                        byte[] data = rtpHeader.getPacket(buffer, sequenceNumber);
                        udpClient.Send(data, data.Length, serverAddress);
                        bytesRead += countOfBytes;
                        sequenceNumber += 1;
                        Thread.Sleep(sendingDelay);
                    }
                    else if (!check && !deviceBufferIsOverflow) //последний пакет
                    {
                        byte[] buffer = new byte[fileStream.Length - fileStream.Position];
                        fileStream.Position = bytesRead;
                        fileStream.Read(buffer, 0, (int)(fileStream.Length - fileStream.Position));
                        RtpHeader rtpHeader = new RtpHeader(pt);
                        byte[] data = rtpHeader.getPacket(buffer, sequenceNumber);
                        udpClient.Send(data, data.Length, serverAddress);
                        fileIsNotSended = false;
                        Thread.Sleep(sendingDelay);
                        if (sendingType == 0)
                        {
                            MessageBox.Show("Отправка файла успешно завершена", "Файл успешно отправлен");
                        }
                        else
                        {
                            sequenceNumber = 0;
                            bytesRead = 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //
                }
            }
            closeFileStream();
        }
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            isRunned = false;
            bool ipIsCorrect = false;
            bool userPortIsCorrect = false;
            if (!isConnected)
            {
                try
                {
                    IPAddress _serverIp = IPAddress.Parse(inputServerIp.Text);
                    ipIsCorrect = true;
                    int _userPort = Convert.ToInt32(inputUserPort.Text);
                    userPortIsCorrect = true;
                    int _serverPort = Convert.ToInt32(inputServerPort.Text);
                    if (_serverPort < 0 || _serverPort > 65535 && _userPort < 0 || _userPort > 65535)
                    {
                        error = "Порты должны варироваться от 0 до 65535";
                        MessageBox.Show(error, errorTitle);
                    }
                    else
                    {
                        pt = (ComboPT.SelectedIndex + 1) * 16;
                        //IPEndPoint userAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _userPort);
                        IPEndPoint userAddress = new IPEndPoint(IPAddress.Any, _userPort);
                        udpClient = new UdpClient(userAddress);
                        serverAddress = new IPEndPoint(_serverIp, _serverPort);
                        //udpClient.Connect(serverAddress);
                        isConnected = true;
                        connectButton.Content = "Отсоеденить";
                    }
                }
                catch (Exception)
                {
                    if (!ipIsCorrect) error = "Неверный формат введённого ip адресса получателя";
                    else if (!userPortIsCorrect) error = "Неверный формат введённого порта отправителя";
                    else error = "Неверный формат введённого порта получателя";
                    MessageBox.Show(connectionError + "\n" + error, errorTitle);
                }
            }
            else //Кнопка отсоеденить
            {
                isRunned = false;
                closeConnection();
                connectButton.Content = "Соеденить";
            }
        }

        private void runProcessButton_Click(object sender, RoutedEventArgs e)
        {
            bool delayIsCorrect = false;
            isRunned = false;
            if (!isConnected)
            {
                error = "Запуск не возможен т.к Соединение не установлено";
                MessageBox.Show(error, "Ошибка");
            }
            else if (path == String.Empty)
            {
                error = "Запуск не возможен, т.к файл не выбран";
                MessageBox.Show(error, "Ошибка");
            }
            else
            {
                try
                {
                    int _delay = Convert.ToInt32(inputDelay.Text);
                    delayIsCorrect = true;
                    int _countOfBytes = Convert.ToInt32(inputByte.Text);
                    if (_delay < 1)
                    {
                        error = "Задержка должна быть не меньше 1 мс";
                        MessageBox.Show(error, errorTitle);
                    }
                    else if (_countOfBytes <= 0)
                    {
                        error = "Количество байтов должно быть больше 0";
                        MessageBox.Show(error, errorTitle);
                    }
                    else if (isConnected && delayIsCorrect)
                    {
                        sendingDelay = _delay;
                        countOfBytes = _countOfBytes;
                        currentValue.Text = $"Текущий интервал: {sendingDelay} мс";
                        sendingType = ComboSendingType.SelectedIndex;
                        sendThread = new Thread(new ThreadStart(sendData));
                        reciveThread = new Thread(new ThreadStart(reciveSignal));
                        isRunned = true;
                        sendThread.Start();
                        reciveThread.Start();
                    }
                }
                catch (Exception)
                {
                    error = "Неверный формат введённого интервала или некоректное количество байт";
                    MessageBox.Show(error, "Ошибка");
                }
            }
        }
        public void closeConnection()
        {
            if (isConnected)
            {
                isRunned = false;
                if (udpClient != null)
                {
                    udpClient.Close();
                    isConnected = false;
                }
            }
        }
        private void choiseFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Data files(*.dat*)|*.dat*|Text files(*.txt*.txt)|*.txt|All files(*.*)|*.*";
            openFileDialog.ShowDialog(); ;
            try
            {
                path = openFileDialog.FileName;
                currentFileName.Text = "Выбранный файл: " + path;
            }
            catch (Exception)
            {
                // MessageBox.Show("Ошибка чтения файла", error);
            }
        }
        public void closeFileStream()
        {
            if (fileStream != null)
            {
                fileStream.Close();
            }
        }
        private void stopProcessButton_Сlick(object sender, RoutedEventArgs e)
        {
            isRunned = false;
        }
        private void OnWindowsClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isRunned = false;
            closeConnection();
            closeFileStream();
        }
    }
}
