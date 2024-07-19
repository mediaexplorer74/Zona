using System;
using System.Diagnostics;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core; // * Smart Navigation *
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

namespace FourPDAClientApp
{
    // MainPage class 
    public sealed partial class MainPage : Page
    {
        private static readonly Uri HomeUri = 
            new Uri("ms-appx-web:///Html/index.html", UriKind.Absolute);                                                                                                                                  // Included HTML file.
      
        private static readonly Uri 
            FourPDAAppUri = new Uri("https://w140.zona.plus", UriKind.Absolute);

      
        private static readonly Color ThemeLight = Color.FromArgb(255, 230, 230, 230);
        private static readonly Color ThemeDark = Color.FromArgb(255, 31, 31, 31);   

        private static int ZoomFactor = 100;                                                                                                                                                                                                // Zoom percentage (100 default, but might set smaller in future since the text is a bit large in my opinion).

        
        private static FourPDAClientApp.WebViewMode CurrentViewMode 
            = FourPDAClientApp.WebViewMode.Compact; // Launch in compact mode.


        //MainPage
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            Initialize();

            //RnD
            ZoomFactor += 1;
            UpdateWebViewZoom();

            ZoomFactor -= 1;
            UpdateWebViewZoom();
        }//MainPage


        // Initialize
        public void Initialize()
        {             
            WebViewControl.Settings.IsJavaScriptEnabled = true;            
            WebViewControl.Settings.IsIndexedDBEnabled = true;

            StorageManager.Init();            
            ReadAppData();

            //RnD
            RemoveClutterFromPage();

            UpdateWebViewZoom();
         
        }//Initialize


      
        // StoreZoomFactor
        public void StoreZoomFactor()
        {
            StorageManager.WriteSimpleSetting(FourPDAClientApp.SAVE_DATA_ZOOMFACTOR, 
                ZoomFactor);
        }//StoreZoomFactor


        // ReadAppData
        // Reads settings from storage and init(s) Light/Dark mode and Zoom factor
        public void ReadAppData()
        {
            // Light/Dark mode
            Object isUiThemeToggleChecked = StorageManager.ReadSimpleSetting(
                FourPDAClientApp.SAVE_DATA_DARKMODE);

          
                ToFourPDAClientApp();
          

            // Zoom factor
            Object textZoomFactor = StorageManager.ReadSimpleSetting(
                FourPDAClientApp.SAVE_DATA_ZOOMFACTOR);

            if (textZoomFactor != null)
                ZoomFactor = (int)textZoomFactor;

            //ChangeUIThemeThroughApp(UiThemeToggle.IsChecked.Value);
        }//ReadAppData


        // OnNavigatedTo 
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //ToFourPDAClientApp();
            //ForcePageOnScreen();
            base.OnNavigatedTo(e);

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility
                = AppViewBackButtonVisibility.Visible;

            SystemNavigationManager.GetForCurrentView().BackRequested += (s, a) =>
            {
                //Debug.WriteLine("Special Back button Requested");
                if (WebViewControl.CanGoBack)
                {
                    WebViewControl.GoBack();
                    a.Handled = true;
                }
            };

            if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
            {
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += (s, a) =>
                {
                    //Debug.WriteLine("Hardware Back button Requested");
                    if (WebViewControl.CanGoBack)
                    {
                        WebViewControl.GoBack();
                    }
                    a.Handled = true;
                };
            }
        }

        // BackButton handler
        private void BackButton_Tapped(object sender, BackRequestedEventArgs e)
        {
            //Debug.WriteLine("BACK button pressed: " + e.ToString());

            if (WebViewControl.CanGoBack)
            {
                WebViewControl.GoBack();
                //Debug.WriteLine("BaseUri: " + WebViewControl.BaseUri.ToString());
            }
        }


        // OnNavigatedFrom
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // do nothing
        }


        // Browser_NavigationCompleted
        private void Browser_NavigationCompleted(WebView sender, 
            WebViewNavigationCompletedEventArgs args)
        {
            // to do more things
            UpdateWebViewZoom();
        }//Browser_NavigationCompleted

      

        //Home_Click
        private void Home_Click(object sender, RoutedEventArgs e)
        {
            ToFourPDAClientApp();
        }//Home_Click

        // ZoomIn handler
        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ZoomFactor += 1;
            UpdateWebViewZoom();
        }

        // ZoomOut handler
        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ZoomFactor -= 1;
            UpdateWebViewZoom();
        }

      

        // ToFourPDAClientApp
        private void ToFourPDAClientApp()
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, FourPDAAppUri);
           //requestMessage.Headers.Add("User-Agent", UserAgentPersonal);             
            WebViewControl.NavigateWithHttpRequestMessage(requestMessage);
        }//ToFourPDAClientApp



        // UpdateWebViewZoom
        private void UpdateWebViewZoom()
        {
            ScriptInjector.InvokeScriptOnWebView
            (
                WebViewControl,
                ScriptInjector.ChangeTextSizeForEachElement(ZoomFactor)
            );
            
            StoreZoomFactor();
        }//UpdateWebViewZoom


        // Back_Click
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (WebViewControl.CanGoBack)
            {
                WebViewControl.GoBack();
            }
        }//Back_Click


        // Forward_Click
        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            // Log it
            Debug.WriteLine("Toolbar FORWARD button pressed: " + e.ToString());

            if (WebViewControl.CanGoForward)
            {
                WebViewControl.GoForward();
                //Debug.WriteLine("BaseUri: " + WebViewControl.BaseUri.ToString());
            }
        }//Forward_Click



        private void RemoveClutterFromPage()
        {
            ScriptInjector.InvokeScriptsOnWebView
            ( 
                WebViewControl,
                ScriptInjector.HideElementByClass(FourPDAClientApp.NOTIFICATION_CLASS),
                ScriptInjector.HideElementByClass(FourPDAClientApp.FLEX_CONTAINER_CLASS),
                ScriptInjector.HideElementByClass(FourPDAClientApp.SETTINGS_BAR_CLASS)
            );
        }



    }//MainPage class end


    // ScriptInjector class
    public class ScriptInjector
    {
        public static string ChangeFontSizeByID(int percentage, string id)
        {
            return $@"document.getElementById('{id}').style.fontSize = '{percentage}%';";
        }

        public static string HideElementByClass(string item)
        {
            return $@"
            const elements = document.getElementsByClassName('{item}');
            for (var i = 0; i < elements.length; i++) elements[i].style.display = 'none';
            ";
        }

        public static string ShowElementByClass(string item)
        {
            return $@"
            const elements = document.getElementsByClassName('{item}');
            for (var i = 0; i < elements.length; i++) elements[i].style.display = '';
            ";
        }

        public static string ModifyElementsWidthByClass(int percentage_width, string item)
        {
            return $@"
            const elements = document.getElementsByClassName('{item}');
            for (var i = 0; i < elements.length; i++) elements[i].style.width='{percentage_width}%';
            ";
        }

        public static string ChangeTextSizeForEachElement(int percentage)
        {
            return $@"
            var elements = document.getElementsByTagName('div');
            for (var i = 0; i < elements.length; i++) 
                elements[i].style.fontSize = '{percentage}%';
            ";
        }

        public static string ChangeMinWidthForEachElement(int minimum_width_px)
        {
            return $@"
            var elements = document.getElementsByTagName('div');
            for (var i = 0; i < elements.length; i++) 
                elements[i].style.minWidth = '{minimum_width_px}px';
            ";
        }

        public static string ChangeBodyClassName(string item)
        {
            return $@"document.body.className = '{item}';";
        }

        public static string ChangeFlexByClass(int percentage, string item)
        {
            return $@"
            const elements = document.getElementsByClassName('{item}');
            for (var i = 0; i < elements.length; i++) elements[i].style.flexBasis='{item}%';
            ";
        }

        public static async void InvokeScriptOnWebView(WebView webView, String script)
        {
            // Do not use await, crashes x86 / x64 app due to invalid javascript syntax somewhere.
            webView.InvokeScriptAsync("eval", new String[] { script });
        }

        public static async void InvokeScriptsOnWebView(WebView webView, 
            params String[] scripts)
        {
            for (int i = 0; i < scripts.Length; i++)
            {
                InvokeScriptOnWebView(webView, scripts[i]);
            }
        }
    }//ScriptInjector class end


    // FourPDAClientApp class
    public static class FourPDAClientApp
    {
        // HTML classes and id's which could be modified using javascript injection.
        // To add more navigate to 4pda.to in Firefox (Developer Edition)
        // on your computer and hit f12 (or right click and hit the inspection button). 
        public static readonly string SETTINGS_BAR_CLASS = "_1QUKR";                   
        public static readonly string NOTIFICATION_CLASS = "_3kqUo";
        public static readonly string FLEX_CONTAINER_CLASS = "v-count";// i2z2ViY8z2V U7p21YIoHNlc";//"h70RQ two";
        public static readonly string AD_CONTAINER_CLASS1 = "ei2Zc";// i2z2ViY8z2V U7p21YIoHNlc";
        public static readonly string AD_CONTAINER_CLASS2 = "ei2Zc i2z2ViY8z2V i2z2z2k4kyhFiHjOXz26ie";
        public static readonly string EFFECTIVE_FLEX_LEFT_CONTAINER_CLASS = "_1xXdX";
        public static readonly string EFFECTIVE_FLEX_RIGHT_CONTAINER_CLASS = "Wu52Z";
        public static readonly string CONTACT_SEARCH_CHAT = "_2EoyP";
        public static readonly string SAVE_DATA_DARKMODE = "theme_dark";
        public static readonly string SAVE_DATA_ZOOMFACTOR = "text_zoomfactor";
        public static readonly string SAVE_DATA_FORUMMODE = "forum_mode";

        public enum WebViewMode
        {
            Compact,
            Chat,
            Contacts
        }
    }//FourPDAClientApp class end


    // StorageManager class
    public static class StorageManager
    {
        private static Windows.Storage.ApplicationDataContainer localSettings;
        private static Windows.Storage.StorageFolder localFolder;

        public static void Init()
        {
            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        }

        public static Object ReadSimpleSetting(string id)
        {
            if (localSettings != null && localFolder != null)
            {
                try
                {
                    return localSettings.Values[id];
                }
                catch { }
            }
            else DebugWarnNotInitialised();

            return null;
        }

        public static void WriteSimpleSetting(string id, Object toStore)
        {
            if (localSettings != null && localFolder != null)
            {
                localSettings.Values[id] = toStore;
            }
            else DebugWarnNotInitialised();
        }

        private static void DebugWarnNotInitialised()
        {
            Debug.WriteLine(@"Have you called ""StorageManager.Init()""?");
        }
    }//StorageManager class end
}

