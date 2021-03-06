﻿namespace RoliSoft.TVShowTracker.Parsers.Downloads.Engines.Torrent
{
    using System;
    using System.Collections.Generic;
    using System.Security.Authentication;
    using System.Text.RegularExpressions;

    using NUnit.Framework;

    /// <summary>
    /// Provides support for scraping TV Chaos UK.
    /// </summary>
    [TestFixture]
    public class TVChaosUK : DownloadSearchEngine
    {
        /// <summary>
        /// Gets the name of the site.
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get
            {
                return "TV Chaos UK";
            }
        }

        /// <summary>
        /// Gets the URL of the site.
        /// </summary>
        /// <value>The site location.</value>
        public override string Site
        {
            get
            {
                return "https://tvchaosuk.com/";
            }
        }

        /// <summary>
        /// Gets the name of the plugin's developer.
        /// </summary>
        /// <value>The name of the plugin's developer.</value>
        public override string Developer
        {
            get
            {
                return "RoliSoft";
            }
        }

        /// <summary>
        /// Gets the version number of the plugin.
        /// </summary>
        /// <value>The version number of the plugin.</value>
        public override Version Version
        {
            get
            {
                return Utils.DateTimeToVersion("2011-09-12 16:35 PM");
            }
        }

        /// <summary>
        /// Gets a value indicating whether the site requires authentication.
        /// </summary>
        /// <value><c>true</c> if requires authentication; otherwise, <c>false</c>.</value>
        public override bool Private
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the names of the required cookies for the authentication.
        /// </summary>
        /// <value>The required cookies for authentication.</value>
        public override string[] RequiredCookies
        {
            get
            {
                return new[] { "c_secure_uid", "c_secure_pass" };
            }
        }

        /// <summary>
        /// Gets a value indicating whether this search engine can login using a username and password.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this search engine can login; otherwise, <c>false</c>.
        /// </value>
        public override bool CanLogin
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the URL to the login page.
        /// </summary>
        /// <value>The URL to the login page.</value>
        public override string LoginURL
        {
            get
            {
                return Site + "takelogin.php";
            }
        }

        /// <summary>
        /// Gets the input fields of the login form.
        /// </summary>
        /// <value>The input fields of the login form.</value>
        public override Dictionary<string, object> LoginFields
        {
            get
            {
                return new Dictionary<string, object>
                    {
                        { "username", LoginFieldTypes.UserName },
                        { "password", LoginFieldTypes.Password },
                    };
            }
        }

        /// <summary>
        /// Gets the type of the link.
        /// </summary>
        /// <value>The type of the link.</value>
        public override Types Type
        {
            get
            {
                return Types.Torrent;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this site is deprecated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if deprecated; otherwise, <c>false</c>.
        /// </value>
        public override bool Deprecated
        {
            get
            {
                // My account on this site has been banned due to inactivity.
                // Because of this, I am unable to unit test and maintain this plugin.
                // If you want to take over the maintenance of the plugin:
                // a) fork the git repo and submit pull requests on Github.
                // b) email me the patches to this file and I will commit them on your behalf.
                return true;
            }
        }

        /// <summary>
        /// Searches for download links on the service.
        /// </summary>
        /// <param name="query">The name of the release to search for.</param>
        /// <returns>List of found download links.</returns>
        public override IEnumerable<Link> Search(string query)
        {
            var html = Utils.GetHTML(Site + "browse.php", "do=search&search_type=t_name&category=0&include_dead_torrents=yes&keywords=" + Utils.EncodeURL(ShowNames.Parser.ReplaceEpisode(query, "- S{0:00}E{1:00}", false, false)), cookies: Cookies);

            if (GazelleTrackerLoginRequired(html.DocumentNode))
            {
                throw new InvalidCredentialException();
            }

            var links = html.DocumentNode.SelectNodes("//div[@class='tooltip-target']/a");

            if (links == null)
            {
                yield break;
            }

            foreach (var node in links)
            {
                var link = new Link(this);

                link.Release = node.GetTextValue("../../div[2]/div[1]").Trim();
                link.InfoURL = node.GetAttributeValue("href");
                link.FileURL = node.GetNodeAttributeValue("../../../td[3]/a", "href");
                link.Size    = node.GetHtmlValue("../../../td[5]").Trim();
                link.Quality = Regex.IsMatch(link.Release, @"\b(HD|720p|1080[ip]|[xh]\.?264)\b")
                               ? Qualities.HDTV720p
                               : Qualities.HDTVXviD;
                link.Infos   = Link.SeedLeechFormat.FormatWith(node.GetTextValue("../../../td[7]").Trim(), node.GetTextValue("../../../td[8]").Trim())
                             + (node.GetHtmlValue("../../div/span/img[starts-with(@title, \"Fast Speed\")]") != null ? " / " + Regex.Match(node.GetNodeAttributeValue("../../div/span/img[starts-with(@title, \"Fast Speed\")]", "title"), @"has (\d+)").Groups[1].Value + " seedbox" : string.Empty)
                             + (node.GetHtmlValue("../../div/span/img[starts-with(@title, \"Free Torrent\")]") != null ? ", Free" : string.Empty);

                yield return link;
            }
        }

        /// <summary>
        /// Authenticates with the site and returns the cookies.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>Cookies on success, <c>string.Empty</c> on failure.</returns>
        public override string Login(string username, string password)
        {
            return GazelleTrackerLogin(username, password);
        }
    }
}
