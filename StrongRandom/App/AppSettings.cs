﻿// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Application;
using Arc.WinAPI;
using Arc.WPF;
using Tinyhand;

namespace StrongRandom
{
    [TinyhandObject]
    public partial class AppSettings : ITinyhandSerializationCallback
    {// Application Settings
        [Key(0)]
        public bool LoadError { get; set; } // True if a load error occured.

        [Key(1)]
        public DipWindowPlacement WindowPlacement { get; set; } = default!;

        [Key(2)]
        public string Culture { get; set; } = AppConst.DefaultCulture; // Default culture

        [Key(3)]
        public double DisplayScaling { get; set; } = 1.0d; // Display Scaling

        [Key(4)]
        public TestItem.GoshujinClass TestItems { get; set; } = default!;

        [Key(5)]
        public bool CopyToClipboard { get; set; } = true;

        public void OnAfterDeserialize()
        {
            Transformer.Instance.ScaleX = this.DisplayScaling;
            Transformer.Instance.ScaleY = this.DisplayScaling;
        }

        public void OnBeforeSerialize()
        {
        }
    }
}
