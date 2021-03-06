﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Mvc;

using ContributorDB = VF_RealmPlayersDatabase.ContributorDB;
using UploadID = VF_RealmPlayersDatabase.UploadID;

using WowRealm = VF_RealmPlayersDatabase.WowRealm;

namespace RealmPlayersServer
{
    public partial class Contributors : System.Web.UI.Page
    {
        public MvcHtmlString m_BreadCrumbHTML = null;
        public MvcHtmlString m_ContributorsInfoHTML = null;
        public MvcHtmlString m_TableHeadHTML = null;
        public MvcHtmlString m_TableBodyHTML = null;
        protected void Page_Load(object sender, EventArgs e)
        {
            this.Title = "Contributors | RealmPlayers";
            //var statisticsData = DatabaseAccess.GetContributorStatistics();
            //if (statisticsData == null)
            //{
            //    PageUtility.RedirectErrorLoading(this, "contributors");
            //    return;
            //}
            m_BreadCrumbHTML = new MvcHtmlString(PageUtility.BreadCrumb_AddHome() + PageUtility.BreadCrumb_AddFinish("Contributors"));

            var statsRealms = new WowRealm[] { WowRealm.Emerald_Dream, WowRealm.Warsong, WowRealm.Al_Akir, WowRealm.Rebirth, WowRealm.Nostalrius, WowRealm.Kronos, WowRealm.Archangel };

            Dictionary<WowRealm, int> totalRealmInspects = new Dictionary<WowRealm, int>();

            string realmInspectsHeaderColumns = "";
            foreach(var statsRealm in statsRealms)
            {
                realmInspectsHeaderColumns += PageUtility.CreateTableColumnHead(StaticValues.ConvertRealmViewing(statsRealm));
                totalRealmInspects.Add(statsRealm, 0);
            }

            m_TableHeadHTML = new MvcHtmlString(PageUtility.CreateTableRow("", PageUtility.CreateTableColumnHead("#Nr") 
                + PageUtility.CreateTableColumnHead("Name") 
                + PageUtility.CreateTableColumnHead("Total inspects")
                + realmInspectsHeaderColumns
                + PageUtility.CreateTableColumnHead("Active since") 
                + PageUtility.CreateTableColumnHead("Last active")));

            SortedList<int, string> tableRows = new SortedList<int, string>();
            string tableBody = "";

            using (VF.SQLComm comm = new VF.SQLComm())
            {
                var contributors = ContributorDB.GetAllTrustWorthyContributors();
                foreach (var statRealm in statsRealms)
                {
                    totalRealmInspects[statRealm] = comm.GetRealmInspectsTotal(statRealm);
                }

                //return DynamicReloader.GetData<ContributorStatistics>(() =>
                //{
                //    VF_RPDatabase.GuildSummaryDatabase summaryDB = null;
                //    summaryDB = VF_RPDatabase.GuildSummaryDatabase.LoadSummaryDatabase(Constants.RPPDbDir);
                //    return summaryDB;
                //}, (_ContributorStatistics, _LastLoadTime) => { return (DateTime.UtcNow - _LastLoadTime).TotalMinutes > 30; });

                //DynamicReloader.GetData(() => { }, () => { return true; }, TimeSpan.FromMinutes(30), false)
                int contributorCounter = 0;
                if (false)
                {
                    foreach (var data in contributors)
                    {
                        var contributor = data.GetAsContributor();

                        DateTime earliestActive;
                        DateTime latestActive;
                        int totalInspects;
                        if (comm.GetInspectsInfoForContributor(contributor, out earliestActive, out latestActive, out totalInspects) == true)
                        {
                            string realmInspectsColumns = "";

                            if (totalInspects > 0 && data.Name != "Unknown"
                                && ((DateTime.UtcNow - latestActive).TotalDays < 15
                                || (totalInspects > 5000 && (DateTime.UtcNow - latestActive).TotalDays < 60)))
                            {
                                foreach (var statRealm in statsRealms)
                                {
                                    int inspects = comm.GetRealmInspectsForContributor(contributor, statRealm);
                                    realmInspectsColumns += PageUtility.CreateTableColumn(inspects.ToString());
                                }

                                int keyToUse = int.MaxValue - totalInspects * 100;
                                while (tableRows.ContainsKey(keyToUse) == true)
                                    keyToUse += 1;


                                tableRows.Add(keyToUse, PageUtility.CreateTableColumn(data.Name)
                                    + PageUtility.CreateTableColumn(totalInspects.ToString())
                                    + realmInspectsColumns
                                    + PageUtility.CreateTableColumn(earliestActive.ToString("yyy-MM-dd"))
                                    + PageUtility.CreateTableColumn(StaticValues.GetTimeSinceLastSeenUTC(latestActive)));
                            }
                        }


                        if (contributorCounter++ % 100 == 0)
                            Logger.ConsoleWriteLine("Generated Contributor Inspects Info for Contributor Nr " + contributorCounter);
                    }
                }
            }
            int totalALLInspects = 0;
            string totalRealmInspectsColumn = "";
            foreach (var statRealm in statsRealms)
            {
                totalRealmInspectsColumn += PageUtility.CreateTableColumn(totalRealmInspects[statRealm].ToString());
                totalALLInspects += totalRealmInspects[statRealm];
            }
            tableBody += PageUtility.CreateTableRow("", PageUtility.CreateTableColumn("#ALL") + PageUtility.CreateTableColumn("TOTAL")
                        + PageUtility.CreateTableColumn(totalALLInspects.ToString())
                        + totalRealmInspectsColumn
                        + PageUtility.CreateTableColumn("-")
                        + PageUtility.CreateTableColumn("-"));
            int i = 1;
            foreach (var tableRow in tableRows)
            {
                tableBody += PageUtility.CreateTableRow("", PageUtility.CreateTableColumn("#" + i++) + tableRow.Value);
            }
            m_TableBodyHTML = new MvcHtmlString(tableBody);

            m_ContributorsInfoHTML = new MvcHtmlString(
                "<h1>Contributors<span class='badge badge-inverse'>" + tableRows.Count() + " Players</span></h1>"
                + "<p>List displays the persons that contribute data to this project. Sorted by the amount of inspects they have contributed with.</p>"
                + "<br/><p>The more contributors the better! If you are interested in being a contributor to help out the project please do not hesitate contacting <a href='http://forum.realmplayers.com/memberlist.php?mode=viewprofile&u=51'>Sethzer</a> on RealmPlayers forums. Make sure you also read the thread: <a href='http://realmplayers.com:5555/viewtopic.php?f=14&t=15'>About Data Contribution</a>.</p><br/>");

        }
    }
}