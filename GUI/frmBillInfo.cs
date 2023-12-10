﻿using BUS;
using DTO;
using System;
using System.Windows.Forms;

namespace GUI
{
    public partial class frmBillInfo : Form
    {
        BUS_Customer busCustomer = new BUS_Customer();
        BUS_Product busProduct = new BUS_Product();
        BUS_Employee busEmployee = new BUS_Employee();
        BUS_BillInfo busBillInfo = new BUS_BillInfo();
        BUS_Bill busBill = new BUS_Bill();
        DTO_BillInfo dtoBillInfo;
        DTO_Bill dtoBill;

        private string[] listCustomerIdName, listProductNameQuantity;
        private DateTime dateTime = new DateTime();
        private string productName, email, str, productQuantity;
        private char separator = '|';
        private string[] strlist;

        public frmBillInfo(string email)
        {
            InitializeComponent();
            this.email = email;
        }

        private void LoadData()
        {
            listCustomerIdName = busCustomer.ListCustomerIdName();

            cboCustomerIdName.Items.Clear();
            foreach (string item in listCustomerIdName)
            {
                cboCustomerIdName.Items.Add(item);
            }

            dateTime = DateTime.Now;
            txtDateTime.Text = dateTime.ToString("dd/MM/yyyy") + " " + dateTime.ToString("HH:mm");

            listProductNameQuantity = busProduct.ListProductNameQuantity();
            cboProductNameQuantity.Items.Clear();
            foreach (string item in listProductNameQuantity)
            {
                cboProductNameQuantity.Items.Add(item);
            }

            txtEmployeeIdName.Text = busEmployee.GetEmployeeIdName(email);
        }

        private void MsgBox(string message, bool isError = false)
        {
            if (isError)
                MessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                MessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadGridView()
        {
            gvBillInfo.Columns[0].HeaderText = "Mã SP";
            gvBillInfo.Columns[1].HeaderText = "Tên SP";
            gvBillInfo.Columns[2].HeaderText = "Số lượng";
            gvBillInfo.Columns[3].HeaderText = "Thành tiền";

            foreach (DataGridViewColumn item in gvBillInfo.Columns)
            {
                item.DividerWidth = 1;
            }

            gvBillInfo.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gvBillInfo.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gvBillInfo.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        private void frmBillInfo_Load(object sender, EventArgs e)
        {
            LoadData();
            gvBillInfo.DataSource = busBillInfo.ListBillInfo();
            LoadGridView();
            btnPay.Enabled = false;
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            str = cboProductNameQuantity.SelectedItem.ToString();
            strlist = str.Split(separator);
            productName = strlist[0].Trim();

            if (txtQuantity.Text != "" && cboCustomerIdName.SelectedIndex != -1 &&
                cboProductNameQuantity.SelectedIndex != -1)
            {

                if ((int.Parse(txtQuantity.Text) <= int.Parse(productQuantity)))
                {
                    dtoBillInfo = new DTO_BillInfo
                    (
                        busProduct.GetProductId(productName),
                        int.Parse(txtQuantity.Text),
                        double.Parse(txtUnitPrice.Text)
                    );

                    if (busBillInfo.InsertBillInfo(dtoBillInfo, int.Parse(txtQuantity.Text)))
                    {
                        gvBillInfo.DataSource = busBillInfo.ListBillInfo();
                        LoadGridView();
                        txtTotalPrice.Text = busBillInfo.GetTotalPrice().ToString();
                        btnPay.Enabled = true;
                        MsgBox("Đã thêm!");
                    }
                    else
                        MsgBox("Không thể thêm!", true);
                }
                else
                {
                    MsgBox("Vượt quá số lượng đang có!", true);
                }

            }
            else
                MsgBox("Vui lòng nhập đủ dữ liệu!", true);
        }

        private void btnPay_Click(object sender, EventArgs e)
        {
            str = txtEmployeeIdName.Text;
            strlist = str.Split(separator);
            string employeeId = strlist[0].Trim();

            str = cboCustomerIdName.SelectedItem.ToString();
            strlist = str.Split(separator);
            string customerId = strlist[0].Trim();

            dtoBill = new DTO_Bill
            (
                int.Parse(employeeId),
                int.Parse(customerId),
                double.Parse(txtTotalPrice.Text)
            );

            if (busBill.InsertBill(dtoBill))
            {
                MsgBox("Thanh toán thành công!");
                txtQuantity.Text = null;
                txtUnitPrice.Text = null;
                txtTotalPrice.Text = null;
                this.Close();
            }
            else
                MsgBox("Thanh toán thất bại!", true);
        }

        private void cboProductName_SelectedIndexChanged(object sender, EventArgs e)
        {
            string str = cboProductNameQuantity.SelectedItem.ToString();
            char separator = '|';
            String[] strlist = str.Split(separator);
            productName = strlist[0].Trim();
            productQuantity = strlist[1].Trim();
            txtUnitPrice.Text = busProduct.GetUnitPrice(productName).ToString();
        }
    }
}