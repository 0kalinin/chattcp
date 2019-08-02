using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientGUI
{
    public partial class clientGUI_form : Form
    {
        static Socket thisClSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static Thread thread1;
        static byte[] buffer = new byte[1024];
        static string username;
        




        public void Recive()
        {
            try
            {
                #region Получение данных с сервера

                Action output = () => rTextBox.Text += ("\n" + Encoding.UTF8.GetString(buffer));

                while (true)
                {
                    buffer = new byte[1024];
                    thisClSocket.Receive(buffer);

                    // передча управления основному потоку
                    if (rTextBox.InvokeRequired) rTextBox.Invoke(output);
                    else output();

                }

                #endregion
            }
            catch (Exception ex)
            {
                Action error = () =>
                {
                    rTextBox.Text += "\n\nОшибка соеденения :";
                    rTextBox.Text += $"\n {ex.Message}";
                    rTextBox.Text += $"\n\n\n\nПерезапустите приложение!";

                };

                Invoke(error);
                

                #region Отключение

                thread1.Abort();
                thisClSocket.Send(Encoding.UTF8.GetBytes($"{username} was disconnected."));
                thisClSocket.Shutdown(SocketShutdown.Both);
                thisClSocket.Close();

                #endregion
                
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                #region Подключение к серверу

                string ip = textBox1.Text;
                int port = Convert.ToInt32(textBox2.Text);
                username = textBox4.Text;

                thisClSocket.Connect(ip, port);
                thisClSocket.Send(Encoding.UTF8.GetBytes(username));
                thisClSocket.Send(Encoding.UTF8.GetBytes(username + " was connected."));

                #endregion

                #region Изменения интерфейса

                label1.Enabled = false;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                textBox4.Enabled = false;
                button1.Enabled = false;
                textBox3.Enabled = true;
                button2.Enabled = true;

                #endregion

                #region Асинхронный вызов Recive()

                thread1 = new Thread(new ThreadStart(() => Recive()));
                thread1.Start();

                #endregion
            }
            catch (Exception ex)
            {
                rTextBox.Text += "\n\nОшибка подключения к серверу :";
                rTextBox.Text += $"\n {ex.Message}";
            }


            

        }

        private void Button2_Click(object sender, EventArgs e)
        {
            try
            {
                #region Отправка сообщения на сервер

                if (textBox3.Text != null)
                    thisClSocket.Send(Encoding.UTF8.GetBytes(textBox3.Text));
                textBox3.Text = null;

                #endregion
            }
            catch (Exception ex)
            {
                rTextBox.Text += "\n\nОшибка отправки сообщения :";
                rTextBox.Text += $"\n {ex.Message}";
            }


        }


        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            #region Отключение

            if (thisClSocket.Connected)
            {
                thisClSocket.Send(Encoding.UTF8.GetBytes($"{username} was disconnected."));
                thread1.Abort();
                thisClSocket.Shutdown(SocketShutdown.Both);
                thisClSocket.Close();
            }
            

            #endregion
        }



        public clientGUI_form()
        {
            InitializeComponent();
        }

    }
}
