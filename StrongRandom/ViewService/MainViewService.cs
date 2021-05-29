// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Application;
using Arc.Mvvm;
using Arc.WPF;
using DryIoc;
using StrongRandom.Views;

namespace StrongRandom.ViewServices
{
    public interface IMainViewService
    {
        void Notification(NotificationMessage msg); // Notification Message

        void MessageID(MessageId id); // Message Id

        Task<MessageBoxResult> Dialog(DialogParam p); // Dialog
    }
}
