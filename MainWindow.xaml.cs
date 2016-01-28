using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;


namespace HitboxDashboard_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static int adCount;
        public String Channel;
        private Boolean GoodToPost;
        private Boolean isEditor;
        private Boolean isLogged;
        private Boolean isNewGame = true;
        private String LastGameSearch = "";
        private String _username = "";
        public String Username
        {
            get { return _username; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new Exception("Username Cannot Be Empty.");
                }
                _username = value;
            }
        }
        private WebSocketData ws;
        private Thread WSThread;
        private DispatcherTimer userListTimer;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            //EventManager.RegisterClassHandler(typeof(ListBoxItem),
            //    MouseRightButtonDownEvent,
            //    new RoutedEventHandler(allUsersList_MouseRightClick));
            InitializeRun();
            AddlDashboard a = new AddlDashboard();
            a.Show();
        }

        private String Game { get; set; }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if (WSThread != null)
                WSThread.Abort();
        }

        private void InitializeRun()
        {
            // Custom items for layout.
            // Hide Info and Dashboard tabs.
            tabControl.SelectedItem = dashboardTab.Visibility = Visibility.Collapsed;
        }

        //private void aboutLabel_click(object sender, EventArgs e)
        //{
        //    using (var box = new AboutBox1())
        //    {
        //        box.ShowDialog(this);
        //    }
        //}
        private void ForceValidation()
        {
            usernameTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }
        private void hitboxSubmitLogin_Click(object sender, RoutedEventArgs e)
        {
            ForceValidation();
            
            //errorProvider.Clear();
            if (Username.Trim().Length != 0)
            {
                // They inputted a username. If they have a password auth them
                if (passwordTextBox.Password.Trim().Length != 0)
                {
                    // Password found, Auth them.
                    //errorProvider.Clear();
                    AuthenticateUser();
                }
                else
                {
                    //errorProvider.SetError(passwordTextBox, "Missing Password.");
                }
            }
            else
            {
                //errorProvider.SetError(usernameTextBox, "Missing Username.");
                //throw new ApplicationException("Missing Username.");
            }
        }

        private void AuthenticateUser()
        {
            if (String.IsNullOrEmpty(AuthToken) && !GetAuthToken())
            {
                return;
            }

            Username = usernameTextBox.Text;
            if (editorCheck.IsChecked != null && (bool) editorCheck.IsChecked && !isEditor)
            {
                isEditor = true;
                GetEditors(Username);
                return;
            }
            if (editorDropDown.SelectedIndex == -1)
            {
                editorCheck.IsChecked = false;
            }
            if (editorCheck.IsChecked != null && (bool) editorCheck.IsChecked && isEditor && editorDropDown.SelectedIndex != -1)
            {
                Channel = editorDropDown.Text;
            }
            else
            {
                Channel = Username;
            }

            // We have a valid Auth Token. Hide the login screen and proceed to the info page.
            tabControl.SelectedItem = dashboardTab.Visibility = Visibility.Visible;
            dashboardTab.IsSelected = true;
            tabControl.SelectedItem = loginTab.Visibility = Visibility.Collapsed;
            GetUserId();
            GetTitle();
            GetGame();
            GetHidden();
            isLogged = true;
            chatBox.Focus();
            Debug.WriteLine("Recording: " + GetRecordings());

            userListTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10000) };
            WSThread = new Thread(() =>
            {
                ws = new WebSocketData(this);
                ws.WebSocketMessage += ws_webSocketMessage;
                userListTimer.Tick += ws.timer_Tick;
            });
            WSThread.Start();
            userListTimer.Start();
        }






        //private void HitboxForm_FormClosing(object sender, FormClosingEventArgs e)
        //{
        //    userListTimer.Stop();
        //    userListTimer.Dispose();
        //    if (WSThread != null)
        //        WSThread.Abort();
        //}

    }

    public class TextBoxNotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string str = value as string;
            if (str != null)
            {
                if (str.Length > 0)
                    return ValidationResult.ValidResult;
            }
            return new ValidationResult(false, Message);
        }

        public String Message { get; set; }
    }
}
