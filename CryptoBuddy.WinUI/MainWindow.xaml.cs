using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Media;
using WinRT;
// Needed for WindowId
using Microsoft.UI;
// Needed for AppWindow
using Microsoft.UI.Windowing;
// Needed for XAML hwnd interop
using WinRT.Interop;
using ABI.System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using Windows.UI.WindowManagement;
//using System.Drawing;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CryptoBuddy
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        Microsoft.UI.Windowing.AppWindow m_appWindow;
        System.Timers.Timer timer;

        public MainWindow()
        {
            this.InitializeComponent();
            m_appWindow = GetAppWindowForCurrentWindow();
            timer = new(interval: 15000);
            timer.Elapsed += (sender, e) => HandleTimer();
            timer.Start();
            ((FrameworkElement)this.Content).RequestedTheme = ElementTheme.Dark;
            pad_clicked1(null, null);
            m_appWindow = GetAppWindowForCurrentWindow();
            m_appWindow.Title = "CryptoBuddy";

            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 565, Height = 380 });


        }

        void HandleTimer()
        {
            pad_clicked1(null, null);
        }

        private void SwtichPresenter_CompOverlay(object sender, RoutedEventArgs e)
        {
            m_appWindow.SetPresenter(AppWindowPresenterKind.CompactOverlay);
        }

        private void SwtichPresenter_Fullscreen(object sender, RoutedEventArgs e)
        {
            m_appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        }

        private void SwtichPresenter_Overlapped(object sender, RoutedEventArgs e)
        {
            m_appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
           // myButton.Content = "Clicked";
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void pad_clicked(object sender, RoutedEventArgs e)
        {
            // get the full path to your app’s folder where it is installed
            var installedPath = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
            // join path above with the sub-paths in your Assets folder and the specific sound file
            var soundFile = Path.Join(installedPath, "Assets", "rim.wav");

            SoundPlayer player = new System.Media.SoundPlayer(soundFile);
            player.Play();
        }

        private async void pad_clicked1(object sender, RoutedEventArgs e)
        {
            var dict = new Dictionary<string, string>()
            {
                {"BTC-USD", "Bitcoin" },
                {"ETH-USD", "Etherium" },
                {"ADA-USD", "Cardano" },
                {"XRP-USD", "XRP" },
                {"DOGE-USD", "Dogecoin" },
                {"LTC-USD", "Litecoin" }
            };

            List<Task<string>> tasks = new List<Task<string>>();

            Dictionary<int, string> taskDict = new Dictionary<int, string>();
            foreach (var k in dict.Keys)
            {
                var url = $"https://www.google.com/finance/quote/{k}";
                HttpClient client = new HttpClient();
                var task1 = client.GetStringAsync(url);
                taskDict[task1.Id] = k;
                tasks.Add(task1);
                await Task.Delay(300);
            }
            try
            {
                await Task.WhenAll(tasks.ToArray());
            }
            catch (System.Exception ex)
            {

            }
            GetStockValuesUpdateControls(tasks, taskDict);
            //this.txtEtherium.Text = sb.ToString();

            //view - source:https://www.google.com/finance/quote/ETC-USD

            // get the full path to your app’s folder where it is installed
            //var installedPath = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
            //// join path above with the sub-paths in your Assets folder and the specific sound file
            //var soundFile = Path.Join(installedPath, "Assets", "kick.wav");

            //SoundPlayer player = new System.Media.SoundPlayer(soundFile);
            //player.Play();
        }

        private void GetStockValuesUpdateControls(List<Task<String>> tasks, Dictionary<int, string> taskDict)
        {
            if (! this.stpPanel.DispatcherQueue.HasThreadAccess)
            {
                this.stpPanel.DispatcherQueue.TryEnqueue(
                      () =>
                      {
                          GetStockValuesUpdateControls(tasks, taskDict);
                      }
                    );
                return;
            }
            StringBuilder sb = new StringBuilder();
            this.stpPanel.Children.Clear();
            foreach (var t in tasks)
            {
                var sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                // sp.Background = new SolidColorBrush(new Windows.UI.Color() { A = 255, R = 0, B = 0, G = 128 });

                var currencyName = taskDict[t.Id];
                string currencyValue = "??";
                string lastClose = "??";
                string rawPage;
                try
                {
                    rawPage = t.Result;
                }
                catch
                {
                    rawPage = "";
                }
                
                currencyValue = GetCurrencyValue(currencyValue, rawPage);
                lastClose = GetLastCloseValue(currencyValue, rawPage);

                if (currencyValue == "??")
                {

                }
                var s4 = currencyName.Replace("-USD", "");
                var s3 = $"{s4}: ${currencyValue} last close: ${lastClose}";
                var s5 = $"${lastClose}";

                var bo = new Border();
                bo.Background = new SolidColorBrush(new Windows.UI.Color() { A = 255, R = 64, B = 0, G = 0 });

                var tb = new TextBlock();
                tb.Text = s4;
                tb.Foreground = new SolidColorBrush(new Windows.UI.Color() { A = 255, R = 192, B = 192, G = 0 });

                tb.HorizontalAlignment = HorizontalAlignment.Left;
                tb.FontSize = 24.0;
                tb.Width = 65.0;
                tb.Margin = new Thickness(10, 0, 15, 10);

                bo.Child = tb;

                sp.Children.Add(bo);

                var bo2 = new Border();
                bo2.Background = new SolidColorBrush(new Windows.UI.Color() { A = 255, R = 0, B = 64, G = 0 });

                var tb2 = new TextBlock();
                tb2.Text = "$" + currencyValue;
                tb2.Foreground = new SolidColorBrush(new Windows.UI.Color() { A = 255, R = 192, B = 255, G = 0 });
                tb2.HorizontalAlignment = HorizontalAlignment.Left;
                tb2.FontSize = 24.0;
                tb2.Width = 110;
                tb2.HorizontalTextAlignment = TextAlignment.Right;
                tb2.Margin = new Thickness(15, 0, 15, 10);

                bo2.Child = tb2;

                sp.Children.Add(bo2);

                var bo3 = new Border();
                bo3.Background = new SolidColorBrush(new Windows.UI.Color() { A = 255, R = 0, B = 0, G = 0 });

                var tb3 = new TextBlock();
                tb3.Text = "close:";
                tb3.Foreground = new SolidColorBrush(new Windows.UI.Color() { A = 255, R = 192, B = 192, G = 192 });
                tb3.HorizontalAlignment = HorizontalAlignment.Left;
                tb3.Width = 55;
                tb3.FontSize = 22.0;
                tb3.Margin = new Thickness(15, 0, 3, 10);

                bo3.Child = tb3;

                sp.Children.Add(bo3);

                var bo4 = new Border();
                bo4.Background = new SolidColorBrush(new Windows.UI.Color() { A = 255, R = 0, B = 24, G = 0 });

                var tb4 = new TextBlock();
                tb4.Text = s5;
                tb4.Foreground = new SolidColorBrush(new Windows.UI.Color() { A = 255, R = 75, B = 145, G = 125 });
                tb4.HorizontalAlignment = HorizontalAlignment.Right;
                tb4.HorizontalTextAlignment = TextAlignment.Right;
                tb4.FontSize = 24.0;
                tb4.Width = 100;
                tb4.Margin = new Thickness(15, 0, 15, 10);

                bo4.Child = tb4;

                sp.Children.Add(bo4);




                //                <TextBlock Text="Some info here" Foreground="Purple" HorizontalAlignment="Left" Margin="0,0,0,5"></TextBlock>

                this.stpPanel.Children.Add(sp);
                //sb.Append(currencyName);
                //sb.Append(": ");
                //sb.Append(currencyValue);
                //sb.AppendLine();
            }
        }
        private static string GetCurrencyValue(string currencyValue, string rawPage)
        {
            var i = rawPage.IndexOf("data-last-price");
            if (i > -1)
            {
                var s1 = rawPage.Substring(i, 60);
                var pattern = @"\""(.*?)\""";
                var match = Regex.Match(s1, pattern);
                if (match.Captures.Any())
                {
                    var s2 = match.Captures[0].ToString();
                    s2 = s2.Replace("\"", "");
                    if (decimal.TryParse(s2, out decimal result))
                    {
                        currencyValue = result.ToString("#,###,##0.####");
                    }
                }
            }

            return currencyValue;
        }

        private static string GetLastCloseValue(string currencyValue, string rawPage)
        {
            var i = rawPage.IndexOf("The last closing price");
            if (i > -1)
            {
                //The last closing price</div></span><div class="P6K39c">0.082</div></div></div><di
                var s1 = rawPage.Substring(i, 100);
                var pattern = @"""P6K39c"">(.*?)</div>";
                var match = Regex.Match(s1, pattern);
                if (match.Captures.Any())
                {
                    var s2 = match.Captures[0].ToString();
                    s2 = s2.Replace("</div>", "");
                    s2 = s2.Replace("\"P6K39c\"", ""); 
                    s2 = s2.Replace(">", "");
                    s2 = s2.Replace("<", "");
                    if (decimal.TryParse(s2, out decimal result))
                    {
                        currencyValue = result.ToString("#,###,##0.####");
                    }
                }
            }

            return currencyValue;
        }

        private void pad_clicked2(object sender, RoutedEventArgs e)
        {
            // get the full path to your app’s folder where it is installed
            var installedPath = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
            // join path above with the sub-paths in your Assets folder and the specific sound file
            var soundFile = Path.Join(installedPath, "Assets", "clip1.wav");

            SoundPlayer player = new System.Media.SoundPlayer(soundFile);
            player.Play();
        }

        private void pad_clicked3(object sender, RoutedEventArgs e)
        {
            // get the full path to your app’s folder where it is installed
            var installedPath = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
            // join path above with the sub-paths in your Assets folder and the specific sound file
            var soundFile = Path.Join(installedPath, "Assets", "clip2.wav");

            SoundPlayer player = new System.Media.SoundPlayer(soundFile);
            player.Play();
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch.IsOn)
            {
                ((FrameworkElement)this.Content).RequestedTheme = ElementTheme.Dark;
            }
            else
            {
                ((FrameworkElement)this.Content).RequestedTheme = ElementTheme.Light;
            }
        }

        private Microsoft.UI.Windowing.AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            return Microsoft.UI.Windowing.AppWindow.GetFromWindowId(myWndId);
        }
    }
}
