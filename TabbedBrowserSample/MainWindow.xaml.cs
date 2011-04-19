using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AwesomiumSharp;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using System.Collections;

namespace TabbedBrowserSample
{
    public partial class MainWindow : Window
    {
        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;
        const int WM_CHAR = 0x0102;
        ArrayList tabViewList;
        DispatcherTimer updateTimer;

        public MainWindow()
        {
            InitializeComponent();

            // Setup Update Timer
            updateTimer = new DispatcherTimer();
            updateTimer.Tick += new EventHandler(update);
            updateTimer.Interval = new TimeSpan(0, 0, 0, 0, 15);
            updateTimer.Start();

            this.SourceInitialized += browserWindowSourceInitialized;
            txtAddress.KeyDown += txtAddressKeyDown;
            tabControl.SelectionChanged += tabControlChanged;

            // Setup Webcore with plugins enabled
            WebCore.Config config = new WebCore.Config();
            config.enablePlugins = true;
            WebCore.Initialize(config);

            tabViewList = new ArrayList();
        }

        protected override void OnClosed(EventArgs e)
        {
            updateTimer.Stop();
            WebCore.Shutdown();
            base.OnClosed(e);
        }

        private void update(object sender, EventArgs e)
        {
            WebCore.Update();

            foreach (TabView i in tabViewList)
                i.render();
        }

        private TabView createTab()
        {
            TabView tab = new TabView(tabControl);
            tab.OnUpdateTitle += onTabTitleUpdated;
            tab.OnUpdateURL += onTabURLUpdated;
            tab.OnOpenExternalLink += onTabOpenExternalLink;
            tabViewList.Add(tab);
            tabControl.SelectedItem = tab.getTab();

            return tab;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            TabView selectedTab = getSelectedTab();
            if (selectedTab != null)
                selectedTab.goToHistoryOffset(-1);
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            TabView selectedTab = getSelectedTab();
            if (selectedTab != null)
                selectedTab.goToHistoryOffset(1);
        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            btnGo.IsDefault = false;

            TabView selectedTab = getSelectedTab();
            if (selectedTab != null)
            {
                if (txtAddress.Text.StartsWith("http://") == true)
                    selectedTab.loadUrl(txtAddress.Text);
                else if (txtAddress.Text != "")
                    selectedTab.loadUrl("http://" + txtAddress.Text);
            }
        }

        private TabView getSelectedTab()
        {
            TabView result = null;

            foreach (TabView i in tabViewList)
            {
                if (i.getTab() == tabControl.SelectedItem)
                {
                    result = i;
                    break;
                }
            }

            return result;
        }

        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            TabView selectedTab = getSelectedTab();
            if (selectedTab != null)
                selectedTab.zoomIn();
        }

        private void btnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            TabView selectedTab = getSelectedTab();
            if (selectedTab != null)
                selectedTab.zoomOut();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            TabView selectedTab = getSelectedTab();
            if (selectedTab != null)
                selectedTab.reload();
        }

        void browserWindowSourceInitialized(object sender, EventArgs e)
        {
            HwndSource source = (HwndSource)PresentationSource.FromVisual(this);
            source.AddHook(HandleMessages);
        }

        IntPtr HandleMessages(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            int message = (msg & 65535);

            if ((message == WM_KEYDOWN || message == WM_KEYUP || message == WM_CHAR) && !txtAddress.IsFocused)
            {
                TabView selectedTab = getSelectedTab();
                if (selectedTab != null)
                {
                    selectedTab.handleKeyboardEvent((int)msg, (int)wParam, (int)lParam);
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        void txtAddressKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                btnGo_Click(null, null);
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            TabView selectedTab = getSelectedTab();
            if (selectedTab != null)
                selectedTab.loadURL("http://www.google.com");
        }

        private void onTabURLUpdated(object sender, EventArgs e)
        {
            TabView selectedTab = getSelectedTab();
            if (selectedTab != null)
                txtAddress.Text = selectedTab.getURL();
        }

        private void onTabTitleUpdated(object sender, EventArgs e)
        {
            TabView selectedTab = getSelectedTab();
            if (selectedTab != null)
                this.Title = "Tabbed Browser Sample - Awesomium - " + selectedTab.getTitle();
        }

        private void onTabOpenExternalLink(object sender, OpenLinkEventArgs e)
        {
            TabView tab = createTab();
            tab.loadUrl(e.url);
            tabControl.SelectedItem = tab.getTab();
        }

        private void addTab_Click(object sender, RoutedEventArgs e)
        {
            createTab();
        }

        private void removeTab_Click(object sender, RoutedEventArgs e)
        {
            if (tabControl.Items.Count > 1)
            {
                TabView selectedTab = getSelectedTab();
                if (selectedTab != null)
                {
                    tabViewList.Remove(selectedTab);
                    tabControl.Items.Remove(tabControl.SelectedItem);
                }
            }
        }

        private void tabControlChanged(object sender, RoutedEventArgs e)
        {
            foreach (TabView i in tabViewList)
            {
                if (i.getTab() == tabControl.SelectedItem)
                {
                    txtAddress.Text = i.getURL();
                    this.Title = "Tabbed Browser Sample - Awesomium - " + i.getTitle();
                    i.focus();
                }
                else
                {
                    i.unfocus();
                }
            }
        }

        private void browserWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TabView tab = createTab();
            tab.loadUrl("http://www.google.com");
        }

        private void print_Click(object sender, RoutedEventArgs e)
        {
            TabView selectedTab = getSelectedTab();
            if (selectedTab != null)
                selectedTab.print();
        }
    }

}