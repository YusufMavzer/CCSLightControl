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

namespace CCSLightControlTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private CCSLightControl CCSLightControl { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            CCSLightControl = new CCSLightControl();
        }

        private void CloseAllPanels()
        {
            NetworkSettingPanel.Visibility = Visibility.Collapsed;
            ControlLightsPanel.Visibility = Visibility.Collapsed;
            ChangeConnectionPanel.Visibility = Visibility.Collapsed;
        }

        private void NetworkSettingButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseAllPanels();
            NetworkSettingPanel.Visibility = Visibility.Visible;
        }
        private void ControlLightButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseAllPanels();
            ControlLightsPanel.Visibility = Visibility.Visible;
        }
        private void ContectionTestButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            using (new WaitCursor())
            {
                if (CCSLightControl.DoErrorStatusCheck() == System.Net.HttpStatusCode.OK)
                    MessageBox.Show("Connection Succesful", "Connection Test", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void ChangeContectionButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseAllPanels();
            ChangeConnectionPanel.Visibility = Visibility.Visible;
        }
        private void SaveNetworkSettingButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            using (new WaitCursor())
            {
                if (CCSLightControl.DoErrorStatusCheck() == System.Net.HttpStatusCode.OK)
                {

                    try
                    {
                        if (string.IsNullOrWhiteSpace(NewIP4TextBox1.Text) || string.IsNullOrWhiteSpace(NewIP4TextBox2.Text) || string.IsNullOrWhiteSpace(NewIP4TextBox3.Text) || string.IsNullOrWhiteSpace(NewIP4TextBox4.Text))
                            throw new Exception("Ip address missing");

                        if (string.IsNullOrWhiteSpace(NewSubnetMaskIP4TextBox1.Text) || string.IsNullOrWhiteSpace(NewSubnetMaskIP4TextBox2.Text) || string.IsNullOrWhiteSpace(NewSubnetMaskIP4TextBox3.Text) || string.IsNullOrWhiteSpace(NewSubnetMaskIP4TextBox4.Text))
                            throw new Exception("Subnet mask missing");

                        if (string.IsNullOrWhiteSpace(NewDefaultGatewayIP4TextBox1.Text) || string.IsNullOrWhiteSpace(NewDefaultGatewayIP4TextBox2.Text) || string.IsNullOrWhiteSpace(NewDefaultGatewayIP4TextBox3.Text) || string.IsNullOrWhiteSpace(NewDefaultGatewayIP4TextBox4.Text))
                            throw new Exception("Default gateway missing");

                        if (string.IsNullOrWhiteSpace(NewPortTextBox.Text))
                            throw new Exception("Port no. is missing");




                        byte[] ip = new byte[4];
                        byte[] subnet = new byte[4];
                        byte[] defaultGateWay = new byte[4];
                        int port = int.Parse(NewPortTextBox.Text);

                        if (port < 0 || port > 65535)
                            throw new Exception("Port number can only be between 0 and 65535");


                        ip[0] = (byte)int.Parse(NewIP4TextBox1.Text);
                        ip[1] = (byte)int.Parse(NewIP4TextBox2.Text);
                        ip[2] = (byte)int.Parse(NewIP4TextBox3.Text);
                        ip[3] = (byte)int.Parse(NewIP4TextBox4.Text);

                        subnet[0] = (byte)int.Parse(NewSubnetMaskIP4TextBox1.Text);
                        subnet[1] = (byte)int.Parse(NewSubnetMaskIP4TextBox2.Text);
                        subnet[2] = (byte)int.Parse(NewSubnetMaskIP4TextBox3.Text);
                        subnet[3] = (byte)int.Parse(NewSubnetMaskIP4TextBox4.Text);

                        defaultGateWay[0] = (byte)int.Parse(NewDefaultGatewayIP4TextBox1.Text);
                        defaultGateWay[1] = (byte)int.Parse(NewDefaultGatewayIP4TextBox2.Text);
                        defaultGateWay[2] = (byte)int.Parse(NewDefaultGatewayIP4TextBox3.Text);
                        defaultGateWay[3] = (byte)int.Parse(NewDefaultGatewayIP4TextBox4.Text);


                        var result = MessageBox.Show("Are you sure you want to update the network settings", "Network settings update verification", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {

                            CCSLightControl.SetIpAddress(ip);
                            CCSLightControl.SetSubnetMask(subnet);
                            CCSLightControl.SetDefaultGateway(defaultGateWay);
                            CCSLightControl.SetPort(port);

                            MessageBox.Show("1) Unplug and plug the power of the device\n2) Change Ipv4 settings on your computer.\n3)Restart the application.", "Network settings update succesful", MessageBoxButton.OK, MessageBoxImage.Information);
                        }


                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($" Error: \n {ex.Message}");
                    }
                }
            }


        }
        private void ConnectButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            using (new WaitCursor())
            {
                try
                {
                    byte[] ipv4 = new byte[4];
                    ipv4[0] = (byte)int.Parse(IP4TextBox1.Text);
                    ipv4[1] = (byte)int.Parse(IP4TextBox2.Text);
                    ipv4[2] = (byte)int.Parse(IP4TextBox3.Text);
                    ipv4[3] = (byte)int.Parse(IP4TextBox4.Text);

                    int port = int.Parse(this.ConnectionPortTextBox.Text);
                    if (port < 0 || port > 65535) throw new Exception("Port invalid");
                    CCSLightControl = new CCSLightControl(ipv4, port);

                    ContectionTestButton_MouseDown(sender, e);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Something went wrong. Please provide correct connection values\nError message: \n{ex.Message}");
                }
            }

        }
    }

    public class WaitCursor : IDisposable
    {
        private readonly Cursor previousCursor;
        public WaitCursor()
        {
            previousCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
        }
        public void Dispose()=> Mouse.OverrideCursor = previousCursor;

    }
}
