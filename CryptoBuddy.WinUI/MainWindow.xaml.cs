using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoBuddy
{
    /// <summary>
    /// Main Crypto Buddy window
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        Microsoft.UI.Windowing.AppWindow m_appWindow;
        System.Timers.Timer timer;
        private bool _topMost = true;
        private readonly Mutex m = new Mutex();

        public MainWindow()
        {
            this.InitializeComponent();
            m_appWindow = GetAppWindowForCurrentWindow();
            timer = new(interval: Constants.TimerPeriodMillis);
            timer.Elapsed += (sender, e) => HandleTimer();
            timer.Start();
            ((FrameworkElement)this.Content).RequestedTheme = ElementTheme.Dark;
            TimeElapsed(null, null);
            m_appWindow.Title = Constants.AppTitle;

            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 565, Height = 380 });
        }

        void HandleTimer()
        {
            TimeElapsed(null, null);
        }

        private void SwtichPresenter_CompOverlay(object sender, RoutedEventArgs e)
        {
            //m_appWindow.SetPresenter(AppWindowPresenterKind.CompactOverlay);
            //m_appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
            //m_appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
        }

        private async void TimeElapsed(object sender, RoutedEventArgs e)
        {
            var dict = Constants.CurrencyDictionary;

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
        }

        private void GetStockValuesUpdateControls(List<Task<String>> tasks, Dictionary<int, string> taskDict)
        {
            // make sure this and related method are called on UI thread, marshall to UI thread if not.
            if (!this.stpPanel.DispatcherQueue.HasThreadAccess)
            {
                this.stpPanel.DispatcherQueue.TryEnqueue(
                      () =>
                      {
                          GetStockValuesUpdateControls(tasks, taskDict);
                      }
                    );
                return;
            }

            // because we are using a timer and some calls could be slow for network traffic, avoid re-entrancy with a mutex
            // also to avoid multiple events stacking on each other creating a packed backlog, put in a timeout.
            if (!m.WaitOne(4000, false))
            {
                return;
            }

            try
            {
                _lookupAndUpdate(tasks, taskDict);
            }
            finally
            {
                m.ReleaseMutex();
            }

        }

        private void _lookupAndUpdate(List<Task<string>> tasks, Dictionary<int, string> taskDict)
        {
            StringBuilder sb = new StringBuilder();
            this.stpPanel.Children.Clear();
            foreach (var task in tasks)
            {
                var stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;
                var currencyName = taskDict[task.Id];
                string currencyValue = "??";
                string lastClose = "??";
                string rawPage;
                try
                {
                    rawPage = task.Result;
                }
                catch
                {
                    rawPage = "";
                }

                currencyValue = GetCurrencyValue(currencyValue, rawPage);
                lastClose = GetLastCloseValue(currencyValue, rawPage);

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

                stackPanel.Children.Add(bo);

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

                stackPanel.Children.Add(bo2);

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

                stackPanel.Children.Add(bo3);

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

                stackPanel.Children.Add(bo4);

                this.stpPanel.Children.Add(stackPanel);
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

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (_topMost)
            {
                m_appWindow.SetPresenter(AppWindowPresenterKind.CompactOverlay);
                _topMost = false;
            }
            else
            {
                m_appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
                _topMost = true;
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
