using WaterTemperature.ViewModels;

namespace WaterTemperature;

public partial class MeasurementsPage : ContentPage
{
    public MeasurementsPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
