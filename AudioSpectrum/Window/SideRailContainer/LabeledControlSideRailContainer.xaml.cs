using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AudioSpectrum.SideRailContainers
{
    /// <summary>
    ///     Interaction logic for LabeledControlSideRailContainer.xaml
    /// </summary>
    public partial class LabeledControlSideRailContainer : UserControl
    {

        public static readonly DependencyProperty AdditionalContentProperty = DependencyProperty.Register("AdditionalContent", typeof(object), typeof(LabeledControlSideRailContainer), new PropertyMetadata(null));

        /// <summary>
        ///     Initializes a new instance of the <see cref="LabeledControlSideRailContainer" /> class.
        /// </summary>
        /// <param name="labelText">
        ///     The label text. "" if you want this control to not be labeled and the contained control to take
        ///     up the entire width of the container
        /// </param>
        /// <param name="control">The control.</param>
        /// <param name="orientation">The orientation.</param>
        /// <param name="controlSpace">
        ///     The control space.  This sets the controls horizontal or vertical space depending on the
        ///     orientation
        /// </param>
        public LabeledControlSideRailContainer(string labelText, Control control, Orientation orientation, double controlSpace)
        {
            var desc = DependencyPropertyDescriptor.FromProperty(AdditionalContentProperty, typeof(UserControl));
            desc.AddValueChanged(this, ContentPropertyChanged);

            InitializeComponent();

            Label.Content = labelText;
            AdditionalContent = control;
            control.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            if (orientation == Orientation.Horizontal)
            {
                if (labelText == string.Empty)
                    LabelColumn.Width = new GridLength(0);
                ControlColumn.Width = labelText == string.Empty ? new GridLength(1, GridUnitType.Auto) : new GridLength(controlSpace);
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

            if (orientation == Orientation.Vertical) control.Height = controlSpace - 4;
            control.Margin = new Thickness(2);
        }

        public object AdditionalContent
        {
            get { return GetValue(AdditionalContentProperty); }
            set { SetValue(AdditionalContentProperty, value); }
        }

        public string LabelText { get; set; }

        private static void ContentPropertyChanged(object sender, EventArgs e)
        {
        }
    }
}