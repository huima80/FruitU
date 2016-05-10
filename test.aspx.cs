using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class test : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string agentOpenID = string.Empty;
        string urlReferrer = @"http://mahui.me/index.aspx?AgentOpenID=o5gbrsi8S5pKF-2kmOR1gU2x9OCY";
        Match rxAgentOpenID = Regex.Match(urlReferrer, @"AgentOpenID=(.*)", RegexOptions.IgnoreCase);
        if (rxAgentOpenID != null && rxAgentOpenID.Groups.Count == 2)
        {
            agentOpenID = rxAgentOpenID.Groups[1].Value;
        }

    }
}