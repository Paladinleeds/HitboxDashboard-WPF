using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Newtonsoft.Json.Linq;

namespace HitboxDashboard_WPF
{
    public partial class MainWindow
    {

        public String AuthToken { get; set; }
        private String UserId { get; set; }

        private Boolean GetAuthToken()
        {
            var json = new JavaScriptSerializer().Serialize(new
            {
                login = usernameTextBox.Text,
                pass = passwordTextBox.Password
            });

            var o = API.Post(API.BaseApi + "/auth/login", json);


            if (o.Equals("auth_failed"))
            {
                //errorProvider.SetError(passwordTextBox, "Password Incorrect.");
                return false;
            }
            AuthToken = JObject.Parse(o.ToString()).GetValue("authToken").ToString();
            Debug.WriteLine("AuthToken: " + AuthToken);
            return true;
        }

        private void GetUserId()
        {
            var o = API.Get(API.BaseApi + String.Format("/media/live/{0}?showHidden=true", Channel));
            var ll = JObject.Parse(o.ToString()).GetValue("livestream").First;

            UserId = JObject.Parse(ll.ToString()).GetValue("media_id").ToString();

            Debug.WriteLine("ID: " + UserId);
        }

        #region Ticks

        private void titleTimer_Tick(object sender, EventArgs e)
        {
            if (isLogged)
            {
                Dispatcher.BeginInvoke( new Action(GetTitle));
            }
        }

        private void hiddenTimer_Tick(object sender, EventArgs e)
        {
            
            if (isLogged)
            {
                Dispatcher.BeginInvoke( new Action(GetHidden));
            }
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {

            if (isLogged)
            {
                Dispatcher.BeginInvoke(new Action( GetGame ));
            }
        }

        #endregion

        #region Stream Title

        // This will get the title on first start.
        private void GetTitle()
        {
            if (streamTitle.IsFocused) return;

            var p = API.Get(String.Format("{0}/media/live/{1}.json?showHidden=true&nocache=true", API.BaseApi, Channel));
            var ll = JObject.Parse(p.ToString()).GetValue("livestream").First;
            var tr = JObject.Parse(ll.ToString()).GetValue("media_status");
            streamTitle.Text = tr.ToString();
        }

        // This will POST an update
        private void PostTitle(String title)
        {
            Media livestream2 = new Media
            {
                media_id = UserId,
                media_status = title,
                media_user_name = Channel,
                media_category_id = GetGameId(gameComboBox.Text),
                media_recording = GetRecordings().ToString(),
                media_hidden = GetHiddenBoolean()
            };
            List<Media> livestream = new List<Media> {livestream2};

            var json = new JavaScriptSerializer().Serialize(new
            {
                livestream
            });

            API.Put(
                String.Format("{0}/media/live/{1}.json?showHidden=true&authToken={2}&nocache=true", API.BaseApi, Channel,
                    AuthToken), json);
            groupBox1.Focus();
        }

        #endregion

        #region Games

        private void GetGame()
        {
            // category_name
            if (gameComboBox.IsFocused) return;
            if (gameComboBox.IsDropDownOpen) return;

            var p = API.Get(String.Format("{0}/media/live/{1}.json?showHidden=true&nocache=true", API.BaseApi, Channel));
            var ll = JObject.Parse(p.ToString()).GetValue("livestream").First;
            var tr = JObject.Parse(ll.ToString()).GetValue("category_name");
            gameComboBox.Text = tr.ToString();
        }

        private void PostGame(String gameName)
        {
            #region Hide

            //String gameObject = API.GetApi(String.Format("{0}/game/{1}?seo=true", API.BaseApi, RemoveDiacritics(gameName.Replace("-", "").Replace(".", " -").Replace(" ", "-").Replace(",", " -").Replace(":", "")))).ToString(); 

            #endregion

            Media livestream2 = new Media
            {
                media_id = UserId,
                media_status = streamTitle.Text,
                media_user_name = Channel,
                media_category_id = GetGameId(gameName),
                media_recording = GetRecordings().ToString(),
                media_hidden = GetHiddenBoolean()
            };
            List<Media> livestream = new List<Media> {livestream2};

            var json = new JavaScriptSerializer().Serialize(new
            {
                livestream
            });

            Debug.WriteLine(json);

            API.Put(
                String.Format("{0}/media/live/{1}.json?showHidden=true&authToken={2}&nocache=true", API.BaseApi, Channel,
                    AuthToken), json);
            groupBox1.Focus();
        }

        private void SearchGames(string text)
        {
            var p = API.Get(String.Format("{0}/games.json?q={1}&nocache=true", API.BaseApi, text));
            var ll = JObject.Parse(p.ToString()).GetValue("categories");
            List<string> gameList = new List<string>();
            gameComboBox.Items.Clear();
            foreach (JToken games in ll)
            {
                Debug.WriteLine(games.SelectToken("category_name").ToString());
                Debug.WriteLine(games.SelectToken("category_id").ToString());
                //(games.SelectToken("category_name").ToString(),
                //games.SelectToken("category_id").ToString());
                gameList.Add(games.SelectToken("category_name").ToString());
            }
            //gameComboBox.Items.AddRange(gameList.ToArray());
            gameList.ToList().ForEach(item => gameComboBox.Items.Add(item));
        }

        #endregion

        #region Visibility

        private void GetHidden()
        {
            var p =
                API.Get(String.Format("{0}/media/live/{1}.json?showHidden=true&authToken={2}&nocache=true", API.BaseApi,
                    Channel, AuthToken));
            var ll = JObject.Parse(p.ToString()).GetValue("livestream").First;
            var tr = JObject.Parse(ll.ToString()).GetValue("media_hidden");
            if (tr == null)
            {
                visibleBox.Visibility = Visibility.Hidden;

            }
            else if (tr.ToString().Equals("1"))
            {
                hiddenButton.IsChecked = true;
            }
            else
            {
                visibleButton.IsChecked = true;
            }
        }

        private void PostHidden(Boolean hidden)
        {
            Media livestream2 = new Media
            {
                media_id = UserId,
                media_status = streamTitle.Text,
                media_user_name = Channel,
                media_category_id = GetGameId(gameComboBox.Text),
                media_recording = GetRecordings().ToString(),
                media_hidden = hidden
            };
            List<Media> livestream = new List<Media> {livestream2};

            var json = new JavaScriptSerializer().Serialize(new
            {
                livestream
            });

            Debug.WriteLine(json);

            API.Put(
                String.Format("{0}/media/live/{1}.json?showHidden=true&authToken={2}&nocache=true", API.BaseApi, Channel,
                    AuthToken), json);
            groupBox1.Focus();
        }

        #endregion

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private int GetRecordings()
        {
            var p =
                API.Get(String.Format("{0}/media/live/{1}.json?showHidden=true&authToken={2}&nocache=true", API.BaseApi,
                    Channel, AuthToken));
            var ll = JObject.Parse(p.ToString()).GetValue("livestream").First;
            var tr = JObject.Parse(ll.ToString()).GetValue("media_recording");
            if (tr == null)
            {
                return int.Parse("0");
            }
            return int.Parse(tr.ToString());
        }

        private static string GetGameId(String rGameName)
        {
            var gameName = API.Get(String.Format("{0}/game/{1}?seo=true", API.BaseApi, RegExGame(rGameName))).ToString();
            return JObject.Parse(gameName).SelectToken("category").SelectToken("category_id").ToString();
        }

        private Boolean GetHiddenBoolean()
        {
            RadioButton checkedButton = visibleGrid.Children.OfType<RadioButton>()
                .FirstOrDefault(r => (bool) r.IsChecked);
            return checkedButton.Content.Equals("Visible") ? false : true;
        }

        private static string RegExGame(String gameName)
        {
            return Regex.Replace(RemoveDiacritics(gameName), @"([&: \,.\/']{1,})", "-");
        }

        private void GetEditors(String username)
        {
            var editorObject = API.Get(String.Format("{0}/editor/{1}?authToken={2}", API.BaseApi, username, AuthToken));
            var editorJson = JObject.Parse(editorObject.ToString()).GetValue("list");
            foreach (var users in editorJson)
            {
                editorDropDown.Items.Add(users["user_name"]);
            }
            editorDropDown.Visibility = Visibility.Visible;
            editorDropDown.IsDropDownOpen = true;
            Cursor = Cursors.Arrow;
        }


    }

    public class Media
    {
        public string media_user_name { get; set; }
        public string media_id { get; set; }
        public string media_status { get; set; }
        public string media_category_id { get; set; }
        public string media_recording { get; set; }
        public Boolean media_hidden { get; set; }
    }
}