// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Application;
using Arc.Mvvm;
using Arc.WPF;
using StrongRandom.Models;
using StrongRandom.ViewServices;
using ValueLink;

#pragma warning disable SA1201 // Elements should appear in the correct order

namespace StrongRandom
{
    [ValueLinkObject]
    public partial class MainViewModel
    {
        private IMainViewService ViewService => App.Resolve<IMainViewService>(); // To avoid a circular dependency, get an instance when necessary.

        private Generator generator;

        public AppOptions Options => App.Options;

        public TestItem.GoshujinClass TestGoshujin { get; } = App.Settings.TestItems;

        [Link(AutoNotify = true)]
        private string resultText = "Result";

        [Link(AutoNotify = true)]
        private bool testNotify;

        private bool toggleCopyToClipboard;

        public bool ToggleCopyToClipboard
        {
            get => this.toggleCopyToClipboard;
            set
            {
                this.SetProperty(ref this.toggleCopyToClipboard, value);
                App.Settings.CopyToClipboard = value;
            }
        }

        private ICommand? commandClearItem;

        public ICommand CommandClearItem
        {
            get
            {
                return this.commandClearItem ?? (this.commandClearItem = new DelegateCommand(
                    () =>
                    {
                        this.TestGoshujin.Clear();
                    }));
            }
        }

        private ICommand? commandMessageId;

        public ICommand CommandMessageId
        {
            get
            {
                return this.commandMessageId ?? (this.commandMessageId = new DelegateCommand<string>(
                    (param) =>
                    { // execute
                        var id = (MessageId)Enum.Parse(typeof(MessageId), param!);
                        this.ViewService.MessageID(id);
                    }));
            }
        }

        private ICommand? commandGenerate;

        public ICommand CommandGenerate
        {
            get
            {
                return this.commandGenerate ?? (this.commandGenerate = new DelegateCommand<string>(
                    (param) =>
                    { // execute
                        var id = (GenerateId)Enum.Parse(typeof(GenerateId), param!);
                        this.ResultTextValue = this.generator.Generate(id);
                        if (this.ToggleCopyToClipboard)
                        {
                            Clipboard.SetText(this.ResultTextValue);
                        }

                        this.ViewService.MessageID(MessageId.SelectResultText);
                    }));
            }
        }

        public MainViewModel(Generator generator)
        {
            this.generator = generator;

            this.ToggleCopyToClipboard = App.Settings.CopyToClipboard;
        }
    }
}
