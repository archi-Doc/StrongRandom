﻿// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Application;
using Arc.Mvvm;
using Arc.Text;
using Arc.WinAPI;
using Arc.WPF;
using CrossChannel;
using StrongRandom.ViewServices;

#pragma warning disable SA1403 // File may only contain a single namespace
#pragma warning disable SA1649 // File name should match first type name

namespace StrongRandom.Views
{
    /// <summary>
    /// Main Window.
    /// </summary>
    public partial class MainWindow : Window, IMainViewService
    {
        private MainViewModel vm; // ViewModel

        private IMainViewService ViewService => App.Resolve<IMainViewService>(); // To avoid a circular dependency, get an instance when necessary.

        public MainWindow(MainViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            this.vm = vm;

            try
            {
                ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(1000));
            }
            catch
            {
            }

            Transformer.Instance.Register(this, true, false);

            this.Title = App.Title;
        }

        public void Notification(NotificationMessage msg)
        { // Multi-thread safe, may be called from non-UI thread/context. App.UI.InvokeAsync()
            App.InvokeAsyncOnUI(() =>
            {
                var result = MessageBox.Show(msg.Notification);
            });
        }

        public void MessageID(MessageId id)
        {// Multi-thread safe, may be called from non-UI thread/context. App.UI.InvokeAsync()
            App.InvokeAsyncOnUI(() =>
            { // UI thread.
                if (id == MessageId.SwitchCulture)
                { // Switch culture.
                    if (App.Settings.Culture == "ja")
                    {
                        App.Settings.Culture = "en";
                    }
                    else
                    {
                        App.Settings.Culture = "ja";
                    }

                    App.C4.ChangeCulture(App.Settings.Culture);
                    Arc.WPF.C4Updater.C4Update();
                }
                else if (id == MessageId.Exit)
                { // Exit application with confirmation.
                    this.Close();
                }
                else if (id == MessageId.ExitWithoutConfirmation)
                { // Exit application without confirmation.
                    App.SessionEnding = true;
                    this.Close();
                }
                else if (id == MessageId.Information)
                {
                    var mit_license = "https://opensource.org/licenses/MIT";
                    var dlg = new Arc.WPF.Dialog(this);
                    dlg.TextBlock.Inlines.Add(
    @"Copyright (c) 2021 archi-Doc
Released under the MIT license
");
                    var h = new Hyperlink() { NavigateUri = new Uri(mit_license) };
                    h.Inlines.Add(mit_license);
                    h.RequestNavigate += (s, e) =>
                    {
                        try
                        {
                            App.OpenBrowser(e.Uri.ToString());
                        }
                        catch
                        {
                        }
                    };
                    dlg.TextBlock.Inlines.Add(h);
                    dlg.ShowDialog();
                }
                else if (id == MessageId.Settings)
                {
                    var dialog = App.Resolve<SettingsWindow>();
                    dialog.Initialize(this);
                    dialog.ShowDialog();
                }
                else if (id == MessageId.DataFolder)
                {
                    // this.Notification(new NotificationMessage(App.LocalDataFolder));
                    System.Diagnostics.Process.Start("Explorer.exe", App.LocalDataFolder);
                }
                else if (id == MessageId.DisplayScaling)
                {
                    Transformer.Instance.Transform(App.Settings.DisplayScaling, App.Settings.DisplayScaling);

                    // this.FontSize = AppConst.DefaultFontSize * App.Settings.DisplayScaling;
                }
                else if (id == MessageId.SelectResultText)
                {
                    this.textBox1.SelectAll();
                    this.textBox1.Focus();
                }
            });
        }

        public async Task<MessageBoxResult> Dialog(DialogParam p)
        { // Multi-thread safe, may be called from non-UI thread/context. App.UI.InvokeAsync()
            var dlg = new Arc.WPF.Dialog(this, p);
            var result = await dlg.ShowDialogAsync();
            return result;
            /*var tcs = new TaskCompletionSource<MessageBoxResult>();
            await this.Dispatcher.InvokeAsync(() => { dlg.ShowDialog(); tcs.SetResult(dlg.Result); }); // Avoid dead lock.
            return tcs.Task.Result;*/
        }

        public async Task<MessageBoxResult> CrossChannel_Dialog(DialogParam p)
        { // Multi-thread safe, may be called from non-UI thread/context. App.UI.InvokeAsync()
            var dlg = new Arc.WPF.Dialog(this, p);
            var result = await dlg.ShowDialogAsync();
            return result;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            if (!App.Settings.LoadError)
            { // Change the UI before this code. The window will be displayed shortly.
                IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                Arc.WinAPI.Methods.GetMonitorDpi(hwnd, out var dpiX, out var dpiY);
                WINDOWPLACEMENT wp = App.Settings.WindowPlacement.ToWINDOWPLACEMENT2(dpiX, dpiY);
                wp.length = System.Runtime.InteropServices.Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                wp.flags = 0;
                wp.showCmd = wp.showCmd == SW.SHOWMINIMIZED ? SW.SHOWNORMAL : wp.showCmd;
                Arc.WinAPI.Methods.SetWindowPlacement(hwnd, ref wp);
                Transformer.Instance.AdjustWindowPosition(this);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Exit1 (Window is still visible)
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            Arc.WinAPI.Methods.GetWindowPlacement(hwnd, out var wp);
            Arc.WinAPI.Methods.GetMonitorDpi(hwnd, out var dpiX, out var dpiY);
            App.Settings.WindowPlacement.FromWINDOWPLACEMENT2(wp, dpiX, dpiY);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void ListView_DropMove(int oldIndex, int newIndex)
        {
            if ((oldIndex >= 0) && (newIndex >= 0))
            {
                this.vm.TestGoshujin.ObservableChain.Move(oldIndex, newIndex);
            }

            return;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems.Cast<TestItem>())
            {
                item.Selection = 0;
            }

            foreach (var item in e.AddedItems.Cast<TestItem>())
            {
                item.Selection = 1;
            }

            var listView = sender as ListView;
            if (listView != null)
            {
                var item = listView.SelectedItem as TestItem;
                if (item != null)
                {
                    item.Selection = 2; // selected+focus
                }
            }
        }
    }
}
