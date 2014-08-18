using BPMM_App.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace BPMM_App
{

    public sealed partial class PhotoPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private String mruToken = null;

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public PhotoPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (e.PageState != null && e.PageState.ContainsKey("mruToken"))
            {
                object value = null;
                if (e.PageState.TryGetValue("mruToken", out value))
                {
                    if (value != null)
                    {
                        mruToken = value.ToString();
                        StorageFile file = await StorageApplicationPermissions.MostRecentlyUsedList.GetFileAsync(mruToken);

                        if (file != null)
                        {
                            IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.SetSource(fileStream);
                            displayImage.Source = bitmapImage;
                            DataContext = file;
                        }
                    }
                }
            }
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (!string.IsNullOrEmpty(mruToken))
            {
                e.PageState["mruToken"] = mruToken;
            }
        }

        #region NavigationHelper Registration
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void GetPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.ViewMode = PickerViewMode.Thumbnail;

            openPicker.FileTypeFilter.Clear();
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".jpg");

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileStream);
                displayImage.Source = bitmapImage;
                this.DataContext = file;

                mruToken = StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
            }
        }

        private void PhotoPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height / e.NewSize.Width >= 1)
            {
                VisualStateManager.GoToState(this, "Portrait", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "DefaultLayout", true);
            }
        }
    }
}
