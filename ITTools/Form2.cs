using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ITTools
{
    public partial class Form2 : Form
    {
        public static int updated_stt = 0, print_stt = 0;
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            lblUD01.Text = ""; lblUD02.Text = ""; lblRT01.Text = ""; lblRT02.Text = "";
            lblBK01.Text = ""; lblBK02.Text = "";
            GetData();
        }

        public void GetData()
        {
            //CHECK BACKUP
            if (Form1.BackupDB == 1)
            {
                lblBK01.Text = Form1.BackupDB_act;
            }else if (Form1.BackupDB == 2)
            {
                lblBK02.Text = Form1.BackupDB_act;
            }
            else
            {
                lblBK01.Text = "";
                lblBK02.Text = "";
            }

            //CHECK RESTORE
            if (Form1.Restored_pos == 1)
            {
                lblRT01.Text = "- " + Form1.Restored_pos_act;
            }
            else
            {
                lblRT01.Text = "";
            }

            if (Form1.Restored_merchant == 1)
            {
                lblRT02.Text = "- " + Form1.Restored_merchant_act;
            }
            else
            {
                lblRT02.Text = "";
            }

            //CHECK UPDATE
            

            if (Form1.Updated_pos == 1)
            {
                lblUD01.Text = "- " + Form1.Updated_pos_act;
            }
            if (Form1.Updated_merchant == 1)
            {
                lblUD02.Text = "- " + Form1.Updated_merchant_act;
            }
            
            if(Form1.Restored_pos == 1 && Form1.Updated_pos != 1 ) //Restored database POS but not update.
            {
                lblUD01.Text = "- BLogicPOS7 => NOT UPDATE";
                lblUD01.ForeColor = Color.White;
                lblUD01.BackColor = Color.Red;
                btn_warning_update_1.Visible = true;
                updated_stt = 1;
            }
            if (Form1.Restored_merchant == 1 && Form1.Updated_merchant != 1) //Restored database MERCHANT but not update.
            {
                lblUD02.Text = "- Merchant => NOT UPDATE";
                lblUD02.ForeColor = Color.White;
                lblUD02.BackColor = Color.Red;
                btn_warning_update_2.Visible = true;
                updated_stt = 1;
            }

            //CHECK PRINTER 
            //Printer_status
            if (Form1.printer_status == " is PAUSED")
            {
                lblPrinterMess.Text = "THE PRINTER IS PAUSED, CHECK AND MUST BE RESUME IT.";
                lblPrinterStatus.Text = Form1.printer_status;
                lblPrinterName.ForeColor = Color.White;
                lblPrinterName.BackColor = Color.Red;
                btnRed.Visible = true;
                lblPrinterName.Text = "The printer " +Form1.printer_name;
                lblPrinterStatus.ForeColor = Color.White;
                lblPrinterStatus.BackColor = Color.Red;
                print_stt = 1;
            }
            else
            {
                lblPrinterName.Text = Form1.printer_name;
                lblPrinterStatus.Text = Form1.printer_status;
                lblPrinterMess.Text = "";
            }
        }

        
    }
}
