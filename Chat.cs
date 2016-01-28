using System;
using System.Drawing;
using System.Web;
using System.Windows.Media;
using Brush = System.Drawing.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

namespace HitboxDashboard_WPF
{
    public partial class MainWindow
    {
        #region ChatMessages

        private void ws_webSocketMessage(object sender, WebSocketMessageEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                if (!e.Message.StartsWith("SYSTEM:") && !e.Message.StartsWith("Info:"))
                {
                    var hextorgb = ColorTranslator.FromHtml("#" + e.Color);
                    var userColor = Color.FromArgb(hextorgb.A, hextorgb.R, hextorgb.G, hextorgb.B);
                    SolidColorBrush brush = new SolidColorBrush(userColor);
                    chatTextBox.Foreground = brush;
                    chatTextBox.Inlines.Add(e.UserName);
                    chatTextBox.Foreground = Brushes.Black;
                    chatTextBox.Inlines.Add(": ");
                    chatTextBox.Inlines.Add(HttpUtility.HtmlDecode(e.Message));
                }
                else
                {
                    chatTextBox.Inlines.Add(String.Format("{0}", e.Message));
                }
                chatTextBox.Inlines.Add(Environment.NewLine);
            }));
        }



        #endregion
    }
}
