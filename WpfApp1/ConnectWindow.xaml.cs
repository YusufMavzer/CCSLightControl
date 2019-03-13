using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CCSLightControlTool
{
    /// <summary>
    /// Interaction logic for ConnectWindow.xaml
    /// </summary>
    public partial class ConnectWindow : Window
    {
        public ConnectWindow()
        {
            InitializeComponent();
            Ip = new byte[] { 192, 168, 0, 2 };
            Port = 40001;
        }

        public byte[] Ip { get; private set; }
        public int Port { get; private set; } 

        private void CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
        private void ConnectButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            

        }

    }
}
