using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace HitboxDashboard_WPF
{
    public partial class MainWindow
    {
        private JArray Admins;
        private JArray isCommunity;
        private JArray isStaff;
        private JArray Mods;
        private JArray Viewers;
        private JArray isSubscriber;
        private List<string> viewerList;

        public List<String> Numbers
        {
            get { return viewerList; }
        }

        #region User List

        private static JArray SortRanks(JArray JSA)
        {
            return new JArray { JSA.OrderBy(e => e).ToArray() };
        }

        public void LoadUserData(string json)
        {
            #region jsonData

            //json = "{\"data\":{\"Guests\":null,\"admin\":[\"Hitakashi-Test\",\"Hitakashi\",\"Staff-Member\",\"AStaff-Member\",\"Community-Member\", \"ACommunity-Member\"],\"user\":[\"Test-User\",\"Another-User\"]," +
            //"\"anon\":[\"Claptrap\",\"AViewer\"],\"isFollower\":[\"Hitakashi\",\"AFollower\"],\"isSubscriber\":[]," +
            //"\"isStaff\":[\"Staff-Member\",\"AStaff-Member\"],\"isCommunity\":[\"Community-Member\",\"ACommunity-Member\"]}}";

            #endregion

            var JS = JObject.Parse(json).SelectToken("data");
            Admins = SortRanks((JArray)JS.SelectToken("admin"));
            Mods = SortRanks((JArray)JS.SelectToken("user"));
            Viewers = SortRanks((JArray)JS.SelectToken("anon"));
            isStaff = SortRanks((JArray)JS.SelectToken("isStaff"));
            isCommunity = SortRanks((JArray)JS.SelectToken("isCommunity"));
            isSubscriber = SortRanks((JArray) JS.SelectToken("isSubscriber"));
            //Debug.WriteLine("Admins: " + Admins);
            //Debug.WriteLine("Staff: " + isStaff);
            PopulateUsers();
        }

        public void LoadBanList(string json)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                //banUsersList.BeginUpdate();
                banUsersList.Items.Clear();
                var JS = JObject.Parse(json).SelectToken("data");
                foreach (String user in JS)
                {
                    Debug.WriteLine("Banned: " + user);
                    banUsersList.Items.Add("[Ban] " + user);
                }
                //banUsersList.EndUpdate();
            }));
        }

        private void PopulateUsers()
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                //allUsersList.BeginUpdate();
                //modUsersList.BeginUpdate();
                //viewerUsersList.BeginUpdate();
                allUsersList.Items.Clear();
                modUsersList.Items.Clear();
                viewerUsersList.Items.Clear();


                foreach (string admins in Admins)
                {
                    if (admins.Equals(Channel, StringComparison.CurrentCultureIgnoreCase))
                    {
                        modUsersList.Items.Insert(0, "[B] " + admins);
                        allUsersList.Items.Insert(0, "[B] " + admins);
                    }
                    else if (!API.Contains(isStaff, admins) & !API.Contains(isCommunity, admins))
                    {
                        // Editors
                        modUsersList.Items.Add("[E] " + admins);
                        allUsersList.Items.Add("[E] " + admins);
                    }
                }

                foreach (string staff in isStaff)
                {
                    modUsersList.Items.Add("[S] " + staff);
                    allUsersList.Items.Add("[S] " + staff);
                }

                foreach (string community in isCommunity)
                {
                    modUsersList.Items.Add("[A] " + community);
                    allUsersList.Items.Add("[A] " + community);
                }

                foreach (string mods in Mods)
                {
                    modUsersList.Items.Add("[M] " + mods);
                    allUsersList.Items.Add("[M] " + mods);
                }

                foreach (string sub in isSubscriber)
                {
                    if (!API.Contains(isStaff, sub) && !API.Contains(isCommunity, sub) && !API.Contains(Admins, sub) && !API.Contains(Mods, sub))
                    {
                        allUsersList.Items.Add("[Sub] " + sub);
                        viewerUsersList.Items.Add("[Sub] " + sub);
                    }
                }

                foreach (string viewers in Viewers)
                {
                    if (!API.Contains(isSubscriber, viewers))
                    {
                        allUsersList.Items.Add(viewers);
                        viewerUsersList.Items.Add(viewers);
                    }
                }

                //allUsersList.EndUpdate();
                //modUsersList.EndUpdate();
                //viewerUsersList.EndUpdate();
            }));
        }

        #endregion
    }
}