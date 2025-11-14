using System;

namespace CT90
{
    public partial class License : MetroFramework.Forms.MetroForm
    {
        public License()
        {
            InitializeComponent();
        }

        private void License_Load(object sender, EventArgs e)
        {
            string announceLicense = "";

            announceLicense += "This application is Copyright ⓒ Hwasan System Corp. All rights reserved." + "\r\n";
            announceLicense += "\r\n";
            announceLicense += "This application use Open Source Software (OSS). You can find the source code of these open source projects, along with applicable license information, below. We are deeply grateful to these developers for their work and contributions. " + "\r\n";
            announceLicense += ("".PadRight(100, '-')) + "\r\n";
            announceLicense += "\r\n";

            announceLicense += "MetroModernUI 1.4.0" + "\r\n";
            announceLicense += "https://github.com/dennismagno/metroframework-modern-ui/" + "\r\n";
            announceLicense += "Copyright (c) 2011 Sven Walter" + "\r\n";
            announceLicense += "Copyright (c) 2013 Dennis Magno" + "\r\n";
            announceLicense += "MIT License" + "\r\n";

            announceLicense += "\r\n";
            announceLicense += "ConfuserEx" + "\r\n";
            announceLicense += "https://github.com/yck1509/ConfuserEx/" + "\r\n";
            announceLicense += "Copyright (c) 2014 yck1509" + "\r\n";
            announceLicense += "MIT License" + "\r\n";

            announceLicense += ("".PadRight(100, '-')) + "\r\n";
            announceLicense += "\r\n";

            announceLicense += "The MIT License (MIT)" + "\r\n";
            announceLicense += "\r\n";
            announceLicense += "Copyright (c) <year>, <copyright holders>" + "\r\n";
            announceLicense += "\r\n";

            announceLicense += "Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the" + @"""Software""" + "), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:" + "\r\n";
            announceLicense += "\r\n";
            announceLicense += "The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software." + "\r\n";
            announceLicense += "\r\n";
            announceLicense += "THE SOFTWARE IS PROVIDED " + @"""AS IS""" + ", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE." + "\r\n";

            mtTxtLicense.Text = announceLicense;
        }
    }
}
