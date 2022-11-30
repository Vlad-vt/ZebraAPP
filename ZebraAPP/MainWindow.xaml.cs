using Linearstar.Windows.RawInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using ZebraAPP.Health;
using ZebraAPP.KeyInput;

namespace ZebraAPP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainWindow _window;
        public MainWindow()
        {
            InitializeComponent();
            _window = this;
            #region Barcode reader
            List<BarcodeDevice> barcodeDevices = new List<BarcodeDevice>();
            Thread keyloggerThread = new Thread(delegate ()
            {
                string repeatDataScan1 = null, repeatDataScan2 = null;
                RawInputDevice[] devices = new RawInputDevice[1];
                IEnumerable<RawInputKeyboard> keyboards = new RawInputKeyboard[1];
                try
                {
                    devices = RawInputDevice.GetDevices();
                    keyboards = devices.OfType<RawInputKeyboard>();
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message);
                }
                foreach (var device in keyboards)
                {
                    //if (device.ProductName.Contains("Symbol Bar Code"))
                    //{
                        barcodeDevices.Add(new BarcodeDevice(device.ProductName + " " + device.Handle.ToString()));
                        if (barcodeDevices.Count >= 2)
                            break;
                    //}
                }
                try
                {
                    App.Current.Dispatcher.Invoke(() => Scanner1.Text = barcodeDevices[0].DeviceName);
                }
                catch(Exception)
                {
                    App.Current.Dispatcher.Invoke(() => Scanner1.Text = "Device NULL");
                }
                try
                {
                    App.Current.Dispatcher.Invoke(() => {
                        if (barcodeDevices.Count >= 2)
                            Scanner2.Text = barcodeDevices[1].DeviceName;
                        else
                            Scanner2.Text = "Device not found!";
                        });
                }
                catch (Exception)
                {
                    App.Current.Dispatcher.Invoke(() => Scanner2.Text = "Device NULL");
                }
                var window = new RawInputReceiverWindow();
                window.Input += (_sender, _e) =>
                {
                    if (_e != null && _e.Data.Device != null)   
                    {
                        try
                        {
                            if ((_e.Data.Device.ProductName.ToString() + " " + _e.Data.Device.Handle.ToString()) == barcodeDevices[0].DeviceName)
                            {
                                if ((repeatDataScan1 == null || repeatDataScan1 != _e.Data.ToString()))
                                {
                                    barcodeDevices[0].Barcode += _e.Data;
                                    repeatDataScan1 = _e.Data.ToString();
                                    App.Current.Dispatcher.Invoke(() => Scanner1TextBox.Text += _e.Data);
                                }
                                else
                                    repeatDataScan1 = "";
                            }
                            if ((_e.Data.Device.ProductName.ToString() + " " + _e.Data.Device.Handle.ToString()) == barcodeDevices[1].DeviceName)
                            {
                                if (repeatDataScan2 == null || repeatDataScan2 != _e.Data.ToString())
                                {
                                    barcodeDevices[1].Barcode += _e.Data;
                                    repeatDataScan2 = _e.Data.ToString();
                                    App.Current.Dispatcher.Invoke(() => Scanner2TextBox.Text += _e.Data);
                                }
                                else
                                    repeatDataScan2 = "";
                            }
                        }
                        catch (ArgumentOutOfRangeException) { }
                        catch (NullReferenceException) { }
                    }
                };
                try
                {
                    Thread.Sleep(1000);
                    RawInputDevice.RegisterDevice(HidUsageAndPage.Keyboard, RawInputDeviceFlags.ExInputSink | RawInputDeviceFlags.NoLegacy, window.Handle);
                    System.Windows.Forms.Application.Run();
                }
                finally
                {
                    RawInputDevice.UnregisterDevice(HidUsageAndPage.Keyboard);
                }
            });
            keyloggerThread.IsBackground = true;
            keyloggerThread.Start();
            Thread checkBarcodeEnterEndDev1 = new Thread(() =>
            {
                Thread.Sleep(4000);
                string barcodeDevice = string.Empty;
                if (barcodeDevices.Count > 0)
                {
                    while (true)
                    {
                        if (barcodeDevice.Length != barcodeDevices[0].Barcode.Length)
                        {
                            barcodeDevice = barcodeDevices[0].Barcode;
                        }
                        else if (barcodeDevice.Length != 0)
                        {
                            barcodeDevices[0].SendToNodeRed();
                            barcodeDevice = "";
                            App.Current.Dispatcher.Invoke(() => Scanner1TextBox.Clear());
                        }
                        Thread.Sleep(500);
                    }
                }
                
            });
            checkBarcodeEnterEndDev1.IsBackground = true;
            checkBarcodeEnterEndDev1.Start();
            Thread checkBarcodeEnterEndDev2 = new Thread(() =>
            {
                Thread.Sleep(4000);
                string barcodeDevice = string.Empty;
                if (barcodeDevices.Count > 1)
                {
                    while (true)
                    {
                        if (barcodeDevice.Length != barcodeDevices[1].Barcode.Length)
                        {
                            barcodeDevice = barcodeDevices[1].Barcode;
                        }
                        else if (barcodeDevice.Length != 0)
                        {
                            barcodeDevices[1].SendToNodeRed();
                            barcodeDevice = "";
                            App.Current.Dispatcher.Invoke(() => Scanner2TextBox.Clear());
                        }
                        Thread.Sleep(500);
                    }
                }
                
            });
            checkBarcodeEnterEndDev2.IsBackground = true;
            checkBarcodeEnterEndDev2.Start();
            #endregion
            Thread healthThread = new Thread(() =>
            {
                ZebraCore zebraCore = new ZebraCore();
                while (true)
                {
                    zebraCore.FilterScannerList();
                    zebraCore.Connect();
                    zebraCore.registerForEvents();
                    System.Diagnostics.Trace.WriteLine("");
                    zebraCore.ShowScanners();
                    zebraCore.SendToKioskLife();
                    zebraCore.ZebraScanners.Clear();
                    AppInfo.Text = "ZebraApp " + zebraCore.AppVersion;
                    Thread.Sleep(2000);
                    zebraCore.Disconnect();
                }
            });
            healthThread.IsBackground = true;
            healthThread.Start();
        }

        private void MovingWin(object sender, RoutedEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                _window.DragMove();
            }
        }

        private void HideWindow_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
