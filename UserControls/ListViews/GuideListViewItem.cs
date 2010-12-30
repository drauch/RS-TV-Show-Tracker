﻿namespace RoliSoft.TVShowTracker
{
    /// <summary>
    /// Represents a TV show episode on the list view.
    /// </summary>
    public class GuideListViewItem
    {
        /// <summary>
        /// Gets or sets a value indicating whether this episode was already seen.
        /// </summary>
        /// <value><c>true</c> if it was seen; otherwise, <c>false</c>.</value>
        public bool SeenIt { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the episode.
        /// </summary>
        /// <value>The episode.</value>
        public string Episode { get; set; }

        /// <summary>
        /// Gets or sets the airdate.
        /// </summary>
        /// <value>The airdate.</value>
        public string Airdate { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        /// <value>The summary.</value>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the URL of the picture.
        /// </summary>
        /// <value>The URL of the picture.</value>
        public string Picture { get; set; }

        /// <summary>
        /// Gets a value indicating whether to show the tooltip.
        /// </summary>
        /// <value><c>true</c> if yes; otherwise, <c>false</c>.</value>
        public bool ShowTooltip
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Summary) || !string.IsNullOrWhiteSpace(Picture);
            }
        }

        /// <summary>
        /// Gets a string value indicating whether to show summary in tooltip.
        /// </summary>
        /// <value>The string boolean.</value>
        public string ShowSummary
        {
            get
            {
                return string.IsNullOrWhiteSpace(Summary) ? "Hidden" : "Visible";
            }
        }

        /// <summary>
        /// Gets a string value indicating whether to show picture in tooltip.
        /// </summary>
        /// <value>The string boolean.</value>
        public string ShowPicture
        {
            get
            {
                return string.IsNullOrWhiteSpace(Picture) ? "Hidden" : "Visible";
            }
        }
    }
}
