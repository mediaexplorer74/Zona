// MainPage

using System;
using System.Diagnostics;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core; 
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

namespace Zona
{
    // MainPage class 
    public sealed partial class MainPage : Page
    {
        private static readonly String UserAgentID =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
            "AppleWebKit/537.36 (KHTML, like Gecko) " +
            "Chrome/70.0.3538.102 Safari/537.36 Edge/18.19041";

        private static readonly Uri 
            ZonaUri = new Uri("https://w140.zona.plus", UriKind.Absolute);

        //MainPage
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            WebViewControl.Settings.IsJavaScriptEnabled = true;
            WebViewControl.Settings.IsIndexedDBEnabled = true;

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, ZonaUri);
            
            //RnD
            //requestMessage.Headers.Add("User-Agent", UserAgentID);
            
            WebViewControl.NavigateWithHttpRequestMessage(requestMessage);
        }//MainPage


        // OnNavigatedTo 
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility
                = AppViewBackButtonVisibility.Visible;

            SystemNavigationManager.GetForCurrentView().BackRequested += (s, a) =>
            {
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
                    if (WebViewControl.CanGoBack)
                    {
                        WebViewControl.GoBack();
                    }
                    a.Handled = true;
                };
            }
        }//OnNavigatedTo


        // OnNavigatedFrom
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //
        }//OnNavigatedFrom


        // Browser_NavigationCompleted
        private void Browser_NavigationCompleted(WebView sender, 
            WebViewNavigationCompletedEventArgs args)
        {
            //
        }//Browser_NavigationCompleted

    }//MainPage class end

}//namespace end

