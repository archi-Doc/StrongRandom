﻿// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Arc.WinUI;

namespace StandardWinUI;

[TinyhandObject(ImplicitKeyAsName = true)]
public partial class AppSettings
{// Application Settings
    public const string Filename = "AppSettings.tinyhand";

    public DipWindowPlacement WindowPlacement { get; set; } = default!;

    public string Culture { get; set; } = string.Empty;

    public double ViewScale { get; set; } = 1.0d;

    [TinyhandOnSerializing]
    public void OnBeforeSerialize()
    {
        this.ViewScale = Scaler.ViewScale;
    }

    [TinyhandOnDeserialized]
    public void OnAfterDeserialize()
    {
        Scaler.ViewScale = this.ViewScale;
    }
}
