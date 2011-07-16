﻿namespace RoliSoft.TVShowTracker.Tables
{
    using System;

    /// <summary>
    /// Represents an episode in the SQLite database.
    /// </summary>
    public class Episode
    {
        /// <summary>
        /// Gets or sets the row ID.
        /// </summary>
        /// <value>
        /// The row ID.
        /// </value>
        public int EpisodeID { get; set; }

        /// <summary>
        /// Gets or sets the show ID.
        /// </summary>
        /// <value>
        /// The show ID.
        /// </value>
        public int ShowID { get; set; }

        /// <summary>
        /// Gets or sets the season number of the episode.
        /// </summary>
        /// <value>
        /// The season number of the episode.
        /// </value>
        public int Season { get; set; }

        /// <summary>
        /// Gets or sets the episode number of the episode.
        /// </summary>
        /// <value>
        /// The episode number of the episode.
        /// </value>
        public int Number { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this episode is marked as watched
        /// </summary>
        /// <value>
        ///   <c>true</c> if marked as watched; otherwise, <c>false</c>.
        /// </value>
        public bool Watched { get; set; }

        /// <summary>
        /// Gets or sets the airdate of the episode.
        /// </summary>
        /// <value>
        /// The airdate of the episode.
        /// </value>
        public DateTime Airdate { get; set; }

        /// <summary>
        /// Gets or sets the name of the episode.
        /// </summary>
        /// <value>
        /// The name of the episode.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the episode.
        /// </summary>
        /// <value>
        /// The description of the episode.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the URL to a screencap from the episode.
        /// </summary>
        /// <value>
        /// The URL to a screencap from the episode.
        /// </value>
        public string Picture { get; set; }

        /// <summary>
        /// Gets or sets the URL of the episode on the source guide.
        /// </summary>
        /// <value>
        /// The URL of the episode on the source guide.
        /// </value>
        public string URL { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Show#{0} S{1:00}E{2:00} {3} {4}", ShowID, Season, Number, Name, Watched ? "✓" : "✗");
        }
    }
}