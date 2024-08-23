// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml.Controls;
using StrongRandom.State;

namespace StrongRandom.Presentation;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        this.InitializeComponent();
        this.State = App.GetService<HomeState>();
    }

    public HomeState State { get; }
}
