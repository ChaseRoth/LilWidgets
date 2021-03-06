﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using LilSamples.Interfaces;

namespace LilSamples
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            DependencyService.Get<INonClientAreaStyler>().SetNonClientArea((Color)Resources["ColorPrimary"], Enumerations.DisplayThemes.Light);
            MainPage = new MasterDetailContainerPage();
        }

        protected override void OnStart()
        {
            
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
