// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.ApplicationModel.DataTransfer;

namespace StrongRandom.PresentationState;

public partial class HomePageState : ObservableObject, IState
{
    private readonly Generator generator = new();
    private readonly IMessageDialogService messageDialogService;

    [ObservableProperty]
    public partial string ResultTextValue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool ToggleCopyToClipboard { get; set; }

    public HomePageState(IMessageDialogService simpleWindowService)
    {
        this.messageDialogService = simpleWindowService;
        this.ToggleCopyToClipboard = true;
    }

    public void StoreState()
    {
    }

    [RelayCommand]
    private void Generate(string param)
    {
        var id = (GenerateId)Enum.Parse(typeof(GenerateId), param!);
        this.ResultTextValue = this.generator.Generate(id);
        if (this.ToggleCopyToClipboard)
        {
            try
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(this.ResultTextValue);
                Clipboard.SetContent(dataPackage);
            }
            catch
            {
            }
        }
    }
}
