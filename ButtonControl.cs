using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Automation.Provider;
using ListView = System.Windows.Controls.ListView;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MenuItem = System.Windows.Controls.MenuItem;

namespace HitboxDashboard_WPF
{
    public partial class MainWindow
    {
        public String un;

        private void usernameTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Console.WriteLine("Username Hit");
                //UIElementAutomationPeer.CreatePeerForElement(hitboxSubmitLogin).GetPattern(PatternInterface.Invoke);

                //UIElementAutomationPeer peer = new UIElementAutomationPeer(hitboxSubmitLogin);

                //IInvokeProvider invokeProv = (IInvokeProvider)peer.GetPattern(PatternInterface.Invoke);
                //invokeProv.Invoke();

                ButtonAutomationPeer peer = new ButtonAutomationPeer(hitboxSubmitLogin);
                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                if (hitboxSubmitLogin.IsEnabled)
                    invokeProv.Invoke();
            }
        }

        private void passwordTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            hitboxSubmitLogin.IsEnabled = passwordTextBox.Password.Length != 0;
            if (e.Key == Key.Enter)
            {
                //UIElementAutomationPeer.CreatePeerForElement(hitboxSubmitLogin).GetPattern(PatternInterface.Invoke);

                //UIElementAutomationPeer peer = new UIElementAutomationPeer(passwordTextBox);
                //IInvokeProvider invokeProv = (IInvokeProvider)peer.GetPattern(PatternInterface.Invoke);
                //if (invokeProv != null) invokeProv.Invoke();

                ButtonAutomationPeer peer = new ButtonAutomationPeer(hitboxSubmitLogin);
                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                if (hitboxSubmitLogin.IsEnabled)
                    invokeProv.Invoke();
            }
        }

        private void gameComboBox_SelectionChangeCommitted(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("Selection Changed");
            GoodToPost = true;
        }

        private void adPlusButton_Click(object sender, RoutedEventArgs e)
        {
            if (adCount < 10)
                adCount++;
            adButton.Content = Regex.Replace(adButton.Content.ToString(), @"[0-9]{1,2}", adCount.ToString());
        }

        private void adMinusButton_Click(object sender, RoutedEventArgs e)
        {
            if (adCount > 0)
                adCount--;
            adButton.Content = Regex.Replace(adButton.Content.ToString(), @"[0-9]{1,2}", adCount.ToString());
        }

        private void adButton_Click(object sender, RoutedEventArgs e)
        {
            if (adCount > 0)
            {
                // Enough Ads to run.
                var json = new JavaScriptSerializer().Serialize(new
                {
                    authToken = AuthToken,
                });

                API.PostAync(String.Format("{0}/ws/combreak/{1}/{2}", API.BaseApi, Channel, adCount), json);
                ws.GuiMessage(Username, "SYSTEM: Sent commercial command.", "000000");
            }
        }

        private void visibleButton_Click(object sender, RoutedEventArgs e)
        {
            PostHidden(false);
        }

        private void hiddenButton_Click(object sender, RoutedEventArgs e)
        {
            PostHidden(true);
        }

        private void gameComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            Game = gameComboBox.Text;
            Debug.WriteLine("Game: " + Game);


            if (e.Key == Key.Enter && GoodToPost && !isNewGame)
            {
                PostGame(gameComboBox.Text);
                GoodToPost = false;
                isNewGame = false;
                LastGameSearch = Game;
            }
            else if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(Game) && isNewGame && !LastGameSearch.Equals(Game))
                {
                    SearchGames(gameComboBox.Text);
                    isNewGame = false;
                    LastGameSearch = Game;
                    gameComboBox.IsDropDownOpen = true;
                    Cursor = Cursors.Arrow;
                }
            }
            else
            {
                isNewGame = true;
                GoodToPost = false;
            }
        }

        private void chatBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ws.SendChat(chatBox.Text);
                chatBox.Clear();
            }
        }

        private void streamTitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PostTitle(streamTitle.Text);
            }
        }

        private void allUsersList_MouseRightClick(object sender, MouseButtonEventArgs e)
        {
            ListView item2 = (ListView)sender;

            un = (String) item2.SelectedItem;

            if (String.IsNullOrEmpty(un))
                return;
            item2.ContextMenu = new ContextMenu();

            MenuItem mention = new MenuItem { Header = "Mention", Name="Test", VerticalContentAlignment = VerticalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center};
            mention.Click += MenuAction;
            MenuItem timeout = new MenuItem { Header = "Timeout", VerticalContentAlignment = VerticalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center };
            timeout.Click += MenuAction;
            MenuItem ban = new MenuItem { Header = "Ban", VerticalContentAlignment = VerticalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center };
            ban.Click += MenuAction;
            MenuItem unban = new MenuItem { Header = "Unban", VerticalContentAlignment = VerticalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center };
            unban.Click += MenuAction;
            MenuItem mod = new MenuItem { Header = "Mod", VerticalContentAlignment = VerticalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center };
            mod.Click += MenuAction;
            MenuItem unmod = new MenuItem { Header = "Unmod", VerticalContentAlignment = VerticalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center };
            unmod.Click += MenuAction;

            // Ban List




            if (un.StartsWith("[B]") ||
                un.StartsWith("[E]") ||
                un.StartsWith("[S]") ||
                un.StartsWith("[A]"))
            {
                item2.ContextMenu.Items.Clear();
                item2.ContextMenu.Items.Add(mention);
            }
            else if (un.StartsWith("[M]"))
            {
                item2.ContextMenu.Items.Clear();
                item2.ContextMenu.Items.Add(mention);
                item2.ContextMenu.Items.Add(timeout);
                item2.ContextMenu.Items.Add(ban);
                item2.ContextMenu.Items.Add(unmod);
            } else if (un.StartsWith("[Ban]"))
            {
                item2.ContextMenu.Items.Clear();
                item2.ContextMenu.Items.Add(unban);

            }
            else
            {
                item2.ContextMenu.Items.Clear();

                item2.ContextMenu.Items.Add(mention);
                item2.ContextMenu.Items.Add(timeout);
                item2.ContextMenu.Items.Add(ban);
                item2.ContextMenu.Items.Add(mod);
            }
        }

        //private void allUsersList_MouseRightClick(object sender, RoutedEventArgs e)
        //{
        //    ListBoxItem item = (ListBoxItem) sender;


        //    MenuItem mention = new MenuItem { Header = "Mention" };
        //    mention.Click += MenuAction;
        //    MenuItem timeout = new MenuItem { Header = "Timeout" };
        //    timeout.Click += MenuAction;
        //    MenuItem ban = new MenuItem { Header = "Ban" };
        //    ban.Click += MenuAction;
        //    MenuItem unban = new MenuItem {Header = "Unban"};
        //    unban.Click += MenuAction;
        //    MenuItem mod = new MenuItem { Header = "Mod" };
        //    mod.Click += MenuAction;
        //    MenuItem unmod = new MenuItem { Header = "Unmod" };
        //    unmod.Click += MenuAction;

        //    // Ban List

           


        //    if (item.Content.ToString().StartsWith("[B]") ||
        //        item.Content.ToString().StartsWith("[E]") ||
        //        item.Content.ToString().StartsWith("[S]") ||
        //        item.Content.ToString().StartsWith("[A]"))
        //    {
        //        tabControl.ContextMenu.Items.Clear();
        //        tabControl.ContextMenu.Items.Add(mention);
        //    }
        //    else if (item.Content.ToString().StartsWith("[M]"))
        //    {
        //        tabControl.ContextMenu.Items.Clear();
        //        tabControl.ContextMenu.Items.Add(mention);
        //        tabControl.ContextMenu.Items.Add(timeout);
        //        tabControl.ContextMenu.Items.Add(ban);
        //        tabControl.ContextMenu.Items.Add(unmod);
        //    }
        //    else
        //    {
        //        tabControl.ContextMenu.Items.Clear();

        //        tabControl.ContextMenu.Items.Add(mention);
        //        tabControl.ContextMenu.Items.Add(timeout);
        //        tabControl.ContextMenu.Items.Add(ban);
        //        tabControl.ContextMenu.Items.Add(mod);
        //    }
        //}

        private void MenuAction(object sender, RoutedEventArgs routedEventArgs)
        {
            MenuItem menu = sender as MenuItem;
            
            String user = Regex.Replace(un, @"(\[(A|S|B|E|M|Ban|Sub)\]\s)", "");


            switch (menu.Header.ToString())
            {
                case "Mention":
                    if (chatBox.Text.Length > 0)
                    {
                        chatBox.AppendText(" @" + user + " ");
                    }
                    else
                    {
                        chatBox.AppendText("@" + user + " ");
                    }
                    break;
                case "Mod":
                    ws.SendMod(user);
                    break;
                case "Unmod":
                    ws.SendUnMod(user);
                    break;
                case "Timeout":
                    ws.SendTimeout(user);
                    break;
                case "Ban":
                    ws.SendBan(user);
                    break;
                case "Unban":
                    ws.SendUnBan(user);
                    break;
            }
        }

        private void AddlButton_Click(object sender, RoutedEventArgs e)
        {

        }


        //private void modUsersList_MouseClick(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Right)
        //    {
        //        if (modUsersList.FocusedItem.Bounds.Contains(e.Location))
        //        {
        //            if (modUsersList.FocusedItem.Text.StartsWith("[B]") ||
        //                modUsersList.FocusedItem.Text.StartsWith("[E]") ||
        //                modUsersList.FocusedItem.Text.StartsWith("[S]") ||
        //                modUsersList.FocusedItem.Text.StartsWith("[A]"))
        //            {
        //                BroadcasterList_Context();
        //            }
        //            else if (modUsersList.FocusedItem.Text.StartsWith("[M]"))
        //            {
        //                ModList_Context();
        //            }
        //            modContextMenu.Show(Cursor.Position);
        //        }
        //    }
        //}
        //private void viewerUsersList_MouseClick(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Right)
        //    {
        //        if (viewerUsersList.FocusedItem.Bounds.Contains(e.Location))
        //        {
        //            viewerList_Context();
        //        }
        //        viewerContextMenu.Show(Cursor.Position);
        //    }
        //}
        //private void banUserList_MouseClick(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Right)
        //    {
        //        if (banUserList.FocusedItem.Bounds.Contains(e.Location))
        //        {
        //            BanList_Context();
        //        }
        //        bannedContextMenu.Show(Cursor.Position);
        //    }
        //}
        //private void viewerList_Context()
        //{
        //    allContextMenu.Items.Clear();
        //    allContextMenu.Items.Add("Mention");
        //    allContextMenu.Items.Add("Timeout");
        //    allContextMenu.Items.Add("Ban");
        //    allContextMenu.Items.Add("Mod");
        //}
        //private void ModList_Context()
        //{
        //    modContextMenu.Items.Clear();
        //    modContextMenu.Items.Add("Mention");
        //    modContextMenu.Items.Add("Timeout");
        //    modContextMenu.Items.Add("Ban");
        //    modContextMenu.Items.Add("Unmod");
        //}
        //private void BroadcasterList_Context()
        //{
        //    modContextMenu.Items.Clear();
        //    modContextMenu.Items.Add("Mention");
        //}
        //private void BanList_Context()
        //{
        //    bannedContextMenu.Items.Clear();
        //    bannedContextMenu.Items.Add("Unban");
        //}
        //private void modContextMenu_ItemClicked(object sender, RoutedEventArgs e)
        //{
        //    MenuItem menuItem = sender as MenuItem;
        //    ContextMenu context = e.OriginalSource as ContextMenu;


        //    String user = Regex.Replace(modUsersList.FocusedItem.Text, @"(\[(A|S|B|E|M|Ban|Sub)\]\s)", "");

        //    switch (e.ClickedItem.Text)
        //    {
        //        case "Mention":
        //            if (chatBox.TextLength > 0)
        //            {
        //                chatBox.AppendText(" @" + user + " ");
        //            }
        //            else
        //            {
        //                chatBox.AppendText("@" + user + " ");
        //            }
        //            break;
        //        case "Timeout":
        //            ws.SendTimeout(user);
        //            break;
        //        case "Ban":
        //            ws.SendBan(user);
        //            break;
        //        case "Unban":
        //            ws.SendUnBan(user);
        //            break;
        //    }
        //}


        //private void viewerContextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        //{
        //    String user = Regex.Replace(viewerUsersList.FocusedItem.Text, @"(\[(A|S|B|E|M|Ban|Sub)\]\s)", "");

        //    switch (e.ClickedItem.Text)
        //    {
        //        case "Mention":
        //            if (chatBox.TextLength > 0)
        //            {
        //                chatBox.AppendText(" @" + user + " ");
        //            }
        //            else
        //            {
        //                chatBox.AppendText("@" + user + " ");
        //            }
        //            break;
        //        case "Timeout":
        //            ws.SendTimeout(user);
        //            break;
        //        case "Ban":
        //            ws.SendBan(user);
        //            break;
        //        case "Unban":
        //            ws.SendUnBan(user);
        //            break;
        //    }
        //}
        //private void bannedContextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        //{
        //    String user = Regex.Replace(banUserList.FocusedItem.Text, @"(\[(A|S|B|E|M|Ban|Sub)\]\s)", "");

        //    switch (e.ClickedItem.Text)
        //    {
        //        case "Mention":
        //            if (chatBox.TextLength > 0)
        //            {
        //                chatBox.AppendText(" @" + user + " ");
        //            }
        //            else
        //            {
        //                chatBox.AppendText("@" + user + " ");
        //            }
        //            break;
        //        case "Timeout":
        //            ws.SendTimeout(user);
        //            break;
        //        case "Ban":
        //            ws.SendBan(user);
        //            break;
        //        case "Unban":
        //            ws.SendUnBan(user);
        //            break;
        //    }
        //}
        //private void allContextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        //{
        //    String user = Regex.Replace(allUsersList.FocusedItem.Text, @"(\[(A|S|B|E|M|Ban|Sub)\]\s)", "");

        //    switch (e.ClickedItem.Text)
        //    {
        //        case "Mention":
        //            if (chatBox.TextLength > 0)
        //            {
        //                chatBox.AppendText(" @" + user + " ");
        //            }
        //            else
        //            {
        //                chatBox.AppendText("@" + user + " ");
        //            }
        //            break;
        //        case "Timeout":
        //            ws.SendTimeout(user);
        //            break;
        //        case "Ban":
        //            ws.SendBan(user);
        //            break;
        //        case "Unban":
        //            ws.SendUnBan(user);
        //            break;
        //    }
        //}
        //private void restartLabel_Click(object sender, EventArgs e)
        //{
        //    // Restart Button
        //    Process.Start(Application.ExecutablePath); // to start new instance of application
        //    Application.Exit();
        //    Close(); //to turn off current app
        //}
        //private void dashPopOutLabel_Click(object sender, EventArgs e)
        //{
        //    //DashboardOption box = new DashboardOption(ws, this);
        //    //using (var box = new DashboardOption())
        //    //{
        //       // box.Show();
        //    //}
        //}
    }
}