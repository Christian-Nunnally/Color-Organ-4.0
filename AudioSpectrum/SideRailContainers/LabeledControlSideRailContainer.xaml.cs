using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AudioSpectrum.SideRailContainers
{
    /// <summary>
    /// Interaction logic for LabeledControlSideRailContainer.xaml
    /// </summary>
    public partial class LabeledControlSideRailContainer : UserControl
    {
        public object AdditionalContent
        {
            get { return GetValue(AdditionalContentProperty); }
            set { SetValue(AdditionalContentProperty, value); }
        }

        public static readonly DependencyProperty AdditionalContentProperty = DependencyProperty.Register("AdditionalContent", typeof(object), typeof(LabeledControlSideRailContainer), new PropertyMetadata(null));

        public string LabelText { get; set; }

        public LabeledControlSideRailContainer(string labelText, Control control, Orientation orientation, double controlSpace)
        {
            var desc = DependencyPropertyDescriptor.FromProperty(AdditionalContentProperty, typeof(UserControl));
            desc.AddValueChanged(this, ContentPropertyChanged);

            InitializeComponent();

            if (orientation == Orientation.Horizontal)
            {
                ControlColumn.Width = new GridLength(controlSpace);
            }
            else
            {
                ContentPresenter.SetValue(Grid.RowProperty, 1);
                ContentPresenter.SetValue(Grid.ColumnProperty, 0);
                ContentPresenter.SetValue(Grid.ColumnSpanProperty, 2);
                Label.SetValue(Grid.ColumnSpanProperty, 2);
                Label.HorizontalContentAlignment = HorizontalAlignment.Center;

                ControlGrid.Height += controlSpace;
            }
            Label.Content = labelText;
            AdditionalContent = control;
            control.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            if (orientation == Orientation.Vertical) control.Height = controlSpace - 4;
            control.Margin = new Thickness(2);
        }

        private static void ContentPropertyChanged(object sender, EventArgs e)
        {

        }
    }
}
