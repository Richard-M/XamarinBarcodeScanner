using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.Content.PM;
using ZXing;
using ZXing.Mobile;
using System.Text;
using Android.Text.Method;

namespace ScanBarcode.Droid
{
	[Activity (Label = "ScanBarcode.Android", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
        private MobileBarcodeScanner scanner;
        private ListView barcodeList;
        ArrayAdapter<string> adapter;
        protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

            // Initialize the scanner first so we can track the current context
            MobileBarcodeScanner.Initialize(Application);

            SetContentView (Resource.Layout.Main);

            //Initialize our instance ZXing Scanner
            scanner = new MobileBarcodeScanner();

            //Initialize barcode displaying list
            barcodeList = FindViewById<ListView>(Resource.Id.barcodeList);
            adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1);            
            barcodeList.Adapter = adapter;

            SetupButtons();
		}

        void HandleScanResult(ZXing.Result result)
        {
            //sometimes if we cancel the scan result can end up being null
            if (result != null)
            {
                this.RunOnUiThread(() =>
                    {
                        //Add Barcode Text to list
                        adapter.Add(result.Text);
                        Toast.MakeText(this, "Barcode Scanned!", ToastLength.Short).Show();
                    }
               );
            }
        }

        private void SetupButtons()
        {
            Button scanButton = FindViewById<Button>(Resource.Id.scanButton);

            scanButton.Click += async delegate {
                scanner.UseCustomOverlay = false;

                //Text as suggested by Zxing
                scanner.TopText = "Hold the camera up to the barcode\nAbout 6 inches away";
                scanner.BottomText = "Wait for the barcode to automatically scan!";

                try
                {
                    //Start scanning
                    var result = await scanner.Scan();
                    HandleScanResult(result);
                }
                catch (Exception e)
                {
                    this.RunOnUiThread(() => Toast.MakeText(this, "Scan failed: " + e.Message, ToastLength.Short).Show());
                }

            };

            //Continous Scan button
            Button scanPlusButton = FindViewById<Button>(Resource.Id.scanContinuousButton);

            scanPlusButton.Click += delegate {
                scanner.UseCustomOverlay = false;

                scanner.TopText = "Hold the camera up to the barcode\nAbout 6 inches away";
                scanner.BottomText = "Wait for the barcode to automatically scan!";
                var options = new MobileBarcodeScanningOptions();
                options.DelayBetweenContinuousScans = 2000;
                try
                {
                    //Start scanning
                    scanner.ScanContinuously(options, HandleScanResult);
                }
                catch (Exception e)
                {
                    this.RunOnUiThread(() => Toast.MakeText(this, "Scan failed: " + e.Message, ToastLength.Short).Show());
                }

            };

            Button clearButton = FindViewById<Button>(Resource.Id.clearButton);

            //Clear displayed barcodes
            clearButton.Click += delegate
            {
                adapter.Clear();
            };


            Button submitButton = FindViewById<Button>(Resource.Id.submitButton);
            //Display Concatenated Scanned Results
            submitButton.Click += delegate
            {
                StringBuilder builder = new StringBuilder();
                string name = builder.ToString();
                for (int i = 0; i < adapter.Count; i++)
                {
                    builder.Append( adapter.GetItem(i) );
                }
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Submitted");
                //In case we scan a lot of barcodes make view scrollable
                TextView msg = new TextView(this);
                msg.MovementMethod = new ScrollingMovementMethod();
                msg.Text = builder.ToString();
                alert.SetView(msg);
                alert.SetPositiveButton("OK", (senderAlert, arg) => {} );
                alert.Show();
            };

        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        protected override void OnResume()
        {
            base.OnResume();

            if (ZXing.Net.Mobile.Android.PermissionsHandler.NeedsPermissionRequest(this))
            {
                ZXing.Net.Mobile.Android.PermissionsHandler.RequestPermissionsAsync(this);
            }
        }
    }
}


