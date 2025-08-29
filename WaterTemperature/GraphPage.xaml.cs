using WaterTemperature.ViewModels;

namespace WaterTemperature;

public partial class GraphPage : ContentPage
{
    public GraphPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
