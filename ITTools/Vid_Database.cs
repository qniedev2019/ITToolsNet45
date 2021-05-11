using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ITTools
{
    public partial class Vid_Database : Form
    {
        public Vid_Database()
        {
            InitializeComponent();
            //const string page = "<html><head><title></title></head><body>{0}</body></html>";
            const string page = @"<!DOCTYPE html>
                <html>
                <head>
                    <meta http-equiv='Content-Type' content='text/html; charset=unicode' />
                    <meta http-equiv='X-UA-Compatible' content='ie = edge' /> 
                    <title></title>
                </head>
<body>{0}</body>
</html>";
            webBrowser2.DocumentText = string.Format(page, "<iframe width=\"747\" height=\"404\" src=\"https://www.youtube.com/embed/cze8-NdV_O8\" frameborder=\"0\" allow=\"accelerometer; autoplay; encrypted - media; gyroscope; picture -in-picture\" allowfullscreen></iframe>");
        }
    }

}
