﻿namespace RoliSoft.TVShowTracker
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using Image = System.Windows.Controls.Image;
    using Label = System.Windows.Controls.Label;
    using Orientation = System.Windows.Controls.Orientation;

    /// <summary>
    /// Interaction logic for XMLTVWindow.xaml
    /// </summary>
    public partial class XMLTVWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XMLTVWindow"/> class.
        /// </summary>
        public XMLTVWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XMLTVWindow"/> class.
        /// </summary>
        /// <param name="config">The config.</param>
        public XMLTVWindow(Dictionary<string, object> config)
        {
            InitializeComponent();

            Title = "Edit XML file";
            Config = config;

            nameTextBox.Text   = (string)config["Name"];
            pathTextBox.Text   = (string)config["File"];
            advParse.IsChecked = config.ContainsKey("Advanced Parsing") && config["Advanced Parsing"] is bool && (bool)config["Advanced Parsing"];
            useMap.IsChecked   = config.ContainsKey("Use English Titles") && config["Use English Titles"] is bool && (bool)config["Use English Titles"];
            tzTextBox.Text     = (config.ContainsKey("Time Zone Correction") && config["Time Zone Correction"] is double ? (double) config["Time Zone Correction"] : 0).ToString("0.0");
        }

        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        /// <value>
        /// The config.
        /// </value>
        public Dictionary<string, object> Config { get; set; }

        /// <summary>
        /// Handles the Loaded event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (AeroGlassCompositionEnabled)
            {
                SetAeroGlassTransparency();
            }

            var sel = Config != null && Config.ContainsKey("Language") && Config["Language"] is string ? (string) Config["Language"] : "en";
            var idx = 0;

            foreach (var lang in Languages.List)
            {
                var sp = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Tag         = lang.Key
                    };

                sp.Children.Add(new Image
                    {
                        Source = new BitmapImage(new Uri("/RSTVShowTracker;component/Images/flag-" + lang.Key + ".png", UriKind.Relative)),
                        Height = 16,
                        Width  = 16,
                        Margin = new Thickness(0, 0, 0, 0)
                    });

                sp.Children.Add(new Label
                    {
                        Content = " " + lang.Value,
                        Padding = new Thickness(0)
                    });

                language.Items.Add(sp);

                if (lang.Key == sel)
                {
                    idx = language.Items.Count - 1;
                }
            }

            language.SelectedIndex = idx;
        }

        /// <summary>
        /// Handles the Click event of the pathBrowseButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void PathBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
                {
                    Title           = "Select the XML file generated by an XMLTV module",
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Multiselect     = false,
                    Filter          = "XML files|*.xml|All files|*.*"
                };

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pathTextBox.Text = ofd.FileName;
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the tzTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.TextChangedEventArgs"/> instance containing the event data.</param>
        private void TzTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            double d;
            tzTextBox.Background = double.TryParse(tzTextBox.Text, out d) ? Brushes.White : Brushes.LightPink;
        }

        /// <summary>
        /// Handles the Click event of the cancelButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the Click event of the saveButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) || nameTextBox.Text == "(your country/TV provider)" || string.IsNullOrWhiteSpace(pathTextBox.Text)) return;

            if (!File.Exists(pathTextBox.Text))
            {
                System.Windows.MessageBox.Show("The specified file does not exists.", "Invalid file", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            double tz;
            if (!double.TryParse(tzTextBox.Text, out tz))
            {
                System.Windows.MessageBox.Show("The specified number for timezone correction is invalid.", "Invalid number", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            var list = Settings.Get<List<Dictionary<string, object>>>("XMLTV");

            if (Config == null)
            {
                Config = new Dictionary<string, object>();
                list.Add(Config);
            }

            Config["Name"] = nameTextBox.Text.Trim();
            Config["File"] = pathTextBox.Text.Trim();
            Config["Language"] = (string) ((StackPanel) language.SelectedItem).Tag;
            Config["Advanced Parsing"] = advParse.IsChecked;
            Config["Use English Titles"] = useMap.IsChecked;
            Config["Time Zone Correction"] = tz;

            Settings.Set("XMLTV", list);
            Settings.Save();

            DialogResult = true;
        }
    }
}
